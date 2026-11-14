using System.Globalization;
using Serilog;

namespace HlidacStatu.ParlamentImport.Senat;

/// <summary>
/// Downloads the senat.cz pages (senator list, leadership, clubs) and builds the current senators.
/// </summary>
internal sealed class SenatImporter(SourceClient client, ImportOptions options, ClubLabeler clubLabeler)
{
    private static readonly ILogger Logger = Log.ForContext<SenatImporter>();

    public async Task<IReadOnlyList<ParliamentMember>> ImportAsync(DateTime importedUtc, CancellationToken cancellationToken)
    {
        var senators = SenatHtmlParser.ParseSenatorList(await client.GetStringAsync(options.SenatSenatorListUrl, null, cancellationToken));
        var leadership = SenatHtmlParser.ParseLeadership(await client.GetStringAsync(options.SenatLeadershipUrl, null, cancellationToken));
        var clubs = SenatHtmlParser.ParseClubList(await client.GetStringAsync(options.SenatClubListUrl, null, cancellationToken));

        Dictionary<string, SenatClubItem> clubBySenator = new(StringComparer.Ordinal);
        foreach (var club in clubs)
        {
            var url = string.Format(CultureInfo.InvariantCulture, options.SenatClubUrlTemplate, club.ClubId);
            var members = SenatHtmlParser.ParseClub(await client.GetStringAsync(url, null, cancellationToken));
            if (members.SenatorIds.Count != members.DeclaredCount)
                throw new InvalidDataException(
                    $"senat.cz club {club.Name}: found {members.SenatorIds.Count} members, the page declares {members.DeclaredCount}.");
            foreach (var id in members.SenatorIds)
                clubBySenator[id] = club;
        }

        return Build(senators, leadership, clubBySenator, importedUtc);
    }

    /// <summary>Combines the parsed pages into records; senators without a club are logged.</summary>
    public IReadOnlyList<ParliamentMember> Build(IReadOnlyList<SenatorListItem> senators, IReadOnlyList<SenatLeadershipItem> leadership,
        IReadOnlyDictionary<string, SenatClubItem> clubBySenator, DateTime importedUtc)
    {
        var functionBySenator = leadership
            .GroupBy(l => l.SenatorId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.Order).First(), StringComparer.Ordinal);

        List<ParliamentMember> records = [];
        foreach (var senator in senators)
        {
            clubBySenator.TryGetValue(senator.SenatorId, out var club);
            if (club is null)
                Logger.Warning("Senator {SenatorId} {FirstName} {LastName} is not a member of any club", senator.SenatorId,
                    senator.FirstName, senator.LastName);
            functionBySenator.TryGetValue(senator.SenatorId, out var function);

            records.Add(new ParliamentMember
            {
                Id = "senat-" + senator.SenatorId,
                Chamber = ParliamentChamber.Senat,
                FirstName = senator.FirstName,
                LastName = senator.LastName,
                FullName = senator.FirstName + " " + senator.LastName,
                Club = club?.Name,
                ClubLabel = club is null ? null : clubLabeler.Label(club.Name, null),
                ClubSourceId = club?.ClubId,
                Party = senator.Affiliation,
                DistrictNumber = senator.DistrictNumber,
                DistrictName = senator.DistrictName,
                Function = function?.Function,
                FunctionOrder = function?.Order,
                SourceUrl = string.Format(CultureInfo.InvariantCulture, options.SenatPersonUrlTemplate, senator.SenatorId),
                ImportedUtc = importedUtc,
            });
        }

        return records;
    }
}
