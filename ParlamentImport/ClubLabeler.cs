namespace HlidacStatu.ParlamentImport;

/// <summary>
/// Turns an official club name into the short label used in charts and filters:
/// "Poslanecký klub ODS" -> "ODS", "Senátorský klub ODS a TOP 09" -> "ODS a TOP 09",
/// a label still longer than the limit falls back to the source abbreviation ("Starostové a nezávislí" -> "STAN"),
/// clubs of unaffiliated members get the shared unaffiliated label.
/// </summary>
internal sealed class ClubLabeler(ImportOptions options)
{
    /// <summary>True when the club groups members without a club.</summary>
    public bool IsUnaffiliated(string clubName) =>
        options.UnaffiliatedClubNamePrefixes.Any(p => clubName.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    /// <summary>Short label of the club; <paramref name="abbreviation"/> is the source abbreviation when one exists.</summary>
    public string Label(string clubName, string? abbreviation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubName);
        if (IsUnaffiliated(clubName))
            return options.UnaffiliatedClubLabel;

        var label = clubName.Trim();
        var prefix = options.ClubNamePrefixes.FirstOrDefault(p => label.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        if (prefix is not null)
            label = label[prefix.Length..].Trim();

        if (label.Length > options.ClubLabelMaxLength && !string.IsNullOrWhiteSpace(abbreviation))
            return abbreviation.Trim();

        return label;
    }
}
