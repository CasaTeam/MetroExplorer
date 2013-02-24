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
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using System.Threading.Tasks;
using Windows.System;
using Windows.Storage.Streams;
using System.Xml.Serialization;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Windows8MetroExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            selectPhotosFromFilePicker();
            //StorageFile file = await selectPhotosFromFilePicker();
            //await Launcher.LaunchFileAsync(file, new LauncherOptions { DisplayApplicationPicker = true });
        }

        private async void selectPhotosFromFilePicker()
        {

            if (Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList != null && Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Entries.Count > 0)
            {
                foreach (var item in Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Entries)
                {
                    //await sampleFile.MoveAsync(storageFolder, "fsadfs.txt");
                    if (item.Metadata == "disc c")
                    {
                       
                    }
                }
            }

            //StorageFolder storageFolder2 = await StorageFolder.GetFolderFromPathAsync("D:\\Documents\\fsadf.txt");
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder storageFolder = await folderPicker.PickSingleFolderAsync();
            //StorageFile sampleFile = await storageFolder.CreateFileAsync("sample.txt", CreationCollisionOption.ReplaceExisting);
            String mruToken = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(storageFolder, "disc c");
            StorageFolder retrievedFolder = await StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruToken);

            IReadOnlyList<IStorageItem> listFiles = await retrievedFolder.GetItemsAsync();
            foreach (var item in listFiles)
            {
                //await sampleFile.MoveAsync(storageFolder, "fsadfs.txt");
                if (item is StorageFolder)
                {
                    foreach (var item2 in await (item as StorageFolder).GetItemsAsync())
                    {
                        TextBlock_FF.Text = item2.Path;
                        if(item2 is StorageFile)
                            await Launcher.LaunchFileAsync(item2 as StorageFile, new LauncherOptions { DisplayApplicationPicker = false });
                    }
                }
            }

            //StorageFile file = await storageFolder.CreateFileAsync("sample.txt", CreationCollisionOption.ReplaceExisting);
            //using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
            //    {
            //        using (DataWriter dataWriter = new DataWriter(outputStream))
            //        {
            //            //TODO: Replace "Bytes" with the type you want to write.

            //            dataWriter.WriteBytes(ReadToEnd(file.OpenStreamForWriteAsync().Result));
            //            await dataWriter.StoreAsync();
            //            dataWriter.DetachStream();
            //        }

            //        await outputStream.FlushAsync();
            //    }
            //}


            //FileOpenPicker openPicker = new FileOpenPicker();
            //openPicker.ViewMode = PickerViewMode.Thumbnail;
            //openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //openPicker.FileTypeFilter.Add("*");
            //////openPicker.FileTypeFilter.Add(".jpg");
            //StorageFile file = await openPicker.PickSingleFileAsync();
            //file.MoveAsync()
            //return file;
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            System.Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            System.Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    System.Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}
