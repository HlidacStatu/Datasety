using HlidacStatu.ParlamentImport.Senat;

namespace HlidacStatu.ParlamentImport.Tests;

public sealed class SenatHtmlParserTests
{
    [Fact]
    public void ParseSenatorList_RealPage_Returns81SenatorsWithDistricts()
    {
        var html = TestData.Fixture("senat-senatori.html");

        var senators = SenatHtmlParser.ParseSenatorList(html);

        Assert.Equal(81, senators.Count);
        var balik = Assert.Single(senators, s => s.SenatorId == "480");
        Assert.Equal("Stanislav", balik.FirstName);
        Assert.Equal("Balík", balik.LastName);
        Assert.Equal(65, balik.DistrictNumber);
        Assert.Equal("Šumperk", balik.DistrictName);
        Assert.Equal("BEZPP", balik.Affiliation);
        Assert.Equal(81, senators.Select(s => s.DistrictNumber).Distinct().Count());
    }

    [Fact]
    public void ParseSenatorList_TwoWordSurname_SplitsOnNoBreakSpace()
    {
        var html = TestData.Fixture("senat-senatori.html");

        var senators = SenatHtmlParser.ParseSenatorList(html);

        var hamplova = Assert.Single(senators, s => s.SenatorId == "368");
        Assert.Equal("Jana", hamplova.FirstName);
        Assert.Equal("Zwyrtek Hamplová", hamplova.LastName);
    }

    [Fact]
    public void ParseLeadership_RealPage_ReturnsChairAndViceChairs()
    {
        var html = TestData.Fixture("senat-vedeni.html");

        var leadership = SenatHtmlParser.ParseLeadership(html);

        Assert.Equal(4, leadership.Count);
        Assert.Contains(leadership, l => l is { SenatorId: "243", Function: "předseda", Order: 1 });
        Assert.Contains(leadership, l => l is { SenatorId: "313", Function: "1. místopředseda", Order: 2 });
        Assert.Contains(leadership, l => l is { SenatorId: "305", Function: "místopředseda", Order: 2 });
        Assert.Contains(leadership, l => l is { SenatorId: "172", Function: "místopředseda", Order: 2 });
    }

    [Fact]
    public void ParseClubList_RealPage_ReturnsSixClubs()
    {
        var html = TestData.Fixture("senat-kluby.html");

        var clubs = SenatHtmlParser.ParseClubList(html);

        Assert.Equal(6, clubs.Count);
        Assert.Contains(clubs, c => c is { ClubId: "588", Name: "Senátorský klub ANO 2011" });
        Assert.Contains(clubs, c => c is { ClubId: "590", Name: "Senátoři nezařazení do klubu" });
    }

    [Fact]
    public void ParseClub_RealPage_CountsFunctionariesAndMembersButNotCommentedOnes()
    {
        var html = TestData.Fixture("senat-klub-588.html");

        var club = SenatHtmlParser.ParseClub(html);

        Assert.Equal(14, club.DeclaredCount);
        Assert.Equal(14, club.SenatorIds.Count);
        Assert.Contains("370", club.SenatorIds);
        Assert.Contains("368", club.SenatorIds);
    }

    [Fact]
    public void Build_SenatorWithClubAndFunction_FillsRecord()
    {
        var importer = new SenatImporter(null!, TestData.Options, new ClubLabeler(TestData.Options));
        SenatorListItem[] senators = [new("243", "Miloš", "Vystrčil", 52, "Jihlava", "ODS"), new("999", "Jan", "Bez Klubu", 1, "Karlovy Vary", null)];
        SenatLeadershipItem[] leadership = [new("243", "předseda", 1)];
        var clubs = new Dictionary<string, SenatClubItem> { ["243"] = new("589", "Senátorský klub ODS a TOP 09") };
        var importedUtc = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc);

        var records = importer.Build(senators, leadership, clubs, importedUtc);

        var vystrcil = records[0];
        Assert.Equal("senat-243", vystrcil.Id);
        Assert.Equal(ParliamentChamber.Senat, vystrcil.Chamber);
        Assert.Equal("ODS a TOP 09", vystrcil.ClubLabel);
        Assert.Equal("589", vystrcil.ClubSourceId);
        Assert.Equal("předseda", vystrcil.Function);
        Assert.Equal(1, vystrcil.FunctionOrder);
        Assert.Equal(52, vystrcil.DistrictNumber);
        Assert.Equal("https://senat.test/senator?id=243", vystrcil.SourceUrl);
        Assert.Null(records[1].Club);
        Assert.Null(records[1].FunctionOrder);
    }
}
