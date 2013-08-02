namespace MetroExplorer.DataSource.DataControllers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using DataConfigurations;
    using DataModels;
    using DataServices;

    public class MapLocationFolderController : IController<MapLocationFolderModel>
    {
        [Dependency("MapServiceDesign")]
        public IMapService MapServiceDesign { get; set; }

        [Dependency("MapServiceSQLite")]
        public IMapService MapServiceSQLite { get; set; }

        public async Task<ObservableCollection<MapLocationFolderModel>> GetSources(DataSourceType serviceName)
        {
            switch (serviceName)
            {
                case DataSourceType.Design:
                    return await MapServiceDesign.LoadLocationFolders(Guid.NewGuid());
                case DataSourceType.Sqlite:
                    return await MapServiceSQLite.LoadLocationFolders(Guid.NewGuid());
                default:
                    return null;
            }
        }

        public Task Add(DataSourceType serviceName, MapLocationFolderModel source)
        {
            throw new NotImplementedException();
        }

        public Task Remove(DataSourceType serviceName, MapLocationFolderModel source)
        {
            throw new NotImplementedException();
        }

        public Task Update(DataSourceType serviceName, MapLocationFolderModel source)
        {
            throw new NotImplementedException();
        }
    }
}
