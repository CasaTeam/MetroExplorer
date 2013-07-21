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
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;

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

        DispatcherTimer _sliderDispatcher = new DispatcherTimer();

        MediaElement _currentPlayMedia;

        public PhotoGallery()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += PhotoGallery_Loaded;
            this.Unloaded += PhotoGallery_Unloaded;
        }

        void ImageFlipVIew_Unloaded(object sender, RoutedEventArgs e)
        {
           
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
            ImageFlipVIew.ItemsSource = GalleryItems;
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
            if (photo.StorageFile == null || (!photo.StorageFile.IsImageFile() && !photo.StorageFile.IsVideoFile())) return false;
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
                if (_currentPlayMedia != null && (_currentPlayMedia.CurrentState != MediaElementState.Closed &&
                 _currentPlayMedia.CurrentState != MediaElementState.Stopped))
                    CloseAndUnloadLastMedia();
            }
            else
            {
                Frame.Navigate(typeof(PageExplorer));
            }
        }

        private void SliderModeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (GalleryItems.Count == 0) return;
            SliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            UnSliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            MyVariableGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ImageFlipVIew.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (ImageFlipVIew.Items != null && ImageFlipVIew.SelectedItem == null && ImageFlipVIew.Items.Count > 0)
                ImageFlipVIew.SelectedIndex = 0;

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

        private async void StartFlipView(ExplorerItem item)
        {
            if(ImageFlipVIew.ItemsSource == null)
                ImageFlipVIew.ItemsSource = GalleryItems;
            ImageFlipVIew.SelectedItem = item;
            await PlayAVideo();
        }

        private void videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            
        }

        private async void ImageFlipVIew_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await PlayAVideo();
        }

        private async System.Threading.Tasks.Task PlayAVideo()
        {
            if (_currentPlayMedia != null && (_currentPlayMedia.CurrentState != MediaElementState.Closed &&
                 _currentPlayMedia.CurrentState != MediaElementState.Stopped))
                CloseAndUnloadLastMedia();

            if (ImageFlipVIew.SelectedItem == null) return;
            var item = (ImageFlipVIew.SelectedItem as ExplorerItem);
            if (item.StorageFile.IsVideoFile())
            {
                var container = ImageFlipVIew.ItemContainerGenerator.ContainerFromItem(item);
                var media = GetMediaElement(container);
                if (media == null) return;
                media.SetSource(await item.StorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read), item.StorageFile.FileType);
                if (_sliderDispatcher != null)
                    _sliderDispatcher.Stop();
                media.MediaFailed += media_MediaFailed;
                media.MediaEnded += media_MediaEnded;
                _currentPlayMedia = media;
            }

            GC.Collect();
        }

        private void CloseAndUnloadLastMedia()
        {
            if (_currentPlayMedia != null)
            {
                _currentPlayMedia.MediaFailed -= media_MediaFailed;
                _currentPlayMedia.MediaEnded -= media_MediaEnded;
                _currentPlayMedia.Stop();
                _currentPlayMedia.Source = null;
                _currentPlayMedia = null;
                
            }
        }

        void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (_sliderDispatcher != null && UnSliderModeButton.Visibility == Windows.UI.Xaml.Visibility.Visible)
                _sliderDispatcher.Start();
        }

        void media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (_sliderDispatcher != null && UnSliderModeButton.Visibility == Windows.UI.Xaml.Visibility.Visible)
                _sliderDispatcher.Start();
        }

        void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (_sliderDispatcher != null && UnSliderModeButton.Visibility == Windows.UI.Xaml.Visibility.Visible)
                _sliderDispatcher.Stop();
        }

        public MediaElement GetMediaElement(DependencyObject parent)
        {
            if (parent == null)
                return null;
            var list = new List<MediaElement> { };
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(parent, i), i), i), 1);
                if (child is MediaElement)
                    return child as MediaElement;
            }
            return null;
        }

        private void MyVariableGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            MyVariableGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ImageFlipVIew.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (e.ClickedItem != null)
                StartFlipView(e.ClickedItem as ExplorerItem);
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

    public sealed class FileTypeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var storageFile = (value as StorageFile);
                return storageFile.IsVideoFile() ? "Visible" : "Collapsed";
            }
            else
                return "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }
    }
}
