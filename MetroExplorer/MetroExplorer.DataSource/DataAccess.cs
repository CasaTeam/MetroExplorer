namespace MetroExplorer.DataSource
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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

        public Guid MapId { get; set; }

        public Guid MapLocationId { get; set; }

        public DataAccess()
        {
            IUnityContainer container = new UnityContainer();
            container = new UnityContainer();
            container.RegisterType<IMapService, MapServiceDesign>(typeof(MapServiceDesign).Name)
                     .RegisterType<IMapService, MapServiceSQLite>(typeof(MapServiceSQLite).Name);

            if (typeof(T) == typeof(MapModel))
                _controller = (IController<T>)container.Resolve<MapController>();
            if (typeof(T) == typeof(MapLocationModel))
                _controller = (IController<T>)container.Resolve<MapLocationController>();
            if (typeof(T) == typeof(MapLocationFolderModel))
                _controller = (IController<T>)container.Resolve<MapLocationFolderController>();
        }

        public async Task<ObservableCollection<T>> GetSources(DataSourceType dataSourceType)
        {
            if (_controller != null)
            {
                if (typeof(T) == typeof(MapLocationModel))
                    ((MapLocationController)_controller).MapId = MapId;
                else if (typeof(T) == typeof(MapLocationFolderModel))
                    ((MapLocationFolderController)_controller).MapLocationId = MapLocationId;
                return await _controller.GetSources(dataSourceType);
            }

            return null;
        }

        public async Task Add(DataSourceType dataSourceType, T source)
        {
            if (_controller != null)
                if (typeof(T) == typeof(MapModel))
                    await _controller.Add(dataSourceType, source);
                else if (typeof(T) == typeof(MapLocationModel))
                {
                    MapLocationController controller = (MapLocationController)_controller;
                    controller.MapId = MapId;
                    await _controller.Add(DataSourceType.Sqlite, source);
                }
                else if (typeof(T) == typeof(MapLocationFolderModel))
                {
                    MapLocationFolderController controller = (MapLocationFolderController)_controller;
                    controller.MapLocationId = MapLocationId;
                    await _controller.Add(DataSourceType.Sqlite, source);
                }
        }

        public async Task Remove(DataSourceType dataSourceType, T source)
        {
            if (_controller != null)
                await _controller.Remove(dataSourceType, source);
        }

        public async Task RemoveMany(DataSourceType dataSourceType, List<T> sources)
        {
            if (_controller != null)
                await _controller.RemoveMany(dataSourceType, sources);
        }

        public async Task Update(DataSourceType dataSourceType, T source)
        {
            if (_controller != null)
                await _controller.Update(dataSourceType, source);
        }
    }
}