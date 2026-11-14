using System.Diagnostics;
using Serilog;

namespace HlidacStatu.ParlamentImport;

/// <summary>
/// HTTP access to the public sources. Requests run one after another with a pause of
/// <see cref="ImportOptions.RequestDelayMilliseconds"/> so the import never floods a source.
/// Every request is logged with its URL, status, size and duration.
/// </summary>
internal sealed class SourceClient(HttpClient httpClient, ImportOptions options)
{
    public const string HttpClientName = "parlament-sources";

    private static readonly ILogger Logger = Log.ForContext<SourceClient>();

    /// <summary>Downloads a text resource (HTML or JSON).</summary>
    public async Task<string> GetStringAsync(string url, string? accept, CancellationToken cancellationToken)
    {
        var bytes = await GetBytesAsync(url, accept, cancellationToken);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    /// <summary>Downloads a binary resource.</summary>
    public async Task<byte[]> GetBytesAsync(string url, string? accept, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        await Task.Delay(options.RequestDelayMilliseconds, cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (accept is not null)
            request.Headers.Accept.ParseAdd(accept);

        var stopwatch = Stopwatch.StartNew();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        Logger.Information("Source request {Url} returned {StatusCode} with {Bytes} bytes in {ElapsedMs} ms",
            url, (int)response.StatusCode, body.Length, stopwatch.ElapsedMilliseconds);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"GET {url} returned {(int)response.StatusCode}.", null, response.StatusCode);
        return body;
    }
}
