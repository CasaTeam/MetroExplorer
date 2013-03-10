using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MetroExplorer.Common;
using System.Collections.ObjectModel;
using MetroExplorer.core.Objects;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.Storage.AccessCache;
using MetroExplorer.core;

// Pour en savoir plus sur le modèle d'élément Page Éléments groupés, consultez la page http://go.microsoft.com/fwlink/?LinkId=234231

namespace MetroExplorer
{
    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>
    public sealed partial class PageMain : LayoutAwarePage, INotifyPropertyChanged
    {
        ObservableCollection<GroupInfoList<ExplorerItem>> explorerGroups;
        public ObservableCollection<GroupInfoList<ExplorerItem>> ExplorerGroups
        {
            get
            {
                return explorerGroups;
            }
            set
            {
                explorerGroups = value;
                NotifyPropertyChanged("ExplorerGroups");   
            }
        }

        Dictionary<ExplorerItem, string> dicItemToken = new Dictionary<ExplorerItem, string>();

        public PageMain()
        {
            this.InitializeComponent();
            DataContext = this;
            ExplorerGroups = new ObservableCollection<GroupInfoList<ExplorerItem>>();
            ExplorerGroups.Add(new GroupInfoList<ExplorerItem>() { Key = StringResources.ResourceLoader.GetString("MainPage_UserFolderGroupTitle") });
            ExplorerGroups.Add(new GroupInfoList<ExplorerItem>() { Key = StringResources.ResourceLoader.GetString("MainPage_SystemFolderGroupTitle") });
            this.Loaded += PageMain_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        async void PageMain_Loaded(object sender, RoutedEventArgs e)
        {
            initializeSystemFolders();
            await initializeUsersFolders();

            ScrollViewer myScrollViewer = itemGridView.GetFirstDescendantOfType<ScrollViewer>();
            myScrollViewer.ViewChanged += myScrollViewer_ViewChanged;
        }

        private async System.Threading.Tasks.Task initializeUsersFolders()
        {
            if (Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList != null && Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Entries.Count > 0)
            {
                foreach (var item in Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Entries)
                {
                    var retrievedItem = await StorageApplicationPermissions.MostRecentlyUsedList.GetItemAsync(item.Token);
                    if (retrievedItem is StorageFolder)
                    {
                        StorageFolder retrievedFolder = retrievedItem as StorageFolder;
                        if (retrievedFolder.Name.Contains(":\\"))
                        {
                            addNewItem(ExplorerGroups[0], retrievedFolder, item.Token);
                        }
                        else
                        {
                            addNewItem(ExplorerGroups[1], retrievedFolder, item.Token);
                        }
                    }
                }
            }
        }

        private void addNewItem(GroupInfoList<ExplorerItem> itemList, StorageFolder retrievedFolder, string token)
        {
            ExplorerItem item = new ExplorerItem()
            {
                Name = retrievedFolder.Name,
                Path = retrievedFolder.Path,
                StorageFolder = retrievedFolder,
                Type = ExplorerItemType.Folder
            };
            itemList.Add(item);
            dicItemToken.Add(item, token);
        }

        private void initializeSystemFolders()
        {
            ExplorerGroups[0].Add(new ExplorerItem()
            {
                Name = KnownFolders.PicturesLibrary.Name,
                Path = KnownFolders.PicturesLibrary.Path,
                StorageFolder = KnownFolders.PicturesLibrary,
                Type = ExplorerItemType.Folder
            });
            ExplorerGroups[0].Add(new ExplorerItem()
            {
                Name = KnownFolders.MusicLibrary.Name,
                Path = KnownFolders.MusicLibrary.Path,
                StorageFolder = KnownFolders.MusicLibrary,
                Type = ExplorerItemType.Folder
            });
            ExplorerGroups[0].Add(new ExplorerItem()
            {
                Name = KnownFolders.DocumentsLibrary.Name,
                Path = KnownFolders.DocumentsLibrary.Path,
                StorageFolder = KnownFolders.DocumentsLibrary,
                Type = ExplorerItemType.Folder
            });
            ExplorerGroups[0].Add(new ExplorerItem()
            {
                Name = KnownFolders.VideosLibrary.Name,
                Path = KnownFolders.VideosLibrary.Path,
                StorageFolder = KnownFolders.VideosLibrary,
                Type = ExplorerItemType.Folder
            });
            if (KnownFolders.RemovableDevices != null)
            {
                ExplorerGroups[0].Add(new ExplorerItem()
                {
                    Name = KnownFolders.RemovableDevices.Name,
                    Path = KnownFolders.RemovableDevices.Path,
                    StorageFolder = KnownFolders.RemovableDevices,
                    Type = ExplorerItemType.Folder
                });
            }
            if (KnownFolders.MediaServerDevices != null)
            {
                ExplorerGroups[0].Add(new ExplorerItem()
                {
                    Name = KnownFolders.MediaServerDevices.Name,
                    Path = KnownFolders.MediaServerDevices.Path,
                    StorageFolder = KnownFolders.MediaServerDevices,
                    Type = ExplorerItemType.Folder
                });
            }
        }

        void myScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            double offset = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).HorizontalOffset;
            double scroll = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).ScrollableWidth;
            double viewportwidth = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).ViewportWidth;
            var delta = offset * 50;
            //var delta = (offset / scroll) * (Image_Background.ActualWidth - viewportwidth);
            Image_Background.Margin = new Thickness(-delta, 0, 0, 0);

        }

        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="navigationParameter">Valeur de paramètre passée à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page.
        /// </param>
        /// <param name="pageState">Dictionnaire d'état conservé par cette page durant une session
        /// antérieure. Null lors de la première visite de la page.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: assignez une collection de groupes pouvant être liés à this.DefaultViewModel["Groups"]
        }

        private void itemGridView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ExplorerItem item = itemGridView.SelectedItem as ExplorerItem;
        }

        private async System.Threading.Tasks.Task addNewFolder()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder storageFolder = await folderPicker.PickSingleFolderAsync();
            if (storageFolder != null)
            {
                string token = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(storageFolder, storageFolder.Name);
                if (storageFolder.Name.Contains(":\\"))
                {
                    addNewItem(ExplorerGroups[0], storageFolder, token);
                }
                else
                    addNewItem(ExplorerGroups[1], storageFolder, token);
            }
        }

        private async void Button_AddNewDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            await addNewFolder();
        }

        private void Button_RemoveDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItems == null || itemGridView.SelectedItems.Count == 0) return;
            while (itemGridView.SelectedItems.Count > 0)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Remove(dicItemToken[(itemGridView.SelectedItems[0] as ExplorerItem)]);
                if (ExplorerGroups[0].Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                {
                    ExplorerGroups[0].Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                }
                else if (ExplorerGroups[1].Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                {
                    ExplorerGroups[1].Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                }
            }
        }

        #region propertychanged
        private void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private void itemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            BottomAppBar.IsOpen = true;
        }
    }
}
