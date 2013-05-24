using MetroExplorer.core;
using MetroExplorer.core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace MetroExplorer
{
    public sealed partial class PageExplorer
    {
        private async Task Search(string navigationParameter)
        {
            if (_dataSource == null || _dataSource.CurrentStorageFolder == null) return;
            ObservableCollection<ExplorerItem> explorerGroups = new ObservableCollection<ExplorerItem>();
            var queryText = navigationParameter;
            var query = queryText.ToLower();
            var items = await _dataSource.CurrentStorageFolder.GetItemsAsync();
            var itemsFilter = items.Select(item => new
            {
                Distance = item.Name.ToLower().Contains(query) ? 1 : Levenshtein.Distance(item.Name.ToLower(), query),
                Item = item
            }
            ).Where(newItem => newItem.Distance >= 0.8).OrderByDescending(newItem => newItem.Distance);
            foreach (var item in itemsFilter)
            {
                if (item.Item is StorageFolder)
                    explorerGroups.AddStorageItem(item.Item as StorageFolder);
                else if (item.Item is StorageFile)
                    explorerGroups.AddFileItem(item.Item as StorageFile);
            }

            if (explorerGroups.Count > 0)
            {
                _dataSource.FromSearch = true;
                _dataSource.SearchedItems = explorerGroups;
            }
        }
    }
}
