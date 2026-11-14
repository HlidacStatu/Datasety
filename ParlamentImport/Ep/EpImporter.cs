using System.Globalization;

namespace HlidacStatu.ParlamentImport.Ep;

/// <summary>
/// Downloads the current MEPs of one country from the European Parliament Open Data API v2,
/// with their date of birth, EP political group and national party.
/// </summary>
internal sealed class EpImporter(SourceClient client, ImportOptions options)
{
    private const string JsonLd = "application/ld+json";
    private const string FormatQuery = "format=application%2Fld%2Bjson";

    public async Task<IReadOnlyList<ParliamentMember>> ImportAsync(DateTime importedUtc, CancellationToken cancellationToken)
    {
        List<EpCurrentMep> current = [];
        for (var offset = 0; ; offset += options.EpPageSize)
        {
            var listUrl = $"{options.EpApiBaseUrl}meps/show-current?country-of-representation={Uri.EscapeDataString(options.EpCountryCode)}"
                          + $"&{FormatQuery}&offset={offset.ToString(CultureInfo.InvariantCulture)}&limit={options.EpPageSize.ToString(CultureInfo.InvariantCulture)}";
            var page = EpApiParser.ParseCurrentList(await client.GetStringAsync(listUrl, JsonLd, cancellationToken));
            current.AddRange(page);
            if (page.Count < options.EpPageSize)
                break;
        }

        var today = DateOnly.FromDateTime(importedUtc);

        Dictionary<string, EpMepDetail> details = new(StringComparer.Ordinal);
        foreach (var mep in current)
        {
            var url = $"{options.EpApiBaseUrl}meps/{Uri.EscapeDataString(mep.Identifier)}?{FormatQuery}";
            details[mep.Identifier] = EpApiParser.ParseDetail(await client.GetStringAsync(url, JsonLd, cancellationToken), today);
        }

        Dictionary<string, EpCorporateBody> bodies = new(StringComparer.Ordinal);
        var bodyIds = details.Values
            .SelectMany(d => new[] { d.PoliticalGroupBodyId, d.NationalPartyBodyId })
            .OfType<string>()
            .Distinct(StringComparer.Ordinal);
        foreach (var bodyId in bodyIds)
        {
            var url = $"{options.EpApiBaseUrl}corporate-bodies/{Uri.EscapeDataString(bodyId)}?{FormatQuery}&language=cs";
            bodies[bodyId] = EpApiParser.ParseCorporateBody(await client.GetStringAsync(url, JsonLd, cancellationToken));
        }

        return Build(current, details, bodies, importedUtc);
    }

    /// <summary>Combines the list, details and corporate bodies into records.</summary>
    public IReadOnlyList<ParliamentMember> Build(IReadOnlyList<EpCurrentMep> current, IReadOnlyDictionary<string, EpMepDetail> details,
        IReadOnlyDictionary<string, EpCorporateBody> bodies, DateTime importedUtc)
    {
        List<ParliamentMember> records = [];
        foreach (var mep in current)
        {
            var detail = details[mep.Identifier];
            var group = detail.PoliticalGroupBodyId is { } g && bodies.TryGetValue(g, out var gb) ? gb : null;
            var party = detail.NationalPartyBodyId is { } p && bodies.TryGetValue(p, out var pb) ? pb : null;

            records.Add(new ParliamentMember
            {
                Id = "ep-" + mep.Identifier,
                Chamber = ParliamentChamber.Ep,
                FirstName = mep.GivenName,
                LastName = mep.FamilyName,
                FullName = mep.GivenName + " " + mep.FamilyName,
                BirthDate = detail.BirthDate,
                Club = group?.CzechPrefLabel ?? group?.Label ?? mep.PoliticalGroupCode,
                ClubLabel = mep.PoliticalGroupCode ?? group?.Label,
                ClubSourceId = detail.PoliticalGroupBodyId,
                Party = party is null ? null : party.CzechAltLabel ?? party.Label,
                SourceUrl = string.Format(CultureInfo.InvariantCulture, options.EpPersonUrlTemplate, mep.Identifier),
                ImportedUtc = importedUtc,
            });
        }

        return records;
    }
}
