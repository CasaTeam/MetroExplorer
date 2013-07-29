namespace MetroExplorer.DataSource.DataServices
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataModels;

    public interface IMapService
    {
        Task<ObservableCollection<MapModel>> Load();

        Task Add(MapModel map);

        Task Remove(MapModel map);

        Task Update(MapModel map);


        Task<ObservableCollection<MapLocationModel>> LoadLocations(Guid mapId);

        Task AddLocation(MapLocationModel mapLocation, Guid mapId);

        Task RemoveLocation(MapLocationModel mapLocation);

        Task UpdateLocation(MapLocationModel mapLocation);
    }
}
