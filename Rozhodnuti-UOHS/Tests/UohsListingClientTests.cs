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
        AreaFilterFormField: ListingHtml.AreaField,
        DetailUrlTemplate: "https://uohs.example/cs/verejne-zakazky/sbirky-rozhodnuti/detail-{0}.html",
        LinkBaseUrl: "https://uohs.example",
        HttpTimeout: TimeSpan.FromSeconds(5));

    /// <summary>
    /// Simulates the UOHS search form: an empty area value clears the area filter, anything else keeps "Veřejné zakázky".
    /// </summary>
    sealed class FakeUohsServer : HttpMessageHandler
    {
        public bool IgnoreEmptyAreaFilter { get; init; }
        public Func<string?, string> RenderPage { get; init; } = area => ListingHtml.Listing(
        [
            ("verejne-zakazky", 24508),
            ("pristup-k-dopravni-infrastrukture", 24509),
            ("hospodarska-soutez", 24500),
        ], area);
        public List<string> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add($"{request.Method} {request.RequestUri!.AbsoluteUri} {body}".TrimEnd());

            var area = !IgnoreEmptyAreaFilter && request.Method == HttpMethod.Post && body == $"{ListingHtml.AreaField}=" ? null : "2";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RenderPage(area), Encoding.UTF8, "text/html"),
            };
        }
    }

    [Fact]
    public async Task GetNewestPageAsync_PostsSearchFormWithEmptyArea()
    {
        var server = new FakeUohsServer();
        using var client = new UohsListingClient(Settings, server);

        var page = await client.GetNewestPageAsync();

        Assert.Equal([$"POST {ListingUrl} odbor="], server.Requests);
        Assert.Equal([24508, 24509, 24500], page.DecisionIds);
        Assert.False(page.IsFilteredByArea);
    }

    [Fact]
    public async Task GetMaxListedDecisionIdAsync_ListingOfAllAreas_ReturnsHighestId()
    {
        using var client = new UohsListingClient(Settings, new FakeUohsServer());

        var maxId = await client.GetMaxListedDecisionIdAsync();

        Assert.Equal(24509, maxId);
    }

    [Fact]
    public async Task GetNewestPageAsync_ServerIgnoresEmptyAreaFilter_Throws()
    {
        using var client = new UohsListingClient(Settings, new FakeUohsServer { IgnoreEmptyAreaFilter = true });

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetNewestPageAsync());
    }

    [Fact]
    public async Task GetNewestPageAsync_ResponseIsNotListing_Throws()
    {
        using var client = new UohsListingClient(Settings, new FakeUohsServer { RenderPage = _ => ListingHtml.Detail(1) });

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetNewestPageAsync());
    }

    [Fact]
    public async Task GetNewestPageAsync_ListingWithoutDecisions_Throws()
    {
        using var client = new UohsListingClient(Settings, new FakeUohsServer { RenderPage = area => ListingHtml.Listing([], area) });

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetNewestPageAsync());
    }
}
