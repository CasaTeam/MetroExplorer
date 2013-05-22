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

        DispatcherTimer _imageChangingDispatcher = new DispatcherTimer();

        private void InitializeChangingDispatcher()
        {
            _imageChangingDispatcher = new DispatcherTimer();
            _imageChangingDispatcher.Tick += ImageChangingDispatcher_Tick;
            _imageChangingDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _imageChangingDispatcher.Start();
        }

        private void StopImageChangingDispatcher()
        {
            if (_imageChangingDispatcher != null)
            {
                _imageChangingDispatcher.Stop();
                _imageChangingDispatcher = null;
            }
        }

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
                await ChangeFolderCover();
                _imageDispatcherLock = false;
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;          
        }

        /// <summary>
        /// 用来控制循环遍历文件夹的变量
        /// </summary>
        int _lastChangedFolder = 0;

        /// <summary>
        /// 普通文件的加载为0.5秒40个，文件夹为1.5秒1次1个
        /// </summary>
        int _folderChangeAgainstFileChange = 0;

        /// <summary>
        /// 每一秒观察一个文件夹，每秒只换一次文件夹
        /// 按顺序换
        /// </summary>
        /// <returns></returns>
        private async Task ChangeFolderCover()
        {
            try
            {
                if (_folderChangeAgainstFileChange % 3 == 0)
                {
                    if (_lastChangedFolder == ExplorerGroups[0].Count)
                        _lastChangedFolder = 0;
                    _lastChangedFolder++;
                    var exploreItem = ExplorerGroups[0][_lastChangedFolder - 1];
                    if (exploreItem.StorageFolder == null) return;
                    
                    var files = await exploreItem.StorageFolder.GetFilesAsync();
                    if (exploreItem.LastImageIndex >= 0)
                    {
                        if (exploreItem.LastImageIndex >= exploreItem.LastImageName.Count)
                            exploreItem.LastImageIndex = 0;
                        await ThumbnailPhoto(exploreItem, files[exploreItem.LastImageIndex]);
                        exploreItem.LastImageIndex++;
                    }
                    // exploreItem.LastImageIndex == -2 代表该文件夹下没有图片
                    else if (exploreItem.LastImageIndex == -2) return;
                    // exploreItem.LastImageIndex == -1 代表该文件夹还没有被发觉
                    else if (exploreItem.LastImageIndex == -1)
                    {
                        exploreItem.LastImageIndex = -2;
                        GetSubFoldersUsableImage(exploreItem, files);
                    }
                }
                if (_folderChangeAgainstFileChange == 1000)
                    _folderChangeAgainstFileChange = 0;
                else
                    _folderChangeAgainstFileChange++;
            }
            catch
            { }
        }

        private async void GetSubFoldersUsableImage(ExplorerItem exploreItem, IReadOnlyList<StorageFile> files)
        {
            int i = 0;
            foreach (var file in files)
            {
                if (file.Name.ToUpper().EndsWith(".PNG") || file.Name.ToUpper().EndsWith(".JPG") || file.Name.ToUpper().EndsWith(".JPEG") ||
                    file.Name.ToUpper().EndsWith(".BMP") || file.Name.ToUpper().EndsWith(".RMVB") || file.Name.ToUpper().EndsWith(".MP4") ||
                    file.Name.ToUpper().EndsWith(".MKV") || file.Name.ToUpper().EndsWith(".PNG"))
                {
                    exploreItem.LastImageName.Add(file.Name);
                    exploreItem.LastImageIndex = 1;
                    await ThumbnailPhoto(exploreItem, file);
                    if ((++i) == 6)
                        break;
                }
            }
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
