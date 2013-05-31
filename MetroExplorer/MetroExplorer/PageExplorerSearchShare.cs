using MetroExplorer.core;
using MetroExplorer.core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MetroExplorer
{
    public sealed partial class PageExplorer
    {
        private async Task InitializeSearch(Object navigationParameter)
        {
            string args = navigationParameter as string;
            if (args != null)
            {
                var currentContent = Window.Current.Content;
                var frame = currentContent as Frame;

                // ToDo: Consider a better solution (Sawyer)
                App.LastQuery = App.CurrentQuery = string.Empty;

                if (args != null)
                    await Search(args);
            }
        }

        private async Task Search(string navigationParameter)
        {
            if (DataSource == null || DataSource.CurrentStorageFolder == null) return;
            ObservableCollection<ExplorerItem> explorerGroups = new ObservableCollection<ExplorerItem>();
            var queryText = navigationParameter;
            var query = queryText.ToLower();
            var items = await DataSource.CurrentStorageFolder.GetItemsAsync();
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
                DataSource.FromSearch = true;
                DataSource.SearchedItems = explorerGroups;
            }
        }

        private void InitializeShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += PageExplorerDataRequested;

            if (DataSource.ShareStorageItems.Count > 0)
                AppBar_BottomAppBar.IsOpen = true;
        }

        private void PageExplorerDataRequested(DataTransferManager sender,
           DataRequestedEventArgs args)
        {
            DataPackage data = args.Request.Data;

            DataRequestDeferral waiter = args.Request.GetDeferral();
            try
            {
                List<IStorageItem> files = new List<IStorageItem>();
                int index = 0;
                foreach (var item in itemGridView.SelectedItems)
                {
                    ExplorerItem explorerItem = (ExplorerItem)item;
                    if (explorerItem != null)
                    {
                        if (index == 0)
                        {
                            data.Properties.Title = explorerItem.Name;
                            RandomAccessStreamReference image = RandomAccessStreamReference.CreateFromFile(explorerItem.StorageFile);
                            data.Properties.Thumbnail = image;
                            data.SetBitmap(image);
                        }
                        files.Add(explorerItem.StorageFile);
                    }
                    index++;
                }
                data.SetStorageItems(files);
                data.SetText("\n");
            }
            finally
            {
                waiter.Complete();
            }
        }
    }
}
