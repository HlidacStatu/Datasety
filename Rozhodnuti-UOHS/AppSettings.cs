using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// Validated runtime configuration loaded from appsettings.json.
    /// Every value is required - a missing or invalid value fails fast at startup, there are no built-in defaults.
    /// </summary>
    public sealed record AppSettings(UohsSettings Uohs, ImportSettings Import)
    {
        public const string FileName = "appsettings.json";

        /// <summary>
        /// Loads and validates the configuration file; throws <see cref="InvalidOperationException"/> describing every problem found.
        /// </summary>
        public static AppSettings Load(string path)
        {
            if (!File.Exists(path))
                throw new InvalidOperationException($"Configuration file '{path}' was not found.");

            return Parse(File.ReadAllText(path));
        }

        /// <summary>
        /// Parses and validates configuration JSON; throws <see cref="InvalidOperationException"/> describing every problem found.
        /// </summary>
        public static AppSettings Parse(string json)
        {
            RawAppSettings? raw;
            try
            {
                raw = JsonSerializer.Deserialize<RawAppSettings>(json, new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                });
            }
            catch (JsonException e)
            {
                throw new InvalidOperationException($"Configuration file '{FileName}' is not valid JSON: {e.Message}", e);
            }

            if (raw == null)
                throw new InvalidOperationException($"Configuration file '{FileName}' is empty.");

            List<string> errors = [];
            var uohs = ValidateUohs(raw.Uohs, errors);
            var import = ValidateImport(raw.Import, errors);

            if (errors.Count > 0 || uohs == null || import == null)
                throw new InvalidOperationException(
                    $"Invalid configuration in '{FileName}':{Environment.NewLine} - "
                    + string.Join(Environment.NewLine + " - ", errors));

            return new AppSettings(uohs, import);
        }

        static UohsSettings? ValidateUohs(RawUohsSettings? raw, List<string> errors)
        {
            if (raw == null)
            {
                errors.Add("Section 'Uohs' is missing.");
                return null;
            }

            var listingUrl = RequireAbsoluteUrl(errors, "Uohs:ListingUrl", raw.ListingUrl);
            var listingPageUrlTemplate = RequireUrlTemplate(errors, "Uohs:ListingPageUrlTemplate", raw.ListingPageUrlTemplate);
            var areaFilterFormField = RequireText(errors, "Uohs:AreaFilterFormField", raw.AreaFilterFormField);
            var detailUrlTemplate = RequireUrlTemplate(errors, "Uohs:DetailUrlTemplate", raw.DetailUrlTemplate);
            var linkBaseUrl = RequireAbsoluteUrl(errors, "Uohs:LinkBaseUrl", raw.LinkBaseUrl);
            var httpTimeoutSeconds = RequireNumber(errors, "Uohs:HttpTimeoutSeconds", raw.HttpTimeoutSeconds, minimum: 1);
            var listingRequestDelayMs = RequireNumber(errors, "Uohs:ListingRequestDelayMs", raw.ListingRequestDelayMs, minimum: 0);
            var stopAfterKnownListingPages = RequireNumber(errors, "Uohs:StopAfterKnownListingPages", raw.StopAfterKnownListingPages, minimum: 1);
            var maxListingPages = RequireNumber(errors, "Uohs:MaxListingPages", raw.MaxListingPages, minimum: 1);
            var recentIdWindowSize = RequireNumber(errors, "Uohs:RecentIdWindowSize", raw.RecentIdWindowSize, minimum: 0);

            if (listingUrl == null || listingPageUrlTemplate == null || areaFilterFormField == null
                || detailUrlTemplate == null || linkBaseUrl == null)
                return null;

            return new UohsSettings(
                listingUrl, listingPageUrlTemplate, areaFilterFormField, detailUrlTemplate, linkBaseUrl,
                TimeSpan.FromSeconds(httpTimeoutSeconds), TimeSpan.FromMilliseconds(listingRequestDelayMs),
                stopAfterKnownListingPages, maxListingPages, recentIdWindowSize);
        }

        static ImportSettings? ValidateImport(RawImportSettings? raw, List<string> errors)
        {
            if (raw == null)
            {
                errors.Add("Section 'Import' is missing.");
                return null;
            }

            return new ImportSettings(
                RequireNumber(errors, "Import:MaxDegreeOfParallelism", raw.MaxDegreeOfParallelism, minimum: 1),
                RequireNumber(errors, "Import:RangeModeDefaultCount", raw.RangeModeDefaultCount, minimum: 1),
                RequireNumber(errors, "Import:EmptyDatasetRangeCount", raw.EmptyDatasetRangeCount, minimum: 1));
        }

        static string? RequireText(List<string> errors, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;

            errors.Add($"'{name}' is required.");
            return null;
        }

        static string? RequireAbsoluteUrl(List<string> errors, string name, string? value)
        {
            if (RequireText(errors, name, value) == null)
                return null;

            if (Uri.TryCreate(value, UriKind.Absolute, out _))
                return value;

            errors.Add($"'{name}' must be an absolute URL, got '{value}'.");
            return null;
        }

        static string? RequireUrlTemplate(List<string> errors, string name, string? value)
        {
            if (RequireText(errors, name, value) == null)
                return null;

            if (!value!.Contains("{0}"))
            {
                errors.Add($"'{name}' must contain the '{{0}}' placeholder, got '{value}'.");
                return null;
            }

            return RequireAbsoluteUrl(errors, name, string.Format(value, 1)) == null ? null : value;
        }

        static int RequireNumber(List<string> errors, string name, int? value, int minimum)
        {
            if (!value.HasValue)
                errors.Add($"'{name}' is required.");
            else if (value.Value < minimum)
                errors.Add($"'{name}' must be >= {minimum}, got {value.Value}.");

            return value ?? minimum;
        }

        sealed class RawAppSettings
        {
            public RawUohsSettings? Uohs { get; set; }
            public RawImportSettings? Import { get; set; }
        }

        sealed class RawUohsSettings
        {
            public string? ListingUrl { get; set; }
            public string? ListingPageUrlTemplate { get; set; }
            public string? AreaFilterFormField { get; set; }
            public string? DetailUrlTemplate { get; set; }
            public string? LinkBaseUrl { get; set; }
            public int? HttpTimeoutSeconds { get; set; }
            public int? ListingRequestDelayMs { get; set; }
            public int? StopAfterKnownListingPages { get; set; }
            public int? MaxListingPages { get; set; }
            public int? RecentIdWindowSize { get; set; }
        }

        sealed class RawImportSettings
        {
            public int? MaxDegreeOfParallelism { get; set; }
            public int? RangeModeDefaultCount { get; set; }
            public int? EmptyDatasetRangeCount { get; set; }
        }
    }

    /// <summary>
    /// UOHS web settings.
    /// <para><c>ListingUrl</c> - search form of the decision collection; POSTing it with an empty area resets the filter to all areas.</para>
    /// <para><c>ListingPageUrlTemplate</c> - listing page 2, 3, ... ({0} = page number); uses the filter stored in the session cookie.</para>
    /// <para><c>AreaFilterFormField</c> - name of the "Oblast" select in the search form.</para>
    /// <para><c>DetailUrlTemplate</c> - decision detail ({0} = decision ID); IDs are shared by all areas.</para>
    /// <para><c>LinkBaseUrl</c> - prepended to relative links (documents, related decisions) found on a detail page.</para>
    /// <para><c>ListingRequestDelay</c> - pause between two listing page requests, to be gentle to the UOHS server.</para>
    /// <para><c>StopAfterKnownListingPages</c> - discovery stops after this many consecutive pages with nothing missing in the dataset.</para>
    /// <para><c>MaxListingPages</c> - safety limit of listing pages scanned in one run.</para>
    /// <para><c>RecentIdWindowSize</c> - number of IDs up to the highest listed ID which are always checked against the dataset;
    /// catches decisions published late, which the listing (ordered by the date of legal force) shows deep behind the stop point.</para>
    /// </summary>
    public sealed record UohsSettings(
        string ListingUrl,
        string ListingPageUrlTemplate,
        string AreaFilterFormField,
        string DetailUrlTemplate,
        string LinkBaseUrl,
        TimeSpan HttpTimeout,
        TimeSpan ListingRequestDelay,
        int StopAfterKnownListingPages,
        int MaxListingPages,
        int RecentIdWindowSize);

    /// <summary>
    /// Import settings.
    /// <para><c>MaxDegreeOfParallelism</c> - number of decision detail pages downloaded in parallel.</para>
    /// <para><c>RangeModeDefaultCount</c> - number of IDs processed in /start range mode when /num is not given.</para>
    /// <para><c>EmptyDatasetRangeCount</c> - number of IDs (from 0) processed when the dataset is empty.</para>
    /// </summary>
    public sealed record ImportSettings(
        int MaxDegreeOfParallelism,
        int RangeModeDefaultCount,
        int EmptyDatasetRangeCount);
}
