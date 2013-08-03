namespace MetroExplorer.DataSource.DataControllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using DataModels;
    using DataControllers;
    using DataConfigurations;
    using DataServices;

    public class MapController : IController<MapModel>
    {
        [Dependency("MapServiceDesign")]
        public IMapService MapServiceDesign { get; set; }

        [Dependency("MapServiceSQLite")]
        public IMapService MapServiceSQLite { get; set; }

        public async Task<ObservableCollection<MapModel>> GetSources(DataSourceType serviceName)
        {
            switch (serviceName)
            {
                case DataSourceType.Design:
                    return await MapServiceDesign.Load();
                case DataSourceType.Sqlite:
                    return await MapServiceSQLite.Load();
                default:
                    return null;
            }
        }

        public async Task Add(DataSourceType serviceName, MapModel map)
        {
            switch (serviceName)
            {
                case DataSourceType.Design:
                    await MapServiceDesign.Add(map);
                    break;
                case DataSourceType.Sqlite:
                    await MapServiceSQLite.Add(map);
                    break;
                default:
                    return;
            }
        }

        public async Task Remove(DataSourceType serviceName, MapModel map)
        {
            switch (serviceName)
            {
                case DataSourceType.Design:
                    await MapServiceDesign.Remove(map);
                    break;
                case DataSourceType.Sqlite:
                    await MapServiceSQLite.Remove(map);
                    break;
                default:
                    return;
            }
        }

        public async Task Update(DataSourceType serviceName, MapModel map)
        {
            switch (serviceName)
            {
                case DataSourceType.Design:
                    await MapServiceDesign.Update(map);
                    break;
                case DataSourceType.Sqlite:
                    await MapServiceSQLite.Update(map);
                    break;
                default:
                    return;
            }
        }


        public Task RemoveMany(DataSourceType serviceName, List<MapModel> sources)
        {
            throw new NotImplementedException();
        }
    }
}
