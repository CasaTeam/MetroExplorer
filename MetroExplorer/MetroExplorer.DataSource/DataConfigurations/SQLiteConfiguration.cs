namespace MetroExplorer.DataSource.DataConfigurations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;

    public static class SQLiteConfiguration
    {
        public static readonly string ConnectionString = Path.Combine(ApplicationData.Current.LocalFolder.Path, "map.sqlite");
    }
}
