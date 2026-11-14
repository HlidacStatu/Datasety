namespace HlidacStatu.ParlamentImport;

/// <summary>
/// Command line of the importer:
/// <c>--apikey &lt;token&gt;</c> (required), <c>--dry-run &lt;path&gt;</c>, <c>--recreate-dataset</c>;
/// every other argument is passed to the configuration as an override (<c>--Section:Key=value</c>).
/// </summary>
internal sealed record RunArguments(string ApiKey, string? DryRunPath, bool RecreateDataset, string[] ConfigurationOverrides)
{
    public const string ApiKeySwitch = "--apikey";
    public const string DryRunSwitch = "--dry-run";
    public const string RecreateDatasetSwitch = "--recreate-dataset";

    public const string Usage = """
        ParlamentImport --apikey <token> [--dry-run <file.json>] [--recreate-dataset] [--Section:Key=value ...]
          --apikey <token>      Hlídač státu API token (required); the dataset is written under its account
          --dry-run <file.json> download, parse and match only; write the snapshot to the file, not to the dataset
          --recreate-dataset    delete the dataset (owner only) and register it again before writing
        """;

    /// <summary>Parses the arguments; throws <see cref="ArgumentException"/> with a readable message when they are invalid.</summary>
    public static RunArguments Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        string? apiKey = null;
        string? dryRunPath = null;
        var recreate = false;
        List<string> overrides = [];

        for (var i = 0; i < args.Count; i++)
        {
            switch (args[i])
            {
                case ApiKeySwitch:
                    apiKey = Value(args, ++i, ApiKeySwitch);
                    break;
                case DryRunSwitch:
                    dryRunPath = Value(args, ++i, DryRunSwitch);
                    break;
                case RecreateDatasetSwitch:
                    recreate = true;
                    break;
                default:
                    overrides.Add(args[i]);
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException($"{ApiKeySwitch} <token> is required.");
        if (recreate && dryRunPath is not null)
            throw new ArgumentException($"{RecreateDatasetSwitch} cannot be combined with {DryRunSwitch}.");

        return new RunArguments(apiKey, dryRunPath, recreate, [.. overrides]);
    }

    private static string Value(IReadOnlyList<string> args, int index, string name) =>
        index < args.Count && !args[index].StartsWith("--", StringComparison.Ordinal)
            ? args[index]
            : throw new ArgumentException($"{name} needs a value.");
}
