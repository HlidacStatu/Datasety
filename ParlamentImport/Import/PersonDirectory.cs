using System.Globalization;
using HlidacStatu.Api.V2;
using Serilog;

namespace HlidacStatu.ParlamentImport.Import;

/// <summary>A function (OsobaEvent) of a Hlídač státu person as returned by api/v2/osoby/{id}.</summary>
internal sealed record PersonFunction(string? Role, string? Organization, DateTime? From, DateTime? To);

/// <summary>A Hlídač státu person with the functions needed to decide whether they hold a chamber mandate.</summary>
internal sealed record PersonDetail(string NameId, string FirstName, string LastName, DateTime? BirthDate, IReadOnlyList<PersonFunction> Functions);

/// <summary>Person lookups used for matching; implemented over the Hlídač státu REST API.</summary>
internal interface IPersonDirectory
{
    /// <summary>Fulltext search of persons by name (api/v2/osoby/hledatFtx).</summary>
    Task<IReadOnlyList<PersonCandidate>> SearchByNameAsync(string fullName, CancellationToken cancellationToken);

    /// <summary>Person detail with functions (api/v2/osoby/{nameId}); null when the person does not exist.</summary>
    Task<PersonDetail?> GetDetailAsync(string nameId, CancellationToken cancellationToken);

    /// <summary>Persons with exactly this name and date of birth (api/v2/osoby/hledat).</summary>
    Task<IReadOnlyList<PersonCandidate>> SearchByNameAndBirthDateAsync(string firstName, string lastName, DateTime birthDate,
        CancellationToken cancellationToken);
}

/// <summary>
/// <see cref="IPersonDirectory"/> over the NuGet client HlidacStatu.Api.V2. Calls are paced by
/// <see cref="ImportOptions.PersonApiDelayMilliseconds"/> and details are cached for the run.
/// </summary>
internal sealed class HlidacPersonDirectory(swaggerClient client, ImportOptions options) : IPersonDirectory
{
    private const int NotFoundStatus = 404;
    private const string BirthDateFormat = "yyyy-MM-dd";
    private static readonly ILogger Logger = Log.ForContext<HlidacPersonDirectory>();
    private readonly Dictionary<string, PersonDetail?> _details = new(StringComparer.Ordinal);

    public async Task<IReadOnlyList<PersonCandidate>> SearchByNameAsync(string fullName, CancellationToken cancellationToken)
    {
        await Task.Delay(options.PersonApiDelayMilliseconds, cancellationToken);
        var found = await client.HledatFtxAsync(fullName, options.PersonSearchStatus, 1, cancellationToken);
        Logger.Debug("Person search {Query} returned {Count} persons", fullName, found?.Count ?? 0);
        return (found ?? []).Select(ToCandidate).ToList();
    }

    public async Task<PersonDetail?> GetDetailAsync(string nameId, CancellationToken cancellationToken)
    {
        if (_details.TryGetValue(nameId, out var cached))
            return cached;

        await Task.Delay(options.PersonApiDelayMilliseconds, cancellationToken);
        PersonDetail? detail;
        try
        {
            var dto = await client.OsobyAsync(nameId, cancellationToken);
            detail = new PersonDetail(dto.NameId, dto.Jmeno, dto.Prijmeni, dto.Narozeni?.DateTime,
                (dto.Udalosti ?? []).Select(e => new PersonFunction(e.Role, e.Organizace, e.DatumOd?.DateTime, e.DatumDo?.DateTime)).ToList());
        }
        catch (ApiException e) when (e.StatusCode == NotFoundStatus)
        {
            detail = null;
        }

        _details[nameId] = detail;
        return detail;
    }

    public async Task<IReadOnlyList<PersonCandidate>> SearchByNameAndBirthDateAsync(string firstName, string lastName, DateTime birthDate,
        CancellationToken cancellationToken)
    {
        await Task.Delay(options.PersonApiDelayMilliseconds, cancellationToken);
        var found = await client.HledatAllAsync(firstName, lastName, birthDate.ToString(BirthDateFormat, CultureInfo.InvariantCulture), false,
            cancellationToken);
        return (found ?? []).Select(ToCandidate).ToList();
    }

    private static PersonCandidate ToCandidate(OsobaDTO dto) => new(dto.NameId, dto.Jmeno, dto.Prijmeni, dto.Narozeni?.DateTime);
}
