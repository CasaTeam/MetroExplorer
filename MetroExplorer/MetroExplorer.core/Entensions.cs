using System;
using System.Linq;
using MetroExplorer.core.Objects;
using Windows.Storage;

namespace MetroExplorer.core
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;

    public static class Entensions
    {
        public static DependencyObject GetParentByName(this DependencyObject obj, string name)
        {
            DependencyObject objFind = VisualTreeHelper.GetParent(obj);
            if (((FrameworkElement)objFind).Name == name)
                return objFind;
            return objFind.GetParentByName(name);
        }

        public async static void AddItem(this GroupInfoList<ExplorerItem> itemList,
            IStorageItem retrievedItem)
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
    }
}
