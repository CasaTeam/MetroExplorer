﻿namespace MetroExplorer
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
    using WinRTXamlToolkit.Controls.Extensions;
    using Windows.Storage.AccessCache;
    using System.Collections.ObjectModel;
    using Common;
    using core;
    using core.Objects;
    using core.Utils;

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
            ChangeTheme(Theme.ThemeLibarary.CurrentTheme);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

        }


        async void PageMain_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeSystemFolders();
            await initializeUsersFolders();

            ScrollViewer myScrollViewer = itemGridView.GetFirstDescendantOfType<ScrollViewer>();
            myScrollViewer.ViewChanged += MyScrollViewer_ViewChanged;

            BottomAppBar.IsOpen = true;
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
                            AddNewItem(ExplorerGroups[0], retrievedFolder, item.Token);
                        }
                        else
                        {
                            AddNewItem(ExplorerGroups[1], retrievedFolder, item.Token);
                        }
                    }
                }
            }
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
            itemList.Add(item);
            dicItemToken.Add(item, token);
        }

        private void InitializeSystemFolders()
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
        }

        //double _lastOffset = 0;
        //double _lastDelta = 0;
        void MyScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //double offset = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).HorizontalOffset;
            //double scroll = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).ScrollableWidth;
            //double viewportwidth = ((Windows.UI.Xaml.Controls.ScrollViewer)sender).ViewportWidth;
            //var delta = _lastDelta + (offset - _lastOffset) * 80;
            //if (Math.Abs(Image_Background.ActualWidth) > Math.Abs(offset * 90))
            //    Image_Background.Margin = new Thickness(-delta, 0, 0, 0);
            //_lastOffset = offset;
            //_lastDelta = delta;
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

        private void ItemGridView_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private async System.Threading.Tasks.Task AddNewFolder()
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
                    AddNewItem(ExplorerGroups[0], storageFolder, token);
                }
                else
                    AddNewItem(ExplorerGroups[1], storageFolder, token);
                EventLogger.onActionEvent(EventLogger.ADD_FOLDER_DONE, EventLogger.LABEL_HOME_PAGE);
            }
            else
            {
                EventLogger.onActionEvent(EventLogger.ADD_FOLDER_CANCEL, EventLogger.LABEL_HOME_PAGE);
            }
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

        private void ItemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ExplorerItem item = e.ClickedItem as ExplorerItem;
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
}
