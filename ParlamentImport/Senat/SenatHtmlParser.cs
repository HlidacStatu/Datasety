using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace HlidacStatu.ParlamentImport.Senat;

/// <summary>Senator row of the senat.cz list of current senators.</summary>
internal sealed record SenatorListItem(string SenatorId, string FirstName, string LastName, int DistrictNumber, string DistrictName,
    string? Affiliation);

/// <summary>Chair or vice-chair of the Senate; <see cref="Order"/> 1 = chair, 2 = vice-chair.</summary>
internal sealed record SenatLeadershipItem(string SenatorId, string Function, int Order);

/// <summary>Senatorial club from the senat.cz list of clubs.</summary>
internal sealed record SenatClubItem(string ClubId, string Name);

/// <summary>Members of one club and the member count the club page declares.</summary>
internal sealed record SenatClubMembers(IReadOnlyList<string> SenatorIds, int DeclaredCount);

/// <summary>
/// Parser of the senat.cz HTML pages (no machine-readable export exists). Senators are identified by the
/// "par_3" parameter of their profile links, clubs by "par_2". Content inside HTML comments is ignored.
/// </summary>
internal static partial class SenatHtmlParser
{
    private const char NoBreakSpace = (char)0xA0;
    private const int ChairOrder = 1;
    private const int ViceChairOrder = 2;
    private const string ChairFunction = "předseda";
    private const string ViceChairFunction = "místopředseda";
    private const string FirstViceChairPrefix = "1.";

    [GeneratedRegex(@"[?&]par_3=(\d+)")]
    private static partial Regex SenatorIdRegex();

    [GeneratedRegex(@"[?&]par_2=(\d+)")]
    private static partial Regex ClubIdRegex();

    [GeneratedRegex(@"^\s*(\d+)\s*-\s*(.+?)\s*$")]
    private static partial Regex DistrictRegex();

    /// <summary>Parses the table of current senators (columns: district, senator, web, affiliation, notification).</summary>
    public static IReadOnlyList<SenatorListItem> ParseSenatorList(string html)
    {
        var document = Load(html);
        var rows = document.DocumentNode.SelectNodes("//table[contains(@class,'PE_zebra')]/tbody/tr")
                   ?? throw new InvalidDataException("senat.cz senator list: table of senators not found.");

        List<SenatorListItem> senators = [];
        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            if (cells is null || cells.Count < 4)
                throw new InvalidDataException("senat.cz senator list: row with fewer than 4 columns.");

            var district = DistrictRegex().Match(Text(cells[0]));
            if (!district.Success)
                throw new InvalidDataException($"senat.cz senator list: unexpected district '{Text(cells[0])}'.");

            var link = cells[1].SelectSingleNode(".//a[@href]")
                       ?? throw new InvalidDataException("senat.cz senator list: senator link missing.");
            var (firstName, lastName) = SplitName(RawText(link));
            var affiliation = Text(cells[3]);

            senators.Add(new SenatorListItem(
                SenatorId(Href(link)),
                firstName, lastName,
                int.Parse(district.Groups[1].Value, CultureInfo.InvariantCulture),
                district.Groups[2].Value,
                affiliation.Length == 0 ? null : affiliation));
        }

        return senators;
    }

    /// <summary>Parses the chair and vice-chairs listed under the "předseda/místopředseda Senátu" headings.</summary>
    public static IReadOnlyList<SenatLeadershipItem> ParseLeadership(string html)
    {
        var document = Load(html);
        var main = document.DocumentNode.SelectSingleNode("//div[contains(@class,'mainCol')]")
                   ?? throw new InvalidDataException("senat.cz leadership: main column not found.");

        List<SenatLeadershipItem> leadership = [];
        string? heading = null;
        foreach (var node in main.Descendants())
        {
            if (node.Name == "h2")
            {
                heading = Text(node).TrimEnd(':').Trim();
            }
            else if (node.Name == "a" && heading is not null && SenatorIdRegex().IsMatch(Href(node)))
            {
                var function = FunctionFromHeading(heading);
                if (function is not null)
                    leadership.Add(new SenatLeadershipItem(SenatorId(Href(node)), function.Value.Name,
                        function.Value.Order));
            }
        }

        if (!leadership.Any(l => l.Order == ChairOrder))
            throw new InvalidDataException("senat.cz leadership: chair of the Senate not found.");
        return leadership;
    }

    /// <summary>Parses the list of senatorial clubs.</summary>
    public static IReadOnlyList<SenatClubItem> ParseClubList(string html)
    {
        var document = Load(html);
        var main = document.DocumentNode.SelectSingleNode("//div[contains(@class,'mainCol')]")
                   ?? throw new InvalidDataException("senat.cz clubs: main column not found.");

        var clubs = main.SelectNodes(".//ul//a[@href]")?
                        .Select(a => (Match: ClubIdRegex().Match(Href(a)), Name: Text(a)))
                        .Where(x => x.Match.Success)
                        .Select(x => new SenatClubItem(x.Match.Groups[1].Value, x.Name))
                        .ToList()
                    ?? [];

        if (clubs.Count == 0)
            throw new InvalidDataException("senat.cz clubs: no club found.");
        return clubs;
    }

    /// <summary>Parses the members of a club page (functionaries and members) and its declared member count.</summary>
    public static SenatClubMembers ParseClub(string html)
    {
        var document = Load(html);
        var panel = document.DocumentNode.SelectSingleNode("//nav[contains(@class,'signpostNav')]")
                    ?? throw new InvalidDataException("senat.cz club: member panel not found.");

        var ids = panel.Descendants("a")
            .Select(a => Href(a))
            .Where(h => SenatorIdRegex().IsMatch(h))
            .Select(SenatorId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var countNode = document.DocumentNode.SelectSingleNode("//section[contains(@class,'authorityModule')][h2]//p")
                        ?? throw new InvalidDataException("senat.cz club: member count not found.");
        var declared = int.Parse(Text(countNode), CultureInfo.InvariantCulture);

        return new SenatClubMembers(ids, declared);
    }

    private static (string Name, int Order)? FunctionFromHeading(string heading)
    {
        var lower = heading.ToLower(CultureInfo.GetCultureInfo("cs-CZ"));
        if (lower.StartsWith(ChairFunction, StringComparison.Ordinal))
            return (ChairFunction, ChairOrder);
        if (lower.StartsWith(FirstViceChairPrefix, StringComparison.Ordinal) && lower.Contains(ViceChairFunction, StringComparison.Ordinal))
            return (FirstViceChairPrefix + " " + ViceChairFunction, ViceChairOrder);
        if (lower.StartsWith("místopředsed", StringComparison.Ordinal))
            return (ViceChairFunction, ViceChairOrder);
        return null;
    }

    private static HtmlDocument Load(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return document;
    }

    private static string SenatorId(string href)
    {
        var match = SenatorIdRegex().Match(href);
        return match.Success ? match.Groups[1].Value : throw new InvalidDataException($"senat.cz: link '{href}' has no senator id.");
    }

    /// <summary>senat.cz separates first name and surname by a no-break space ("Jana&amp;nbsp;Zwyrtek Hamplová").</summary>
    private static (string FirstName, string LastName) SplitName(string rawName)
    {
        var index = rawName.IndexOf(NoBreakSpace, StringComparison.Ordinal);
        if (index < 0)
            index = rawName.IndexOf(' ', StringComparison.Ordinal);
        if (index <= 0)
            throw new InvalidDataException($"senat.cz: cannot split name '{rawName}'.");
        return (Normalize(rawName[..index]), Normalize(rawName[(index + 1)..]));
    }

    private static string Href(HtmlNode node) => WebUtility.HtmlDecode(node.GetAttributeValue("href", string.Empty));

    private static string RawText(HtmlNode node) => WebUtility.HtmlDecode(node.InnerText).Trim();

    private static string Text(HtmlNode node) => Normalize(WebUtility.HtmlDecode(node.InnerText));

    private static string Normalize(string text) => string.Join(' ',
        text.Replace(NoBreakSpace, ' ').Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
