﻿namespace MetroExplorer
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
    using Components.Navigator.Objects;
    using core;
    using core.Objects;
    using core.Utils;

    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>
    public sealed partial class PageExplorer : INotifyPropertyChanged
    {
        private readonly MetroExplorerLocalDataSource _dataSource;
        ObservableCollection<GroupInfoList<ExplorerItem>> _explorerGroups;
        public ObservableCollection<GroupInfoList<ExplorerItem>> ExplorerGroups
        {
            get
            {
                return _explorerGroups;
            }
            set
            {
                _explorerGroups = value;
                NotifyPropertyChanged("ExplorerGroups");
            }
        }

        /// <summary>
        /// 是大方块显示，还是列表显示。如果是true，那就指大方块显示
        /// </summary>
        public static bool BigSquareMode = true;

        public PageExplorer()
        {
            InitializeComponent();
            DataContext = this;

            _dataSource = Singleton<MetroExplorerLocalDataSource>.Instance;

            Loaded += PageExplorer_Loaded;
        }

        void PageExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            

            if (BigSquareMode)
                itemGridView.ItemTemplate = Resources["Standard300x180ItemTemplate"] as DataTemplate;
            else
                itemGridView.ItemTemplate = Resources["Standard300x80ItemTemplate"] as DataTemplate;
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            _imageChangingDispatcher.Stop();
            _imageChangingDispatcher.Tick -= ImageChangingDispatcher_Tick;
            _imageChangingDispatcher = null;
            if (e != null && e.Parameter != null && e.Parameter is string)
                await Search(e.Parameter as string);
            //_navigatorStorageFolders = null;
            //_currentStorageFolder = null;
            //ExplorerGroups = new ObservableCollection<GroupInfoList<ExplorerItem>>();
            //ExplorerGroups = null;
            GC.Collect();
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

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            EventLogger.onActionEvent(EventLogger.FOLDER_OPENED);
            ExplorerGroups = new ObservableCollection<GroupInfoList<ExplorerItem>>
                {
                    new GroupInfoList<ExplorerItem>
                        {
                            Key = StringResources.ResourceLoader.GetString("MainExplorer_UserFolderGroupTitle")
                        },
                    new GroupInfoList<ExplorerItem>
                        {
                            Key = StringResources.ResourceLoader.GetString("MainExplorer_UserFileGroupTitle")
                        }
                };
            //_navigatorStorageFolders = (IList<StorageFolder>)e.Parameter;
            //_currentStorageFolder = _navigatorStorageFolders.LastOrDefault();

            ChangeTheme(Theme.ThemeLibarary.CurrentTheme);
            await RefreshLocalFiles();
            GroupedItemsViewSource.Source = ExplorerGroups;

            InitializeChangingDispatcher();
        }

        private async Task RefreshLocalFiles()
        {
            //if (_currentStorageFolder != null)
            if (_dataSource.CurrentStorageFolder != null)
            {
                // TODO: 添加try catch是避免用户在加载过程中连续两次刷新页面造成的ExplorerGroups清零混乱
                try
                {
                    await InitializeNavigator();
                    //var listFiles = await _currentStorageFolder.GetItemsAsync();
                    if (_dataSource.FromSearch)
                    {
                        ExplorerGroups = _dataSource.SearchedItems;
                        _dataSource.FromSearch = false;
                        _dataSource.SearchedItems = null;
                    }
                    else
                    {
                        var listFiles = await _dataSource.CurrentStorageFolder.GetItemsAsync();
                        foreach (var item in listFiles)
                        {
                            if (item is StorageFolder)
                            {
                                ExplorerGroups[0].AddItem(item);
                            }
                            else if (item is StorageFile)
                            {
                                ExplorerGroups[1].AddItem(item);
                            }
                        }
                    }
                    //SortItems(PageExplorer.CurrentFileListSortType);
                }
                catch { }
            }
        }

        private async Task InitializeNavigator()
        {
            List<List<string>> itemListArray = new List<List<string>>();
            foreach (StorageFolder storageFolder in _dataSource.NavigatorStorageFolders)//_navigatorStorageFolders)
            {
                var items = await storageFolder.GetItemsAsync();
                List<string> folderNames = items.OfType<StorageFolder>().Select(item => item.Name).ToList();

                itemListArray.Add(folderNames);
            }
            Navigator.ItemListArray = itemListArray.ToArray();
            Navigator.Path = _dataSource.GetPath();
        }

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
            var itemsFilter = items.Where(item => item.Name.ToLower().Contains(query));
            int count = 0;
            foreach (var item in itemsFilter)
            {
                if (item is StorageFolder)
                    explorerGroups[0].AddItem(item);
                else if (item is StorageFile)
                    explorerGroups[1].AddItem(item);
                count++;
            }

            if (explorerGroups.Count > 0)
            {
                _dataSource.FromSearch = true;
                _dataSource.SearchedItems = explorerGroups;
            }
        }

        private async void NavigatorPathChanged(object sender, NavigatorNodeCommandArgument e)
        {
            if (e.CommandType == NavigatorNodeCommandType.None) return;

            _dataSource.CutNavigatorFromIndex(e.Index);
            if (e.CommandType == NavigatorNodeCommandType.Reduce)
            {
                _imageChangingDispatcher.Stop();
                Frame.Navigate(typeof(PageExplorer), null);
            }
            else if (e.CommandType == NavigatorNodeCommandType.Change)
            {
                StorageFolder lastStorageFolder = _dataSource.CurrentStorageFolder;
                if (lastStorageFolder != null)
                {
                    var results = await lastStorageFolder.GetItemsAsync();
                    string changedNode = e.Path.Split('\\').LastOrDefault();
                    foreach (var item in results)
                    {
                        if (item is StorageFolder && item.Name == changedNode)
                        {
                            StorageFolder storageFolder = (StorageFolder)item;
                            _dataSource.NavigatorStorageFolders.Add(storageFolder);
                            break;
                        }
                    }
                }
                _imageChangingDispatcher.Stop();
                Frame.Navigate(typeof(PageExplorer), null);
            }
        }

        private void ButtonMainPage_Click_1(object sender, RoutedEventArgs e)
        {
            _dataSource.NavigatorStorageFolders = new List<StorageFolder>();
            _imageChangingDispatcher.Stop();
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


    /// <summary>
    /// Properties for change theme color
    /// </summary>
    public sealed partial class PageExplorer
    {
        private string _backgroundColor = Theme.ThemeLibarary.BackgroundColor;
        public string BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                NotifyPropertyChanged("BackgroundColor");
            }
        }

        private string _bottomBarBackground = Theme.ThemeLibarary.BottomBarBackground;
        public string BottomBarBackground
        {
            get
            {
                return _bottomBarBackground;
            }
            set
            {
                _bottomBarBackground = value;
                NotifyPropertyChanged("BottomBarBackground");
            }
        }

        private string _titleForeground = Theme.ThemeLibarary.TitleForeground;
        public string TitleForeground
        {
            get
            {
                return _titleForeground;
            }
            set
            {
                _titleForeground = value;
                NotifyPropertyChanged("TitleForeground");
            }
        }

        private string _itemBackground = Theme.ThemeLibarary.ItemBackground;
        public string ItemBackground
        {
            get
            {
                return _itemBackground;
            }
            set
            {
                _itemBackground = value;
                NotifyPropertyChanged("ItemBackground");
            }
        }

        private string _itemSmallBackground = Theme.ThemeLibarary.ItemSmallBackground;
        public string ItemSmallBackground
        {
            get
            {
                return _itemSmallBackground;
            }
            set
            {
                _itemSmallBackground = value;
                NotifyPropertyChanged("ItemSmallBackground");
            }
        }

        private string _itemSelectedBorderColor = Theme.ThemeLibarary.ItemSelectedBorderColor;
        public string ItemSelectedBorderColor
        {
            get
            {
                return _itemSelectedBorderColor;
            }
            set
            {
                _itemSelectedBorderColor = value;
                NotifyPropertyChanged("ItemSelectedBorderColor");
            }
        }

        private string _itemTextForeground = Theme.ThemeLibarary.ItemSelectedBorderColor;
        public string ItemTextForeground
        {
            get
            {
                return _itemTextForeground;
            }
            set
            {
                _itemTextForeground = value;
                NotifyPropertyChanged("ItemTextForeground");
            }
        }

        private string _itemBigBackground = Theme.ThemeLibarary.ItemBigBackground;
        public string ItemBigBackground
        {
            get
            {
                return _itemBigBackground;
            }
            set
            {
                _itemBigBackground = value;
                NotifyPropertyChanged("ItemBigBackground");
            }
        }

        private void ChangeTheme(Theme.Themes themeYouWant)
        {
            Theme.ThemeLibarary.ChangeTheme(themeYouWant);
            BackgroundColor = Theme.ThemeLibarary.BackgroundColor;
            BottomBarBackground = Theme.ThemeLibarary.BottomBarBackground;
            TitleForeground = Theme.ThemeLibarary.TitleForeground;
            ItemBackground = Theme.ThemeLibarary.ItemBackground;
            ItemSmallBackground = Theme.ThemeLibarary.ItemSmallBackground;
            ItemSelectedBorderColor = Theme.ThemeLibarary.ItemSelectedBorderColor;
            ItemTextForeground = Theme.ThemeLibarary.ItemTextForeground;
            ItemBigBackground = Theme.ThemeLibarary.ItemBigBackground;
        }
    }


    #region value converter
    public class RenameBoxVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() == "Collapsed" ? "Visible" : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    #region FolderFaceConverter, because in metro sdk, there is no imultivalueconverter, so i have to use two converter to acheive my object
    /// <summary>
    /// For item who are not folder
    /// </summary>
    public class FolderFace1Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as ExplorerItemType? == ExplorerItemType.Folder ? "Collapsed" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// For item who are folder
    /// </summary>
    public class FolderFace2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as ExplorerItemType? == ExplorerItemType.Folder ? "Visible" : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
    #endregion

    public class FileSizeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
                return Math.Round(System.Convert.ToDouble(value) / 1024, 2).ToString() + " MB";
            return "0 MB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
    #endregion
}
