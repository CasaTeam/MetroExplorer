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

        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            EventLogger.onActionEvent(EventLogger.FOLDER_OPENED);
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var _expoloreItems = navigationParameter as List<ExplorerItem>;
            int photoCount = 0;
            foreach (ExplorerItem item in _expoloreItems)
            {
                if (photoCount > 50)
                    break;
                if (await PhotoThumbnail(item))
                {
                    GalleryItems.Add(item);
                    photoCount++;
                }
            }
            MyVariableGridView.ItemsSource = GalleryItems;
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            LoadingProgressBar.Opacity = 0;
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            foreach(var item in GalleryItems)
            {
                item.Image = null;
            }
            GalleryItems.Clear();
            GalleryItems = null;
            MyVariableGridView.ItemsSource = null;
            ImageFlipVIew.ItemsSource = null;
            if (_sliderDispatcher != null)
            {
                _sliderDispatcher.Stop();
                _sliderDispatcher = null;
            }
            GC.Collect();
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
                photo.Width = (bitmapImage.PixelHeight / bitmapImage.PixelWidth == 1) ? 1 : 2;
                photo.Height = (bitmapImage.PixelWidth / bitmapImage.PixelHeight == 1) ? 1 : 2;
                if (photo.Width == 1 && photo.Height == 1 && bitmapImage.PixelWidth > 600 && bitmapImage.PixelHeight > 600)
                {
                    photo.Width = 2;
                    photo.Height = 2;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void GoBack2(object sender, RoutedEventArgs e)
        {
            if (MyVariableGridView.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                MyVariableGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ImageFlipVIew.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                Frame.Navigate(typeof(PageExplorer));
            }
        }

        private void SliderModeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            UnSliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            MyVariableGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ImageFlipVIew.Visibility = Windows.UI.Xaml.Visibility.Visible;
            StartFlipView(MyVariableGridView.SelectedIndex);

            if (_sliderDispatcher == null)
                _sliderDispatcher = new DispatcherTimer();
            _sliderDispatcher.Tick += SliderDispatcher_Tick;
            _sliderDispatcher.Interval = new TimeSpan(0, 0, 0, 3);
            _sliderDispatcher.Start();
            BottomAppBar.IsOpen = false;
        }

        void SliderDispatcher_Tick(object sender, object e)
        {
            if (ImageFlipVIew != null && ImageFlipVIew.Items != null && ImageFlipVIew.Items.Count > 0)
            {
                if (ImageFlipVIew.Items.Count - 1 == ImageFlipVIew.SelectedIndex)
                    ImageFlipVIew.SelectedIndex = 0;
                else
                    ImageFlipVIew.SelectedIndex++;
            }
        }

        private void UnSliderModeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UnSliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (_sliderDispatcher != null)
            {
                _sliderDispatcher.Stop();
                _sliderDispatcher = null;
            }
        }

        private void Button_SetInterval_Click(object sender, object e)
        {

        }

        private void SliderSettingButton_Click(object sender, RoutedEventArgs e)
        {
            Popup_SetInterval.IsOpen = true;
            Popup_SetInterval.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_SetInterval.Margin = new Thickness(0, 0, 0, 232);
        }

        private void MyVariableGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyVariableGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ImageFlipVIew.Visibility = Windows.UI.Xaml.Visibility.Visible;
            StartFlipView(MyVariableGridView.SelectedIndex);
        }

        private void StartFlipView(int startPosition = 0)
        {
            if(ImageFlipVIew.ItemsSource == null)
                ImageFlipVIew.ItemsSource = GalleryItems;
            ImageFlipVIew.SelectedIndex = startPosition;
        }

        private void SliderModeButton_Click_1(object sender, RoutedEventArgs e)
        {

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
