using MetroExplorer.Components.Navigator.Objects;
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
        IList<StorageFolder> _navigatorStorageFolders;
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
            _navigatorStorageFolders = new List<StorageFolder>();
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
            //currentStorageFolder = e.Parameter as StorageFolder;

            _navigatorStorageFolders = (IList<StorageFolder>)e.Parameter;
            currentStorageFolder = _navigatorStorageFolders.LastOrDefault();

            if (currentStorageFolder != null)
            {
                List<List<string>> itemListArray = new List<List<string>>();
                foreach (StorageFolder storageFolder in _navigatorStorageFolders)
                {
                    var items = await storageFolder.GetItemsAsync();
                    List<string> folderNames = new List<string>();
                    foreach (var item in items)
                    {
                        if (item is StorageFolder)
                            folderNames.Add(item.Name);
                    }

                    itemListArray.Add(folderNames);
                }
                Navigator.ItemListArray = itemListArray.ToArray();
                Navigator.Path = currentStorageFolder.Path;
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

        private async void NavigatorPathChanged(object sender, NavigatorNodeCommandArgument e)
        {
            IList<StorageFolder> parameters = new List<StorageFolder>();
            parameters = _navigatorStorageFolders.Take(e.Index + 1).ToList();

            if (e.CommandType == NavigatorNodeCommandType.Reduce)
            {
                imageChangingDispatcher.Stop();
                Frame.Navigate(typeof(PageExplorer), parameters);
            }
            else if (e.CommandType == NavigatorNodeCommandType.Change)
            {
                StorageFolder lastStorageFolder = parameters.LastOrDefault();
                if (lastStorageFolder != null)
                {
                    var results = await lastStorageFolder.GetItemsAsync();
                    string changedNode = e.Path.Split('\\').LastOrDefault();
                    StorageFolder storageFolder = null;
                    foreach (var item in results)
                    {
                        if (item is StorageFolder && item.Name == changedNode)
                        {
                            storageFolder = (StorageFolder)item;
                            parameters.Add(storageFolder);
                            break;
                        }
                    }
                }
                imageChangingDispatcher.Stop();
                Frame.Navigate(typeof(PageExplorer), parameters);
            }
        }

        private void ButtonMainPage_Click_1(object sender, RoutedEventArgs e)
        {
            imageChangingDispatcher.Stop();
            Frame.Navigate(typeof(PageMain));
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
