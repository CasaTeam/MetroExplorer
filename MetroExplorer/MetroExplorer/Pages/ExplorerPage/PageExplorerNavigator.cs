namespace MetroExplorer.Pages.ExplorerPage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Navigation;
    using Windows.Storage.FileProperties;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Storage.Streams;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Search;
    using Common;
    using Components.Navigator.Objects;

    public sealed partial class PageExplorer : LayoutAwarePage, INotifyPropertyChanged
    {
        private async Task InitializeNavigator()
        {
            List<List<string>> itemListArray = new List<List<string>>();
            foreach (StorageFolder storageFolder in DataSource.NavigatorStorageFolders)
            {
                var items = await storageFolder.GetItemsAsync();
                List<string> folderNames = items.OfType<StorageFolder>().Select(item => item.Name).ToList();

                itemListArray.Add(folderNames);
            }
            Navigator.ItemListArray = itemListArray.ToArray();
            Navigator.Path = DataSource.GetPath();

            var currentItems = await DataSource.CurrentStorageFolder.GetItemsAsync();
            CurrentItems = currentItems.Select(item => item.Name).ToList();
        }

        private async void NavigatorPathChanged(object sender,
            NavigatorNodeCommandArgument e)
        {
            if (e.CommandType == NavigatorNodeCommandType.None) return;

            DataSource.CutNavigatorFromIndex(e.Index);
            if (e.CommandType == NavigatorNodeCommandType.Reduce)
            {
                StopImageChangingDispatcher();
                Frame.Navigate(typeof(PageExplorer), null);
            }
            else if (e.CommandType == NavigatorNodeCommandType.Change)
            {
                StorageFolder lastStorageFolder = DataSource.CurrentStorageFolder;
                if (lastStorageFolder != null)
                {
                    var results = await lastStorageFolder.GetItemsAsync();
                    string changedNode = e.Path.Split('\\').LastOrDefault();
                    foreach (var item in results)
                    {
                        if (item is StorageFolder && item.Name == changedNode)
                        {
                            StorageFolder storageFolder = (StorageFolder)item;
                            DataSource.NavigatorStorageFolders.Add(storageFolder);
                            break;
                        }
                    }
                }
                StopImageChangingDispatcher();
                Frame.Navigate(typeof(PageExplorer), null);
            }
        }

        private void UpFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            DataSource.ToPreviousFolder();
            Frame.Navigate(typeof(PageExplorer), null);
        }
    }
}
