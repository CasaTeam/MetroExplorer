namespace MetroExplorer.DataSource.Maps.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.UI.Xaml.Media.Imaging;

    public class MapModel : IEquatable<MapModel>
    {
        #region Properties

        public Guid ID { get; private set; }

        public string Name { get; private set; }

        public string ThumbnailUri { get; private set; }

        public BitmapImage Thumbnail { get; private set; }

        #endregion

        #region Constructors

        public MapModel(Guid id, string name, string thumbnailUri, BitmapImage thumbnail)
        {
            ID = id;
            Name = name;
            ThumbnailUri = thumbnailUri;
            Thumbnail = thumbnail;
        }

        #endregion

        #region Override IEquatable

        public bool Equals(MapModel other)
        {
            return ID.Equals(other.ID);
        }

        #endregion
    }
}
