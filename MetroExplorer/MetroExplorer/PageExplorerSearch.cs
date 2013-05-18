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
            ObservableCollection<GroupInfoList<ExplorerItem>> explorerGroups = new ObservableCollection<GroupInfoList<ExplorerItem>>
                {
                    new GroupInfoList<ExplorerItem>
                        {
                            Key = StringResources.ResourceLoader.GetString("MainPage_UserFolderGroupTitle")
                        },
                    new GroupInfoList<ExplorerItem>
                        {
                            Key = StringResources.ResourceLoader.GetString("MainPage_SystemFolderGroupTitle")
                        }
                };
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
                    explorerGroups[0].AddItem(item.Item);
                else if (item.Item is StorageFile)
                    explorerGroups[1].AddItem(item.Item);
            }

            if (explorerGroups.Count > 0)
            {
                _dataSource.FromSearch = true;
                _dataSource.SearchedItems = explorerGroups;
            }
        }
    }
}
