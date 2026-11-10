using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class UohsListingParserTests
{
    [Fact]
    public void Parse_ListingOfAllAreas_ReturnsDistinctIdsInPageOrder()
    {
        var html = ListingHtml.Listing(
        [
            ("pristup-k-dopravni-infrastrukture", 24504),
            ("hospodarska-soutez", 24505),
            ("verejne-zakazky", 24508),
            ("verejne-zakazky", 24502),
        ]);

        var page = UohsListingParser.Parse(html, ListingHtml.AreaField);

        Assert.True(page.IsListing);
        Assert.Equal([24504, 24505, 24508, 24502], page.DecisionIds);
    }

    [Fact]
    public void Parse_LinkOutsideListingTable_IsIgnored()
    {
        var html = ListingHtml.Listing(24508);

        var page = UohsListingParser.Parse(html, ListingHtml.AreaField);

        Assert.DoesNotContain(99999, page.DecisionIds);
    }

    [Fact]
    public void Parse_NoAreaSelected_IsNotFilteredByArea()
    {
        var html = ListingHtml.Listing(24508);

        var page = UohsListingParser.Parse(html, ListingHtml.AreaField);

        Assert.Null(page.SelectedAreaFilter);
        Assert.False(page.IsFilteredByArea);
    }

    [Fact]
    public void Parse_AreaSelected_ReportsSelectedArea()
    {
        var html = ListingHtml.Listing([("verejne-zakazky", 24508)], selectedArea: "2");

        var page = UohsListingParser.Parse(html, ListingHtml.AreaField);

        Assert.Equal("2", page.SelectedAreaFilter);
        Assert.True(page.IsFilteredByArea);
    }

    [Fact]
    public void Parse_EmptyAreaOptionSelected_IsNotFilteredByArea()
    {
        var html = ListingHtml.Listing([("verejne-zakazky", 24508)], selectedArea: "");

        var page = UohsListingParser.Parse(html, ListingHtml.AreaField);

        Assert.False(page.IsFilteredByArea);
    }

    [Fact]
    public void Parse_DecisionDetailInsteadOfListing_IsNotListing()
    {
        var html = ListingHtml.Detail(2244);

        var page = UohsListingParser.Parse(html, ListingHtml.AreaField);

        Assert.False(page.IsListing);
        Assert.Empty(page.DecisionIds);
    }

    [Fact]
    public void Parse_ListingWithoutDecisions_ReturnsNoIds()
    {
        var html = ListingHtml.Listing();

        var page = UohsListingParser.Parse(html, ListingHtml.AreaField);

        Assert.True(page.IsListing);
        Assert.Empty(page.DecisionIds);
    }

    [Theory]
    [InlineData("/cs/verejne-zakazky/sbirky-rozhodnuti/detail-24508.html", 24508)]
    [InlineData("https://uohs.gov.cz/cs/hospodarska-soutez/sbirky-rozhodnuti/detail-15500.html", 15500)]
    [InlineData("/cs/verejne-zakazky/sbirky-rozhodnuti/2.html", null)]
    [InlineData("/download/sbirky_rozhodnuti/dokumenty/2026_S0814.pdf", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void ParseDecisionId_VariousUrls_ReturnsIdOrNull(string? url, int? expected)
    {
        var id = UohsListingParser.ParseDecisionId(url);

        Assert.Equal(expected, id);
    }
}
