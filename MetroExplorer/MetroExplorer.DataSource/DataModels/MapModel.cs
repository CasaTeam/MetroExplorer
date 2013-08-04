namespace MetroExplorer.DataSource.DataModels
{
    using SQLite;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Media.Imaging;

    [Table("Maps")]
    public class MapModel : IEquatable<MapModel>
    {
        #region Properties

        [PrimaryKey]
        public Guid ID { get; private set; }

        public string Name { get; private set; }

        public string ThumbnailUri { get; private set; }

        [Ignore]
        public BitmapImage Thumbnail
        {
            get
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(ThumbnailUri));
                return bitmapImage;
            }
        }

        #endregion

        #region Constructors

        public MapModel() { }

        public MapModel(Guid id, string name, string thumbnailUri)
        {
            ID = id;
            Name = name;
            ThumbnailUri = thumbnailUri;
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
