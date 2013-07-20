namespace MetroExplorer.DataSource.DataControllers
{
    using System;
    using System.Collections.Generic;
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

        public async Task<IEnumerable<MapModel>> GetSources(DataSourceType serviceName)
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
    }
}
