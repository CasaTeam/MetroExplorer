namespace MetroExplorer
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.System;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Windows.Storage;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.Storage.FileProperties;
    using Core;
    using Core.Objects;
    using Core.Utils;
    using Common;
    using UserPreferenceRecord;
    using MetroExplorer.Pages.ExplorerPage;

    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>

    public sealed partial class PhotoGallery : LayoutAwarePage, INotifyPropertyChanged
    {
        public static double ActualScreenHeight = 0;
        List<ExplorerItem> _expoloreItems = new List<ExplorerItem>();

        ObservableCollection<ExplorerItem> _galleryItems = new ObservableCollection<ExplorerItem>();
        public ObservableCollection<ExplorerItem> GalleryItems
        {
            get
            {
                return _galleryItems;
            }
            set
            {
                _galleryItems = value;
                NotifyPropertyChanged("GalleryItems");
            }
        }

        private readonly MetroExplorerLocalDataSource _dataSource;

        DispatcherTimer _sliderDispatcher = new DispatcherTimer();

        public PhotoGallery()
        {
            InitializeComponent();
            DataContext = this;
            _dataSource = Singleton<MetroExplorerLocalDataSource>.Instance;
            this.Loaded += PhotoGallery_Loaded;
            this.Unloaded += PhotoGallery_Unloaded;
        }

        void PhotoGallery_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        void PhotoGallery_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _sliderDispatcher.Stop();
            _sliderDispatcher = null;
            GC.Collect();
        }

        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //FlipView flipview = (FlipView)sender;
            //ExplorerItem selected = (ExplorerItem)flipview.SelectedItem;
            //if (Photos.Count < _potentialPhotos.Count)
            //{
            //    if (Photos.Contains(_potentialPhotos[Photos.Count])) return;
            //    await PhotoThumbnail(_potentialPhotos[Photos.Count]);
            //    Photos.Add(_potentialPhotos[Photos.Count]);
            //}
        }

        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            EventLogger.onActionEvent(EventLogger.FOLDER_OPENED);
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            _expoloreItems = navigationParameter as List<ExplorerItem>;
            ImageFlipVIew.ItemsSource = GalleryItems;
            foreach (ExplorerItem item in _expoloreItems)
            {
                if (await PhotoThumbnail(item))
                    GalleryItems.Add(item);
            }
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async System.Threading.Tasks.Task<bool> PhotoThumbnail(ExplorerItem photo)
        {
            if (photo.StorageFile == null || !photo.StorageFile.IsImageFile()) return false;
            try
            {
                StorageItemThumbnail fileThumbnail = await photo.StorageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)ActualScreenHeight, ThumbnailOptions.UseCurrentScale);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileThumbnail);
                photo.Image = bitmapImage;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void GoBack2(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageExplorer));            
        }

        private void SliderModeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            UnSliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            _sliderDispatcher.Tick += SliderDispatcher_Tick;
            _sliderDispatcher.Interval = new TimeSpan(0, 0, 0, 1,500);
            _sliderDispatcher.Start();
        }

        void SliderDispatcher_Tick(object sender, object e)
        {
            if (ImageFlipVIew.Items.Count - 1 == ImageFlipVIew.SelectedIndex)
                ImageFlipVIew.SelectedIndex = 0;
            else
                ImageFlipVIew.SelectedIndex++;
        }

        private void UnSliderModeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UnSliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            _sliderDispatcher.Stop();
            _sliderDispatcher = null;
        }

        //private async void OpenPhotoButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if ((ImageFlipVIew.SelectedItem as ExplorerItem).StorageFile == null) return;
        //    try
        //    {
        //        await (ImageFlipVIew.SelectedItem as ExplorerItem).StorageFile.OpenAsync(FileAccessMode.Read);
        //        await Launcher.LaunchFileAsync((ImageFlipVIew.SelectedItem as ExplorerItem).StorageFile, new LauncherOptions { DisplayApplicationPicker = true });
        //    }
        //    catch (Exception exp)
        //    {
        //        //EventLogger.onActionEvent(EventLogger.);
        //    }
        //}
    }

    /// <summary>
    /// Properties for change theme color
    /// </summary>
    public sealed partial class PhotoGallery
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

    public sealed partial class PhotoGallery
    {
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
}
