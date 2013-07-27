namespace MetroExplorer.DataSource.DataServices
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SQLite;
    using DataModels;
    using DataConfigurations;

    public class MapServiceSQLite : IMapService
    {
        private ObservableCollection<MapModel> _dataSources;

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

        public async Task<ObservableCollection<MapModel>> Load()
        {
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(SQLiteConfiguration.ConnectionString);

            _dataSources = new ObservableCollection<MapModel>(await connection.QueryAsync<MapModel>("SELECT * FROM Maps"));

            return _dataSources;
        }

        public Task Add(MapModel map)
        {
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(SQLiteConfiguration.ConnectionString);
            _dataSources.Add(map);
            return connection.InsertAsync(map);
        }

        public Task Remove(MapModel map)
        {
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(SQLiteConfiguration.ConnectionString);
            _dataSources.Remove(map);
            return connection.DeleteAsync(map);
        }

        public Task Update(MapModel map)
        {
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(SQLiteConfiguration.ConnectionString);
            _dataSources.Remove(_dataSources.First(source => source.ID.Equals(map.ID)));
            _dataSources.Add(map);
            return connection.UpdateAsync(map);
        }
    }
}
