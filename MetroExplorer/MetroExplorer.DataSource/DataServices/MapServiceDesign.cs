namespace MetroExplorer.DataSource.Maps.DataServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataModels;

    public class MapServiceDesign : IMapService
    {
        public IEnumerable<MapModel> GenerateMapModels()
        {
            return new List<MapModel>{
                new MapModel(Guid.NewGuid(),"Map1",@"C:\Users\Sawyer\Pictures\(1).bmp"),
                new MapModel(Guid.NewGuid(),"Map2",@"C:\Users\Sawyer\Pictures\(8).jpg"),
                new MapModel(Guid.NewGuid(),"Map3",@"C:\Users\Sawyer\Pictures\(9).jpg")
            };
        }
    }
}
