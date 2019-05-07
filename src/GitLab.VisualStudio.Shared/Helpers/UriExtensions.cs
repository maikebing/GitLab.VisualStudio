using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLab.VisualStudio.Shared.Helpers
{
    public static class UriExtensions
    {
        /// <summary>
        /// Appends a relative path to the URL.
        /// </summary>
        /// <remarks>
        /// The Uri constructor for combining relative URLs have a different behavior with URLs that end with /
        /// than those that don't.
        /// </remarks>
        public static Uri Append(this Uri uri, string relativePath)
        {
            if (!uri.AbsolutePath.EndsWith("/", StringComparison.Ordinal))
            {
                uri = new Uri(uri + "/");
            }
            return new Uri(uri, new Uri(relativePath, UriKind.Relative));
        }

        public static bool IsHypertextTransferProtocol(this Uri uri)
        {
            return uri.Scheme == "http" || uri.Scheme == "https";
        }

        public static bool IsSameHost(this Uri uri, Uri compareUri)
        {
            return uri.Host.Equals(compareUri.Host, StringComparison.OrdinalIgnoreCase);
        }
        public static string RightAfterLast(this string s, string search)
        {

            if (s == null) return null;
            int lastIndex = s.LastIndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (lastIndex < 0)
                return null;

            return s.Substring(lastIndex + search.Length);
        }
        public static string TrimEnd(this string s, string suffix)
        {

            if (s == null) return null;
            if (!s.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return s;

            return s.Substring(0, s.Length - suffix.Length);
        }
    }
}
