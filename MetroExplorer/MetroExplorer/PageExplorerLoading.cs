using MetroExplorer.core;
using MetroExplorer.core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace MetroExplorer
{
    public sealed partial class PageExplorer
    {
        int _loadingImageCount;
        int _loadingFileSizeCount;
        bool _imageDispatcherLock;
        async void ImageChangingDispatcher_Tick(object sender, object e)
        {
            if (_imageDispatcherLock == false && ExplorerGroups != null)
            {
                _imageDispatcherLock = true;
                int loadingCount = (PageExplorer.BigSquareMode == false) ? _loadingFileSizeCount : _loadingImageCount;
                if (ExplorerGroups != null && ExplorerGroups[1] != null && loadingCount < ExplorerGroups[1].Count)
                {
                    for (int i = 1; i % 40 != 0 && ExplorerGroups != null && loadingCount < ExplorerGroups[1].Count; i++)
                    {
                        var file = ExplorerGroups[1][loadingCount].StorageFile;
                        if (PageExplorer.BigSquareMode == false)
                        {
                            if (ExplorerGroups[1][loadingCount].Size == 0)
                            {
                                ExplorerGroups[1][loadingCount].Size = (await file.GetBasicPropertiesAsync()).Size;
                                ExplorerGroups[1][loadingCount].ModifiedDateTime = (await file.GetBasicPropertiesAsync()).DateModified.DateTime;
                            }
                        }
                        else
                            await ThumbnailPhoto(ExplorerGroups[1][loadingCount], file, true);
                        loadingCount++;
                    }
                    if (PageExplorer.BigSquareMode == false)
                        _loadingFileSizeCount = loadingCount;
                    else
                        _loadingImageCount = loadingCount;
                }
                else
                    _imageChangingDispatcher.Interval = new TimeSpan(0, 0, 0, 2);
                await ChangeFolderCover();
                _imageDispatcherLock = false;
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        int _lastChangedFolder = 0;
        private async Task ChangeFolderCover()
        {
            try
            {
                if (_lastChangedFolder == ExplorerGroups[0].Count)
                    _lastChangedFolder = 0;
                _lastChangedFolder++;
                var exploreItem = ExplorerGroups[0][_lastChangedFolder];
                if (exploreItem.StorageFolder == null) return;
                var files = await exploreItem.StorageFolder.GetFilesAsync();
                if (exploreItem.LastImageIndex == -2) return;
                if (exploreItem.LastImageIndex == -1)
                    foreach (var file in files)
                    {
                        exploreItem.LastImageIndex = -2;
                        if (file.Name.ToUpper().EndsWith(".PNG") || file.Name.ToUpper().EndsWith(".JPG") || file.Name.ToUpper().EndsWith(".JPEG") ||
                            file.Name.ToUpper().EndsWith(".BMP") || file.Name.ToUpper().EndsWith(".RMVB") || file.Name.ToUpper().EndsWith(".MP4") ||
                            file.Name.ToUpper().EndsWith(".MKV") || file.Name.ToUpper().EndsWith(".PNG"))
                        {
                            exploreItem.LastImageName.Add(file.Name);
                            exploreItem.LastImageIndex = 0;
                        }
                    }
                if (exploreItem.LastImageIndex == exploreItem.LastImageName.Count - 1)
                    exploreItem.LastImageIndex = 0;
                await ThumbnailPhoto(exploreItem, files[exploreItem.LastImageIndex]);
                exploreItem.LastImageIndex++;
            }
            catch
            { }
        }

        private async Task ThumbnailPhoto(ExplorerItem item, StorageFile sf, bool file = false)
        {
            if (item == null && item.DefautImage != null) return;

            StorageItemThumbnail fileThumbnail = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 250);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            if (file == false)
                item.Image = bitmapImage;
            else
                item.DefautImage = bitmapImage;
        }
    }
}
