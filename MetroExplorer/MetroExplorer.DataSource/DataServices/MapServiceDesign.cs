namespace MetroExplorer.DataSource.Maps.DataServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataModels;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.Storage;
    using Windows.Storage.FileProperties;

    public class MapServiceDesign : IMapService
    {
        public IEnumerable<MapModel> GenerateMapModels()
        {
            string thumbnailPath = @"ms-appx:///MetroExplorer.Components.Maps/DesignAssets/MapBackground.bmp";

            return new List<MapModel>{
                new MapModel(Guid.NewGuid(),"Map1",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map2",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map3",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map4",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map5",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map6",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map7",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map8",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map9",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map10",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map11",thumbnailPath, GenerateThumbnail(thumbnailPath))
                ,new MapModel(Guid.NewGuid(),"Map12",thumbnailPath, GenerateThumbnail(thumbnailPath))
            };
        }

        private BitmapImage GenerateThumbnail(string uri)
        {
            try
            {
                //BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:/" + url));
                //StorageFile storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
                //StorageItemThumbnail fileThumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 800);
                BitmapImage bitmapImage = new BitmapImage(new Uri(uri));
                //bitmapImage.SetSource(fileThumbnail);
                return bitmapImage;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
