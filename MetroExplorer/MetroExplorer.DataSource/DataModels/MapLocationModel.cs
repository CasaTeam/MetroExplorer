namespace MetroExplorer.DataSource.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SQLite;

    [Table("MapLocations")]
    public class MapLocationModel : IEquatable<MapLocationModel>
    {
        #region Properties

        [PrimaryKey]
        public Guid ID { get; set; }

        [Indexed]
        public Guid MapId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }


        #endregion

        public bool Equals(MapLocationModel other)
        {
            return ID.Equals(other.ID);
        }
    }
}
