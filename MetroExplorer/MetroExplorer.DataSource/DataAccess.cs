namespace MetroExplorer.DataSource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using Maps.DataControllers;
    using Maps.DataServices;

    public static class DataAccess
    {
        public static MapController GetMapController()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<IMapService, MapServiceDesign>(typeof(MapServiceDesign).Name);
            return container.Resolve<MapController>();
        }
    }
}