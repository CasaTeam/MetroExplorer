namespace MetroExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Windows.Storage;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.Storage.FileProperties;
    using Common;
    using core;
    using core.Objects;
    using core.Utils;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using Windows.UI.Xaml;

    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>

    public sealed partial class PhotoGallery : LayoutAwarePage, INotifyPropertyChanged
    {
        ObservableCollection<ExplorerItem> _photos = new ObservableCollection<ExplorerItem>();
        public ObservableCollection<ExplorerItem> Photos
        {
            get {
                return _photos;
            }
            set
            {
                _photos = value;
                NotifyPropertyChanged("Photos");
            }
        }

        private List<ExplorerItem> _potentialPhotos;

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

        async void PhotoGallery_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ImageFlipVIew.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            await PhotoThumbnail(_potentialPhotos[0]);
            Photos.Add(_potentialPhotos[0]);

            if (_potentialPhotos.Count > 1)
            {
                await PhotoThumbnail(_potentialPhotos[1]);
                Photos.Add(_potentialPhotos[1]);
            }

            ImageFlipVIew.Visibility = Windows.UI.Xaml.Visibility.Visible;
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _sliderDispatcher.Stop();
            _sliderDispatcher = null;
            GC.Collect();
        }

        private async void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlipView flipview = (FlipView)sender;
            ExplorerItem selected = (ExplorerItem)flipview.SelectedItem;
            if (Photos.Count < _potentialPhotos.Count)
            {
                if (Photos.Contains(_potentialPhotos[Photos.Count])) return;
                await PhotoThumbnail(_potentialPhotos[Photos.Count]);
                Photos.Add(_potentialPhotos[Photos.Count]);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ChangeTheme(Theme.ThemeLibarary.CurrentTheme);

            ImageFlipVIew.ItemsSource = Photos;

            _potentialPhotos = e.Parameter as List<ExplorerItem>;

            EventLogger.onActionEvent(EventLogger.PHOTO_VIEWED);
        }

        private async System.Threading.Tasks.Task PhotoThumbnail(ExplorerItem photo)
        {
            StorageItemThumbnail fileThumbnail = await photo.StorageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)this.ActualHeight, ThumbnailOptions.UseCurrentScale);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            photo.Image = bitmapImage;
        }

        private void SliderModeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            UnSliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            _sliderDispatcher.Tick += SliderDispatcher_Tick;
            _sliderDispatcher.Interval = new TimeSpan(0, 0, 0, 3);
            _sliderDispatcher.Start();
        }

        async void SliderDispatcher_Tick(object sender, object e)
        {
            if (Photos.Count == _potentialPhotos.Count && ImageFlipVIew.SelectedIndex == Photos.Count - 1)
            {
                ImageFlipVIew.SelectedIndex = 0;
                return;
            }
            else if (Photos.Count < _potentialPhotos.Count)
            {
                await PhotoThumbnail(_potentialPhotos[Photos.Count]);
                Photos.Add(_potentialPhotos[Photos.Count]);
            }
            ImageFlipVIew.SelectedIndex++;
        }

        private void UnSliderModeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UnSliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            _sliderDispatcher.Stop();
            _sliderDispatcher = null;
        }
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
