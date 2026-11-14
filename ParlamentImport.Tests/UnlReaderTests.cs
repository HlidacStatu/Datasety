using HlidacStatu.ParlamentImport.Psp;

namespace HlidacStatu.ParlamentImport.Tests;

public sealed class UnlReaderTests
{
    [Fact]
    public void Parse_TrailingSeparatorAndEmptyColumns_ReturnsNulls()
    {
        var rows = UnlReader.Parse("1|15|0|1992-06-06 00|||\r\n2|x||");

        Assert.Equal(2, rows.Count);
        Assert.Equal<IEnumerable<string?>>(["1", "15", "0", "1992-06-06 00", null, null], rows[0]);
        Assert.Equal<IEnumerable<string?>>(["2", "x", null], rows[1]);
    }

    [Fact]
    public void Parse_EscapedSeparator_KeepsItInValue()
    {
        var rows = UnlReader.Parse(@"1|a\|b|");

        Assert.Equal<IEnumerable<string?>>(["1", "a|b"], Assert.Single(rows));
    }

    [Fact]
    public void ParseDate_UnknownBirthDate1900_ReturnsNull()
    {
        Assert.Null(UnlReader.ParseDate("01.01.1900"));
        Assert.Equal(new DateTime(1972, 7, 4), UnlReader.ParseDate("04.07.1972"));
        Assert.Null(UnlReader.ParseDate(null));
    }

    [Fact]
    public void ParseDateTime_HourOnlyFormat_IsParsed()
    {
        Assert.Equal(new DateTime(2025, 11, 5, 18, 0, 0), UnlReader.ParseDateTime("2025-11-05 18"));
    }

    [Fact]
    public void ParseDate_InvalidValue_Throws()
    {
        Assert.Throws<FormatException>(() => UnlReader.ParseDate("2025-11-05"));
    }
}
