using Serilog;

namespace HlidacStatu.ParlamentImport.Import;

/// <summary>
/// Fills <see cref="ParliamentMember.NameId"/>:
/// <list type="number">
/// <item>a member already stored in the dataset with a NameId keeps it (no API call);</item>
/// <item>otherwise persons with the same name are searched (api/v2/osoby/hledatFtx), only those who currently hold
/// the mandate of the member's chamber are kept (<see cref="ChamberRoles"/>) and <see cref="PersonMatcher.MatchByRole"/> decides;</item>
/// <item>a member not found that way is matched by name + exact date of birth (api/v2/osoby/hledat) when unique.</item>
/// </list>
/// Every decision is logged; unmatched members keep a null NameId.
/// </summary>
internal sealed class NameIdResolver(IPersonDirectory directory, ChamberRoles roles)
{
    public const string ReasonPreviousRun = "previous-run";

    private static readonly ILogger Logger = Log.ForContext<NameIdResolver>();

    /// <summary>Returns the members with NameId filled where a unique match exists.</summary>
    public async Task<IReadOnlyList<ParliamentMember>> ResolveAsync(IReadOnlyList<ParliamentMember> members,
        IReadOnlyDictionary<string, string> previousNameIds, DateTime today, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(members);
        ArgumentNullException.ThrowIfNull(previousNameIds);

        List<ParliamentMember> resolved = [];
        foreach (var member in members)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = previousNameIds.TryGetValue(member.Id, out var previous)
                ? new MatchResult(previous, ReasonPreviousRun)
                : await MatchAsync(member, today, cancellationToken);

            if (result.NameId is null)
                Logger.Warning("Member {MemberId} {FullName} not matched to a person: {Reason}", member.Id, member.FullName, result.Reason);
            else
                Logger.Information("Member {MemberId} {FullName} matched to {NameId} by {Reason}", member.Id, member.FullName,
                    result.NameId, result.Reason);

            resolved.Add(member with { NameId = result.NameId });
        }

        return resolved;
    }

    private async Task<MatchResult> MatchAsync(ParliamentMember member, DateTime today, CancellationToken cancellationToken)
    {
        var key = PersonMatcher.NameKey(member.FirstName, member.LastName);
        var sameName = (await directory.SearchByNameAsync(member.FirstName + " " + member.LastName, cancellationToken))
            .Where(c => PersonMatcher.NameKey(c.FirstName, c.LastName) == key)
            .DistinctBy(c => c.NameId, StringComparer.Ordinal)
            .ToList();

        List<PersonCandidate> roleHolders = [];
        foreach (var candidate in sameName)
        {
            var detail = await directory.GetDetailAsync(candidate.NameId, cancellationToken);
            if (detail is not null && roles.HoldsMandate(detail, member.Chamber, today))
                roleHolders.Add(candidate);
        }

        var result = PersonMatcher.MatchByRole(member, roleHolders);
        if (result.NameId is null && result.Reason == PersonMatcher.ReasonNotFound && member.BirthDate is { } birthDate)
        {
            var sameBirthDate = await directory.SearchByNameAndBirthDateAsync(member.FirstName, member.LastName, birthDate, cancellationToken);
            result = PersonMatcher.MatchByBirthDate(member, sameBirthDate);
        }

        return result;
    }
}
