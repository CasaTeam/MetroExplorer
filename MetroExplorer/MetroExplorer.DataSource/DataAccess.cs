namespace MetroExplorer.DataSource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using DataControllers;
    using DataModels;
    using DataServices;
    using DataConfigurations;

    public class DataAccess<T>
    {
        private IController<T> _controller;

        public DataAccess()
        {
            IUnityContainer container = new UnityContainer();
            container = new UnityContainer();
            if (typeof(T) == typeof(MapModel))
            {
                container.RegisterType<IMapService, MapServiceDesign>(typeof(MapServiceDesign).Name)
                         .RegisterType<IMapService, MapServiceSQLite>(typeof(MapServiceSQLite).Name);
                _controller = (IController<T>)container.Resolve<MapController>();
            }
        }

        public async Task<IEnumerable<T>> GetSources(DataSourceType dataSourceType)
        {
            if (_controller != null)
                return await _controller.GetSources(dataSourceType);

            return null;
        }

        public async Task Add(DataSourceType dataSourceType, MapModel map)
        {
            if (_controller != null)
                await _controller.Add(dataSourceType, map);
        }

        public async Task Remove(DataSourceType dataSourceType, MapModel map)
        {
            if (_controller != null)
                await _controller.Remove(dataSourceType, map);
        }

        public async Task Update(DataSourceType dataSourceType, MapModel map)
        {
            if (_controller != null)
                await _controller.Update(dataSourceType, map);
        }
    }
}