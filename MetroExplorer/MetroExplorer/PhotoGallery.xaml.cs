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

    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>

    public sealed partial class PhotoGallery : LayoutAwarePage
    {
        List<ExplorerItem> items;
        private readonly MetroExplorerLocalDataSource _dataSource;
        StorageFile seletedFile;
        int mSeletedIndex;

        private bool isFadeInFirst = true;

        public PhotoGallery()
        {
            InitializeComponent();
            items = new List<ExplorerItem>();
            _dataSource = Singleton<MetroExplorerLocalDataSource>.Instance;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            items = null;
            seletedFile = null;
            GC.Collect();
        }

        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlipView flipview = (FlipView)sender;
            ExplorerItem selected = (ExplorerItem)flipview.SelectedItem;
            if (mSeletedIndex != 0 && isFadeInFirst)
            {
                if (flipview.SelectedIndex != mSeletedIndex)
                {
                    flipview.FadeOutCustom(new TimeSpan(0, 0, 0, 0, 0));
                }
                else
                {
                    flipview.FadeOut(new TimeSpan(0, 0, 0, 0, 0));
                    flipview.FadeInCustom(new TimeSpan(0, 0, 0, 1, 0));
                    isFadeInFirst = false;
                }
            }

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            EventLogger.onActionEvent(EventLogger.PHOTO_VIEWED);
            Object[] parameters = (Object[])e.Parameter;
            seletedFile = (StorageFile)parameters[1];
            StorageFolder currentStorageFolder = _dataSource.CurrentStorageFolder;
            if (currentStorageFolder != null)
            {
                IReadOnlyList<IStorageItem> listFiles = await currentStorageFolder.GetItemsAsync();
                foreach (var item in listFiles)
                {

                    if (item is StorageFile)
                    {
                        StorageFile file = (StorageFile)item;

                        if (file != null && file.IsImageFile())
                        {
                            StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)this.ActualHeight, ThumbnailOptions.UseCurrentScale);
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.SetSource(fileThumbnail);
                            ExplorerItem photoItem = new ExplorerItem();
                            photoItem.Name = file.Name;
                            photoItem.Image = bitmapImage;
                            items.Add(photoItem);
                            // Ensure the stream is disposed once the image is loaded
                            //using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                            //{
                            //    // Set the image source to the selected bitmap
                            //    BitmapImage bitmapImage = new BitmapImage();

                            //    await bitmapImage.SetSourceAsync(fileStream);

                            //    ExplorerItem photoItem = new ExplorerItem();
                            //    photoItem.Name = file.Name;
                            //    photoItem.Image = bitmapImage;
                            //    items.Add(photoItem);
                            //}  
                        }
                    }
                }
                for (int i = 0; i < items.Count; i++)
                {
                    ExplorerItem item = items.ElementAt(i) as ExplorerItem;
                    if (item != null && seletedFile.Name.Equals(item.Name))
                    {
                        mSeletedIndex = i;
                    }
                }
                ImageFlipVIew.ItemsSource = items;
                ImageFlipVIew.SelectedIndex = mSeletedIndex;
            }
            LoadingProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

    }
}
