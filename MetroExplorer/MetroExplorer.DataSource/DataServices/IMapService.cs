namespace MetroExplorer.DataSource.DataServices
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataModels;
    using System.Collections.Generic;

    public interface IMapService
    {
        Task<ObservableCollection<MapModel>> Load();

        Task Add(MapModel map);

        Task Remove(MapModel map);

        Task Update(MapModel map);


        Task<ObservableCollection<MapLocationModel>> LoadLocations(Guid mapId);

        Task<int> AddLocation(MapLocationModel mapLocation, Guid mapId);

        Task RemoveLocation(MapLocationModel mapLocation);

        Task UpdateLocation(MapLocationModel mapLocation);


        Task<ObservableCollection<MapLocationFolderModel>> LoadLocationFolders(Guid locationId);

        Task<int> AddLocationFolder(MapLocationFolderModel locationFolder);

        Task RemoveLocationFolders(List<MapLocationFolderModel> locationFolders);

        Task UpdateLocationFolder(MapLocationFolderModel locationFolder);
    }
}
