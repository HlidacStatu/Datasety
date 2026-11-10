using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class DecisionIdSelectorTests
{
    static readonly ImportSettings Settings = new(MaxDegreeOfParallelism: 2, RangeModeDefaultCount: 3, EmptyDatasetRangeCount: 4);

    static readonly IReadOnlyList<int> DiscoveredIds = [24509, 24508];

    static Task<IReadOnlyList<int>> Discover(CancellationToken _) => Task.FromResult(DiscoveredIds);

    static Task<IReadOnlyList<int>> DiscoveryMustNotRun(CancellationToken _) =>
        throw new InvalidOperationException("discovery must not run");

    [Fact]
    public async Task SelectAsync_NoStartArgument_DiscoversNewDecisions()
    {
        var options = RunOptions.Parse(["/apikey=secret"]);

        var selection = await new DecisionIdSelector(Settings).SelectAsync(options, () => false, Discover);

        Assert.Equal(SelectionMode.Discovery, selection.Mode);
        Assert.Equal(DiscoveredIds, selection.Ids);
    }

    [Fact]
    public async Task SelectAsync_StartWithoutNum_UsesConfiguredCount()
    {
        var options = RunOptions.Parse(["/apikey=secret", "/start=100"]);

        var selection = await new DecisionIdSelector(Settings).SelectAsync(options, () => false, DiscoveryMustNotRun);

        Assert.Equal(SelectionMode.IdRange, selection.Mode);
        Assert.Equal([100, 101, 102], selection.Ids);
    }

    [Fact]
    public async Task SelectAsync_StartAndNum_ReturnsRequestedRange()
    {
        var options = RunOptions.Parse(["/apikey=secret", "/start=0", "/num=2"]);

        var selection = await new DecisionIdSelector(Settings).SelectAsync(options, () => true, DiscoveryMustNotRun);

        Assert.Equal(SelectionMode.IdRange, selection.Mode);
        Assert.Equal([0, 1], selection.Ids);
    }

    [Fact]
    public async Task SelectAsync_EmptyDataset_ReturnsWholeConfiguredRange()
    {
        var options = RunOptions.Parse(["/apikey=secret"]);

        var selection = await new DecisionIdSelector(Settings).SelectAsync(options, () => true, DiscoveryMustNotRun);

        Assert.Equal(SelectionMode.EmptyDataset, selection.Mode);
        Assert.Equal([0, 1, 2, 3], selection.Ids);
    }
}
