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
        Automatic,
    }

    public sealed record DecisionIdSelection(SelectionMode Mode, IReadOnlyList<int> Ids);

    /// <summary>
    /// Chooses decision IDs to download: an explicit /start (+ /num) ID range,
    /// otherwise the automatic range computed from the dataset and the UOHS listing.
    /// </summary>
    public sealed class DecisionIdSelector(ImportSettings settings)
    {
        readonly ImportSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));

        public async Task<DecisionIdSelection> SelectAsync(
            RunOptions options,
            Func<CancellationToken, Task<IReadOnlyList<int>>> automaticIds,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(automaticIds);

            if (options.StartId.HasValue)
            {
                var count = options.Count ?? settings.RangeModeDefaultCount;
                return new DecisionIdSelection(SelectionMode.IdRange, Enumerable.Range(options.StartId.Value, count).ToArray());
            }

            return new DecisionIdSelection(SelectionMode.Automatic, await automaticIds(cancellationToken));
        }
    }
}
