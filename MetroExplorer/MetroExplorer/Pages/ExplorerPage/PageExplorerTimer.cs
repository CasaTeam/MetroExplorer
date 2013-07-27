namespace MetroExplorer.Pages.ExplorerPage
{
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
    using Core;
    using Core.Objects;

    public sealed partial class PageExplorer
    {
        /// <summary>
        /// 因为文件的图片加载，以及size加载是循序渐进的。所以这个变量作为宏观的变量，来记录加载记录到哪一个项
        /// </summary>
        int _counterForLoadUnloadedItems;

        bool _fileInfoLoadDispatcherLock;

        DispatcherTimer _fileInfoLoadDispatcher = new DispatcherTimer();

        private void InitializeChangingDispatcher()
        {
            _fileInfoLoadDispatcher = new DispatcherTimer();
            _fileInfoLoadDispatcher.Tick += ImageChangingDispatcher_Tick;
            _fileInfoLoadDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _fileInfoLoadDispatcher.Start();
        }

        private void StopImageChangingDispatcher()
        {
            if (_fileInfoLoadDispatcher != null)
            {
                _fileInfoLoadDispatcher.Stop();
                _fileInfoLoadDispatcher = null;
            }
        }

        async void ImageChangingDispatcher_Tick(object sender, object e)
        {
            if (_fileInfoLoadDispatcherLock == false && ExplorerItems != null)
            {
                _fileInfoLoadDispatcherLock = true;
                try
                {
                    for (int i = 1; i % 40 != 0 && ExplorerItems != null && _counterForLoadUnloadedItems < ExplorerItems.Count; i++)
                    {
                        if (ExplorerItems[_counterForLoadUnloadedItems].StorageFile != null)
                        {
                            var file = ExplorerItems[_counterForLoadUnloadedItems].StorageFile;
                            if (ExplorerItems[_counterForLoadUnloadedItems].Size == 0)
                            {
                                ExplorerItems[_counterForLoadUnloadedItems].Size = (await file.GetBasicPropertiesAsync()).Size;
                                ExplorerItems[_counterForLoadUnloadedItems].ModifiedDateTime = (await file.GetBasicPropertiesAsync()).DateModified.DateTime;
                            }
                            await ThumbnailPhoto(ExplorerItems[_counterForLoadUnloadedItems], file, true);
                            SetImageStrech(ExplorerItems[_counterForLoadUnloadedItems]);
                        }
                        else if (ExplorerItems[_counterForLoadUnloadedItems].StorageFolder != null)
                        {
                            var folder = ExplorerItems[_counterForLoadUnloadedItems].StorageFolder;
                            ExplorerItems[_counterForLoadUnloadedItems].Image = GetBitMapImageFromLocalSource("Assets/Folder.png");
                            ExplorerItems[_counterForLoadUnloadedItems].ImageStretch = "None";
                            ExplorerItems[_counterForLoadUnloadedItems].Size = 0;
                            ExplorerItems[_counterForLoadUnloadedItems].ModifiedDateTime = (await folder.GetBasicPropertiesAsync()).DateModified.DateTime;
                        }
                        _counterForLoadUnloadedItems++;
                    }
                }
                catch
                { }
                _fileInfoLoadDispatcherLock = false;
            }        
        }

        private async Task ThumbnailPhoto(ExplorerItem item, StorageFile sf, bool file = false)
        {
            if (item == null && item.Image != null) return;

            StorageItemThumbnail fileThumbnail = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 250);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            if (file == false)
                item.Image = bitmapImage;
            else
                item.Image = bitmapImage;
        }

        private void SetImageStrech(ExplorerItem item)
        {
            if (item.Name.ToUpper().EndsWith(".JPG") ||
                item.Name.ToUpper().EndsWith(".JPEG") ||
                item.Name.ToUpper().EndsWith(".PNG") ||
                item.Name.ToUpper().EndsWith(".BMP") ||
                item.Name.ToUpper().EndsWith(".MP4") || item.Name.ToUpper().EndsWith(".WMV"))
                item.ImageStretch = "UniformToFill";
            else
                item.ImageStretch = "Uniform";
        }

        private BitmapImage GetBitMapImageFromLocalSource(string url)
        {
            var result = new BitmapImage(new Uri("ms-appx:/" + url));
            //result.UriSource = uri;
            return result;
        }
    }
}
