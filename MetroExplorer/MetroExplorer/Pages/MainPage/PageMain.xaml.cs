namespace MetroExplorer.Pages.MainPage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Linq;
    using Windows.ApplicationModel.DataTransfer.ShareTarget;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Foundation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.Storage;
    using Windows.Storage.AccessCache;
    using Windows.Storage.FileProperties;
    using Windows.Storage.Pickers;
    using Common;
    using Core;
    using Core.Objects;
    using Core.Utils;
    using ExplorerPage;

    public sealed partial class PageMain : LayoutAwarePage, INotifyPropertyChanged
    {
        ObservableCollection<GroupInfoList<HomeItem>> explorerGroups;
        public ObservableCollection<GroupInfoList<HomeItem>> ExplorerGroups
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

        Dictionary<HomeItem, string> _dicItemToken = new Dictionary<HomeItem, string>();

        DispatcherTimer _folderImageChangeDispatcher;

        public PageMain()
        {
            InitializeComponent();
            DataContext = this;
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            ExplorerGroups = new ObservableCollection<GroupInfoList<HomeItem>>();
            ExplorerGroups.Add(new GroupInfoList<HomeItem>() { Key = StringResources.ResourceLoader.GetString("MainPage_UserFolderGroupTitle") });
            ExplorerGroups.Add(new GroupInfoList<HomeItem>() { Key = StringResources.ResourceLoader.GetString("MainPage_SystemFolderGroupTitle") });
            Loaded += PageMain_Loaded;
        }

        protected override void SaveState(Dictionary<string, object> pageState)
        {
            _folderImageChangeDispatcher.Stop();
        }

        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (navigationParameter is ShareOperation)
            {
                ShareOperation shareOperation = (ShareOperation)navigationParameter;
                if (shareOperation.Data.Contains(StandardDataFormats.StorageItems))
                    DataSource.ShareStorageItems = await shareOperation.Data.GetStorageItemsAsync();
            }
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        async void PageMain_Loaded(object sender, RoutedEventArgs e)
        {
            InitTheme();
            if (_folderImageChangeDispatcher != null)
            {
                RefreshExporerGroups1();
                _folderImageChangeDispatcher.Start();
                LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return;
            }
            InitializeSystemFolders();
            await InitializeUsersFolders();

            groupedItemsViewSource.Source = ExplorerGroups;
            BottomAppBar.IsOpen = true;

            _folderImageChangeDispatcher = new DispatcherTimer();
            _folderImageChangeDispatcher.Tick += FolderImageChangeDispatcher_Tick;
            _folderImageChangeDispatcher.Interval = new TimeSpan(0, 0, 0, 1, 500);
            _folderImageChangeDispatcher.Start();
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        async void FolderImageChangeDispatcher_Tick(object sender, object e)
        {
            try
            {
                if (ExplorerGroups != null && ExplorerGroups[1] != null && ExplorerGroups[1].Count > 0)
                {
                    var rad = new Random();
                    await GetSubImage(ExplorerGroups[1].ElementAt(rad.Next(0, ExplorerGroups[1].Count)));
                }
            }
            catch
            { }
        }

        private async Task ThumbnailPhoto(ExplorerItem item, StorageFile sf)
        {
            if (item == null) return;
            if (!sf.Name.ToUpper().EndsWith(".PNG") && !sf.Name.ToUpper().EndsWith(".JPG") && !sf.Name.ToUpper().EndsWith(".JPEG") &&
               !sf.Name.ToUpper().EndsWith(".BMP") && !sf.Name.ToUpper().EndsWith(".RMVB") && !sf.Name.ToUpper().EndsWith(".MP4") &&
               !sf.Name.ToUpper().EndsWith(".PNG")) return;
            StorageItemThumbnail fileThumbnail = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 280);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            item.Image = bitmapImage;
            item.Image = null;
        }

        private async Task InitializeUsersFolders()
        {
            if (Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList != null && Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries.Count > 0)
            {
                var lostTokens = new List<string>(); // TODO: 避免有些已经不用的token仍然存在MostRecentlyUsedList中
                foreach (var item in Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries)
                {
                    try
                    {
                        var retrievedItem = await StorageApplicationPermissions.FutureAccessList.GetItemAsync(item.Token);
                        StorageFolder retrievedFolder = retrievedItem as StorageFolder;
                        if (retrievedFolder.Name.Contains(":\\") || retrievedFolder.Name == "Documents")
                        {
                            AddNewItem(ExplorerGroups[0], retrievedFolder, item.Token);
                        }
                        else
                            await AddAUserFolder(item, retrievedFolder);
                    }
                    catch  // 出现异常。可能是因为用户修改了某个文件夹的信息
                    {
                        lostTokens.Add(item.Token);
                    }
                }
                foreach (var token in lostTokens)
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(token);
            }
        }

        private async Task AddAUserFolder(AccessListEntry item, StorageFolder retrievedFolder)
        {
            var folderItem = AddNewItem(ExplorerGroups[1], retrievedFolder, item.Token);
            await GetSubImage(folderItem);
        }

        private async Task GetSubImage(HomeItem folderItem)
        {
            if (folderItem.StorageFolder == null) return;
            var files = (await folderItem.StorageFolder.GetFilesAsync());
            if (files != null && files.Count > 0)
            {
                if (files.Any(p => p.Name.ToUpper().EndsWith(".PNG") || p.Name.ToUpper().EndsWith(".JPG") ||
                             p.Name.ToUpper().EndsWith(".JPEG") || p.Name.ToUpper().EndsWith(".BMP") ||
                             p.Name.ToUpper().EndsWith(".RMVB") || p.Name.ToUpper().EndsWith(".MP4")))
                {
                    var files2 = files.Where(p => (p.Name.ToUpper().EndsWith(".PNG") || p.Name.ToUpper().EndsWith(".JPG") ||
                                     p.Name.ToUpper().EndsWith(".JPEG") || p.Name.ToUpper().EndsWith(".BMP") ||
                                     p.Name.ToUpper().EndsWith(".RMVB") || p.Name.ToUpper().EndsWith(".MP4")) &&
                                     p.Name != folderItem.SubImageName);
                    if (files2 != null && files2.Count() > 0)
                    {
                        var file = files2.First();
                        StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 280);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(fileThumbnail);
                        folderItem.Image = bitmapImage;
                        folderItem.SubImageName = file.Name;
                    }
                }
            }
        }

        private void AddDefaultDiskC()
        {
            HomeItem diskC = new HomeItem()
            {
                Name = "C:\\",
                Path = "",
                StorageFolder = null,
            };
            ExplorerGroups[0].Add(diskC);
            _dicItemToken.Add(diskC, diskC.Name);
        }

        private HomeItem AddNewItem(GroupInfoList<HomeItem> itemList, StorageFolder retrievedFolder, string token)
        {
            HomeItem item = new HomeItem()
            {
                Name = retrievedFolder.Name,
                Path = retrievedFolder.Path,
                StorageFolder = retrievedFolder,
            };
            if (item.Name.Contains(":\\"))
            {
                item.DefautImage = GetBitMapImageFromLocalSource("Assets/Disk.png");
                item.ImageStretch = "None";
                item.IfImageChanged = "Collapsed";
            }
            else if (item.Name == "Documents")
            {
                item.DefautImage = GetBitMapImageFromLocalSource("Assets/File.png");
                item.ImageStretch = "None";
                item.IfImageChanged = "Collapsed";
            }
            else
            {
                item.DefautImage = GetBitMapImageFromLocalSource("Assets/Folder.png");
                item.ImageStretch = "UniformToFill";
            }
            itemList.Add(item);
            _dicItemToken.Add(item, token);
            return item;
        }

        private void InitializeSystemFolders()
        {
            ExplorerGroups[0].Add(new HomeItem()
            {
                Name = KnownFolders.PicturesLibrary.Name,
                Path = KnownFolders.PicturesLibrary.Path,
                StorageFolder = KnownFolders.PicturesLibrary,
                DefautImage = GetBitMapImageFromLocalSource("Assets/Photo.png"),
                ImageStretch = "None",
                IfImageChanged = "Collapsed"
            });
            ExplorerGroups[0].Add(new HomeItem()
            {
                Name = KnownFolders.MusicLibrary.Name,
                Path = KnownFolders.MusicLibrary.Path,
                StorageFolder = KnownFolders.MusicLibrary,
                DefautImage = GetBitMapImageFromLocalSource("Assets/Music.png"),
                ImageStretch = "None",
                IfImageChanged = "Collapsed"
            });
            ExplorerGroups[0].Add(new HomeItem()
            {
                Name = KnownFolders.VideosLibrary.Name,
                Path = KnownFolders.VideosLibrary.Path,
                StorageFolder = KnownFolders.VideosLibrary,
                DefautImage = GetBitMapImageFromLocalSource("Assets/Video.png"),
                ImageStretch = "None",
                IfImageChanged = "Collapsed"
            });
            InitAddNewFolderItem();
        }

        private BitmapImage GetBitMapImageFromLocalSource(string url)
        {
            var result = new BitmapImage(new Uri("ms-appx:/" + url));
            //result.UriSource = uri;
            return result;
        }

        private async Task AddNewFolder()
        {
            StorageFolder storageFolder = await GetStorageFolderFromFolderPicker();
            if (storageFolder == null)
            {
                EventLogger.onActionEvent(EventLogger.ADD_FOLDER_CANCEL, EventLogger.LABEL_HOME_PAGE);
                return;
            }
            foreach (var key in _dicItemToken.Keys)
            {
                if (key.Path == storageFolder.Path)
                    return;
            }
            AddNewFolder2(storageFolder);
            EventLogger.onActionEvent(EventLogger.ADD_FOLDER_DONE, EventLogger.LABEL_HOME_PAGE);
        }

        private async void AddNewFolder2(StorageFolder storageFolder)
        {
            foreach (var item in _dicItemToken)
            {
                if (item.Key.StorageFolder == storageFolder)
                    return;
            }
            string token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFolder, storageFolder.Name);
            if (storageFolder.Name.Contains(":\\") || storageFolder.Name == "Documents")
            {
                AddNewItem(ExplorerGroups[0], storageFolder, token);
            }
            else
            {
                var item = AddNewItem(ExplorerGroups[1], storageFolder, token);
                await GetSubImage(item);
            }
        }

        private static async Task<StorageFolder> GetStorageFolderFromFolderPicker()
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
                if (!_dicItemToken.ContainsKey((itemGridView.SelectedItems[0] as HomeItem)))
                {
                    if (itemGridView.SelectedItems.Count == 1)
                        break;
                    continue;
                }
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(_dicItemToken[(itemGridView.SelectedItems[0] as HomeItem)]);
                if (ExplorerGroups[0].Contains(itemGridView.SelectedItems[0] as HomeItem))
                {
                    _dicItemToken.Remove(itemGridView.SelectedItems[0] as HomeItem);
                    ExplorerGroups[0].Remove(itemGridView.SelectedItems[0] as HomeItem);
                }
                else if (ExplorerGroups[1].Contains(itemGridView.SelectedItems[0] as HomeItem))
                {
                    _dicItemToken.Remove(itemGridView.SelectedItems[0] as HomeItem);
                    ExplorerGroups[1].Remove(itemGridView.SelectedItems[0] as HomeItem);
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

        private async void ItemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            HomeItem item = e.ClickedItem as HomeItem;
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

        private async Task ClickedOnUndefinedDiskCItem(HomeItem item)
        {
            var storageFolder = await GetStorageFolderFromFolderPicker();
            if (storageFolder != null && storageFolder.Name == item.Name)
            {
                item.Path = storageFolder.Path;
                item.StorageFolder = storageFolder;
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(item.StorageFolder, item.Name);
                NavigateToExplorer(item);
            }
            else if (storageFolder != null)
            {
                AddNewFolder2(storageFolder);
            }
        }

        private void NavigateToExplorer(HomeItem item)
        {
            Singleton<MetroExplorerLocalDataSource>.Instance.NavigatorStorageFolders =
                new List<StorageFolder>
                {
                    item.StorageFolder
                };
            Frame.Navigate(typeof(PageExplorer), null);
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

        private void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshExporerGroups1();
        }

        private async void RefreshExporerGroups1()
        {
            var lostTokens = new List<string>(); // TODO: 避免有些已经不用的token仍然存在MostRecentlyUsedList中
            var availableStorages = new Dictionary<StorageFolder, AccessListEntry>();
            // 获取当前所有的可用快捷方式文件夹的信息
            if (Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList != null && Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries.Count > 0)
            {
                foreach (var item in Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries)
                {
                    try
                    {
                        var retrievedItem = await StorageApplicationPermissions.FutureAccessList.GetItemAsync(item.Token);
                        StorageFolder retrievedFolder = retrievedItem as StorageFolder;
                        if (!(retrievedFolder.Name.Contains(":\\") || retrievedFolder.Name == "Documents"))
                            availableStorages.Add(retrievedFolder, item);
                    }
                    catch  // 出现异常。可能是因为用户修改了某个文件夹的信息
                    {
                        lostTokens.Add(item.Token);
                    }
                }
                foreach (var token in lostTokens)
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(token);
            }
            else return;
            // 检查主页已失效的文件夹，并将其从列表中删除
            if (ExplorerGroups[1].Count(p => availableStorages.All(pp => pp.Key.Path != p.Path)) > 0)
            {
                var notInAvailableListItems = new List<HomeItem>();
                foreach (var item in ExplorerGroups[1].Where(p => availableStorages.All(pp => pp.Key.Path != p.Path)))
                {
                    _dicItemToken.Remove(item);
                    notInAvailableListItems.Add(item);
                }
                foreach (var item in notInAvailableListItems)
                    ExplorerGroups[1].Remove(item);
            }
            // 重新添加改变后的文件夹

            if (availableStorages.Count(p => ExplorerGroups[1].All(pp => pp.Path != p.Key.Path)) > 0)
            {
                foreach (var item in availableStorages.Where(p => ExplorerGroups[1].All(pp => pp.Path != p.Key.Path)))
                {
                    await AddAUserFolder(item.Value, item.Key);
                }
            }

            InitAddNewFolderItem();

        }

        private void InitAddNewFolderItem()
        {
            if (ExplorerGroups[1].All(p => p.Path != StringResources.ResourceLoader.GetString("String_AddNewShortCutFolder")))
            {
                ExplorerGroups[1].Insert(0, new HomeItem()
                {
                    Name = StringResources.ResourceLoader.GetString("String_AddNewShortCutFolder"),
                    Path = StringResources.ResourceLoader.GetString("String_AddNewShortCutFolder"),
                    StorageFolder = null,
                    DefautImage = GetBitMapImageFromLocalSource("Assets/AddNewFolder.png"),
                    ImageStretch = "None",
                    IfImageChanged = "Collapsed"
                });
            }
        }
    }


    /// <summary>
    /// Properties for change theme color
    /// </summary>
    public sealed partial class PageMain : INotifyPropertyChanged
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

        private void InitTheme()
        {
            Theme.ThemeLibarary.ChangeTheme();
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
}
