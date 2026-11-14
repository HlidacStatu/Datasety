using System.Globalization;
using System.Text;

namespace HlidacStatu.ParlamentImport.Import;

/// <summary>A Hlídač státu person that can be matched to a parliament member.</summary>
internal sealed record PersonCandidate(string NameId, string FirstName, string LastName, DateTime? BirthDate);

/// <summary>Result of matching one member: the NameId or null, and how it was decided.</summary>
internal sealed record MatchResult(string? NameId, string Reason);

/// <summary>
/// Matches a parliament member to a Hlídač státu person by name among people who already hold the same
/// function in OsobaEvent (deputies, senators, MEPs). Names are compared without diacritics and case.
/// One name match is accepted; several are narrowed by the date of birth; anything else stays unmatched.
/// A member not found among the role holders can still be matched by name + exact date of birth
/// when exactly one such person exists (<see cref="MatchByBirthDate"/>).
/// </summary>
internal static class PersonMatcher
{
    public const string ReasonRoleName = "role+name";
    public const string ReasonRoleNameBirthDate = "role+name+birthdate";
    public const string ReasonNameBirthDate = "name+birthdate";
    public const string ReasonAmbiguous = "ambiguous";
    public const string ReasonNotFound = "not-found";

    /// <summary>Matches among the holders of the member's function.</summary>
    public static MatchResult MatchByRole(ParliamentMember member, IReadOnlyList<PersonCandidate> roleHolders)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(roleHolders);

        var key = NameKey(member.FirstName, member.LastName);
        var byName = roleHolders.Where(c => NameKey(c.FirstName, c.LastName) == key).ToList();

        if (byName.Count == 1)
            return new MatchResult(byName[0].NameId, ReasonRoleName);
        if (byName.Count == 0)
            return new MatchResult(null, ReasonNotFound);

        var byBirthDate = member.BirthDate is null
            ? []
            : byName.Where(c => c.BirthDate?.Date == member.BirthDate.Value.Date).ToList();
        return byBirthDate.Count == 1
            ? new MatchResult(byBirthDate[0].NameId, ReasonRoleNameBirthDate)
            : new MatchResult(null, ReasonAmbiguous);
    }

    /// <summary>Matches among all persons with the same name and date of birth (the caller loads them).</summary>
    public static MatchResult MatchByBirthDate(ParliamentMember member, IReadOnlyList<PersonCandidate> sameNameAndBirthDate)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(sameNameAndBirthDate);

        if (member.BirthDate is null)
            return new MatchResult(null, ReasonNotFound);

        var distinct = sameNameAndBirthDate.Select(c => c.NameId).Distinct(StringComparer.Ordinal).ToList();
        return distinct.Count switch
        {
            1 => new MatchResult(distinct[0], ReasonNameBirthDate),
            0 => new MatchResult(null, ReasonNotFound),
            _ => new MatchResult(null, ReasonAmbiguous),
        };
    }

    /// <summary>Lower-case name without diacritics and with single spaces, "Jana Zwyrtek Hamplová" -> "jana zwyrtek hamplova".</summary>
    public static string NameKey(string firstName, string lastName)
    {
        var text = (firstName + " " + lastName).Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.IsWhiteSpace(ch) || ch == '-' ? ' ' : char.ToLowerInvariant(ch));
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
