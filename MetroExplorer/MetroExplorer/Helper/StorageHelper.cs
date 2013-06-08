using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace MetroExplorer.Helper
{
    public class FileSerializationHelper
    {
        public static async Task<bool> CheckFileExist(StorageFolder folder, string fileName)
        {
            try
            {
                var files = await folder.GetFileAsync(fileName);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public async Task<bool> CheckFileExist(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var files = await folder.GetFilesAsync();
            return files.Any(file => file.Name == fileName);
        }

        /// <summary>
        /// 创建实体到 LocalFolder中
        /// 若存在则替换
        /// </summary>
        /// <typeparam name="T">数据对象</typeparam>
        /// <param name="t"></param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static async Task CreatEntity<T>(T t, string fileName)
        {
            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using (IRandomAccessStream randomAccessSteam = await file.OpenAsync(FileAccessMode.ReadWrite))
                using (IOutputStream outputStream = randomAccessSteam.GetOutputStreamAt(0))
                {
                    await XmlSerializationHelper.XMLSerialize(t, outputStream.AsStreamForWrite());
                    await outputStream.FlushAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 删除 LocalFolder中的数据
        /// </summary>
        /// <param name="fileNmae">文件名称</param>
        /// <returns></returns>
        public static async Task<bool> DeleteEntity(string fileNmae)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            try
            {
                if (await CheckFileExist(folder, fileNmae))
                {
                    StorageFile files = await folder.GetFileAsync(fileNmae);
                    await files.DeleteAsync();
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 从LocalFolder ,出文件保存的对象
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static async Task<TResult> GetContent<TResult>(string fileName) where TResult : class,new()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            try
            {
                if (await CheckFileExist(folder, fileName))
                {
                    StorageFile file = await folder.GetFileAsync(fileName);
                    using (IRandomAccessStream randomAccessStream = await file.OpenAsync(FileAccessMode.Read))
                    using (IInputStream inPutStream = randomAccessStream.GetInputStreamAt(0))
                    {
                        return await XmlSerializationHelper.XMLDeserialize<TResult>(inPutStream.AsStreamForRead());
                    }
                }
                else { return null; }
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// 储存文件到本地
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static async Task<string> SaveImages(IBuffer buffer, string imageName)
        {
            var folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Put'em on maps", CreationCollisionOption.OpenIfExists);
            try
            {
                StorageFile file = await folder.CreateFileAsync(imageName, CreationCollisionOption.GenerateUniqueName);
                using (var readStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var outPutStream = readStream.GetOutputStreamAt(0))
                    {
                        await outPutStream.WriteAsync(buffer);
                        await outPutStream.FlushAsync();
                    }
                }
                if (file != null && file.Path != "") return file.Path;
            }
            catch (Exception ex) { throw ex; }
            return "";
        }

        public static async Task<string> SaveFiles(IBuffer buffer, string imageName)
        {
            var folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Put'em on maps", CreationCollisionOption.OpenIfExists);
            try
            {
                StorageFile file = await folder.CreateFileAsync(imageName, CreationCollisionOption.GenerateUniqueName);
                using (var readStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var outPutStream = readStream.GetOutputStreamAt(0))
                    {
                        await outPutStream.WriteAsync(buffer);
                        await outPutStream.FlushAsync();
                    }
                }
                if (file != null && file.Path != "") return file.Path;
            }
            catch (Exception ex) { throw ex; }
            return "";
        }

        /// <summary>
        /// 创建本地推送
        /// </summary>
        /// <returns></returns>
        public static async Task CreateLocalNotifications()
        {
            TileUpdater notifier = TileUpdateManager.CreateTileUpdaterForApplication();
            if (notifier.Setting == NotificationSetting.Enabled)
            {
                notifier.EnableNotificationQueue(true);
                var folder = ApplicationData.Current.LocalFolder;
                var view = await folder.GetFilesAsync();

                for (int i = 0; i < 5; i++)
                {
                    var SquareTemplate = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareImage);
                    var SquareAttributes = SquareTemplate.GetElementsByTagName("image")[0];
                    SquareAttributes.Attributes[1].NodeValue = "ms-appdata:///local/" + i.ToString() + ".png";
                    var WideTemplate = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideImage);
                    var WideAttributes = WideTemplate.GetElementsByTagName("image")[0];
                    WideAttributes.Attributes[1].NodeValue = "ms-appdata:///local/" + i.ToString() + ".png";
                    var node = WideTemplate.ImportNode(SquareTemplate.GetElementsByTagName("binding").Item(0), true);
                    WideTemplate.GetElementsByTagName("visual").Item(0).AppendChild(node);
                    var tileNotification = new TileNotification(WideTemplate);
                    notifier.Update(tileNotification);
                }

                var MainSquareTemplate2 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareImage);
                var MainSquareAttributes2 = MainSquareTemplate2.GetElementsByTagName("image")[0];
                MainSquareAttributes2.Attributes[1].NodeValue = "ms-appx:///Assets/Logo.png";
                var MainWideTemplate2 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideImage);
                var MainWideAttributes2 = MainWideTemplate2.GetElementsByTagName("image")[0];
                MainWideAttributes2.Attributes[1].NodeValue = "ms-appx:///Assets/putemonmaps.png";
                var Mainnode2 = MainWideTemplate2.ImportNode(MainSquareTemplate2.GetElementsByTagName("binding").Item(0), true);
                MainWideTemplate2.GetElementsByTagName("visual").Item(0).AppendChild(Mainnode2);
                var MaintileNotification2 = new TileNotification(MainWideTemplate2);
                notifier.Update(MaintileNotification2);
            }
        }
    }
}
