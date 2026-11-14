using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// Inclusive range of decision IDs downloaded by the automatic run.
    /// </summary>
    public sealed record DecisionIdRange(int FirstId, int LastId)
    {
        public int Count => LastId - FirstId + 1;

        public IReadOnlyList<int> ToIds() => Enumerable.Range(FirstId, Count).ToArray();
    }

    /// <summary>
    /// Computes which decision IDs the automatic run (without /start) downloads.
    /// UOHS assigns decision IDs in publication order across all areas, so new decisions have IDs above
    /// the highest ID already stored in the dataset.
    /// </summary>
    public static class NewDecisionRange
    {
        /// <summary>
        /// Returns IDs from (highest dataset ID - <paramref name="datasetIdOverlap"/>) to (highest UOHS ID + <paramref name="uohsIdLookahead"/>).
        /// The overlap re-reads recently stored decisions, the lookahead covers decisions newer than the listing page.
        /// An empty dataset (<paramref name="maxDatasetId"/> null) starts from ID 0.
        /// </summary>
        public static DecisionIdRange Compute(int? maxDatasetId, int maxUohsId, int datasetIdOverlap, int uohsIdLookahead)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxUohsId);
            ArgumentOutOfRangeException.ThrowIfNegative(datasetIdOverlap);
            ArgumentOutOfRangeException.ThrowIfNegative(uohsIdLookahead);
            if (maxDatasetId < 0)
                throw new ArgumentOutOfRangeException(nameof(maxDatasetId), maxDatasetId, "Must not be negative.");

            var firstId = Math.Max(0, (maxDatasetId ?? 0) - datasetIdOverlap);
            var lastId = maxUohsId + uohsIdLookahead;
            if (firstId > lastId)
                throw new InvalidOperationException(
                    $"Highest dataset ID {maxDatasetId} minus overlap {datasetIdOverlap} is above highest UOHS ID {maxUohsId} plus lookahead {uohsIdLookahead}; "
                    + "the UOHS listing probably did not return the newest decisions.");

            return new DecisionIdRange(firstId, lastId);
        }

        /// <summary>
        /// Finds the highest numeric item ID in the dataset. The dataset sorts IDs as text ("9999" before "24509"),
        /// so IDs are searched by length from <paramref name="maxDigits"/> down to 1 with a wildcard query (<c>Id:?????</c>)
        /// sorted descending; within one length the text order equals the numeric order.
        /// <paramref name="searchIdsSortedDescending"/> runs the query and returns the item IDs of the first result page.
        /// Returns null for an empty dataset.
        /// </summary>
        public static int? FindMaxDatasetId(Func<string, IEnumerable<string>> searchIdsSortedDescending, int maxDigits)
        {
            ArgumentNullException.ThrowIfNull(searchIdsSortedDescending);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxDigits, 1);

            for (var digits = maxDigits; digits >= 1; digits--)
            {
                var numericIds = searchIdsSortedDescending(IdLengthQuery(digits))
                    .Select(id => int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : (int?)null)
                    .OfType<int>()
                    .ToArray();

                if (numericIds.Length > 0)
                    return numericIds.Max();
            }

            return null;
        }

        /// <summary>
        /// Dataset query matching item IDs with exactly <paramref name="digits"/> characters.
        /// </summary>
        public static string IdLengthQuery(int digits) => "Id:" + new string('?', digits);

        /// <summary>
        /// Number of decimal digits of a non-negative number.
        /// </summary>
        public static int DigitCount(int value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            return value.ToString(CultureInfo.InvariantCulture).Length;
        }
    }
}
