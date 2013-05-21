namespace MetroExplorer
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
    using Windows.Storage.FileProperties;
    using Windows.UI.Xaml.Media.Imaging;
    using MetroExplorer.Common;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Storage.Streams;

    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>
    public sealed partial class PageExplorer : LayoutAwarePage, INotifyPropertyChanged
    {
        private readonly MetroExplorerLocalDataSource _dataSource;
        public static List<string> CurrentItems;

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

        public static Uri BaseUriStatic { get; set; }

        public PageExplorer()
        {
            InitializeComponent();
            DataContext = this;
            //this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            _dataSource = Singleton<MetroExplorerLocalDataSource>.Instance;
            Loaded += PageExplorer_Loaded;

            BaseUriStatic = this.BaseUri;
        }

        async void PageExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            if (await UserPreferenceRecord.GetInstance().IfListMode())
                BigSquareMode = false;
            else
                BigSquareMode = true;
            if (BigSquareMode)
                itemGridView.ItemTemplate = Resources["Standard300x180ItemTemplate"] as DataTemplate;
            else
                itemGridView.ItemTemplate = Resources["Standard300x80ItemTemplate"] as DataTemplate;
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            string args = navigationParameter as string;
            if (args != null)
            {
                _imageChangingDispatcher.Stop();
                _imageChangingDispatcher.Tick -= ImageChangingDispatcher_Tick;
                _imageChangingDispatcher = null;
                var currentContent = Window.Current.Content;
                var frame = currentContent as Frame;

                App.LastQuery = App.CurrentQuery = string.Empty;

                if (args != null && args is string)
                    await Search(args as string);
                GC.Collect();
            }
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += PageExplorerDataRequested;

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

            FolderNameTextBlock.Text = _dataSource.CurrentStorageFolder.Name;
            ChangeTheme(Theme.ThemeLibarary.CurrentTheme);
            await RefreshLocalFiles();
            GroupedItemsViewSource.Source = ExplorerGroups;

            InitializeChangingDispatcher();

        }

        protected override void SaveState(Dictionary<string, object> pageState)
        {
            base.SaveState(pageState);
            DataTransferManager.GetForCurrentView().DataRequested -= PageExplorerDataRequested;
        }

        void PageExplorerDataRequested(DataTransferManager sender,
            DataRequestedEventArgs args)
        {
            try
            {
                var request = args.Request;
                foreach (ExplorerItem item in itemGridView.SelectedItems)
                {
                    if (item.Type == ExplorerItemType.File)
                    {
                        var reference = RandomAccessStreamReference.CreateFromUri(new Uri(item.Path));
                        if (reference != null)
                        {
                            request.Data.Properties.Title = item.Name;
                            request.Data.Properties.Thumbnail = reference;
                        }
                    }
                }

                var recipe = "\r\nTest\r\n";
                recipe += ("\r\n\r\nTest\r\n");
                request.Data.SetText(recipe);
            }
            catch (Exception)
            {
                DataTransferManager.GetForCurrentView().DataRequested -= PageExplorerDataRequested;
            }
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
                    if (!_dataSource.FromSearch)
                    {
                        var listFiles = await _dataSource.CurrentStorageFolder.GetItemsAsync();
                        foreach (var item in listFiles)
                        {
                            if (item is StorageFile)
                            {
                                if (ExplorerGroups[1].All(p => p.Name != item.Name))
                                    ExplorerGroups[1].AddFileItem(item as StorageFile);
                            }
                            else if (item is StorageFolder)
                            {
                                if (ExplorerGroups[0].All(p => p.Name != item.Name))
                                    ExplorerGroups[0].AddStorageItem(item as StorageFolder);
                            }
                        }
                    }
                    else
                    {
                        ExplorerGroups = _dataSource.SearchedItems;
                        _dataSource.FromSearch = false;
                        _dataSource.SearchedItems = null;
                    }
                    _loadingFileSizeCount = 0;
                    _loadingImageCount = 0;
                }
                catch
                { }
            }
        }

        private async Task InitializeNavigator()
        {
            List<List<string>> itemListArray = new List<List<string>>();
            foreach (StorageFolder storageFolder in _dataSource.NavigatorStorageFolders)
            {
                var items = await storageFolder.GetItemsAsync();
                List<string> folderNames = items.OfType<StorageFolder>().Select(item => item.Name).ToList();

                itemListArray.Add(folderNames);
            }
            Navigator.ItemListArray = itemListArray.ToArray();
            Navigator.Path = _dataSource.GetPath();

            var currentItems = await _dataSource.CurrentStorageFolder.GetItemsAsync();
            CurrentItems = currentItems.Select(item => item.Name).ToList();
        }

        private async void NavigatorPathChanged(object sender, NavigatorNodeCommandArgument e)
        {
            if (e.CommandType == NavigatorNodeCommandType.None) return;

            _dataSource.CutNavigatorFromIndex(e.Index);
            if (e.CommandType == NavigatorNodeCommandType.Reduce)
            {
                if (_imageChangingDispatcher != null)
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
            CurrentItems = null;
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
            {
                if (System.Convert.ToDouble(value) < 1024 * 1024)
                    return Math.Round(System.Convert.ToDouble(value) / 1024, 2).ToString() + " KB";
                else if (System.Convert.ToDouble(value) <= 1024 * 1024 * 1024)
                    return Math.Round(System.Convert.ToDouble(value) / (1024 * 1024), 2).ToString() + " MB";
                else if (System.Convert.ToDouble(value) > 1024 * 1024 * 1024)
                    return Math.Round(System.Convert.ToDouble(value) / (1024 * 1024 * 1024), 2).ToString() + " GB";
            }
            return "0 KB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class StorageFileToConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage resultBitMap = null;
            if (value != null)
            {
                if (value is StorageFile)
                {
                    if ((value as StorageFile).Name.ToUpper().EndsWith(".PNG") || (value as StorageFile).Name.ToUpper().EndsWith(".JPG") ||
                       (value as StorageFile).Name.ToUpper().EndsWith(".JEPG") || (value as StorageFile).Name.ToUpper().EndsWith(".BMP"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.image.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".MP3") || (value as StorageFile).Name.ToUpper().EndsWith(".WMA") ||
                            (value as StorageFile).Name.ToUpper().EndsWith(".WAV") || (value as StorageFile).Name.ToUpper().EndsWith(".WM"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.music.png"));
                    else if((value as StorageFile).Name.ToUpper().EndsWith(".GIF"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.page.gif.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".MP4") || (value as StorageFile).Name.ToUpper().EndsWith(".RMVB") ||
                       (value as StorageFile).Name.ToUpper().EndsWith(".MKV") || (value as StorageFile).Name.ToUpper().EndsWith(".BMP"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.film.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".XLS") || (value as StorageFile).Name.ToUpper().EndsWith(".XLSX"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.page.excel.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".PDF"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.page.pdf.text.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".DOCX") || (value as StorageFile).Name.ToUpper().EndsWith(".DOC"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.page.word.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".PPT") || (value as StorageFile).Name.ToUpper().EndsWith(".PPTX"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.page.powerpoint.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".TXT"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.page.txt.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".XML") || (value as StorageFile).Name.ToUpper().EndsWith(".HTML") ||
                             (value as StorageFile).Name.ToUpper().EndsWith(".XAML"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.page.xml.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".CS"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.language.csharp.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".JAVA"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.language.java.text.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".EXE"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.os.windows.8.png"));
                    else if ((value as StorageFile).Name.ToUpper().EndsWith(".DLL"))
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.infinite.png"));
                    else
                        resultBitMap = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.page.text.png"));
                    return resultBitMap;
                }
            }
            var result2 = new BitmapImage(new Uri(PageExplorer.BaseUriStatic, @"Assets/FilesIcon/appbar.folder.png"));
            return result2;
        }

        public async void GetStorageItemThumbnail(StorageFile sf, StorageItemThumbnail result)
        {
            result = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 250);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
    #endregion
}
