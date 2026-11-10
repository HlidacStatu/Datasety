using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class UohsLinksTests
{
    const string BaseUrl = "http://www.uohs.cz";

    [Theory]
    [InlineData("/download/sbirky_rozhodnuti/dokumenty/2026_S0814.pdf", "http://www.uohs.cz/download/sbirky_rozhodnuti/dokumenty/2026_S0814.pdf")]
    [InlineData("/cs/verejne-zakazky/sbirky-rozhodnuti/detail-24508.html", "http://www.uohs.cz/cs/verejne-zakazky/sbirky-rozhodnuti/detail-24508.html")]
    [InlineData("https://uohs.gov.cz/cs/hospodarska-soutez/sbirky-rozhodnuti/detail-15500.html", "https://uohs.gov.cz/cs/hospodarska-soutez/sbirky-rozhodnuti/detail-15500.html")]
    [InlineData("  /a.pdf ", "http://www.uohs.cz/a.pdf")]
    public void ToAbsoluteUrl_Link_ReturnsAbsoluteUrl(string href, string expected)
    {
        var url = UohsLinks.ToAbsoluteUrl(BaseUrl, href);

        Assert.Equal(expected, url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToAbsoluteUrl_EmptyLink_ReturnsNull(string? href)
    {
        var url = UohsLinks.ToAbsoluteUrl(BaseUrl, href);

        Assert.Null(url);
    }
}
