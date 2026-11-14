using System.Globalization;

namespace HlidacStatu.ParlamentImport.Import;

/// <summary>
/// Decides from the functions of a person (api/v2/osoby/{id} "udalosti") whether they currently hold a mandate
/// in a chamber: a function valid today whose role and organization start with the configured prefixes
/// (e.g. role "Poslanec" at "Poslanecká sněmovna PČR", role "Europoslanec" or "Poslanec EP" at "Evropský parlament").
/// </summary>
internal sealed class ChamberRoles(ImportOptions options)
{
    private static readonly CultureInfo Czech = CultureInfo.GetCultureInfo("cs-CZ");

    /// <summary>True when the person holds the chamber mandate on <paramref name="today"/>.</summary>
    public bool HoldsMandate(PersonDetail person, ParliamentChamber chamber, DateTime today)
    {
        ArgumentNullException.ThrowIfNull(person);
        var (rolePrefixes, organizationPrefixes) = chamber switch
        {
            ParliamentChamber.Psp => (options.PspRolePrefixes, options.PspOrganizationPrefixes),
            ParliamentChamber.Senat => (options.SenatRolePrefixes, options.SenatOrganizationPrefixes),
            ParliamentChamber.Ep => (options.EpRolePrefixes, options.EpOrganizationPrefixes),
            _ => throw new ArgumentOutOfRangeException(nameof(chamber), chamber, null),
        };

        return person.Functions.Any(f =>
            (f.From is null || f.From.Value.Date <= today.Date)
            && (f.To is null || f.To.Value.Date >= today.Date)
            && StartsWithAny(f.Role, rolePrefixes)
            && StartsWithAny(f.Organization, organizationPrefixes));
    }

    private static bool StartsWithAny(string? text, IEnumerable<string> prefixes) =>
        !string.IsNullOrWhiteSpace(text)
        && prefixes.Any(p => text.Trim().ToLower(Czech).StartsWith(p.ToLower(Czech), StringComparison.Ordinal));
}
