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

    public class MapController
    {
        [Dependency("MapServiceDesign")]
        public IMapService MapServiceDesign { get; set; }

        public IEnumerable<MapModel> GetSources(string serviceName)
        {
            switch (serviceName)
            {
                case "MapServiceDesign":
                    return MapServiceDesign.GenerateMapModels();
                default:
                    return null;
            }
        }
    }
}
