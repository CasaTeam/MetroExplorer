namespace MetroExplorer.DataSource.DataServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.Storage;
    using Windows.Storage.FileProperties;
    using DataModels;

    public class MapServiceDesign : IMapService
    {
        public Task<IEnumerable<MapModel>> Load()
        {
            string thumbnailPath = @"ms-appx:///MetroExplorer.Components.Maps/DesignAssets/MapBackground.bmp";


            return new Task<IEnumerable<MapModel>>(() =>
            {
                return new List<MapModel>{
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
           );
        }
    }
}
