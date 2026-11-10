using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class NewDecisionFinderTests
{
    static ListingPage Listing(params int[] ids) => new(true, ids, null);

    static Func<int, CancellationToken, Task<ListingPage>> Pages(params ListingPage[] pages) =>
        (pageNumber, _) => Task.FromResult(pageNumber <= pages.Length ? pages[pageNumber - 1] : Listing());

    static NewDecisionFinder Finder(int stopAfterKnownPages = 2, int maxPages = 100, int recentIdWindowSize = 0) =>
        new(stopAfterKnownPages, maxPages, recentIdWindowSize, TimeSpan.Zero);

    [Fact]
    public async Task FindAsync_NewDecisionsOnFirstPages_ReturnsOnlyMissingIds()
    {
        HashSet<int> stored = [100, 101, 102, 103, 104, 105];
        var fetch = Pages(Listing(110, 109, 105), Listing(108, 104, 103), Listing(102, 101), Listing(100));

        var result = await Finder().FindAsync(fetch, stored.Contains);

        Assert.Equal([110, 109, 108], result.NewIds);
        Assert.Equal(DiscoveryStopReason.KnownPagesThresholdReached, result.StopReason);
        Assert.Equal(4, result.PagesScanned);
    }

    [Fact]
    public async Task FindAsync_MissingDecisionWithLowerIdBehindNewOnes_IsFound()
    {
        HashSet<int> stored = [200, 201, 202];
        var fetch = Pages(Listing(205, 150), Listing(202, 201), Listing(200));

        var result = await Finder().FindAsync(fetch, stored.Contains);

        Assert.Equal([205, 150], result.NewIds);
    }

    [Fact]
    public async Task FindAsync_KnownPageBetweenNewOnes_ResetsKnownPagesCounter()
    {
        HashSet<int> stored = [1, 2, 3, 4];
        var fetch = Pages(Listing(1), Listing(10), Listing(2), Listing(11), Listing(3), Listing(4), Listing(12));

        var result = await Finder(stopAfterKnownPages: 2).FindAsync(fetch, stored.Contains);

        Assert.Equal([10, 11], result.NewIds);
        Assert.Equal(6, result.PagesScanned);
    }

    [Fact]
    public async Task FindAsync_IdRepeatedOnNextPage_IsReturnedOnce()
    {
        var fetch = Pages(Listing(10, 9), Listing(9, 8));

        var result = await Finder().FindAsync(fetch, _ => false);

        Assert.Equal([10, 9, 8], result.NewIds);
    }

    [Fact]
    public async Task FindAsync_EmptyPage_StopsAsEndOfListing()
    {
        var fetch = Pages(Listing(3, 2), Listing(1));

        var result = await Finder().FindAsync(fetch, _ => false);

        Assert.Equal([3, 2, 1], result.NewIds);
        Assert.Equal(DiscoveryStopReason.EndOfListing, result.StopReason);
        Assert.Equal(2, result.PagesScanned);
    }

    [Fact]
    public async Task FindAsync_PageIsNotListing_StopsAndKeepsFoundIds()
    {
        var fetch = Pages(Listing(3), new ListingPage(false, [], null), Listing(2));

        var result = await Finder().FindAsync(fetch, _ => false);

        Assert.Equal([3], result.NewIds);
        Assert.Equal(DiscoveryStopReason.NotAListingPage, result.StopReason);
        Assert.Equal(1, result.PagesScanned);
    }

    [Fact]
    public async Task FindAsync_MaxPagesReached_StopsAtLimit()
    {
        var requestedPages = new List<int>();
        Func<int, CancellationToken, Task<ListingPage>> fetch = (pageNumber, _) =>
        {
            requestedPages.Add(pageNumber);
            return Task.FromResult(Listing(1000 - pageNumber));
        };

        var result = await Finder(maxPages: 3).FindAsync(fetch, _ => false);

        Assert.Equal([1, 2, 3], requestedPages);
        Assert.Equal(DiscoveryStopReason.MaxPagesReached, result.StopReason);
        Assert.Equal(3, result.PagesScanned);
    }

    [Fact]
    public async Task FindAsync_PageScanned_ReportsPageSummary()
    {
        HashSet<int> stored = [5];
        var fetch = Pages(Listing(7, 6, 5), Listing(5));
        var scans = new List<ListingPageScan>();

        await Finder(stopAfterKnownPages: 1).FindAsync(fetch, stored.Contains, scans.Add);

        Assert.Equal(
            [new ListingPageScan(1, 3, 2, 2), new ListingPageScan(2, 1, 0, 2)],
            scans);
    }

    [Fact]
    public async Task FindAsync_Cancelled_ThrowsOperationCanceled()
    {
        using var cancellation = new CancellationTokenSource();
        Func<int, CancellationToken, Task<ListingPage>> fetch = (pageNumber, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(Listing(pageNumber));
        };
        var finder = new NewDecisionFinder(5, 10, 0, TimeSpan.FromMilliseconds(1));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => finder.FindAsync(fetch, _ => false, cancellationToken: cancellation.Token));
    }

    [Fact]
    public async Task FindAsync_LatePublishedDecisionDeepInListing_IsFoundByRecentIdWindow()
    {
        HashSet<int> stored = [.. Enumerable.Range(400, 98), 501, 502];
        var fetch = Pages(Listing(509, 508, 502, 501), Listing(497, 496), Listing(495, 494), Listing(500, 493));

        var result = await Finder(stopAfterKnownPages: 2, recentIdWindowSize: 20).FindAsync(fetch, stored.Contains);

        Assert.Equal(DiscoveryStopReason.KnownPagesThresholdReached, result.StopReason);
        Assert.Equal(3, result.PagesScanned);
        Assert.Equal([509, 508], result.ListingIds);
        Assert.Equal([507, 506, 505, 504, 503, 500, 499, 498], result.RecentWindowIds);
        Assert.Equal(509, result.MaxListedId);
        Assert.Equal([509, 508, 507, 506, 505, 504, 503, 500, 499, 498], result.NewIds);
    }

    [Fact]
    public async Task FindAsync_RecentWindow_SkipsIdsAlreadySeenOnListing()
    {
        var checkedIds = new List<int>();
        var fetch = Pages(Listing(10, 8), Listing());

        var result = await Finder(recentIdWindowSize: 5).FindAsync(fetch, id =>
        {
            checkedIds.Add(id);
            return false;
        });

        Assert.Equal([10, 8, 9, 7, 6], checkedIds);
        Assert.Equal([9, 7, 6], result.RecentWindowIds);
    }

    [Fact]
    public async Task FindAsync_RecentWindowLargerThanIds_StopsAtZero()
    {
        var fetch = Pages(Listing(2), Listing());

        var result = await Finder(recentIdWindowSize: 10).FindAsync(fetch, _ => false);

        Assert.Equal([1, 0], result.RecentWindowIds);
    }

    [Fact]
    public async Task FindAsync_NothingListed_ChecksNoWindow()
    {
        var fetch = Pages(new ListingPage(false, [], null));

        var result = await Finder(recentIdWindowSize: 10).FindAsync(fetch, _ => throw new InvalidOperationException("must not be called"));

        Assert.Empty(result.NewIds);
        Assert.Null(result.MaxListedId);
    }

    [Fact]
    public async Task FindAsync_DatasetCheckFails_PropagatesException()
    {
        var fetch = Pages(Listing(10));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Finder().FindAsync(fetch, _ => throw new InvalidOperationException("dataset API failed")));
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 10, -1)]
    public void Constructor_InvalidLimits_Throws(int stopAfterKnownPages, int maxPages, int recentIdWindowSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new NewDecisionFinder(stopAfterKnownPages, maxPages, recentIdWindowSize, TimeSpan.Zero));
    }
}
