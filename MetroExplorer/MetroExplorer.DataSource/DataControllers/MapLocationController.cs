namespace MetroExplorer.DataSource.DataControllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataConfigurations;
    using Microsoft.Practices.Unity;
    using DataServices;
    using DataModels;

    public class MapLocationController : IController<MapLocationModel>
    {
        [Dependency("MapServiceSQLite")]
        public IMapService MapServiceSQLite { get; set; }

        public Guid MapId { get; set; }

        public async Task<ObservableCollection<MapLocationModel>> GetSources(DataSourceType serviceName)
        {
            return await MapServiceSQLite.LoadLocations(MapId);
        }

        public async Task Add(DataSourceType serviceName, MapLocationModel location)
        {
            await MapServiceSQLite.AddLocation(location, MapId);
        }

        public async Task Remove(DataSourceType serviceName, MapLocationModel location)
        {
            await MapServiceSQLite.RemoveLocation(location);
        }

        public async Task Update(DataSourceType serviceName, MapLocationModel location)
        {
            await MapServiceSQLite.UpdateLocation(location);
        }


        public Task RemoveMany(DataSourceType serviceName, List<MapLocationModel> sources)
        {
            throw new NotImplementedException();
        }
    }
}
