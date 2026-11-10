using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// One page of the UOHS decision collection listing.
    /// <para><c>IsListing</c> - false when the server returned something else than a listing (e.g. a decision detail,
    /// which happens for very high page numbers).</para>
    /// <para><c>DecisionIds</c> - decision IDs in the order shown on the page, without duplicates.</para>
    /// <para><c>SelectedAreaFilter</c> - value of the selected "Oblast" option, null when no area is selected (= all areas).</para>
    /// </summary>
    public sealed record ListingPage(bool IsListing, IReadOnlyList<int> DecisionIds, string? SelectedAreaFilter)
    {
        public bool IsFilteredByArea => !string.IsNullOrEmpty(SelectedAreaFilter);
    }

    /// <summary>
    /// Parses listing pages of https://uohs.gov.cz/cs/verejne-zakazky/sbirky-rozhodnuti.html.
    /// </summary>
    public static partial class UohsListingParser
    {
        const string ListingTableXPath = "//table[@id='resolutions_list']";
        const string DetailLinksXPath = ListingTableXPath + "//a[contains(@href,'/sbirky-rozhodnuti/detail-')]";

        [GeneratedRegex(@"/sbirky-rozhodnuti/detail-(?<id>\d+)\.html", RegexOptions.CultureInvariant)]
        private static partial Regex DetailIdRegex();

        /// <summary>
        /// Extracts decision IDs and the active area filter from a listing page.
        /// </summary>
        public static ListingPage Parse(string html, string areaFilterFormField)
        {
            ArgumentNullException.ThrowIfNull(html);
            ArgumentException.ThrowIfNullOrWhiteSpace(areaFilterFormField);

            var page = new Devmasters.XPath(html);
            var selectedArea = page
                .GetNode($"//select[@name='{areaFilterFormField}']/option[@selected]")
                ?.GetAttributeValue("value", string.Empty);

            if (page.GetNode(ListingTableXPath) == null)
                return new ListingPage(false, [], selectedArea);

            var ids = (page.GetNodes(DetailLinksXPath) ?? [])
                .Select(link => ParseDecisionId(link.GetAttributeValue("href", string.Empty)))
                .OfType<int>()
                .Distinct()
                .ToArray();

            return new ListingPage(true, ids, selectedArea);
        }

        /// <summary>
        /// Returns the decision ID from a detail URL like /cs/verejne-zakazky/sbirky-rozhodnuti/detail-24508.html, or null.
        /// </summary>
        public static int? ParseDecisionId(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            var match = DetailIdRegex().Match(url);
            if (!match.Success)
                return null;

            return int.TryParse(match.Groups["id"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
                ? id
                : null;
        }
    }
}
