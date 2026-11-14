using System.Diagnostics;
using HlidacStatu.Api.V2;
using HlidacStatu.Api.V2.Dataset.Api;
using HlidacStatu.ParlamentImport;
using HlidacStatu.ParlamentImport.Ep;
using HlidacStatu.ParlamentImport.Import;
using HlidacStatu.ParlamentImport.Psp;
using HlidacStatu.ParlamentImport.Senat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

const int ExitSuccess = 0;
const int ExitFailure = 1;
const int ExitInvalidArguments = 2;
const string HlidacApiClientName = "hlidac-api";
const string AuthorizationHeader = "Authorization";

RunArguments run;
try
{
    run = RunArguments.Parse(args);
}
catch (ArgumentException e)
{
    await Console.Error.WriteLineAsync(e.Message);
    await Console.Error.WriteLineAsync(RunArguments.Usage);
    return ExitInvalidArguments;
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("logger.serilog.json", optional: false)
    .AddCommandLine(run.ConfigurationOverrides)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("application_name", "ParlamentImport")
    .WriteTo.Console()
    .CreateLogger();
var logger = Log.ForContext("SourceContext", "HlidacStatu.ParlamentImport");

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};
var cancellationToken = cancellation.Token;
var stopwatch = Stopwatch.StartNew();

try
{
    var options = ImportOptions.Load(configuration);
    logger.Information("ParlamentImport started, dry run: {DryRun}, recreate dataset: {Recreate}", run.DryRunPath is not null, run.RecreateDataset);

    var services = new ServiceCollection();
    services.AddHttpClient(SourceClient.HttpClientName, client =>
    {
        client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
    });
    services.AddHttpClient(HlidacApiClientName, client =>
    {
        client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        client.DefaultRequestHeaders.TryAddWithoutValidation(AuthorizationHeader, run.ApiKey);
    });
    await using var provider = services.BuildServiceProvider();
    var httpClients = provider.GetRequiredService<IHttpClientFactory>();
    var sourceClient = new SourceClient(httpClients.CreateClient(SourceClient.HttpClientName), options);
    var hlidacApiHttp = httpClients.CreateClient(HlidacApiClientName);

    var datasetConfiguration = new HlidacStatu.Api.V2.Dataset.Client.Configuration
    {
        BasePath = options.HlidacApiBaseUrl,
        UserAgent = options.UserAgent,
        Timeout = (int)TimeSpan.FromSeconds(options.HttpTimeoutSeconds).TotalMilliseconds,
    };
    datasetConfiguration.DefaultHeaders.Add(AuthorizationHeader, run.ApiKey);
    var writer = new DatasetApiWriter(new DatasetyApi(datasetConfiguration), new CoreApi(datasetConfiguration), hlidacApiHttp, options);
    var resolver = new NameIdResolver(new HlidacPersonDirectory(new swaggerClient(options.HlidacApiBaseUrl, hlidacApiHttp), options),
        new ChamberRoles(options));

    var importedUtc = DateTime.UtcNow;
    var clubLabeler = new ClubLabeler(options);

    var pspZip = await sourceClient.GetBytesAsync(options.PspOpenDataZipUrl, null, cancellationToken);
    IReadOnlyList<ParliamentMember> psp;
    using (var zipStream = new MemoryStream(pspZip))
        psp = new PspOpenDataParser(options, clubLabeler).Parse(zipStream, importedUtc);
    var senat = await new SenatImporter(sourceClient, options, clubLabeler).ImportAsync(importedUtc, cancellationToken);
    var ep = await new EpImporter(sourceClient, options).ImportAsync(importedUtc, cancellationToken);

    logger.Information("Sources parsed: {PspCount} deputies, {SenatCount} senators, {EpCount} MEPs", psp.Count, senat.Count, ep.Count);
    EnsureMinimum(ParliamentChamber.Psp, psp.Count, options.MinimumPspMembers);
    EnsureMinimum(ParliamentChamber.Senat, senat.Count, options.MinimumSenatMembers);
    EnsureMinimum(ParliamentChamber.Ep, ep.Count, options.MinimumEpMembers);

    if (run.DryRunPath is null)
        await writer.EnsureDatasetAsync(run.RecreateDataset, cancellationToken);
    var stored = run.RecreateDataset ? [] : await LoadStoredOrEmptyAsync(writer, run.DryRunPath is not null, cancellationToken);
    var previousNameIds = stored
        .Where(s => !string.IsNullOrWhiteSpace(s.NameId))
        .ToDictionary(s => s.Id, s => s.NameId!, StringComparer.Ordinal);
    logger.Information("Dataset holds {StoredCount} records, {PreviousCount} with a NameId to reuse", stored.Count, previousNameIds.Count);

    var members = await resolver.ResolveAsync([.. psp, .. senat, .. ep], previousNameIds, DateTime.Today, cancellationToken);
    var unmatched = members.Count(m => m.NameId is null);
    logger.Information("Matched {MatchedCount} of {MemberCount} members to persons, {UnmatchedCount} unmatched",
        members.Count - unmatched, members.Count, unmatched);

    if (run.DryRunPath is not null)
    {
        await File.WriteAllTextAsync(run.DryRunPath,
            JsonConvert.SerializeObject(members.Select(DatasetApiWriter.ToJson), Formatting.Indented), cancellationToken);
        logger.Information("Dry run: {MemberCount} members written to {Path}, dataset not touched", members.Count, run.DryRunPath);
    }
    else
    {
        var (written, deleted) = await writer.WriteSnapshotAsync(members, stored.Select(s => s.Id).ToList(), cancellationToken);
        logger.Information("Dataset {DatasetId}: {Written} records written, {Deleted} deleted", ParliamentMember.DatasetId, written, deleted);
    }

    logger.Information("ParlamentImport finished in {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
    return ExitSuccess;
}
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    logger.Warning("ParlamentImport cancelled after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
    return ExitFailure;
}
catch (Exception e)
{
    logger.Error(e, "ParlamentImport failed after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
    return ExitFailure;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static void EnsureMinimum(ParliamentChamber chamber, int count, int minimum)
{
    if (count < minimum)
        throw new InvalidDataException($"{chamber}: only {count} members parsed, at least {minimum} expected; import stopped.");
}

static async Task<IReadOnlyList<ParliamentMember>> LoadStoredOrEmptyAsync(DatasetApiWriter writer, bool dryRun, CancellationToken cancellationToken)
{
    if (!dryRun)
        return await writer.LoadStoredAsync(cancellationToken);

    try
    {
        return await writer.LoadStoredAsync(cancellationToken);
    }
    catch (HlidacStatu.Api.V2.Dataset.Client.ApiException e)
    {
        Log.Warning("Dry run: dataset {DatasetId} cannot be read ({ErrorCode}), all members are matched from scratch",
            ParliamentMember.DatasetId, e.ErrorCode);
        return [];
    }
}
