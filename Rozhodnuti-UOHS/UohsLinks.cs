using System;

#nullable enable

namespace Rozhodnuti_UOHS
{
    /// <summary>
    /// Builds absolute URLs from links found on UOHS pages.
    /// </summary>
    public static class UohsLinks
    {
        /// <summary>
        /// Returns <paramref name="href"/> as an absolute URL: absolute links are kept, relative ones are resolved against
        /// <paramref name="linkBaseUrl"/>. Returns null for an empty link.
        /// </summary>
        public static string? ToAbsoluteUrl(string linkBaseUrl, string? href)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(linkBaseUrl);

            if (string.IsNullOrWhiteSpace(href))
                return null;

            var trimmed = href.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute)
                && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
                return absolute.AbsoluteUri;

            return new Uri(new Uri(linkBaseUrl), trimmed).AbsoluteUri;
        }
    }
}
