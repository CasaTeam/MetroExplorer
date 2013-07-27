namespace MetroExplorer.DataSource.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SQLite;

    [Table("MapLocations")]
    public class MapLocationModel
    {
        #region Properties

        [PrimaryKey]
        public Guid ID { get; set; }

        #endregion
    }
}
