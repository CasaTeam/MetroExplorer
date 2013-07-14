namespace MetroExplorer.Pages.ExplorerPage
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
    using Windows.Storage.FileProperties;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Storage.Streams;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Search;
    using Windows.System;
    using Common;
    using Components.Navigator.Objects;
    using Core;
    using Core.Objects;
    using Core.Utils;
    using UserPreferenceRecord;

    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>
    public sealed partial class PageExplorer : LayoutAwarePage, INotifyPropertyChanged
    {
        public static List<string> CurrentItems;

        private ObservableCollection<ExplorerItem> _explorerItems;
        public ObservableCollection<ExplorerItem> ExplorerItems
        {
            get
            {
                return _explorerItems;
            }
            set
            {
                _explorerItems = value;
                NotifyPropertyChanged("ExplorerItems");
            }
        }

        /// <summary>
        /// 是大方块显示，还是列表显示。如果是true，那就指大方块显示
        /// </summary>
        public static bool BigSquareMode = true;

        public PageExplorer()
        {
            InitializeComponent();
            InitTheme();
            ExplorerItems = new ObservableCollection<ExplorerItem>();
            _searchPane = SearchPane.GetForCurrentView();
            DataContext = this;
            Loaded += PageExplorer_Loaded;
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

        protected async override void LoadState(
            Object navigationParameter,
            Dictionary<String, Object> pageState)
        {
            EventLogger.onActionEvent(EventLogger.FOLDER_OPENED);

            await InitializeSearch(navigationParameter);
            InitializeShare();

            FolderNameTextBlock.Text = DataSource.CurrentStorageFolder.Name;
            await RefreshLocalFiles();
            ItemsViewSource.Source = ExplorerItems;

            InitializeChangingDispatcher();
            GC.Collect();
        }

        protected override void SaveState(
            Dictionary<string, object> pageState)
        {
            if (ExplorerItems != null && ExplorerItems.Count > 0)
            {
                foreach (var item in ExplorerItems)
                {
                    item.Image = null;
                }
                ExplorerItems.Clear();
                ExplorerItems = null;
                itemGridView.ItemsSource = null;
                itemListView.ItemsSource = null;
            }
            DataTransferManager.GetForCurrentView().DataRequested -= PageExplorerDataRequested;
            GC.Collect();
        }

        private async Task RefreshLocalFiles()
        {
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            ExplorerItems.Clear();
            //if (_currentStorageFolder != null)
            if (DataSource.CurrentStorageFolder != null)
            {
                // TODO: 添加try catch是避免用户在加载过程中连续两次刷新页面造成的ExplorerGroups清零混乱
                try
                {
                    await InitializeNavigator();
                    //var listFiles = await _currentStorageFolder.GetItemsAsync();
                    if (!DataSource.FromSearch)
                    {
                        var listFiles = await DataSource.CurrentStorageFolder.GetItemsAsync();
                        foreach (var item in listFiles)
                        {
                            if (item is StorageFile)
                            {
                                ExplorerItems.AddFileItem(item as StorageFile);
                            }
                            else if (item is StorageFolder)
                            {
                                ExplorerItems.AddStorageItem(item as StorageFolder);
                            }
                        }
                    }
                    else
                    {
                        ExplorerItems = DataSource.SearchedItems;
                        DataSource.FromSearch = false;
                        DataSource.SearchedItems = null;
                    }
                    _counterForLoadUnloadedItems = 0;
                }
                catch
                { }
            }
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        /// <summary>
        /// 点击某个文件夹或文件后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ItemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ExplorerItem item = e.ClickedItem as ExplorerItem;
            if (item.Type == ExplorerItemType.Folder)
            {
                DataSource.NavigatorStorageFolders.Add(item.StorageFolder);
                Frame.Navigate(typeof(PageExplorer), null);
            }
            else if (item.Type == ExplorerItemType.File)
            {
                //if (item.StorageFile != null && item.StorageFile.IsImageFile())
                //{
                //    var parameters = ExplorerItems.Where(p => p.StorageFile != null &&
                //                                             (p.StorageFile.FileType.ToUpper().Equals(".JPG") ||
                //                                              p.StorageFile.FileType.ToUpper().Equals(".JPEG") ||
                //                                              p.StorageFile.FileType.ToUpper().Equals(".PNG") ||
                //                                              p.StorageFile.FileType.ToUpper().Equals(".BMP"))).ToList();
                //    parameters.Remove(item);
                //    parameters.Insert(0, item);
                //    Frame.Navigate(typeof(PhotoGallery), parameters);
                //}
                //else
                //{
                var file = await StorageFile.GetFileFromPathAsync(item.Path);
                await file.OpenAsync(FileAccessMode.Read);
                EventLogger.onActionEvent(EventLogger.FILE_OPENED);
                await Launcher.LaunchFileAsync(file, new LauncherOptions { DisplayApplicationPicker = true });
                //}
            }
        }

        #region NotifyPropertyChanged
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
                if (System.Convert.ToDouble(value) == 0)
                    return "";
                else if (System.Convert.ToDouble(value) < 1024 * 1024)
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
    #endregion
}
