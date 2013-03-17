using MetroExplorer.core;
using MetroExplorer.core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Page Éléments groupés, consultez la page http://go.microsoft.com/fwlink/?LinkId=234231

namespace MetroExplorer
{
    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>
    public sealed partial class PageExplorer : MetroExplorer.Common.LayoutAwarePage, INotifyPropertyChanged
    {
        StorageFolder currentStorageFolder;
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

        public PageExplorer()
        {
            this.InitializeComponent();
            DataContext = this;
            ExplorerGroups = new ObservableCollection<GroupInfoList<ExplorerItem>>();
            ExplorerGroups.Add(new GroupInfoList<ExplorerItem>() { Key = StringResources.ResourceLoader.GetString("MainExplorer_UserFolderGroupTitle") });
            ExplorerGroups.Add(new GroupInfoList<ExplorerItem>() { Key = StringResources.ResourceLoader.GetString("MainExplorer_UserFileGroupTitle") });
            this.Loaded += PageExplorer_Loaded;
        }

        void PageExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            currentStorageFolder = e.Parameter as StorageFolder;

            if (currentStorageFolder != null)
            {
                IReadOnlyList<IStorageItem> listFiles = await currentStorageFolder.GetItemsAsync();
                foreach (var item in listFiles)
                {
                    if (item is StorageFolder)
                    {
                        addNewItem(ExplorerGroups[0], item as StorageFolder);
                    }
                    else if (item is StorageFile)
                    {
                        addNewItem(ExplorerGroups[1], item as StorageFile);
                    }
                }
            }

        }

        private async void addNewItem(GroupInfoList<ExplorerItem> itemList, IStorageItem retrievedItem)
        {
            ExplorerItem item = new ExplorerItem()
            {
                Name = retrievedItem.Name,
                Path = retrievedItem.Path
            };
            if (retrievedItem is StorageFolder)
            {
                item.StorageFolder = retrievedItem as StorageFolder;
                item.Type = ExplorerItemType.Folder;
            }
            else if (retrievedItem is StorageFile)
            {
                item.StorageFile = retrievedItem as StorageFile;
                item.Type = ExplorerItemType.File;

                await thumbnailPhoto(item);
            }

            itemList.Add(item);
        }

        private static async System.Threading.Tasks.Task thumbnailPhoto(ExplorerItem item)
        {
            StorageItemThumbnail fileThumbnail = await item.StorageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 300);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            item.Image = bitmapImage;
        }

        private void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ExplorerItem item = e.ClickedItem as ExplorerItem;
            this.Frame.Navigate(typeof(PageExplorer), item.StorageFolder);
        }

        private async void Button_AddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder sf = await currentStorageFolder.CreateFolderAsync(StringResources.ResourceLoader.GetString("String_NewFolder"), CreationCollisionOption.GenerateUniqueName);
            ExplorerItem item = new ExplorerItem()
            {
                Name = sf.Name,
                RenamingName = sf.Name,
                Path = sf.Path,
                Type = ExplorerItemType.Folder,
                RenameBoxVisibility = "Visible",
                StorageFolder = sf
            };
            ExplorerGroups[0].Add(item);
            itemGridView.SelectedItem = item;
        }

        private void itemGridView_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void itemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            foreach (var selectedItem in e.RemovedItems)
            {
                if ((selectedItem as ExplorerItem).RenameBoxVisibility == "Visible")
                    (selectedItem as ExplorerItem).RenameBoxVisibility = "Collapsed";
            }
            foreach(var selectedItem in itemGridView.SelectedItems)
            {
                if (itemGridView.SelectedItems.Count > 1 && (selectedItem as ExplorerItem).RenameBoxVisibility == "Visible")
                    (selectedItem as ExplorerItem).RenameBoxVisibility = "Collapsed";
            }
            if (itemGridView.SelectedItems.Count == 1 && (itemGridView.SelectedItems[0] as ExplorerItem).RenameBoxVisibility == "Visible")
                BottomAppBar.IsOpen = false;
            else
                BottomAppBar.IsOpen = true;
        }

        private void Button_CancelRename_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItem != null)
            {
                (itemGridView.SelectedItem as ExplorerItem).RenameBoxVisibility = "Collapsed";
            }
        }

        private async void Button_RenameFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItem != null)
            {
                (itemGridView.SelectedItem as ExplorerItem).Name = (itemGridView.SelectedItem as ExplorerItem).RenamingName;
                (itemGridView.SelectedItem as ExplorerItem).RenameBoxVisibility = "Collapsed";
                await (itemGridView.SelectedItem as ExplorerItem).StorageFolder.RenameAsync((itemGridView.SelectedItem as ExplorerItem).RenamingName, NameCollisionOption.GenerateUniqueName);
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

        private async void Button_RemoveDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItems == null || itemGridView.SelectedItems.Count == 0) return;
            while (itemGridView.SelectedItems.Count > 0)
            {
                if (ExplorerGroups[0].Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                {
                    await (itemGridView.SelectedItems[0] as ExplorerItem).StorageFolder.DeleteAsync();
                    ExplorerGroups[0].Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                }
                else if (ExplorerGroups[1].Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                {
                    await (itemGridView.SelectedItems[0] as ExplorerItem).StorageFile.DeleteAsync();
                    ExplorerGroups[1].Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                }
            }
            BottomAppBar.IsOpen = false;
        }

        private void Button_Detail_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_RenameDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItems.Count == 1)
            {
                (itemGridView.SelectedItem as ExplorerItem).RenameBoxVisibility = "Visible";
            }
        }

        private void AppBar_BottomAppBar_Opened_1(object sender, object e)
        {
            if (itemGridView.SelectedItems.Count == 1)
            {
                Button_RenameDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                Button_RenameDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            if (itemGridView.SelectedItems.Count == 0)
            {
                Button_RemoveDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                Button_RemoveDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
    }

    public class RenameBoxVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString() == "Collapsed")
                return "Visible";
            else
                return "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
