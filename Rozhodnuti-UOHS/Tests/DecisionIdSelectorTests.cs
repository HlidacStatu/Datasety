using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class DecisionIdSelectorTests
{
    static readonly ImportSettings Settings = new(MaxDegreeOfParallelism: 2, RangeModeDefaultCount: 3, DatasetIdOverlap: 500, UohsIdLookahead: 200);

    static readonly IReadOnlyList<int> AutomaticIds = [24009, 24010];

    static Task<IReadOnlyList<int>> Automatic(CancellationToken _) => Task.FromResult(AutomaticIds);

    static Task<IReadOnlyList<int>> AutomaticMustNotRun(CancellationToken _) =>
        throw new InvalidOperationException("automatic selection must not run");

    [Fact]
    public async Task SelectAsync_NoStartArgument_UsesAutomaticRange()
    {
        var options = RunOptions.Parse(["/apikey=secret"]);

        var selection = await new DecisionIdSelector(Settings).SelectAsync(options, Automatic);

        Assert.Equal(SelectionMode.Automatic, selection.Mode);
        Assert.Equal(AutomaticIds, selection.Ids);
    }

    [Fact]
    public async Task SelectAsync_StartWithoutNum_UsesConfiguredCount()
    {
        var options = RunOptions.Parse(["/apikey=secret", "/start=100"]);

        var selection = await new DecisionIdSelector(Settings).SelectAsync(options, AutomaticMustNotRun);

        Assert.Equal(SelectionMode.IdRange, selection.Mode);
        Assert.Equal([100, 101, 102], selection.Ids);
    }

    [Fact]
    public async Task SelectAsync_StartAndNum_ReturnsRequestedRange()
    {
        var options = RunOptions.Parse(["/apikey=secret", "/start=0", "/num=2"]);

        var selection = await new DecisionIdSelector(Settings).SelectAsync(options, AutomaticMustNotRun);

        Assert.Equal(SelectionMode.IdRange, selection.Mode);
        Assert.Equal([0, 1], selection.Ids);
    }
}
