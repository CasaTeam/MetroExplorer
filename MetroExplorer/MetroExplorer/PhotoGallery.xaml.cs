using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MetroExplorer.Common;
using System.Collections.ObjectModel;
using MetroExplorer.core.Objects;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage.AccessCache;
using MetroExplorer.core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

// Pour en savoir plus sur le modèle d'élément Page Éléments groupés, consultez la page http://go.microsoft.com/fwlink/?LinkId=234231

namespace MetroExplorer
{
    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>

    public sealed partial class PhotoGallery : LayoutAwarePage
    {
        StorageFolder currentStorageFolder;
        List<ExplorerItem> items;
        IList<StorageFolder> _navigatorStorageFolders;
        StorageFile seletedFile;
        int mSeletedIndex;

        private bool isFadeInFirst = true;

        public PhotoGallery()
        {
            this.InitializeComponent();
            items = new List<ExplorerItem>();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

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
                else {
                    flipview.FadeInCustom(new TimeSpan(0, 0, 0, 2, 0));
                    isFadeInFirst = false;
                }
            }

        }

        private Boolean isImageFile(StorageFile file)
        {
            if (file.FileType.Equals(".jpg") ||
                file.FileType.Equals(".jpeg") ||
                file.FileType.Equals(".png") ||
                file.FileType.Equals(".bmp"))
                return true;
            return false;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Object[] parameters = (Object[])e.Parameter;
            _navigatorStorageFolders = (IList<StorageFolder>)parameters[0];
            seletedFile = (StorageFile)parameters[1];
            currentStorageFolder = _navigatorStorageFolders.LastOrDefault();
            if (currentStorageFolder != null)
            {
                IReadOnlyList<IStorageItem> listFiles = await currentStorageFolder.GetItemsAsync();
                foreach (var item in listFiles)
                {
                    
                    if (item is StorageFile)
                    {
                        StorageFile file = (StorageFile) item;

                        if (file != null && isImageFile(file))
                        {
                            // Ensure the stream is disposed once the image is loaded
                            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                            {
                                // Set the image source to the selected bitmap
                                BitmapImage bitmapImage = new BitmapImage();

                                await bitmapImage.SetSourceAsync(fileStream);

                                ExplorerItem photoItem = new ExplorerItem();
                                photoItem.Name = file.Name;
                                photoItem.Image = bitmapImage;
                                items.Add(photoItem);
                            }
                            
                        }                    
                    }
                }
                for (int i = 0; i < items.Count; i++ )
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
         
        }

    }
}
