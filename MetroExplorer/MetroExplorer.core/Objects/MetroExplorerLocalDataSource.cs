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

        private MetroExplorerLocalDataSource() { }

        public string GetPath()
        {
            return _navigatorStorageFolders.First().Path.Contains(":")
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
