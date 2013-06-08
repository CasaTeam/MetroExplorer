using MetroExplorer.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MetroExplorer.UserPreferenceRecord
{
    /// <summary>
    /// 操作.txt的一个帮助文件。暂时放在这里
    /// </summary>
    public class TextFileAccess
    {
        private TextFileAccess()
        {
            
        }

        public async Task<StorageFile> GetStorageFile(string fileName)
        {
            try
            {
                var storageFolder = ApplicationData.Current.RoamingFolder;
                var storageFile = await storageFolder.GetFileAsync(fileName);
                return storageFile;
            }
            catch
            {
                UserPreferenceRecord.GetInstance().WriteUserPreferenceRecord("");
                return null;
            }
        }

        public async Task<StorageFile> CreateATextFile(string fileName)
        {
            StorageFolder storageFolder = ApplicationData.Current.RoamingFolder;
            return await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        }

        public async void CreateWriteText(string text, string fileName)
        {
            var storageFile = await GetStorageFile(fileName);
            if (storageFile != null)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    await FileIO.WriteTextAsync(storageFile, text);
                }
            }
            else
            {
                var file = await CreateATextFile(fileName);
                if (!String.IsNullOrEmpty(text))
                {
                    await FileIO.WriteTextAsync(file, text);
                }
            }
        }

        public async Task<string> ReadText(string fileName)
        {
            var storageFile = await GetStorageFile(fileName);
            if (storageFile != null)
            {
                string fileContent = await FileIO.ReadTextAsync(storageFile);
                return fileContent;
            }
            return "";
        }

        public static TextFileAccess GetInstance()
        {
            return Singleton<TextFileAccess>.Instance;
        }
    }
}
