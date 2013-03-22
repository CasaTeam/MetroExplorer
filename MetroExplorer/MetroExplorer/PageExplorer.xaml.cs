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
using Windows.UI.Xaml.Media.Animation;
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
            initializeChangingDispatcher();
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

        private void addNewItem(GroupInfoList<ExplorerItem> itemList, IStorageItem retrievedItem)
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
                //foreach (StorageFile st in await item.StorageFolder.GetFilesAsync())
                //{ 
                //    if(st.Name.ToUpper().EndsWith(".JPG") || st.Name.ToUpper().EndsWith(".JPEG") || st.Name.ToUpper().EndsWith(".PNG") ||
                //         st.Name.ToUpper().EndsWith(".BMP"))
                //        await thumbnailPhoto(item, st);
                //}
            }
            else if (retrievedItem is StorageFile)
            {
                item.StorageFile = retrievedItem as StorageFile;
                item.Type = ExplorerItemType.File;
                //await thumbnailPhoto(item, item.StorageFile);
            }    
            // sqdf
            itemList.Add(item);
        }

        private async System.Threading.Tasks.Task thumbnailPhoto(ExplorerItem item, StorageFile sf)
        {
            StorageItemThumbnail fileThumbnail = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 300);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            item.Image = bitmapImage;
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
