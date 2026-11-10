using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// How the decision IDs to download were chosen.
    /// </summary>
    public enum SelectionMode
    {
        IdRange,
        EmptyDataset,
        Discovery,
    }

    public sealed record DecisionIdSelection(SelectionMode Mode, IReadOnlyList<int> Ids);

    /// <summary>
    /// Chooses decision IDs to download: an explicit /start (+ /num) ID range, the whole ID range when the dataset is empty,
    /// otherwise decisions found on UOHS which are missing in the dataset.
    /// </summary>
    public sealed class DecisionIdSelector(ImportSettings settings)
    {
        readonly ImportSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));

        public async Task<DecisionIdSelection> SelectAsync(
            RunOptions options,
            Func<bool> isDatasetEmpty,
            Func<CancellationToken, Task<IReadOnlyList<int>>> discoverNewIds,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(isDatasetEmpty);
            ArgumentNullException.ThrowIfNull(discoverNewIds);

            if (options.StartId.HasValue)
            {
                var count = options.Count ?? settings.RangeModeDefaultCount;
                return new DecisionIdSelection(SelectionMode.IdRange, Enumerable.Range(options.StartId.Value, count).ToArray());
            }

            if (isDatasetEmpty())
                return new DecisionIdSelection(SelectionMode.EmptyDataset, Enumerable.Range(0, settings.EmptyDatasetRangeCount).ToArray());

            return new DecisionIdSelection(SelectionMode.Discovery, await discoverNewIds(cancellationToken));
        }
    }
}
