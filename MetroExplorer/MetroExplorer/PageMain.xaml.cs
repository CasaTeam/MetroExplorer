namespace MetroExplorer
{
    using System;
    using System.Collections.Generic;
    using Windows.Foundation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using System.ComponentModel;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    
    using Windows.Storage.AccessCache;
    using System.Collections.ObjectModel;
    using Common;
    using core;
    using core.Objects;
    using core.Utils;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.Storage.FileProperties;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Data;

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

        Dictionary<ExplorerItem, string> _dicItemToken = new Dictionary<ExplorerItem, string>();

        DispatcherTimer _folderImageChangeDispatcher = new DispatcherTimer();

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
            ChangeTheme(Theme.ThemeLibarary.CurrentTheme);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            
        }


        async void PageMain_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeSystemFolders();
            await initializeUsersFolders();

            BottomAppBar.IsOpen = true;

            _folderImageChangeDispatcher.Tick += FolderImageChangeDispatcher_Tick;
            _folderImageChangeDispatcher.Interval = new TimeSpan(0, 0, 0, 1, 500);
            _folderImageChangeDispatcher.Start();
        }

        int _lastChangedFolder = 0;
        async void FolderImageChangeDispatcher_Tick(object sender, object e)
        {
            try
            {
                if (ExplorerGroups == null || ExplorerGroups[1] == null || ExplorerGroups[1].Count == 0)
                    return;
                if (_lastChangedFolder == explorerGroups[1].Count)
                    _lastChangedFolder = 0;
                _lastChangedFolder++;
                var exploreItem = explorerGroups[1][_lastChangedFolder];
                if(exploreItem.StorageFolder == null) return;
                var files = await exploreItem.StorageFolder.GetFilesAsync();

                if (exploreItem.LastImageIndex == -2) return;
                if (exploreItem.LastImageIndex == -1)
                    foreach (var file in files)
                    {
                        exploreItem.LastImageIndex = -2;
                        if (file.Name.ToUpper().EndsWith(".PNG") || file.Name.ToUpper().EndsWith(".JPG") || file.Name.ToUpper().EndsWith(".JPEG") ||
                            file.Name.ToUpper().EndsWith(".BMP") || file.Name.ToUpper().EndsWith(".RMVB") || file.Name.ToUpper().EndsWith(".MP4") ||
                            file.Name.ToUpper().EndsWith(".MKV") || file.Name.ToUpper().EndsWith(".PNG"))
                        {
                            exploreItem.LastImageName.Add(file.Name);
                            exploreItem.LastImageIndex = 0;
                        }
                    }
                
                if (exploreItem.LastImageIndex == exploreItem.LastImageName.Count - 1)
                    exploreItem.LastImageIndex = 0;
                await ThumbnailPhoto(exploreItem, files[exploreItem.LastImageIndex]);
                exploreItem.LastImageIndex++;
            }
            catch
            { }
        }

        private async Task ThumbnailPhoto(ExplorerItem item, StorageFile sf)
        {
            if (item == null) return;
            if (!sf.Name.ToUpper().EndsWith(".PNG") && !sf.Name.ToUpper().EndsWith(".JPG") && !sf.Name.ToUpper().EndsWith(".JPEG") &&
               !sf.Name.ToUpper().EndsWith(".BMP") && !sf.Name.ToUpper().EndsWith(".RMVB") && !sf.Name.ToUpper().EndsWith(".MP4") &&
               !sf.Name.ToUpper().EndsWith(".MKV") && !sf.Name.ToUpper().EndsWith(".PNG")) return;
            StorageItemThumbnail fileThumbnail = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 280);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            item.Image = bitmapImage;
            item.DefautImage = null;
        }

        private async System.Threading.Tasks.Task initializeUsersFolders()
        {
            if (Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList != null && Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Entries.Count > 0)
            {
                bool ifDiskCExist = false; // TODO: 确定是否C盘已经被添加
                List<string> lostTokens = new List<string>(); // TODO: 避免有些已经不用的token仍然存在MostRecentlyUsedList中
                foreach (var item in Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Entries)
                {
                    try
                    {
                        var retrievedItem = await StorageApplicationPermissions.MostRecentlyUsedList.GetItemAsync(item.Token);
                        if (retrievedItem is StorageFolder)
                        {
                            StorageFolder retrievedFolder = retrievedItem as StorageFolder;
                            if (retrievedFolder.Name.Contains(":\\"))
                            {
                                AddNewItem(ExplorerGroups[0], retrievedFolder, item.Token);
                                if (retrievedFolder.Name == "C:\\")
                                    ifDiskCExist = true;
                            }
                            else
                            {
                                AddNewItem(ExplorerGroups[1], retrievedFolder, item.Token);
                            }
                        }
                    }
                    catch
                    {
                        // 出现异常。可能是因为用户修改了某个文件夹的信息
                        lostTokens.Add(item.Token);
                    }
                }
                if (!ifDiskCExist) // 如果c盘没有被加入过，则初始默认设置
                {
                    AddDefaultDiskC();
                }
                foreach (var token in lostTokens)
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Remove(token);
            }
        }

        private void AddDefaultDiskC()
        {
            ExplorerItem diskC = new ExplorerItem()
            {
                Name = "C:\\",
                Path = "",
                StorageFolder = null,
                Type = ExplorerItemType.Folder
            };
            ExplorerGroups[0].Add(diskC);
            _dicItemToken.Add(diskC, diskC.Name);
        }

        private void AddNewItem(GroupInfoList<ExplorerItem> itemList, StorageFolder retrievedFolder, string token)
        {
            ExplorerItem item = new ExplorerItem()
            {
                Name = retrievedFolder.Name,
                Path = retrievedFolder.Path,
                StorageFolder = retrievedFolder,
                Type = ExplorerItemType.Folder
            };
            if (item.Name.Contains(":\\"))
                item.DefautImage = GetBitMapImageFromLocalSource("Assets/DiskLogo.png");
            else
                item.DefautImage = GetBitMapImageFromLocalSource("Assets/FolderLogo2.png");
            itemList.Add(item);
            _dicItemToken.Add(item, token);
        }

        private void InitializeSystemFolders()
        {
            ExplorerGroups[0].Add(new ExplorerItem()
            {
                Name = KnownFolders.PicturesLibrary.Name,
                Path = KnownFolders.PicturesLibrary.Path,
                StorageFolder = KnownFolders.PicturesLibrary,
                Type = ExplorerItemType.Folder,
                DefautImage = GetBitMapImageFromLocalSource("Assets/FolderLogo.png")
            });
            ExplorerGroups[0].Add(new ExplorerItem()
            {
                Name = KnownFolders.MusicLibrary.Name,
                Path = KnownFolders.MusicLibrary.Path,
                StorageFolder = KnownFolders.MusicLibrary,
                Type = ExplorerItemType.Folder,
                DefautImage = GetBitMapImageFromLocalSource("Assets/FolderLogo.png")
            });
            ExplorerGroups[0].Add(new ExplorerItem()
            {
                Name = KnownFolders.DocumentsLibrary.Name,
                Path = KnownFolders.DocumentsLibrary.Path,
                StorageFolder = KnownFolders.DocumentsLibrary,
                Type = ExplorerItemType.Folder,
                DefautImage = GetBitMapImageFromLocalSource("Assets/FolderLogo.png")
            });
            ExplorerGroups[0].Add(new ExplorerItem()
            {
                Name = KnownFolders.VideosLibrary.Name,
                Path = KnownFolders.VideosLibrary.Path,
                StorageFolder = KnownFolders.VideosLibrary,
                Type = ExplorerItemType.Folder,
                DefautImage = GetBitMapImageFromLocalSource("Assets/FolderLogo.png")
            });

            ExplorerGroups[1].Add(new ExplorerItem()
            {
                Name = StringResources.ResourceLoader.GetString("String_AddNewShortCutFolder"),
                Path = StringResources.ResourceLoader.GetString("String_AddNewShortCutFolder"),
                StorageFolder = null,
                Type = ExplorerItemType.Folder,
                DefautImage = GetBitMapImageFromLocalSource("Assets/FolderLogo2.png")
            });
        }

        private BitmapImage GetBitMapImageFromLocalSource(string url)
        {


            var result = new BitmapImage(new Uri(this.BaseUri, @url));
    
            //result.UriSource = uri;
            return result;
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
        protected override void LoadState(
            Object navigationParameter, 
            Dictionary<String, Object> pageState)
        {
            // TODO: assignez une collection de groupes pouvant être liés à this.DefaultViewModel["Groups"]
        }

        private void ItemGridView_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private async System.Threading.Tasks.Task AddNewFolder()
        {
            StorageFolder storageFolder = await GetStorageFolderFromFolderPicker();
            if (storageFolder == null)
            {
                EventLogger.onActionEvent(EventLogger.ADD_FOLDER_CANCEL, EventLogger.LABEL_HOME_PAGE); 
                return;
            }
            foreach (var key in _dicItemToken.Keys)
            {
                if (key.Name == storageFolder.Name)
                    return;
            }
            AddNewFolder2(storageFolder);
            EventLogger.onActionEvent(EventLogger.ADD_FOLDER_DONE, EventLogger.LABEL_HOME_PAGE);
        }

        private void AddNewFolder2(StorageFolder storageFolder)
        {
            foreach (var item in _dicItemToken)
            {
                if (item.Key.StorageFolder == storageFolder)
                    return;
            }
            string token = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(storageFolder, storageFolder.Name);
            if (storageFolder.Name.Contains(":\\"))
            {
                AddNewItem(ExplorerGroups[0], storageFolder, token);
            }
            else
                AddNewItem(ExplorerGroups[1], storageFolder, token);
        }

        private static async System.Threading.Tasks.Task<StorageFolder> GetStorageFolderFromFolderPicker()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder storageFolder = await folderPicker.PickSingleFolderAsync();
            return storageFolder;
        }

        private async void Button_AddNewDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            EventLogger.onActionEvent(EventLogger.ADD_FOLDER_CLICK, EventLogger.LABEL_HOME_PAGE);
            await AddNewFolder();
        }

        public Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        private void Button_RemoveDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItems == null || itemGridView.SelectedItems.Count == 0) return;
            while (itemGridView.SelectedItems.Count > 0)
            {
                if (!_dicItemToken.ContainsKey((itemGridView.SelectedItems[0] as ExplorerItem)))
                {
                    if (itemGridView.SelectedItems.Count == 1)
                        break;
                    continue;
                } 
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Remove(_dicItemToken[(itemGridView.SelectedItems[0] as ExplorerItem)]);
                if (ExplorerGroups[0].Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                {
                    ExplorerGroups[0].Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                }
                else if (ExplorerGroups[1].Contains(itemGridView.SelectedItems[0] as ExplorerItem))
                {
                    ExplorerGroups[1].Remove(itemGridView.SelectedItems[0] as ExplorerItem);
                }
            }
            BottomAppBar.IsOpen = false;
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

        private void ItemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (itemGridView.SelectedItems.Count > 0)
                BottomAppBar.IsOpen = true;
        }

        private async void ItemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ExplorerItem item = e.ClickedItem as ExplorerItem;
            if (item.StorageFolder != null)
            {
                NavigateToExplorer(item);
            }
            else
            {
                if (item.Path == StringResources.ResourceLoader.GetString("String_NewFolder") && item.Name == StringResources.ResourceLoader.GetString("String_NewFolder"))
                { 
                    // TODO: 添加新快捷方式文件夹
                    await AddNewFolder();
                }
                else
                {
                    await ClickedOnUndefinedDiskCItem(item);
                }
            }
        }

        private async System.Threading.Tasks.Task ClickedOnUndefinedDiskCItem(ExplorerItem item)
        {
            var storageFolder = await GetStorageFolderFromFolderPicker();
            if (storageFolder != null && storageFolder.Name == item.Name)
            {
                item.Path = storageFolder.Path;
                item.StorageFolder = storageFolder;
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(item.StorageFolder, item.Name);
                NavigateToExplorer(item);
            }
            else if (storageFolder != null)
            {
                AddNewFolder2(storageFolder);
            }
        }

        private void NavigateToExplorer(ExplorerItem item)
        {
            IList<StorageFolder> _navigatorStorageFolders = new List<StorageFolder> { item.StorageFolder };
            this.Frame.Navigate(typeof(PageExplorer), _navigatorStorageFolders);
        }

        private void AppBar_BottomAppBar_Opened_1(object sender, object e)
        {
            if (itemGridView.SelectedItems.Count > 0)
                Button_RemoveDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                Button_RemoveDiskFolder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void ExplorerItemImage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }


    /// <summary>
    /// Properties for change theme color
    /// </summary>
    public sealed partial class PageMain : LayoutAwarePage, INotifyPropertyChanged
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


    //public class ImageSourceConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        //return value.ToString() == "Collapsed" ? "Visible" : "Collapsed";
    //        return value;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        return null;
    //    }
    //}
}
