namespace MetroExplorer.Core.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Windows.Storage;

    /// <summary>
    /// 用于保存Navigator, Share 和 Search状态的对象
    /// 并且帮助页面切换更好地传值
    /// </summary>
    public class MetroExplorerLocalDataSource
    {
        private IList<StorageFolder> _navigatorStorageFolders;
        private IReadOnlyList<IStorageItem> _shareStorageItems;

        public IList<StorageFolder> NavigatorStorageFolders
        {
            get { return _navigatorStorageFolders; }
            set
            {
                _navigatorStorageFolders = value;
            }
        }

        public IReadOnlyList<IStorageItem> ShareStorageItems
        {
            get
            {
                return _shareStorageItems;
            }
            set { _shareStorageItems = value; }
        }

        public StorageFolder CurrentStorageFolder
        {
            get { return _navigatorStorageFolders.LastOrDefault(); }
        }

        public Guid? FocusedLocationId { get; set; }

        public IList<StorageFolder> SelectedStorageFolders { get; set; }

        public bool FromSearch { get; set; }

        public ObservableCollection<ExplorerItem> SearchedItems { get; set; }

        private MetroExplorerLocalDataSource()
        {
            _navigatorStorageFolders = new List<StorageFolder>();
            _shareStorageItems = new List<IStorageItem>();
        }

        public string GetPath()
        {
            return _navigatorStorageFolders.First().FolderRelativeId.Split('\\')[0] == "0"
                ? CurrentStorageFolder.Path :
                _navigatorStorageFolders.Aggregate(string.Empty,
                (current, next) =>
                {
                    current += next.Name + "\\";
                    return current;
                }
                );
        }

        public void CutNavigatorFromIndex(int index)
        {
            _navigatorStorageFolders =
            _navigatorStorageFolders.Take(index + 1).ToList();
        }

        public void ToPreviousFolder()
        {
            if (_navigatorStorageFolders.Count > 1)
            {
                _navigatorStorageFolders = _navigatorStorageFolders
                    .Take(_navigatorStorageFolders.Count - 1).ToList<StorageFolder>();
            }
        }
    }
}
