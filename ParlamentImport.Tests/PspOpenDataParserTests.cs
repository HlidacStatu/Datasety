using System.IO.Compression;
using HlidacStatu.ParlamentImport.Psp;

namespace HlidacStatu.ParlamentImport.Tests;

public sealed class PspOpenDataParserTests
{
    private const string Organs = """
        173||11|PSP9|Poslanecká sněmovna|Chamber of Deputies|09.10.2021|08.10.2025||0|
        174||11|PSP10|Poslanecká sněmovna|Chamber of Deputies|04.10.2025||174||
        1537|173|1|ANO|Poslanecký klub ANO 2011|Political group ANO 2011|12.10.2021|08.10.2025||0|
        1750|174|1|ANO2011|Poslanecký klub ANO 2011|Political group ANO 2011|08.10.2025||1||
        1752|174|1|STAN|Poslanecký klub Starostové a nezávislí|Political group Mayors|10.10.2025||3||
        1894|174|1|Nezařaz|Nezařazení|Unaffiliated Deputies|01.11.2026||9||
        1104||6|ANO2011|ANO2011||||||
        """;

    private const string Persons = """
        6105||Okamura|Tomio||04.07.1972|M|||
        6487|Ing.|Nacher|Patrik||17.10.1974|M|||
        7000|Mgr.|Nováková|Jana|Ph.D.|01.01.1900|Ž|||
        7001||Bývalý|Poslanec||02.02.1960|M|||
        7002||Přeběhlík|Petr||03.03.1970|M|||
        """;

    private const string Placements = """
        6105|174|0|2025-10-04 00||||
        6487|174|0|2025-10-04 00||||
        7000|174|0|2025-10-04 00||||
        7001|174|0|2025-10-04 00|2026-01-31 00|||
        7002|174|0|2025-10-04 00||||
        6105|1750|0|2025-10-08 00||||
        6487|1750|0|2025-10-08 00||||
        7000|1752|0|2025-10-10 00||||
        7002|1750|0|2025-10-08 00|2026-11-01 00|||
        7002|1894|0|2026-11-01 00||||
        6105|2993|1|2025-11-05 18||||
        6487|2994|1|2025-11-05 20||||
        7000|2995|1|2025-11-05 20||||
        """;

    private const string Functions = """
        2993|174|18|Předseda|1|
        2994|174|16|Místopředseda|2|
        2995|174|17|Ověřovatel|5|
        """;

    private const string FunctionTypes = """
        18|11|Předseda|Chairman|1|1|
        16|11|Místopředseda|Vice-chairman|2|2|
        17|11|Ověřovatel|Verifier|5|3|
        """;

    private const string Deputies = """
        2001|6105|584|1104|174||||||||||1|
        2002|6487|584|1104|174||||||||||1|
        """;

    private static readonly DateTime ImportedUtc = new(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc);

    private static IReadOnlyList<ParliamentMember> Parse() =>
        new PspOpenDataParser(TestData.Options, new ClubLabeler(TestData.Options)).Parse(
            UnlReader.Parse(Organs), UnlReader.Parse(Persons), UnlReader.Parse(Placements), UnlReader.Parse(Functions),
            UnlReader.Parse(FunctionTypes), UnlReader.Parse(Deputies), ImportedUtc);

    [Fact]
    public void Parse_CurrentTerm_SkipsDeputyWhoseMandateEnded()
    {
        var members = Parse();

        Assert.Equal(["psp-6105", "psp-6487", "psp-7000", "psp-7002"], members.Select(m => m.Id).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Parse_Chair_GetsFunctionOrderOneAndClubOfCurrentTerm()
    {
        var okamura = Parse().Single(m => m.Id == "psp-6105");

        Assert.Equal("předseda", okamura.Function);
        Assert.Equal(1, okamura.FunctionOrder);
        Assert.Equal("Poslanecký klub ANO 2011", okamura.Club);
        Assert.Equal("ANO 2011", okamura.ClubLabel);
        Assert.Equal("1750", okamura.ClubSourceId);
        Assert.Equal("ANO2011", okamura.Party);
        Assert.Equal(new DateTime(1972, 7, 4), okamura.BirthDate);
        Assert.Equal("https://psp.test/detail?id=6105", okamura.SourceUrl);
        Assert.Equal(ParliamentChamber.Psp, okamura.Chamber);
    }

    [Fact]
    public void Parse_VerifierFunction_IsNotLeadership()
    {
        var novakova = Parse().Single(m => m.Id == "psp-7000");

        Assert.Null(novakova.Function);
        Assert.Null(novakova.FunctionOrder);
        Assert.Null(novakova.BirthDate);
        Assert.Equal("STAN", novakova.ClubLabel);
        Assert.Equal("Mgr. Jana Nováková, Ph.D.", novakova.FullName);
    }

    [Fact]
    public void Parse_DeputyWhoLeftClub_IsInUnaffiliatedGroup()
    {
        var prebehlik = Parse().Single(m => m.Id == "psp-7002");

        Assert.Equal("Nezařazení", prebehlik.Club);
        Assert.Equal("nezařazení", prebehlik.ClubLabel);
    }

    [Fact]
    public void Parse_ZipArchive_ReadsWindows1250Tables()
    {
        using var zip = new MemoryStream();
        using (var archive = new ZipArchive(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            void Add(string name, string content)
            {
                using var writer = new StreamWriter(archive.CreateEntry(name + ".unl").Open(), UnlReader.SourceEncoding);
                writer.Write(content);
            }

            Add("organy", Organs);
            Add("osoby", Persons);
            Add("zarazeni", Placements);
            Add("funkce", Functions);
            Add("typ_funkce", FunctionTypes);
            Add("poslanec", Deputies);
        }

        zip.Position = 0;

        var members = new PspOpenDataParser(TestData.Options, new ClubLabeler(TestData.Options)).Parse(zip, ImportedUtc);

        Assert.Equal("Přeběhlík", members.Single(m => m.Id == "psp-7002").LastName);
    }

    [Fact]
    public void Parse_NoOpenTerm_Throws()
    {
        var parser = new PspOpenDataParser(TestData.Options, new ClubLabeler(TestData.Options));
        var organs = UnlReader.Parse("173||11|PSP9|Poslanecká sněmovna|Chamber|09.10.2021|08.10.2025||0|");

        Assert.Throws<InvalidDataException>(() => parser.Parse(organs, [], [], [], [], [], ImportedUtc));
    }
}
