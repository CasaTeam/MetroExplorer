namespace MetroExplorer.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using DataSource;
    using DataSource.Maps.DataControllers;
    using DataSource.Maps.DataModels;
    using DataSource.DataConfigurations;

    [TestClass]
    public class DataSourceMapTest
    {
        [TestMethod]
        public void DataSourceDesign()
        {
            IEnumerable<MapModel> expacted = new List<MapModel>{
                new MapModel(Guid.NewGuid(),"Map1",@"C:\Users\Sawyer\Pictures\(1).bmp", null),
                new MapModel(Guid.NewGuid(),"Map2",@"C:\Users\Sawyer\Pictures\(8).jpg", null),
                new MapModel(Guid.NewGuid(),"Map3",@"C:\Users\Sawyer\Pictures\(9).jpg", null)
            };
            DataAccess<MapModel> dataAccess = new DataAccess<MapModel>();
            IEnumerable<MapModel> sources = dataAccess.GetSources(DataSourceType.Design);
            Assert.AreEqual(expacted.Count(), sources.Count());
        }
    }
}
