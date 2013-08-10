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
    using Windows.UI.Xaml.Input;

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
        Slider _currentVideoTimerSlider;

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
            ImageFlipVIew.SelectedIndex = -1;
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
                SliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                UnSliderModeButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (_currentPlayMedia != null && (_currentPlayMedia.CurrentState != MediaElementState.Closed &&
                 _currentPlayMedia.CurrentState != MediaElementState.Stopped))
                    CloseAndUnloadLastMedia();
                ImageFlipVIew.SelectedIndex = -1;
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

        private void StartFlipView(ExplorerItem item)
        {
            if(ImageFlipVIew.ItemsSource == null)
                ImageFlipVIew.ItemsSource = GalleryItems;
            if (item != null && GalleryItems.Contains(item))
            {
                ImageFlipVIew.SelectedIndex = GalleryItems.IndexOf(item);
            }
            else if (ImageFlipVIew.Items.Count > 0)
                ImageFlipVIew.SelectedIndex = 0;
        }

        private async void ImageFlipVIew_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentPlayMedia != null && (_currentPlayMedia.CurrentState != MediaElementState.Closed &&
                 _currentPlayMedia.CurrentState != MediaElementState.Stopped))
                CloseAndUnloadLastMedia();

            if (ImageFlipVIew.SelectedItem == null) return;
            var item = (ImageFlipVIew.SelectedItem as ExplorerItem);
            if (item.StorageFile.IsVideoFile())
            {
                var container = ImageFlipVIew.ItemContainerGenerator.ContainerFromItem(item);
                if (container == null)
                    return;
                var media = GetMediaElement(container);
                if (media == null) return;
                _currentVideoTimerSlider = GetSlider(container);
                media.SetSource(await item.StorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read), item.StorageFile.FileType);
                if (_sliderDispatcher != null)
                    _sliderDispatcher.Stop();
                media.MediaFailed += media_MediaFailed;
                media.MediaEnded += media_MediaEnded;
                media.MediaOpened += media_MediaOpened;
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
            if (_currentVideoTimerSlider != null)
            { 
                _currentVideoTimerSlider.ValueChanged -= _currentVideoTimerSlider_ValueChanged;
                _currentVideoTimerSlider.StepFrequency = 0;
                _currentVideoTimerSlider.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                _currentVideoTimerSlider = null;
            }
            StopTimerForVideoSlider();
        }

        void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (_sliderDispatcher != null && UnSliderModeButton.Visibility == Windows.UI.Xaml.Visibility.Visible)
                _sliderDispatcher.Start();
            CloseAndUnloadLastMedia();
        }

        void media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (_sliderDispatcher != null && UnSliderModeButton.Visibility == Windows.UI.Xaml.Visibility.Visible)
                _sliderDispatcher.Start();
            CloseAndUnloadLastMedia();
        }

        void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (_sliderDispatcher != null && UnSliderModeButton.Visibility == Windows.UI.Xaml.Visibility.Visible)
                _sliderDispatcher.Stop();
            if (_currentPlayMedia != null && _currentVideoTimerSlider != null)
            { 
                double absvalue = (int)Math.Round(
                    _currentPlayMedia.NaturalDuration.TimeSpan.TotalSeconds,
                    MidpointRounding.AwayFromZero);
                _currentVideoTimerSlider.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                _currentVideoTimerSlider.Maximum = absvalue;
                _currentVideoTimerSlider.ValueChanged += _currentVideoTimerSlider_ValueChanged;
                _currentVideoTimerSlider.StepFrequency = SliderFrequency(_currentPlayMedia.NaturalDuration.TimeSpan);
                SetupTimerForVideoSlider();

            }
        }

        void _currentVideoTimerSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_currentPlayMedia != null)
            {
                _currentPlayMedia.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }

        private double SliderFrequency(TimeSpan timevalue)
        {
            double stepfrequency = -1;

            double absvalue = (int)Math.Round(
                timevalue.TotalSeconds, MidpointRounding.AwayFromZero);

            stepfrequency = (int)(Math.Round(absvalue / 100));

            if (timevalue.TotalMinutes >= 10 && timevalue.TotalMinutes < 30)
            {
                stepfrequency = 10;
            }
            else if (timevalue.TotalMinutes >= 30 && timevalue.TotalMinutes < 60)
            {
                stepfrequency = 30;
            }
            else if (timevalue.TotalHours >= 1)
            {
                stepfrequency = 60;
            }
            if (stepfrequency == 0) stepfrequency += 1;

            if (stepfrequency == 1)
            {
                stepfrequency = absvalue / 100;
            }
            return stepfrequency;
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

        public Slider GetSlider(DependencyObject parent)
        {
            if (parent == null)
                return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(parent, i), i), i), 2);
                if (child is Slider)
                    return child as Slider;
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

        private void MediaElement_PointerPressed_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void MediaElement_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_currentPlayMedia != null)
            {
                if (_currentPlayMedia.CurrentState == MediaElementState.Playing)
                {
                    _currentPlayMedia.Pause();
                    _currentVideoTimerSlider.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else if (_currentPlayMedia.CurrentState == MediaElementState.Paused)
                {
                    _currentPlayMedia.Play();
                    _currentVideoTimerSlider.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        private DispatcherTimer _timerForVideoSlider;

        private void SetupTimerForVideoSlider()
        {
            _timerForVideoSlider = new DispatcherTimer();
            _timerForVideoSlider.Interval = TimeSpan.FromSeconds(_currentVideoTimerSlider.StepFrequency);
            StartTimerForVideoSlider();
        }

        private void __timerForVideoSlider_Tick(object sender, object e)
        {
            if (!_sliderpressed && _currentVideoTimerSlider != null && _currentPlayMedia != null)
            {
                _currentVideoTimerSlider.Value = _currentPlayMedia.Position.TotalSeconds;
            }
        }

        private void StartTimerForVideoSlider()
        {
            _timerForVideoSlider.Tick += __timerForVideoSlider_Tick;
            _timerForVideoSlider.Start();
        }

        private void StopTimerForVideoSlider()
        {
            if (_timerForVideoSlider != null)
            {
                _timerForVideoSlider.Stop();
                _timerForVideoSlider.Tick -= __timerForVideoSlider_Tick;
            }
        }

        private bool _sliderpressed = false;

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (_currentVideoTimerSlider != null && _currentPlayMedia != null)
            {
                _currentPlayMedia.Position = TimeSpan.FromSeconds(_currentVideoTimerSlider.Value);
                _sliderpressed = false;
            }
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
