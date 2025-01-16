using System;

namespace VirtoCommerce.CatalogCsvImportModule.Core;
public static class UrlHelper
{
    public static string ExtractFileNameFromUrl(string url)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(url);

        if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            if (!uri.IsAbsoluteUri)
            {
                uri = new Uri(new Uri("http://dummy-base/"), url);
            }

            // Get the file name from the path
            var localPath = uri.LocalPath;
            return localPath.Substring(localPath.LastIndexOf('/') + 1);
        }
        else
        {
            throw new UriFormatException($"Invalid URL format {url}.");
        }
    }
}
