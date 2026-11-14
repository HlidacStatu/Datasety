using HlidacStatu.ParlamentImport.Import;
using Newtonsoft.Json.Linq;

namespace HlidacStatu.ParlamentImport.Tests;

public sealed class RunArgumentsTests
{
    [Fact]
    public void Parse_AllSwitches_SplitsOverridesFromSwitches()
    {
        var run = RunArguments.Parse(["--apikey", "abc", "--dry-run", "out.json", "--ParlamentImport:RequestDelayMilliseconds=5"]);

        Assert.Equal("abc", run.ApiKey);
        Assert.Equal("out.json", run.DryRunPath);
        Assert.False(run.RecreateDataset);
        Assert.Equal(["--ParlamentImport:RequestDelayMilliseconds=5"], run.ConfigurationOverrides);
    }

    [Fact]
    public void Parse_RecreateDataset_IsSet()
    {
        Assert.True(RunArguments.Parse(["--recreate-dataset", "--apikey", "abc"]).RecreateDataset);
    }

    public static TheoryData<string[]> InvalidArguments => new()
    {
        Array.Empty<string>(),
        new[] { "--apikey" },
        new[] { "--apikey", "--dry-run", "x.json" },
        new[] { "--apikey", "abc", "--dry-run" },
        new[] { "--apikey", "abc", "--dry-run", "x.json", "--recreate-dataset" },
    };

    [Theory]
    [MemberData(nameof(InvalidArguments))]
    public void Parse_InvalidArguments_Throws(string[] args)
    {
        Assert.Throws<ArgumentException>(() => RunArguments.Parse(args));
    }
}

public sealed class ChamberRolesTests
{
    private static readonly ChamberRoles Roles = new(TestData.Options);
    private static readonly DateTime Today = new(2026, 11, 14);

    private static PersonDetail Person(params PersonFunction[] functions) => new("x", "Jan", "Novák", null, functions);

    [Fact]
    public void HoldsMandate_CurrentDeputy_True()
    {
        var person = Person(new PersonFunction("Poslanec", "Poslanecká sněmovna PČR", new DateTime(2025, 10, 4), null));

        Assert.True(Roles.HoldsMandate(person, ParliamentChamber.Psp, Today));
        Assert.False(Roles.HoldsMandate(person, ParliamentChamber.Senat, Today));
    }

    [Fact]
    public void HoldsMandate_EndedMandate_False()
    {
        var person = Person(new PersonFunction("Senátor", "Senát PČR", new DateTime(2014, 10, 18), new DateTime(2020, 10, 17)));

        Assert.False(Roles.HoldsMandate(person, ParliamentChamber.Senat, Today));
    }

    [Theory]
    [InlineData("Europoslanec")]
    [InlineData("Poslankyně EP")]
    public void HoldsMandate_MepRoleVariants_True(string role)
    {
        var person = Person(new PersonFunction(role, "Evropský parlament", new DateTime(2024, 7, 16), null));

        Assert.True(Roles.HoldsMandate(person, ParliamentChamber.Ep, Today));
        Assert.False(Roles.HoldsMandate(person, ParliamentChamber.Psp, Today));
    }

    [Fact]
    public void HoldsMandate_MissingOrganization_False()
    {
        var person = Person(new PersonFunction("Poslanec", null, null, null));

        Assert.False(Roles.HoldsMandate(person, ParliamentChamber.Psp, Today));
    }
}

public sealed class NameIdResolverTests
{
    private sealed class FakeDirectory : IPersonDirectory
    {
        public List<PersonCandidate> SearchResults { get; } = [];
        public Dictionary<string, PersonDetail> Details { get; } = new(StringComparer.Ordinal);
        public List<PersonCandidate> BirthDateResults { get; } = [];
        public int Calls { get; private set; }

        public Task<IReadOnlyList<PersonCandidate>> SearchByNameAsync(string fullName, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult<IReadOnlyList<PersonCandidate>>(SearchResults);
        }

        public Task<PersonDetail?> GetDetailAsync(string nameId, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(Details.GetValueOrDefault(nameId));
        }

        public Task<IReadOnlyList<PersonCandidate>> SearchByNameAndBirthDateAsync(string firstName, string lastName, DateTime birthDate,
            CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult<IReadOnlyList<PersonCandidate>>(BirthDateResults);
        }
    }

    private static readonly DateTime Today = new(2026, 11, 14);

    private static ParliamentMember Member(string id, DateTime? birthDate = null) => new()
    {
        Id = id,
        Chamber = ParliamentChamber.Psp,
        FirstName = "Jan",
        LastName = "Novák",
        FullName = "Jan Novák",
        BirthDate = birthDate,
        SourceUrl = "https://psp.test",
        ImportedUtc = Today,
    };

    private static PersonDetail Deputy(string nameId) =>
        new(nameId, "Jan", "Novák", null, [new PersonFunction("Poslanec", "Poslanecká sněmovna PČR", new DateTime(2025, 10, 4), null)]);

    private static PersonDetail Councillor(string nameId) =>
        new(nameId, "Jan", "Novák", null, [new PersonFunction("Zastupitel", "Obec Lhota", null, null)]);

    [Fact]
    public async Task ResolveAsync_PreviousNameId_IsReusedWithoutApiCall()
    {
        var directory = new FakeDirectory();
        var resolver = new NameIdResolver(directory, new ChamberRoles(TestData.Options));

        var result = await resolver.ResolveAsync([Member("psp-1")], new Dictionary<string, string> { ["psp-1"] = "jan-novak-7" }, Today,
            CancellationToken.None);

        Assert.Equal("jan-novak-7", result[0].NameId);
        Assert.Equal(0, directory.Calls);
    }

    [Fact]
    public async Task ResolveAsync_OnlyNamesakeHoldingMandate_IsMatched()
    {
        var directory = new FakeDirectory();
        directory.SearchResults.AddRange([new("jan-novak-1", "Jan", "Novák", null), new("jan-novak-2", "Jan", "Novák", null),
            new("jana-novakova", "Jana", "Nováková", null)]);
        directory.Details["jan-novak-1"] = Councillor("jan-novak-1");
        directory.Details["jan-novak-2"] = Deputy("jan-novak-2");
        var resolver = new NameIdResolver(directory, new ChamberRoles(TestData.Options));

        var result = await resolver.ResolveAsync([Member("psp-1")], new Dictionary<string, string>(), Today, CancellationToken.None);

        Assert.Equal("jan-novak-2", result[0].NameId);
    }

    [Fact]
    public async Task ResolveAsync_NoMandateHolder_FallsBackToBirthDate()
    {
        var directory = new FakeDirectory();
        directory.SearchResults.Add(new("jan-novak-1", "Jan", "Novák", null));
        directory.Details["jan-novak-1"] = Councillor("jan-novak-1");
        directory.BirthDateResults.Add(new("jan-novak-9", "Jan", "Novák", new DateTime(1970, 1, 1)));
        var resolver = new NameIdResolver(directory, new ChamberRoles(TestData.Options));

        var result = await resolver.ResolveAsync([Member("psp-1", new DateTime(1970, 1, 1))], new Dictionary<string, string>(), Today,
            CancellationToken.None);

        Assert.Equal("jan-novak-9", result[0].NameId);
    }

    [Fact]
    public async Task ResolveAsync_NothingFound_LeavesNameIdNull()
    {
        var resolver = new NameIdResolver(new FakeDirectory(), new ChamberRoles(TestData.Options));

        var result = await resolver.ResolveAsync([Member("psp-1")], new Dictionary<string, string>(), Today, CancellationToken.None);

        Assert.Null(result[0].NameId);
    }
}

public sealed class DatasetApiWriterTests
{
    [Fact]
    public void ToJson_UsesDatasetContractNames()
    {
        var member = new ParliamentMember
        {
            Id = "senat-243",
            Chamber = ParliamentChamber.Senat,
            FirstName = "Miloš",
            LastName = "Vystrčil",
            FullName = "Miloš Vystrčil",
            DistrictNumber = 52,
            SourceUrl = "https://senat.test/243",
            ImportedUtc = new DateTime(2026, 11, 14, 18, 0, 0, DateTimeKind.Utc),
        };

        var json = DatasetApiWriter.ToJson(member);

        Assert.Equal("senat-243", (string?)json["id"]);
        Assert.Equal("Senat", (string?)json["chamber"]);
        Assert.Equal(52, (int?)json["districtNumber"]);
        Assert.Equal(JTokenType.Null, json["nameId"]!.Type);
        Assert.Null(json["Id"]);
    }

    [Fact]
    public void CreateRegistration_SchemaListsContractPropertiesWithoutRequired()
    {
        var registration = DatasetApiWriter.CreateRegistration();

        var schema = JObject.Parse(registration.JsonSchema);
        var properties = ((JObject)schema["properties"]!).Properties().Select(p => p.Name).ToList();
        Assert.Contains("id", properties);
        Assert.Contains("clubLabel", properties);
        Assert.Contains("importedUtc", properties);
        Assert.Null(schema["required"]);
        Assert.Equal(ParliamentMember.DatasetId, registration.DatasetId);
        Assert.False(registration.AllowWriteAccess);
        Assert.NotNull(registration.OrderList);
        Assert.Equal("DbCreated", registration.OrderList[0, 1]);
    }
}
