using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// Reads the UOHS decision collection listing with no "Oblast" (area) selected, i.e. decisions of all areas.
    /// The filter lives in the server session (PHPSESSID cookie): it is set by POSTing the search form
    /// and subsequent listing pages are read with the same cookie.
    /// </summary>
    public sealed class UohsListingClient : IDisposable
    {
        readonly UohsSettings settings;
        readonly HttpClient httpClient;
        readonly Devmasters.Log.Logger logger;

        public UohsListingClient(UohsSettings settings, HttpMessageHandler handler, Devmasters.Log.Logger logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings;
            this.logger = logger;
            httpClient = new HttpClient(handler, disposeHandler: true) { Timeout = settings.HttpTimeout };
        }

        /// <summary>
        /// Creates a client with its own cookie container, so the session filter is kept between requests.
        /// </summary>
        public static UohsListingClient Create(UohsSettings settings, Devmasters.Log.Logger logger)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
                AllowAutoRedirect = true,
            };
            return new UohsListingClient(settings, handler, logger);
        }

        /// <summary>
        /// Returns the listing page with the given 1-based number. Page 1 is the response of the search form
        /// submitted with an empty area. When the server forgets the filter (expired session),
        /// the filter is applied again and the page is re-read once.
        /// </summary>
        public async Task<ListingPage> GetPageAsync(int pageNumber, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);

            if (pageNumber == 1)
                return await ApplyAllAreasFilterAsync(cancellationToken);

            var page = await GetListingPageAsync(pageNumber, cancellationToken);
            if (!page.IsFilteredByArea)
                return page;

            logger.Warning("UOHS listing page {PageNumber} is filtered by area {Area}, the session filter was lost; applying it again",
                pageNumber, page.SelectedAreaFilter);
            await ApplyAllAreasFilterAsync(cancellationToken);

            page = await GetListingPageAsync(pageNumber, cancellationToken);
            if (page.IsFilteredByArea)
                throw new InvalidOperationException(
                    $"UOHS listing page {pageNumber} is still filtered by area '{page.SelectedAreaFilter}' after the filter was reset.");

            return page;
        }

        async Task<ListingPage> ApplyAllAreasFilterAsync(CancellationToken cancellationToken)
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

            logger.Info("UOHS listing filter set to all areas, page 1 contains {IdCount} decisions", page.DecisionIds.Count);
            return page;
        }

        async Task<ListingPage> GetListingPageAsync(int pageNumber, CancellationToken cancellationToken)
        {
            var url = string.Format(CultureInfo.InvariantCulture, settings.ListingPageUrlTemplate, pageNumber);
            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return UohsListingParser.Parse(await response.Content.ReadAsStringAsync(cancellationToken), settings.AreaFilterFormField);
        }

        public void Dispose() => httpClient.Dispose();
    }
}
