using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// Why the listing scan ended.
    /// </summary>
    public enum DiscoveryStopReason
    {
        KnownPagesThresholdReached,
        EndOfListing,
        NotAListingPage,
        MaxPagesReached,
    }

    /// <summary>
    /// Outcome of the discovery.
    /// <para><c>ListingIds</c> - IDs found on the scanned listing pages and missing in the dataset, in listing order.</para>
    /// <para><c>RecentWindowIds</c> - IDs from the recent ID window, not seen on the scanned pages and missing in the dataset
    /// (decisions published late, deep in the listing, or IDs which are not published at all).</para>
    /// <para><c>MaxListedId</c> - highest decision ID seen on the scanned pages, null when no ID was seen.</para>
    /// </summary>
    public sealed record DiscoveryResult(
        IReadOnlyList<int> ListingIds,
        IReadOnlyList<int> RecentWindowIds,
        int PagesScanned,
        DiscoveryStopReason StopReason,
        int? MaxListedId)
    {
        public IReadOnlyList<int> NewIds => [.. ListingIds, .. RecentWindowIds];
    }

    /// <summary>
    /// Summary of one scanned listing page, reported for logging.
    /// </summary>
    public sealed record ListingPageScan(int PageNumber, int IdsOnPage, int NewIdsOnPage, int TotalNewIds);

    /// <summary>
    /// Finds decisions published on UOHS which are not yet stored in the dataset.
    /// <para>1. Listing pages (all areas) are read from page 1; the scan stops after a configured number of consecutive pages
    /// on which every decision is already stored.</para>
    /// <para>2. The listing is ordered by the date of legal force, so a decision published today with an old date of legal force
    /// appears deep in the listing, behind the stop point. UOHS assigns IDs in publication order, therefore every ID in the window
    /// of <c>recentIdWindowSize</c> IDs up to the highest listed ID is checked against the dataset as well.</para>
    /// </summary>
    public sealed class NewDecisionFinder
    {
        readonly int stopAfterKnownPages;
        readonly int maxPages;
        readonly int recentIdWindowSize;
        readonly TimeSpan delayBetweenPages;

        public NewDecisionFinder(int stopAfterKnownPages, int maxPages, int recentIdWindowSize, TimeSpan delayBetweenPages)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(stopAfterKnownPages, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxPages, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(recentIdWindowSize);
            ArgumentOutOfRangeException.ThrowIfLessThan(delayBetweenPages, TimeSpan.Zero);

            this.stopAfterKnownPages = stopAfterKnownPages;
            this.maxPages = maxPages;
            this.recentIdWindowSize = recentIdWindowSize;
            this.delayBetweenPages = delayBetweenPages;
        }

        /// <summary>
        /// Scans the listing and the recent ID window and returns IDs which <paramref name="existsInDataset"/> reports as missing.
        /// </summary>
        public async Task<DiscoveryResult> FindAsync(
            Func<int, CancellationToken, Task<ListingPage>> fetchPage,
            Func<int, bool> existsInDataset,
            Action<ListingPageScan>? onPageScanned = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(fetchPage);
            ArgumentNullException.ThrowIfNull(existsInDataset);

            List<int> listingIds = [];
            HashSet<int> seenIds = [];
            var (pagesScanned, stopReason) = await ScanListingAsync(
                fetchPage, existsInDataset, onPageScanned, listingIds, seenIds, cancellationToken);

            int? maxListedId = seenIds.Count == 0 ? null : seenIds.Max();
            var recentWindowIds = maxListedId.HasValue
                ? FindMissingInRecentWindow(maxListedId.Value, seenIds, existsInDataset, cancellationToken)
                : [];

            return new DiscoveryResult(listingIds, recentWindowIds, pagesScanned, stopReason, maxListedId);
        }

        async Task<(int PagesScanned, DiscoveryStopReason StopReason)> ScanListingAsync(
            Func<int, CancellationToken, Task<ListingPage>> fetchPage,
            Func<int, bool> existsInDataset,
            Action<ListingPageScan>? onPageScanned,
            List<int> listingIds,
            HashSet<int> seenIds,
            CancellationToken cancellationToken)
        {
            var pagesWithoutNewIds = 0;

            for (var pageNumber = 1; pageNumber <= maxPages; pageNumber++)
            {
                if (pageNumber > 1 && delayBetweenPages > TimeSpan.Zero)
                    await Task.Delay(delayBetweenPages, cancellationToken);

                var page = await fetchPage(pageNumber, cancellationToken);
                if (!page.IsListing)
                    return (pageNumber - 1, DiscoveryStopReason.NotAListingPage);
                if (page.DecisionIds.Count == 0)
                    return (pageNumber - 1, DiscoveryStopReason.EndOfListing);

                var newOnPage = 0;
                foreach (var id in page.DecisionIds)
                {
                    if (seenIds.Add(id) && !existsInDataset(id))
                    {
                        listingIds.Add(id);
                        newOnPage++;
                    }
                }

                onPageScanned?.Invoke(new ListingPageScan(pageNumber, page.DecisionIds.Count, newOnPage, listingIds.Count));

                pagesWithoutNewIds = newOnPage == 0 ? pagesWithoutNewIds + 1 : 0;
                if (pagesWithoutNewIds >= stopAfterKnownPages)
                    return (pageNumber, DiscoveryStopReason.KnownPagesThresholdReached);
            }

            return (maxPages, DiscoveryStopReason.MaxPagesReached);
        }

        List<int> FindMissingInRecentWindow(
            int maxListedId, HashSet<int> seenIds, Func<int, bool> existsInDataset, CancellationToken cancellationToken)
        {
            List<int> missing = [];
            var lowestId = Math.Max(0, maxListedId - recentIdWindowSize + 1);

            for (var id = maxListedId; id >= lowestId; id--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!seenIds.Contains(id) && !existsInDataset(id))
                    missing.Add(id);
            }

            return missing;
        }
    }
}
