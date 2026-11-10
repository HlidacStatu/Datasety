using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class UohsListingClientTests
{
    const string ListingUrl = "https://uohs.example/cs/verejne-zakazky/sbirky-rozhodnuti.html";

    static readonly UohsSettings Settings = new(
        ListingUrl: ListingUrl,
        ListingPageUrlTemplate: "https://uohs.example/cs/verejne-zakazky/sbirky-rozhodnuti/{0}.html",
        AreaFilterFormField: ListingHtml.AreaField,
        DetailUrlTemplate: "https://uohs.example/cs/verejne-zakazky/sbirky-rozhodnuti/detail-{0}.html",
        LinkBaseUrl: "https://uohs.example",
        HttpTimeout: TimeSpan.FromSeconds(5),
        ListingRequestDelay: TimeSpan.Zero,
        StopAfterKnownListingPages: 1,
        MaxListingPages: 10,
        RecentIdWindowSize: 0);

    static readonly Devmasters.Log.Logger SilentLogger =
        Devmasters.Log.Logger.CreateLogger("Rozhodnuti-UOHS.Tests", new Serilog.LoggerConfiguration(), setAsGlobalLogger: false);

    /// <summary>
    /// Simulates the UOHS server: the search form stores the area filter in a session, listing pages render it.
    /// </summary>
    sealed class FakeUohsServer : HttpMessageHandler
    {
        public string? SessionArea { get; set; } = "2";
        public bool IgnoreEmptyAreaFilter { get; set; }
        public Func<int, string?, string> RenderPage { get; set; } = (pageNumber, area) =>
            ListingHtml.Listing([("verejne-zakazky", 1000 - pageNumber)], area);
        public List<string> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add($"{request.Method} {request.RequestUri!.AbsolutePath} {body}".TrimEnd());

            if (request.Method == HttpMethod.Post && request.RequestUri.AbsoluteUri == ListingUrl)
            {
                if (!IgnoreEmptyAreaFilter && body == $"{ListingHtml.AreaField}=")
                    SessionArea = null;
                return Html(RenderPage(1, SessionArea));
            }

            var segment = request.RequestUri.Segments[^1].Replace(".html", "");
            return Html(RenderPage(int.Parse(segment), SessionArea));
        }

        static HttpResponseMessage Html(string html) =>
            new(HttpStatusCode.OK) { Content = new StringContent(html, Encoding.UTF8, "text/html") };
    }

    [Fact]
    public async Task GetPageAsync_FirstPage_PostsSearchFormWithEmptyArea()
    {
        var server = new FakeUohsServer();
        using var client = new UohsListingClient(Settings, server, SilentLogger);

        var page = await client.GetPageAsync(1);

        Assert.Equal(["POST /cs/verejne-zakazky/sbirky-rozhodnuti.html odbor="], server.Requests);
        Assert.Equal([999], page.DecisionIds);
        Assert.False(page.IsFilteredByArea);
    }

    [Fact]
    public async Task GetPageAsync_NextPage_ReadsPageUrl()
    {
        var server = new FakeUohsServer();
        using var client = new UohsListingClient(Settings, server, SilentLogger);
        await client.GetPageAsync(1);

        var page = await client.GetPageAsync(2);

        Assert.Equal("GET /cs/verejne-zakazky/sbirky-rozhodnuti/2.html", server.Requests[^1]);
        Assert.Equal([998], page.DecisionIds);
    }

    [Fact]
    public async Task GetPageAsync_SessionFilterLost_AppliesFilterAgainAndRereadsPage()
    {
        var server = new FakeUohsServer();
        using var client = new UohsListingClient(Settings, server, SilentLogger);
        await client.GetPageAsync(1);
        server.SessionArea = "2";

        var page = await client.GetPageAsync(3);

        Assert.Equal(
        [
            "POST /cs/verejne-zakazky/sbirky-rozhodnuti.html odbor=",
            "GET /cs/verejne-zakazky/sbirky-rozhodnuti/3.html",
            "POST /cs/verejne-zakazky/sbirky-rozhodnuti.html odbor=",
            "GET /cs/verejne-zakazky/sbirky-rozhodnuti/3.html",
        ], server.Requests);
        Assert.False(page.IsFilteredByArea);
        Assert.Equal([997], page.DecisionIds);
    }

    [Fact]
    public async Task GetPageAsync_ServerIgnoresEmptyAreaFilter_Throws()
    {
        var server = new FakeUohsServer { IgnoreEmptyAreaFilter = true };
        using var client = new UohsListingClient(Settings, server, SilentLogger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetPageAsync(1));
    }

    [Fact]
    public async Task GetPageAsync_SearchFormDoesNotReturnListing_Throws()
    {
        var server = new FakeUohsServer { RenderPage = (_, _) => ListingHtml.Detail(1) };
        using var client = new UohsListingClient(Settings, server, SilentLogger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetPageAsync(1));
    }

    [Fact]
    public async Task GetPageAsync_SearchFormListingWithoutDecisions_Throws()
    {
        var server = new FakeUohsServer { RenderPage = (_, area) => ListingHtml.Listing([], area) };
        using var client = new UohsListingClient(Settings, server, SilentLogger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetPageAsync(1));
    }

    [Fact]
    public async Task GetPageAsync_PageNumberBelowOne_Throws()
    {
        using var client = new UohsListingClient(Settings, new FakeUohsServer(), SilentLogger);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetPageAsync(0));
    }
}
