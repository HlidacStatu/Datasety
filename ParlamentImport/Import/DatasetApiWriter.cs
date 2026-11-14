using System.Net;
using HlidacStatu.Api.V2.Dataset.Api;
using HlidacStatu.Api.V2.Dataset.Client;
using HlidacStatu.Api.V2.Dataset.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using Serilog;

namespace HlidacStatu.ParlamentImport.Import;

/// <summary>
/// Writes the snapshot into the public dataset <see cref="ParliamentMember.DatasetId"/> through the Hlídač státu REST API:
/// the NuGet client HlidacStatu.Api.V2.Dataset registers the dataset, reads the stored records and upserts members
/// ("rewrite" mode); members no longer present are removed with DELETE api/v2/datasety/{id}/zaznamy/{itemId},
/// which the NuGet client does not cover, so it is called over plain HTTP with the same API token.
/// </summary>
internal sealed class DatasetApiWriter(IDatasetyApiAsync datasets, ICoreApiAsync core, HttpClient httpClient, ImportOptions options)
{
    /// <summary>Page size of api/v2/datasety/{id}/hledat (ApiV2Controller.DefaultResultPageSize on the server).</summary>
    private const int SearchPageSize = 25;

    private const string InsertModeRewrite = "rewrite";

    /// <summary>
    /// Sort option of the dataset UI; the same default the server uses. It must be sent: the server
    /// (Registration.orderList setter) fails with HTTP 500 when the API client sends orderList as null.
    /// </summary>
    private const string OrderByImportLabel = "Datumu importu do db";

    private const string OrderByImportField = "DbCreated";
    private const string MatchAllQuery = "*";

    private const string DatasetName = "Složení Parlamentu ČR a europoslanci za ČR";

    private const string DatasetDescription =
        "Aktuální poslanci, senátoři a poslanci Evropského parlamentu zvolení v České republice: poslanecké a senátorské kluby, "
        + "frakce v Evropském parlamentu, senátní obvody a vedení komor. Data denně přebíráme z otevřených dat Poslanecké sněmovny (psp.cz), "
        + "z webu Senátu (senat.cz) a z Open Data API Evropského parlamentu.";

    private const string DatasetOrigUrl = "https://www.psp.cz/sqw/hp.sqw?k=1300";

    private const string DatasetSourceCodeUrl = "https://github.com/HlidacStatu/Datasety/tree/master/ParlamentImport";

    /// <summary>Default sort of the dataset UI, the same the server uses when none is registered.</summary>
    private const string DefaultOrderBy = "DbCreated desc";

    /// <summary>Placeholder owner; the server always replaces it with the account of the API token.</summary>
    private const string CreatedByPlaceholder = "api-token-owner";

    /// <summary>Template header and footer are inserted verbatim around the body; HTML comments keep the page unchanged.</summary>
    private const string TemplateHeader = "<!-- parlament-slozeni -->";

    private const string TemplateFooter = "<!-- /parlament-slozeni -->";

    private const string SearchTemplate = """
        <table class="table table-hover">
        <thead><tr><th>Jméno</th><th>Komora</th><th>Klub / frakce</th><th>Strana</th><th>Obvod</th><th>Funkce</th></tr></thead>
        <tbody>
        {{ for item in model.Result }}
        <tr>
        <td><a href="{{ fn_DatasetItemUrl item.id }}">{{ item.fullName }}</a></td>
        <td>{{ item.chamber }}</td>
        <td>{{ item.club }}</td>
        <td>{{ item.party }}</td>
        <td>{{ if item.districtNumber }}{{ item.districtNumber }} – {{ item.districtName }}{{ end }}</td>
        <td>{{ item.function }}</td>
        </tr>
        {{ end }}
        </tbody></table>
        """;

    private const string DetailTemplate = """
        {{this.item = model}}
        <table class="table table-hover"><tbody>
        <tr><td>Jméno</td><td>{{ item.fullName }}</td></tr>
        <tr><td>Profil na Hlídači státu</td><td>{{ fn_RenderPersonWithLink2 item.nameId }}</td></tr>
        <tr><td>Komora</td><td>{{ item.chamber }}</td></tr>
        <tr><td>Klub / frakce</td><td>{{ item.club }}</td></tr>
        <tr><td>Strana</td><td>{{ item.party }}</td></tr>
        <tr><td>Volební obvod</td><td>{{ if item.districtNumber }}{{ item.districtNumber }} – {{ item.districtName }}{{ end }}</td></tr>
        <tr><td>Funkce</td><td>{{ item.function }}</td></tr>
        <tr><td>Zdroj</td><td><a href="{{ item.sourceUrl }}" target="_blank">{{ item.sourceUrl }}</a></td></tr>
        <tr><td>Aktualizováno</td><td>{{ fn_FormatDate item.importedUtc }}</td></tr>
        </tbody></table>
        """;

    private static readonly ILogger Logger = Log.ForContext<DatasetApiWriter>();

    private static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Include,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
    });

    /// <summary>
    /// Makes sure the dataset exists; with <paramref name="recreate"/> it is deleted first (only its owner may do that).
    /// </summary>
    public async Task EnsureDatasetAsync(bool recreate, CancellationToken cancellationToken)
    {
        if (recreate)
        {
            var deleted = await datasets.ApiV2DatasetyDeleteAsync(ParliamentMember.DatasetId, cancellationToken: cancellationToken);
            Logger.Information("Dataset {DatasetId} deleted before re-creation: {Deleted}", ParliamentMember.DatasetId, deleted);
        }
        else if (await ExistsAsync(cancellationToken))
        {
            return;
        }

        var created = EnsureResponse(await datasets.ApiV2DatasetyCreateAsync(CreateRegistration(), cancellationToken: cancellationToken),
            "register the dataset");
        Logger.Information("Dataset {DatasetId} registered", created.DatasetId);
    }

    /// <summary>All records stored in the dataset now (paged search over api/v2/datasety/{id}/hledat).</summary>
    public async Task<IReadOnlyList<ParliamentMember>> LoadStoredAsync(CancellationToken cancellationToken)
    {
        List<ParliamentMember> stored = [];
        for (var page = 1; ; page++)
        {
            var result = EnsureResponse(await datasets.ApiV2DatasetyDatasetSearchAsync(ParliamentMember.DatasetId, MatchAllQuery, page,
                cancellationToken: cancellationToken), "read the dataset records");
            var items = result.Results ?? [];
            stored.AddRange(items.Select(i => JToken.FromObject(i).ToObject<ParliamentMember>(Serializer)!));
            if (items.Count < SearchPageSize || stored.Count >= result.Total)
                break;
        }

        return stored;
    }

    /// <summary>Upserts every member and deletes stored records whose id is not in the snapshot; returns (written, deleted).</summary>
    public async Task<(int Written, int Deleted)> WriteSnapshotAsync(IReadOnlyList<ParliamentMember> members,
        IReadOnlyCollection<string> storedIds, CancellationToken cancellationToken)
    {
        foreach (var member in members)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureResponse(await datasets.ApiV2DatasetyDatasetItemUpdateAsync(ParliamentMember.DatasetId, member.Id, ToJson(member),
                InsertModeRewrite, cancellationToken: cancellationToken), "write record " + member.Id);
        }

        var snapshotIds = members.Select(m => m.Id).ToHashSet(StringComparer.Ordinal);
        var deleted = 0;
        foreach (var staleId in storedIds.Where(id => !snapshotIds.Contains(id)))
        {
            await DeleteItemAsync(staleId, cancellationToken);
            deleted++;
            Logger.Information("Record {RecordId} deleted from dataset {DatasetId}", staleId, ParliamentMember.DatasetId);
        }

        return (members.Count, deleted);
    }

    /// <summary>JSON object of a record as stored in the dataset.</summary>
    public static JObject ToJson(ParliamentMember member) => JObject.FromObject(member, Serializer);

    /// <summary>
    /// Registration of the dataset: JSON schema generated from <see cref="ParliamentMember"/> and the HTML templates.
    /// Every text field is filled: the server model (HlidacStatu.Datasets.Registration, nullable reference types on)
    /// treats them all as required and answers a registration with a null field with HTTP 500.
    /// </summary>
    public static Registration CreateRegistration()
    {
        var generator = new JSchemaGenerator { DefaultRequired = Required.Default };
        generator.GenerationProviders.Add(new StringEnumGenerationProvider());

        return new Registration(
            name: DatasetName,
            datasetId: ParliamentMember.DatasetId,
            origUrl: DatasetOrigUrl,
            sourcecodeUrl: DatasetSourceCodeUrl,
            createdBy: CreatedByPlaceholder,
            defaultOrderBy: DefaultOrderBy,
            description: DatasetDescription,
            jsonSchema: generator.Generate(typeof(ParliamentMember)).ToString(),
            betaversion: false,
            allowWriteAccess: false,
            hidden: false,
            searchResultTemplate: CreateTemplate(SearchTemplate, "Složení parlamentu"),
            detailTemplate: CreateTemplate(DetailTemplate, "Člen parlamentu"),
            orderList: new[,] { { OrderByImportLabel, OrderByImportField } });
    }

    private static Template CreateTemplate(string body, string title) => new()
    {
        Header = TemplateHeader,
        Body = body,
        Footer = TemplateFooter,
        Title = title,
        Properties = [],
    };

    /// <summary>
    /// The generated client returns null instead of throwing when the API answers with a redirect to the login page,
    /// which is what an invalid or missing API token gets.
    /// </summary>
    private static T EnsureResponse<T>(T? response, string operation) where T : class =>
        response ?? throw new InvalidOperationException(
            $"Hlídač státu API returned no data when trying to {operation}; check the API token (--apikey).");

    private async Task<bool> ExistsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var registration = await datasets.ApiV2DatasetyDetailAsync(ParliamentMember.DatasetId, cancellationToken: cancellationToken);
            if (registration is null)
                await PingAsync(cancellationToken);
            return string.Equals(registration?.DatasetId, ParliamentMember.DatasetId, StringComparison.OrdinalIgnoreCase);
        }
        catch (ApiException e)
        {
            Logger.Information("Dataset {DatasetId} not found ({ErrorCode}), it will be registered", ParliamentMember.DatasetId, e.ErrorCode);
            return false;
        }
    }

    /// <summary>
    /// Distinguishes "dataset does not exist" from "the token is not accepted": api/v2/ping answers an accepted token,
    /// a refused one is redirected to the login page and the client returns null.
    /// </summary>
    private async Task PingAsync(CancellationToken cancellationToken)
    {
        var pong = await core.ApiV2PingAsync(ParliamentMember.DatasetId, cancellationToken: cancellationToken);
        if (string.IsNullOrEmpty(pong))
            throw new InvalidOperationException("Hlídač státu API does not accept the API token (api/v2/ping returned nothing); check --apikey.");
    }

    private async Task DeleteItemAsync(string itemId, CancellationToken cancellationToken)
    {
        var url = $"{options.HlidacApiBaseUrl}/api/v2/datasety/{Uri.EscapeDataString(ParliamentMember.DatasetId)}/zaznamy/{Uri.EscapeDataString(itemId)}";
        using var response = await httpClient.DeleteAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Logger.Warning("Record {RecordId} was already missing in dataset {DatasetId}", itemId, ParliamentMember.DatasetId);
            return;
        }

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"DELETE {url} returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(cancellationToken)}", null,
                response.StatusCode);
    }
}
