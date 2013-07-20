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
    using System.Collections.ObjectModel;

    public class MapServiceSQLite : IMapService
    {
        public MapServiceSQLite()
        {
            using (SQLiteConnection connection = new SQLiteConnection(SQLiteConfiguration.ConnectionString))
            {
                connection.CreateTable<MapModel>();
                if (connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Maps") == 0)
                    connection.RunInTransaction(() =>
                    {
                        connection.Insert(new MapModel(Guid.NewGuid(), "Default Map", @"ms-appx:///MetroExplorer.Components.Maps/DesignAssets/MapBackground.bmp"));
                    });
            }
        }

        public async Task<IEnumerable<MapModel>> Load()
        {
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(SQLiteConfiguration.ConnectionString);
            return new List<MapModel>(await connection.QueryAsync<MapModel>("SELECT * FROM Maps"));
        }
    }
}
