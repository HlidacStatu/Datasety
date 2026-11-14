using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// Reads the newest page of the UOHS decision collection with no "Oblast" (area) selected, i.e. decisions of all areas.
    /// </summary>
    public sealed class UohsListingClient : IDisposable
    {
        readonly UohsSettings settings;
        readonly HttpClient httpClient;

        public UohsListingClient(UohsSettings settings, HttpMessageHandler handler)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(handler);

            this.settings = settings;
            httpClient = new HttpClient(handler, disposeHandler: true) { Timeout = settings.HttpTimeout };
        }

        /// <summary>
        /// Creates a client with its own cookie container, so the server session starts with no stored filter.
        /// </summary>
        public static UohsListingClient Create(UohsSettings settings)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
                AllowAutoRedirect = true,
            };
            return new UohsListingClient(settings, handler);
        }

        /// <summary>
        /// Submits the search form with an empty area and returns the first listing page (all areas).
        /// Throws <see cref="InvalidOperationException"/> when the response is not an unfiltered listing with decisions.
        /// </summary>
        public async Task<ListingPage> GetNewestPageAsync(CancellationToken cancellationToken = default)
        {
            using var form = new FormUrlEncodedContent([new KeyValuePair<string, string>(settings.AreaFilterFormField, string.Empty)]);
            using var response = await httpClient.PostAsync(settings.ListingUrl, form, cancellationToken);
            response.EnsureSuccessStatusCode();
            var page = UohsListingParser.Parse(await response.Content.ReadAsStringAsync(cancellationToken), settings.AreaFilterFormField);

            if (!page.IsListing)
                throw new InvalidOperationException($"UOHS search form {settings.ListingUrl} did not return a decision listing.");
            if (page.IsFilteredByArea)
                throw new InvalidOperationException(
                    $"UOHS search form {settings.ListingUrl} ignored the empty area filter, the listing is filtered by area '{page.SelectedAreaFilter}'.");
            if (page.DecisionIds.Count == 0)
                throw new InvalidOperationException(
                    $"UOHS search form {settings.ListingUrl} returned a listing without any decision link; the page markup has probably changed.");

            return page;
        }

        /// <summary>
        /// Returns the highest decision ID on the newest listing page of all areas.
        /// </summary>
        public async Task<int> GetMaxListedDecisionIdAsync(CancellationToken cancellationToken = default) =>
            (await GetNewestPageAsync(cancellationToken)).DecisionIds.Max();

        public void Dispose() => httpClient.Dispose();
    }
}
