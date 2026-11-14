using System.Globalization;
using System.Text;

namespace HlidacStatu.ParlamentImport.Psp;

/// <summary>
/// Reader of the psp.cz open data UNL format: windows-1250 text, one record per line,
/// columns separated by '|' (the line ends with a trailing '|'), '\' escapes the next character,
/// an empty column means NULL.
/// </summary>
internal static class UnlReader
{
    private const char Separator = '|';
    private const char Escape = '\\';

    /// <summary>Encoding of all psp.cz UNL files.</summary>
    public static Encoding SourceEncoding { get; } = CreateEncoding();

    private static Encoding CreateEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1250);
    }

    /// <summary>Splits the UNL text into rows of columns; empty columns become null.</summary>
    public static IReadOnlyList<string?[]> Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        List<string?[]> rows = [];
        List<string?> columns = [];
        var current = new StringBuilder();
        var escaped = false;

        foreach (var ch in text)
        {
            if (escaped)
            {
                current.Append(ch);
                escaped = false;
            }
            else if (ch == Escape)
            {
                escaped = true;
            }
            else if (ch == Separator)
            {
                columns.Add(current.Length == 0 ? null : current.ToString());
                current.Clear();
            }
            else if (ch == '\n')
            {
                EndRow(rows, columns, current);
            }
            else if (ch != '\r')
            {
                current.Append(ch);
            }
        }

        EndRow(rows, columns, current);
        return rows;
    }

    private static void EndRow(List<string?[]> rows, List<string?> columns, StringBuilder current)
    {
        if (current.Length > 0)
            columns.Add(current.ToString().Trim());
        current.Clear();
        if (columns.Count > 0)
            rows.Add([.. columns.Select(c => c?.Trim() is { Length: > 0 } t ? t : null)]);
        columns.Clear();
    }

    /// <summary>Parses a psp.cz date "DD.MM.YYYY"; null for NULL and for the "unknown" value 1.1.1900.</summary>
    public static DateTime? ParseDate(string? value)
    {
        if (value is null)
            return null;
        if (!DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new FormatException($"Unexpected psp.cz date '{value}'.");
        return date.Year <= 1900 ? null : date;
    }

    /// <summary>Parses a psp.cz datetime "YYYY-MM-DD HH" (optionally with minutes and seconds).</summary>
    public static DateTime? ParseDateTime(string? value)
    {
        if (value is null)
            return null;
        string[] formats = ["yyyy-MM-dd HH", "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss"];
        if (!DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new FormatException($"Unexpected psp.cz datetime '{value}'.");
        return date;
    }

    /// <summary>Parses an integer column; throws when the value is missing or not a number.</summary>
    public static int ParseInt(string? value, string column) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number
            : throw new FormatException($"Column {column}: '{value}' is not an integer.");
}
