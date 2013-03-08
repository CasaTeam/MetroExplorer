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

// Pour en savoir plus sur le modèle d'élément Page Éléments groupés, consultez la page http://go.microsoft.com/fwlink/?LinkId=234231

namespace MetroExplorer
{
    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>
    public sealed partial class PageMain : LayoutAwarePage, INotifyPropertyChanged
    {
        private ObservableCollection<ExplorerItem> _explorerItems;
        public ObservableCollection<ExplorerItem> ExplorerItems
        {
            get { return _explorerItems; }
            set
            {
                _explorerItems = value;
                NotifyPropertyChanged("ExplorerItems");
            }
        }

        public PageMain()
        {
            this.InitializeComponent();
            DataContext = this;
            ExplorerItems = new ObservableCollection<ExplorerItem>();

            this.Loaded += PageMain_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.AppBar_BottomAppBar.IsOpen = true;
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
                        ExplorerItems.Add(new ExplorerItem()
                        {
                            Name = retrievedFolder.Name,
                            Path = retrievedFolder.Path,
                            StorageFolder = retrievedFolder,
                            Type = ExplorerItemType.Folder
                        });
                    }
                }
            }
        }

        private void initializeSystemFolders()
        {
            ExplorerItems.Add(new ExplorerItem()
            {
                Name = KnownFolders.PicturesLibrary.Name,
                Path = KnownFolders.PicturesLibrary.Path,
                StorageFolder = KnownFolders.PicturesLibrary,
                Type = ExplorerItemType.Folder
            });
            ExplorerItems.Add(new ExplorerItem()
            {
                Name = KnownFolders.MusicLibrary.Name,
                Path = KnownFolders.MusicLibrary.Path,
                StorageFolder = KnownFolders.MusicLibrary,
                Type = ExplorerItemType.Folder
            });
            ExplorerItems.Add(new ExplorerItem()
            {
                Name = KnownFolders.DocumentsLibrary.Name,
                Path = KnownFolders.DocumentsLibrary.Path,
                StorageFolder = KnownFolders.DocumentsLibrary,
                Type = ExplorerItemType.Folder
            });
            ExplorerItems.Add(new ExplorerItem()
            {
                Name = KnownFolders.VideosLibrary.Name,
                Path = KnownFolders.VideosLibrary.Path,
                StorageFolder = KnownFolders.VideosLibrary,
                Type = ExplorerItemType.Folder
            });
            if (KnownFolders.RemovableDevices != null)
            {
                ExplorerItems.Add(new ExplorerItem()
                {
                    Name = KnownFolders.RemovableDevices.Name,
                    Path = KnownFolders.RemovableDevices.Path,
                    StorageFolder = KnownFolders.RemovableDevices,
                    Type = ExplorerItemType.Folder
                });
            }
            if (KnownFolders.MediaServerDevices != null)
            {
                ExplorerItems.Add(new ExplorerItem()
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
            var delta = offset * 90;
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

                itemGridView.SelectedItem = null;

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
                ExplorerItem eplItem = new ExplorerItem()
                {
                    Name = storageFolder.Name,
                    Path = storageFolder.Path,
                    StorageFolder = storageFolder,
                    Type = ExplorerItemType.Folder
                };
                ExplorerItems.Add(eplItem);
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(storageFolder, storageFolder.Name);
            }
        }

        private async void Button_AddNewDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            await addNewFolder();
        }

        private void Button_RemoveDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItems == null || itemGridView.SelectedItems.Count == 0) return;
            for (int i = itemGridView.SelectedItems.Count - 1; i > -1 ; i--)
            {
                ExplorerItems.Remove(itemGridView.SelectedItems[i] as ExplorerItem);
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

    public class GroupInfoList<T> : ObservableCollection<T>
    {
        public string Key { get; set; }

        public new IEnumerator<T> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<T>)base.GetEnumerator();
        }
    }
}
