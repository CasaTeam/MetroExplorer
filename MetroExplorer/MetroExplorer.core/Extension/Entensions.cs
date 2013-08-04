namespace MetroExplorer.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;
    using Objects;

    /// <summary>
    /// 各种帮助工程的extension
    /// 目前没有特别区分
    /// </summary>
    public static class Entensions
    {
        public async static void AffactItem(this ExplorerItem item, IStorageItem realItem)
        {
            item.Name = realItem.Name;
            item.Path = realItem.Path;

            if (realItem is StorageFolder)
            {
                item.StorageFolder = realItem as StorageFolder;
                item.Type = ExplorerItemType.Folder;
            }
            else if (realItem is StorageFile)
            {
                item.StorageFile = realItem as StorageFile;
                item.Type = ExplorerItemType.File;
                item.Size = (await item.StorageFile.GetBasicPropertiesAsync()).Size;
                item.ModifiedDateTime = (await item.StorageFile.GetBasicPropertiesAsync()).DateModified.DateTime;
            }
        }

        public async static void AddItem(this GroupInfoList<ExplorerItem> itemList, IStorageItem retrievedItem)
        {
            ExplorerItem item = new ExplorerItem
                {
                    Name = retrievedItem.Name,
                    Path = retrievedItem.Path
                };
            if (retrievedItem is StorageFolder)
            {
                item.StorageFolder = retrievedItem as StorageFolder;
                item.Type = ExplorerItemType.Folder;
            }
            else if (retrievedItem is StorageFile)
            {
                item.StorageFile = retrievedItem as StorageFile;
                item.Type = ExplorerItemType.File;
                item.Size = (await item.StorageFile.GetBasicPropertiesAsync()).Size;
                item.ModifiedDateTime = (await item.StorageFile.GetBasicPropertiesAsync()).DateModified.DateTime;
            }
            if (itemList.All(p => p.Name != item.Name))
                itemList.Add(item);
        }

        public static void AddStorageItem(this ObservableCollection<ExplorerItem> itemList, StorageFolder retrievedItem)
        {
            ExplorerItem item = new ExplorerItem
            {
                Name = retrievedItem.Name,
                Path = retrievedItem.Path,
                StorageFolder = retrievedItem as StorageFolder,
                Type = ExplorerItemType.Folder
            };
            itemList.Insert(0, item);
        }

        public static void AddFileItem(this ObservableCollection<ExplorerItem> itemList, StorageFile retrievedItem)
        {
            ExplorerItem item = new ExplorerItem
            {
                Name = retrievedItem.Name,
                Path = retrievedItem.Path,
                StorageFile = retrievedItem,
                Type = ExplorerItemType.File,
            };
            itemList.Add(item);
        }

        public static DependencyObject GetParentByName(this DependencyObject obj, string name)
        {
            DependencyObject objFind = VisualTreeHelper.GetParent(obj);
            if (((FrameworkElement)objFind).Name == name)
                return objFind;
            return objFind.GetParentByName(name);
        }

        public static void AddFakeItem(this GroupInfoList<ExplorerItem> itemList)
        {
            itemList.Add(new ExplorerItem());
        }

        public static bool EqualTo(this StorageFolder folder, StorageFolder other)
        {
            return folder.DateCreated == other.DateCreated && folder.Name == other.Name && folder.Path == other.Path;
        }
    }
}
