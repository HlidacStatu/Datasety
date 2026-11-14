using Microsoft.Extensions.Configuration;

namespace HlidacStatu.ParlamentImport;

/// <summary>
/// Settings of the import, bound from the "ParlamentImport" section of appsettings.json.
/// Every value is required; <see cref="Load"/> fails at startup when one is missing or invalid.
/// </summary>
public sealed record ImportOptions
{
    public const string SectionName = "ParlamentImport";

    /// <summary>psp.cz open data archive "poslanci.zip" (agenda Poslanci a osoby).</summary>
    public required string PspOpenDataZipUrl { get; init; }

    /// <summary>Deputy profile on psp.cz, {0} = id_osoba.</summary>
    public required string PspPersonUrlTemplate { get; init; }

    /// <summary>senat.cz list of current senators with district and political affiliation.</summary>
    public required string SenatSenatorListUrl { get; init; }

    /// <summary>senat.cz page listing the Senate chair and vice-chairs.</summary>
    public required string SenatLeadershipUrl { get; init; }

    /// <summary>senat.cz page listing the senatorial clubs.</summary>
    public required string SenatClubListUrl { get; init; }

    /// <summary>senat.cz page of one club, {0} = club id (par_2).</summary>
    public required string SenatClubUrlTemplate { get; init; }

    /// <summary>Senator profile on senat.cz, {0} = senator id (par_3).</summary>
    public required string SenatPersonUrlTemplate { get; init; }

    /// <summary>Base address of the European Parliament Open Data API v2, ending with a slash.</summary>
    public required string EpApiBaseUrl { get; init; }

    /// <summary>ISO 3166-1 alpha-2 country whose MEPs are imported.</summary>
    public required string EpCountryCode { get; init; }

    /// <summary>Page size of the EP list of current MEPs.</summary>
    public required int EpPageSize { get; init; }

    /// <summary>MEP profile on europarl.europa.eu, {0} = MEP identifier.</summary>
    public required string EpPersonUrlTemplate { get; init; }

    /// <summary>User-Agent sent to all sources (EP asks for one without personal data).</summary>
    public required string UserAgent { get; init; }

    public required int HttpTimeoutSeconds { get; init; }

    /// <summary>Pause between two requests to the same source.</summary>
    public required int RequestDelayMilliseconds { get; init; }

    /// <summary>Club names longer than this (after stripping a prefix) are shown by their source abbreviation.</summary>
    public required int ClubLabelMaxLength { get; init; }

    /// <summary>Prefixes stripped from club names to get the label, e.g. "Poslanecký klub ".</summary>
    public required string[] ClubNamePrefixes { get; init; }

    /// <summary>Club names starting with one of these mark members without a club.</summary>
    public required string[] UnaffiliatedClubNamePrefixes { get; init; }

    /// <summary>Label of the group of members without a club.</summary>
    public required string UnaffiliatedClubLabel { get; init; }

    /// <summary>Base address of the Hlídač státu REST API (dataset and person endpoints), without a trailing slash.</summary>
    public required string HlidacApiBaseUrl { get; init; }

    /// <summary>"status" filter of the person fulltext search (api/v2/osoby/hledatFtx): 1 = politicians, sponsors, officials.</summary>
    public required int PersonSearchStatus { get; init; }

    /// <summary>Pause between two calls of the Hlídač státu person API.</summary>
    public required int PersonApiDelayMilliseconds { get; init; }

    /// <summary>
    /// A person holds the function of a chamber member when one of their current events has a role starting with one of
    /// the role prefixes and an organization starting with one of the organization prefixes (lower case, as in OsobaEvent).
    /// </summary>
    public required string[] PspRolePrefixes { get; init; }

    public required string[] PspOrganizationPrefixes { get; init; }

    public required string[] SenatRolePrefixes { get; init; }

    public required string[] SenatOrganizationPrefixes { get; init; }

    public required string[] EpRolePrefixes { get; init; }

    public required string[] EpOrganizationPrefixes { get; init; }

    /// <summary>Safety limits: an import with fewer members fails instead of deleting the missing ones.</summary>
    public required int MinimumPspMembers { get; init; }

    public required int MinimumSenatMembers { get; init; }

    public required int MinimumEpMembers { get; init; }

    /// <summary>
    /// Reads and validates the section; throws <see cref="InvalidOperationException"/> naming every missing or invalid key.
    /// </summary>
    public static ImportOptions Load(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var section = configuration.GetSection(SectionName);
        List<string> errors = [];

        string Text(string key)
        {
            var value = section[key];
            if (string.IsNullOrWhiteSpace(value))
                errors.Add(key);
            return value ?? string.Empty;
        }

        int PositiveInt(string key)
        {
            if (int.TryParse(section[key], System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var value) && value > 0)
                return value;
            errors.Add(key);
            return 0;
        }

        string[] List(string key)
        {
            var values = section.GetSection(key).GetChildren().Select(c => c.Value ?? string.Empty).ToArray();
            if (values.Length == 0 || values.Any(string.IsNullOrWhiteSpace))
                errors.Add(key);
            return values;
        }

        var options = new ImportOptions
        {
            PspOpenDataZipUrl = Text(nameof(PspOpenDataZipUrl)),
            PspPersonUrlTemplate = Text(nameof(PspPersonUrlTemplate)),
            SenatSenatorListUrl = Text(nameof(SenatSenatorListUrl)),
            SenatLeadershipUrl = Text(nameof(SenatLeadershipUrl)),
            SenatClubListUrl = Text(nameof(SenatClubListUrl)),
            SenatClubUrlTemplate = Text(nameof(SenatClubUrlTemplate)),
            SenatPersonUrlTemplate = Text(nameof(SenatPersonUrlTemplate)),
            EpApiBaseUrl = Text(nameof(EpApiBaseUrl)),
            EpCountryCode = Text(nameof(EpCountryCode)),
            EpPageSize = PositiveInt(nameof(EpPageSize)),
            EpPersonUrlTemplate = Text(nameof(EpPersonUrlTemplate)),
            UserAgent = Text(nameof(UserAgent)),
            HttpTimeoutSeconds = PositiveInt(nameof(HttpTimeoutSeconds)),
            RequestDelayMilliseconds = PositiveInt(nameof(RequestDelayMilliseconds)),
            ClubLabelMaxLength = PositiveInt(nameof(ClubLabelMaxLength)),
            ClubNamePrefixes = List(nameof(ClubNamePrefixes)),
            UnaffiliatedClubNamePrefixes = List(nameof(UnaffiliatedClubNamePrefixes)),
            UnaffiliatedClubLabel = Text(nameof(UnaffiliatedClubLabel)),
            HlidacApiBaseUrl = Text(nameof(HlidacApiBaseUrl)),
            PersonSearchStatus = PositiveInt(nameof(PersonSearchStatus)),
            PersonApiDelayMilliseconds = PositiveInt(nameof(PersonApiDelayMilliseconds)),
            PspRolePrefixes = List(nameof(PspRolePrefixes)),
            PspOrganizationPrefixes = List(nameof(PspOrganizationPrefixes)),
            SenatRolePrefixes = List(nameof(SenatRolePrefixes)),
            SenatOrganizationPrefixes = List(nameof(SenatOrganizationPrefixes)),
            EpRolePrefixes = List(nameof(EpRolePrefixes)),
            EpOrganizationPrefixes = List(nameof(EpOrganizationPrefixes)),
            MinimumPspMembers = PositiveInt(nameof(MinimumPspMembers)),
            MinimumSenatMembers = PositiveInt(nameof(MinimumSenatMembers)),
            MinimumEpMembers = PositiveInt(nameof(MinimumEpMembers)),
        };

        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"Missing or invalid configuration {SectionName}: {string.Join(", ", errors.Select(e => SectionName + ":" + e))}");

        if (!options.EpApiBaseUrl.EndsWith('/'))
            throw new InvalidOperationException($"{SectionName}:{nameof(EpApiBaseUrl)} must end with '/'.");
        if (options.HlidacApiBaseUrl.EndsWith('/'))
            throw new InvalidOperationException($"{SectionName}:{nameof(HlidacApiBaseUrl)} must not end with '/'.");

        return options;
    }
}
