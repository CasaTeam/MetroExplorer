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

        public Guid MapId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }


        #endregion

        public bool Equals(MapLocationModel other)
        {
            return ID.Equals(other.MapId);
        }
    }
}
