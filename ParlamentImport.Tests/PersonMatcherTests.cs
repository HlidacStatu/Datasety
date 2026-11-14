using HlidacStatu.ParlamentImport.Import;

namespace HlidacStatu.ParlamentImport.Tests;

public sealed class PersonMatcherTests
{
    private static ParliamentMember Member(string first, string last, DateTime? birthDate) => new()
    {
        Id = "psp-1",
        Chamber = ParliamentChamber.Psp,
        FirstName = first,
        LastName = last,
        FullName = first + " " + last,
        BirthDate = birthDate,
        SourceUrl = "https://psp.test",
        ImportedUtc = DateTime.UtcNow,
    };

    [Fact]
    public void MatchByRole_SingleNameMatchIgnoringDiacriticsAndCase_ReturnsNameId()
    {
        PersonCandidate[] holders = [new("berenika-pestova-1", "Berenika", "Peštová", new DateTime(1971, 9, 19)), new("x", "Jan", "Novák", null)];

        var result = PersonMatcher.MatchByRole(Member("BERENIKA", "Pestova", new DateTime(1971, 9, 18)), holders);

        Assert.Equal("berenika-pestova-1", result.NameId);
        Assert.Equal(PersonMatcher.ReasonRoleName, result.Reason);
    }

    [Fact]
    public void MatchByRole_TwoNamesakes_DecidesByBirthDate()
    {
        PersonCandidate[] holders = [new("jan-novak-1", "Jan", "Novák", new DateTime(1960, 1, 1)), new("jan-novak-2", "Jan", "Novák", new DateTime(1980, 5, 5))];

        var result = PersonMatcher.MatchByRole(Member("Jan", "Novák", new DateTime(1980, 5, 5)), holders);

        Assert.Equal("jan-novak-2", result.NameId);
        Assert.Equal(PersonMatcher.ReasonRoleNameBirthDate, result.Reason);
    }

    [Fact]
    public void MatchByRole_TwoNamesakesWithoutBirthDate_IsAmbiguous()
    {
        PersonCandidate[] holders = [new("jan-novak-1", "Jan", "Novák", null), new("jan-novak-2", "Jan", "Novák", null)];

        var result = PersonMatcher.MatchByRole(Member("Jan", "Novák", null), holders);

        Assert.Null(result.NameId);
        Assert.Equal(PersonMatcher.ReasonAmbiguous, result.Reason);
    }

    [Fact]
    public void MatchByRole_NoNameMatch_IsNotFound()
    {
        var result = PersonMatcher.MatchByRole(Member("Libor", "Forman", null), [new("x", "Jan", "Novák", null)]);

        Assert.Null(result.NameId);
        Assert.Equal(PersonMatcher.ReasonNotFound, result.Reason);
    }

    [Fact]
    public void MatchByBirthDate_ExactlyOnePerson_ReturnsNameId()
    {
        var result = PersonMatcher.MatchByBirthDate(Member("Libor", "Forman", new DateTime(1966, 2, 16)),
            [new("libor-forman-2", "Libor", "Forman", new DateTime(1966, 2, 16))]);

        Assert.Equal("libor-forman-2", result.NameId);
        Assert.Equal(PersonMatcher.ReasonNameBirthDate, result.Reason);
    }

    [Fact]
    public void MatchByBirthDate_MemberWithoutBirthDate_IsNotFound()
    {
        var result = PersonMatcher.MatchByBirthDate(Member("Libor", "Forman", null), [new("libor-forman-2", "Libor", "Forman", null)]);

        Assert.Null(result.NameId);
    }

    [Fact]
    public void MatchByBirthDate_TwoPersons_IsAmbiguous()
    {
        var result = PersonMatcher.MatchByBirthDate(Member("Jan", "Novák", new DateTime(1980, 5, 5)),
            [new("jan-novak-1", "Jan", "Novák", null), new("jan-novak-2", "Jan", "Novák", null)]);

        Assert.Null(result.NameId);
        Assert.Equal(PersonMatcher.ReasonAmbiguous, result.Reason);
    }

    [Theory]
    [InlineData("Jana", "Zwyrtek  Hamplová", "jana zwyrtek hamplova")]
    [InlineData("Zuzana", "Majerová-Zahradníková", "zuzana majerova zahradnikova")]
    public void NameKey_NormalizesDiacriticsSpacesAndHyphens(string first, string last, string expected)
    {
        Assert.Equal(expected, PersonMatcher.NameKey(first, last));
    }
}
