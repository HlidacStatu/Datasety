using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// Thrown when the command line is invalid; the message is meant for the user.
    /// </summary>
    public sealed class RunOptionsException(string message) : Exception(message);

    /// <summary>
    /// Validated command line options.
    /// <para><c>StartId</c>/<c>Count</c> - explicit ID range mode (/start=N [/num=M]); null StartId = discover new decisions on UOHS.</para>
    /// </summary>
    public sealed record RunOptions(string ApiKey, bool Debug, bool RecreateDataset, bool DryRun, int? StartId, int? Count)
    {
        public const string Usage = """
            /apikey=xxx [/start=N [/num=M]] [/new] [/dryrun] [/debug]
              without /start: downloads decisions listed on UOHS (all areas) which are missing in the dataset
              /start=N [/num=M]: downloads decision IDs N .. N+M-1
              /new: deletes and recreates the dataset
              /dryrun: only reports the decision IDs which would be downloaded
            """;

        const string ApiKeyName = "/apikey";
        const string StartName = "/start";
        const string NumName = "/num";
        const string NewName = "/new";
        const string DryRunName = "/dryrun";
        const string DebugName = "/debug";
        const string SecretMask = "***";

        static readonly string[] KnownNames = [ApiKeyName, StartName, NumName, NewName, DryRunName, DebugName];

        /// <summary>
        /// Parses arguments in the /name or /name=value form (names are case-insensitive).
        /// Throws <see cref="RunOptionsException"/> for missing, unknown or invalid arguments - no value is silently replaced.
        /// </summary>
        public static RunOptions Parse(IReadOnlyList<string> args)
        {
            ArgumentNullException.ThrowIfNull(args);

            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var arg in args)
            {
                var (name, value) = SplitArgument(arg);
                if (!KnownNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    throw new RunOptionsException($"Unknown argument '{name}'.");
                if (!values.TryAdd(name, value))
                    throw new RunOptionsException($"Argument '{name}' is given more than once.");
            }

            if (!values.TryGetValue(ApiKeyName, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
                throw new RunOptionsException($"Argument '{ApiKeyName}=' is required.");

            var startId = ParseNumber(values, StartName, minimum: 0);
            var count = ParseNumber(values, NumName, minimum: 1);
            if (count.HasValue && !startId.HasValue)
                throw new RunOptionsException($"Argument '{NumName}' can be used only together with '{StartName}'.");

            var recreate = ParseFlag(values, NewName);
            var dryRun = ParseFlag(values, DryRunName);
            if (dryRun && recreate)
                throw new RunOptionsException($"'{DryRunName}' cannot be combined with '{NewName}', '{NewName}' deletes the dataset.");

            return new RunOptions(apiKey, ParseFlag(values, DebugName), recreate, dryRun, startId, count);
        }

        /// <summary>
        /// Returns the arguments for logging, with the API key value replaced by a mask.
        /// </summary>
        public static string ToLogString(IEnumerable<string> args) =>
            string.Join(" ", args.Select(arg =>
                string.Equals(SplitArgument(arg).Name, ApiKeyName, StringComparison.OrdinalIgnoreCase)
                    ? $"{ApiKeyName}={SecretMask}"
                    : arg));

        static (string Name, string? Value) SplitArgument(string arg)
        {
            var trimmed = arg.Trim();
            var delimiter = trimmed.IndexOf('=');
            return delimiter < 0
                ? (trimmed, null)
                : (trimmed[..delimiter], trimmed[(delimiter + 1)..]);
        }

        static int? ParseNumber(Dictionary<string, string?> values, string name, int minimum)
        {
            if (!values.TryGetValue(name, out var text))
                return null;

            if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number < minimum)
                throw new RunOptionsException($"Argument '{name}' must be a whole number >= {minimum}, got '{text}'.");

            return number;
        }

        static bool ParseFlag(Dictionary<string, string?> values, string name)
        {
            if (!values.TryGetValue(name, out var value))
                return false;
            if (value != null)
                throw new RunOptionsException($"Argument '{name}' does not take a value.");

            return true;
        }
    }
}
