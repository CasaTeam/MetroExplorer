namespace MetroExplorer.DataSource.DataServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SQLite;
    using DataModels;
    using DataConfigurations;

    public class MapServiceSQLite : IMapService
    {
        public MapServiceSQLite()
        {
            using (SQLiteConnection connection = new SQLiteConnection(SQLiteConfiguration.ConnectionString))
                connection.CreateTable<MapModel>();
        }

        public IEnumerable<MapModel> Load()
        {
            return null;
        }
    }
}
