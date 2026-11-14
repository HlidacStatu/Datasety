using HlidacStatu.ParlamentImport.Ep;

namespace HlidacStatu.ParlamentImport.Tests;

public sealed class EpApiParserTests
{
    [Fact]
    public void ParseCurrentList_RealResponse_Returns21CzechMeps()
    {
        var json = TestData.Fixture("ep-show-current.json");

        var meps = EpApiParser.ParseCurrentList(json);

        Assert.Equal(21, meps.Count);
        var niedermayer = Assert.Single(meps, m => m.Identifier == "124701");
        Assert.Equal("Luděk", niedermayer.GivenName);
        Assert.Equal("Niedermayer", niedermayer.FamilyName);
        Assert.Equal("PPE", niedermayer.PoliticalGroupCode);
    }

    [Fact]
    public void ParseDetail_RealResponse_ReturnsBirthDateAndCurrentBodies()
    {
        var json = TestData.Fixture("ep-mep-124701.json");

        var detail = EpApiParser.ParseDetail(json, new DateOnly(2026, 11, 14));

        Assert.Equal(new DateTime(1966, 3, 13), detail.BirthDate);
        Assert.Equal("7018", detail.PoliticalGroupBodyId);
        Assert.Equal("6793", detail.NationalPartyBodyId);
    }

    [Fact]
    public void ParseDetail_MembershipEndedBeforeToday_IsIgnored()
    {
        const string json = """
            {"data":[{"identifier":"1","hasMembership":[
              {"membershipClassification":"def/ep-entities/NATIONAL_POLITICAL_GROUP","organization":"org/1","memberDuring":{"startDate":"2019-07-02","endDate":"2024-07-15"}},
              {"membershipClassification":"def/ep-entities/NATIONAL_POLITICAL_GROUP","organization":"org/2","memberDuring":{"startDate":"2024-07-16"}}
            ]}]}
            """;

        var detail = EpApiParser.ParseDetail(json, new DateOnly(2026, 11, 14));

        Assert.Equal("2", detail.NationalPartyBodyId);
        Assert.Null(detail.PoliticalGroupBodyId);
        Assert.Null(detail.BirthDate);
    }

    [Fact]
    public void ParseCorporateBody_RealResponse_ReturnsCzechLabels()
    {
        var json = TestData.Fixture("ep-body-6793.json");

        var body = EpApiParser.ParseCorporateBody(json);

        Assert.Equal("TOP09/STAN", body.Label);
        Assert.Equal("TOP 09", body.CzechAltLabel);
    }

    [Fact]
    public void Build_MepWithBodies_UsesGroupCodeAsLabelAndCzechPartyName()
    {
        var importer = new EpImporter(null!, TestData.Options);
        EpCurrentMep[] current = [new("124701", "Luděk", "Niedermayer", "PPE")];
        var details = new Dictionary<string, EpMepDetail> { ["124701"] = new(new DateTime(1966, 3, 13), "7018", "6793") };
        var bodies = new Dictionary<string, EpCorporateBody>
        {
            ["7018"] = new("PPE", "poslanecký klub Evropské lidové strany", "poslanecký klub Evropské lidové strany"),
            ["6793"] = new("TOP09/STAN", "TOP 09", "Czechia – TOP 09"),
        };

        var record = Assert.Single(importer.Build(current, details, bodies, DateTime.UtcNow));

        Assert.Equal("ep-124701", record.Id);
        Assert.Equal("PPE", record.ClubLabel);
        Assert.Equal("poslanecký klub Evropské lidové strany", record.Club);
        Assert.Equal("TOP 09", record.Party);
        Assert.Equal("https://ep.test/meps/124701", record.SourceUrl);
    }
}
