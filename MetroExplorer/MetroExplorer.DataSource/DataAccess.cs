namespace MetroExplorer.DataSource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using Maps.DataControllers;
    using Maps.DataModels;
    using Maps.DataServices;
    using DataControllers;
    using DataSource.DataConfigurations;

    public class DataAccess<T>
    {
        private IController<T> _controller;

        public DataAccess()
        {
            IUnityContainer container = new UnityContainer();
            container = new UnityContainer();
            if (typeof(T) == typeof(MapModel))
            {
                container.RegisterType<IMapService, MapServiceDesign>(typeof(MapServiceDesign).Name);
                _controller = (IController<T>)container.Resolve<MapController>();
            }
        }

        public IEnumerable<T> GetSources(DataSourceType dataSourceType)
        {
            if (_controller != null)
                return _controller.GetSources(dataSourceType);

            return null;
        }
    }
}