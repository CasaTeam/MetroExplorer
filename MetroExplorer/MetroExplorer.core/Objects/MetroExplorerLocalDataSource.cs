using System.Collections.ObjectModel;

namespace MetroExplorer.core.Objects
{
    using System.Collections.Generic;
    using System.Linq;
    using Windows.Storage;

    public class MetroExplorerLocalDataSource
    {
        private IList<StorageFolder> _navigatorStorageFolders;

        public IList<StorageFolder> NavigatorStorageFolders
        {
            get { return _navigatorStorageFolders; }
            set
            {
                _navigatorStorageFolders = value;
            }
        }

        public StorageFolder CurrentStorageFolder
        {
            get { return _navigatorStorageFolders.LastOrDefault(); }
        }

        public bool FromSearch { get; set; }

        public ObservableCollection<ExplorerItem> SearchedItems { get; set; }

        private MetroExplorerLocalDataSource()
        {
            _navigatorStorageFolders = new List<StorageFolder>();
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

    }
}
