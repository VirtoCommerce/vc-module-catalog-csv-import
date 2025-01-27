// Remove this file after deriving IInventorySearchService from ISearchService<>.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogCsvImportModule.Core.Extensions;

public static class InventorySearchServiceExtensions
{
    /// <summary>
    /// Returns data from the cache without cloning. This consumes less memory, but the returned data must not be modified.
    /// </summary>
    public static async Task<IList<InventoryInfo>> SearchAllNoCloneAsync(this IInventorySearchService searchService, InventorySearchCriteria searchCriteria)
    {
        var result = new List<InventoryInfo>();

        await foreach (var searchResult in searchService.SearchBatchesNoCloneAsync(searchCriteria))
        {
            result.AddRange(searchResult.Results);
        }

        return result;
    }

    /// <summary>
    /// Returns data from the cache without cloning. This consumes less memory, but the returned data must not be modified.
    /// </summary>
    public static async IAsyncEnumerable<InventoryInfoSearchResult> SearchBatchesNoCloneAsync(this IInventorySearchService searchService, InventorySearchCriteria searchCriteria)
    {
        int totalCount;
        searchCriteria = searchCriteria.CloneTyped();

        do
        {
            var searchResult = await searchService.SearchInventoriesAsync(searchCriteria);

            if (searchCriteria.Take == 0 ||
                searchResult.Results.Any())
            {
                yield return searchResult;
            }

            if (searchCriteria.Take == 0)
            {
                yield break;
            }

            totalCount = searchResult.TotalCount;
            searchCriteria.Skip += searchCriteria.Take;
        }
        while (searchCriteria.Skip < totalCount);
    }
}
