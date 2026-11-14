using System.Globalization;
using System.Text.Json;

namespace HlidacStatu.ParlamentImport.Ep;

/// <summary>MEP from /meps/show-current: identifier, names and the code of the EP political group.</summary>
internal sealed record EpCurrentMep(string Identifier, string GivenName, string FamilyName, string? PoliticalGroupCode);

/// <summary>Detail of an MEP from /meps/{id}: date of birth and the current EP group and national party bodies.</summary>
internal sealed record EpMepDetail(DateTime? BirthDate, string? PoliticalGroupBodyId, string? NationalPartyBodyId);

/// <summary>Corporate body from /corporate-bodies/{id}: short label and Czech names.</summary>
internal sealed record EpCorporateBody(string Label, string? CzechAltLabel, string? CzechPrefLabel);

/// <summary>
/// Parser of the European Parliament Open Data API v2 JSON-LD responses (https://data.europarl.europa.eu/api/v2).
/// </summary>
internal static class EpApiParser
{
    private const string EuPoliticalGroupClassification = "def/ep-entities/EU_POLITICAL_GROUP";
    private const string NationalPartyClassification = "def/ep-entities/NATIONAL_POLITICAL_GROUP";
    private const string OrganizationPrefix = "org/";
    private const string CzechLanguage = "cs";

    /// <summary>Parses the list of current MEPs.</summary>
    public static IReadOnlyList<EpCurrentMep> ParseCurrentList(string json)
    {
        using var document = JsonDocument.Parse(json);
        return Data(document.RootElement)
            .Select(p => new EpCurrentMep(
                RequiredString(p, "identifier"),
                RequiredString(p, "givenName"),
                RequiredString(p, "familyName"),
                OptionalString(p, "api:political-group")))
            .ToList();
    }

    /// <summary>Parses an MEP detail; memberships without an end date (or ending after <paramref name="today"/>) are current.</summary>
    public static EpMepDetail ParseDetail(string json, DateOnly today)
    {
        using var document = JsonDocument.Parse(json);
        var person = Data(document.RootElement).FirstOrDefault();
        if (person.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("EP API: MEP detail has no data.");

        DateTime? birthDate = OptionalString(person, "bday") is { } bday
            ? DateTime.ParseExact(bday, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;

        List<(string Classification, string Organization, DateOnly Start)> current = [];
        if (person.TryGetProperty("hasMembership", out var memberships) && memberships.ValueKind == JsonValueKind.Array)
        {
            foreach (var membership in memberships.EnumerateArray())
            {
                var classification = OptionalString(membership, "membershipClassification");
                var organization = OptionalString(membership, "organization");
                if (classification is null || organization is null || !membership.TryGetProperty("memberDuring", out var during))
                    continue;

                var start = OptionalString(during, "startDate") is { } s ? DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture) : DateOnly.MinValue;
                var end = OptionalString(during, "endDate") is { } e ? DateOnly.ParseExact(e, "yyyy-MM-dd", CultureInfo.InvariantCulture) : (DateOnly?)null;
                if (end is null || end >= today)
                    current.Add((classification, organization, start));
            }
        }

        string? CurrentBody(string classification) => current
            .Where(c => c.Classification == classification)
            .OrderByDescending(c => c.Start)
            .Select(c => c.Organization.StartsWith(OrganizationPrefix, StringComparison.Ordinal)
                ? c.Organization[OrganizationPrefix.Length..]
                : c.Organization)
            .FirstOrDefault();

        return new EpMepDetail(birthDate, CurrentBody(EuPoliticalGroupClassification), CurrentBody(NationalPartyClassification));
    }

    /// <summary>Parses a corporate body (EP political group or national party).</summary>
    public static EpCorporateBody ParseCorporateBody(string json)
    {
        using var document = JsonDocument.Parse(json);
        var body = Data(document.RootElement).FirstOrDefault();
        if (body.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("EP API: corporate body has no data.");

        return new EpCorporateBody(RequiredString(body, "label"), Localized(body, "altLabel"), Localized(body, "prefLabel"));
    }

    private static IEnumerable<JsonElement> Data(JsonElement root) =>
        root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array
            ? data.EnumerateArray().Select(e => e.Clone()).ToList()
            : throw new InvalidDataException("EP API: response has no data array.");

    private static string? Localized(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Object
                                                       && value.TryGetProperty(CzechLanguage, out var text)
                                                       && text.ValueKind == JsonValueKind.String
            ? text.GetString()
            : null;

    private static string RequiredString(JsonElement element, string property) =>
        OptionalString(element, property) ?? throw new InvalidDataException($"EP API: property '{property}' is missing.");

    private static string? OptionalString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;
}
