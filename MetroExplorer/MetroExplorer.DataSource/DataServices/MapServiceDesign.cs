namespace MetroExplorer.DataSource.DataServices
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.Storage;
    using Windows.Storage.FileProperties;
    using DataModels;
    using System.Collections.Generic;

    public class MapServiceDesign : IMapService
    {
        public async Task<ObservableCollection<MapModel>> Load()
        {
            string thumbnailPath = @"ms-appx:///MetroExplorer.Components.Maps/DesignAssets/MapBackground.bmp";

            return new ObservableCollection<MapModel>{
                new MapModel(Guid.NewGuid(),"Map1",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map2",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map3",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map4",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map5",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map6",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map7",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map8",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map9",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map10",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map11",thumbnailPath)
                ,new MapModel(Guid.NewGuid(),"Map12",thumbnailPath)};
        }

        public Task Add(MapModel map)
        {
            throw new NotImplementedException();
        }

        public Task Remove(MapModel map)
        {
            throw new NotImplementedException();
        }

        public Task Update(MapModel map)
        {
            throw new NotImplementedException();
        }


        public Task<ObservableCollection<MapLocationModel>> LoadLocations(Guid mapId)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddLocation(MapLocationModel mapLocation, Guid mapId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLocation(MapLocationModel mapLocation)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLocation(MapLocationModel mapLocation)
        {
            throw new NotImplementedException();
        }


        public async Task<ObservableCollection<MapLocationFolderModel>> LoadLocationFolders(Guid locationId)
        {
            return new ObservableCollection<MapLocationFolderModel>{
                new MapLocationFolderModel{ID = Guid.NewGuid(), Name= "Folder 1"},
                new MapLocationFolderModel{ID = Guid.NewGuid(), Name= "Folder 2"},
                new MapLocationFolderModel{ID = Guid.NewGuid(), Name= "Folder 3"},
                new MapLocationFolderModel{ID = Guid.NewGuid(), Name= "Folder 4"},
                new MapLocationFolderModel{ID = Guid.NewGuid(), Name= "Folder 5"}
            };
        }


        public Task<int> AddLocationFolder(MapLocationFolderModel locationFolder)
        {
            throw new NotImplementedException();
        }


        public Task RemoveLocationFolders(List<MapLocationFolderModel> locationFolder)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLocationFolder(MapLocationFolderModel locationFolder)
        {
            throw new NotImplementedException();
        }
    }
}
