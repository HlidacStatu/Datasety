using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class NewDecisionRangeTests
{
    [Fact]
    public void Compute_DatasetBehindUohs_ReturnsOverlapToLookahead()
    {
        var range = NewDecisionRange.Compute(maxDatasetId: 18143, maxUohsId: 24509, datasetIdOverlap: 500, uohsIdLookahead: 200);

        Assert.Equal(new DecisionIdRange(17643, 24709), range);
        Assert.Equal(7067, range.Count);
    }

    [Fact]
    public void Compute_DatasetUpToDate_ReturnsOverlapAndLookaheadOnly()
    {
        var range = NewDecisionRange.Compute(maxDatasetId: 24509, maxUohsId: 24509, datasetIdOverlap: 500, uohsIdLookahead: 200);

        Assert.Equal(new DecisionIdRange(24009, 24709), range);
    }

    [Fact]
    public void Compute_EmptyDataset_StartsFromZero()
    {
        var range = NewDecisionRange.Compute(maxDatasetId: null, maxUohsId: 24509, datasetIdOverlap: 500, uohsIdLookahead: 200);

        Assert.Equal(new DecisionIdRange(0, 24709), range);
    }

    [Fact]
    public void Compute_OverlapBelowZero_StartsFromZero()
    {
        var range = NewDecisionRange.Compute(maxDatasetId: 100, maxUohsId: 150, datasetIdOverlap: 500, uohsIdLookahead: 200);

        Assert.Equal(0, range.FirstId);
    }

    [Fact]
    public void Compute_DatasetFarAboveUohs_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => NewDecisionRange.Compute(maxDatasetId: 30000, maxUohsId: 100, datasetIdOverlap: 500, uohsIdLookahead: 200));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    public void Compute_NegativeArgument_Throws(int maxUohsId, int datasetIdOverlap, int uohsIdLookahead)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => NewDecisionRange.Compute(10, maxUohsId, datasetIdOverlap, uohsIdLookahead));
    }

    [Fact]
    public void ToIds_Range_ReturnsAllIdsInclusive()
    {
        var ids = new DecisionIdRange(5, 8).ToIds();

        Assert.Equal([5, 6, 7, 8], ids);
    }

    /// <summary>
    /// Simulates the dataset search: IDs matching the length query, sorted descending as text.
    /// </summary>
    static Func<string, IEnumerable<string>> Dataset(List<string> queries, params string[] ids) => query =>
    {
        queries.Add(query);
        var length = query.Length - "Id:".Length;
        return ids.Where(id => id.Length == length).OrderByDescending(id => id, StringComparer.Ordinal);
    };

    [Fact]
    public void FindMaxDatasetId_MixedLengths_ReturnsNumericMaximum()
    {
        List<string> queries = [];

        var maxId = NewDecisionRange.FindMaxDatasetId(Dataset(queries, "9999", "24509", "18143", "999", "5"), maxDigits: 6);

        Assert.Equal(24509, maxId);
        Assert.Equal(["Id:??????", "Id:?????"], queries);
    }

    [Fact]
    public void FindMaxDatasetId_EmptyDataset_ReturnsNull()
    {
        List<string> queries = [];

        var maxId = NewDecisionRange.FindMaxDatasetId(Dataset(queries), maxDigits: 3);

        Assert.Null(maxId);
        Assert.Equal(["Id:???", "Id:??", "Id:?"], queries);
    }

    [Fact]
    public void FindMaxDatasetId_NonNumericIdsOfSameLength_AreIgnored()
    {
        List<string> queries = [];

        var maxId = NewDecisionRange.FindMaxDatasetId(Dataset(queries, "abcde", "1234"), maxDigits: 5);

        Assert.Equal(1234, maxId);
    }

    [Fact]
    public void FindMaxDatasetId_ResultsNotSorted_ReturnsMaximumOfPage()
    {
        var maxId = NewDecisionRange.FindMaxDatasetId(_ => ["24508", "24509", "24001"], maxDigits: 5);

        Assert.Equal(24509, maxId);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(9, 1)]
    [InlineData(24509, 5)]
    [InlineData(100000, 6)]
    public void DigitCount_Number_ReturnsDecimalDigits(int value, int expected)
    {
        Assert.Equal(expected, NewDecisionRange.DigitCount(value));
    }
}
