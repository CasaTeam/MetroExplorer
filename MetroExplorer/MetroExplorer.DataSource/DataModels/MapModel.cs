namespace MetroExplorer.DataSource.Maps.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MapModel : IEquatable<MapModel>
    {
        #region Properties

        public Guid ID { get; private set; }

        public string Name { get; private set; }

        public string ThumbnailSource { get; private set; }

        #endregion

        #region Constructors

        public MapModel(Guid id, string name, string thumbnailSource)
        {
            ID = id;
            Name = name;
            ThumbnailSource = thumbnailSource;
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
