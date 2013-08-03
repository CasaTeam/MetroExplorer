namespace MetroExplorer.DataSource.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SQLite;

    [Table("MapLocationFolders")]
    public class MapLocationFolderModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [Indexed]
        public Guid MapLocationId { get; set; }

        public string Token { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
