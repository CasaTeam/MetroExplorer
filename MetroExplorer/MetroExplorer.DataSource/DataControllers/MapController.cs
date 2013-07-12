namespace MetroExplorer.DataSource.Maps.DataControllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using DataModels;
    using DataServices;
    using DataSource.DataControllers;
    using DataSource.DataConfigurations;

    public class MapController : IController<MapModel>
    {
        [Dependency("MapServiceDesign")]
        public IMapService MapServiceDesign { get; set; }

        public IEnumerable<MapModel> GetSources(DataSourceType serviceName)
        {
            switch (serviceName)
            {
                case DataSourceType.Design:
                    return MapServiceDesign.GenerateMapModels();
                default:
                    return null;
            }
        }
    }
}
