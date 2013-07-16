namespace MetroExplorer.DataSource.DataServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataModels;

    public interface IMapService
    {
        IEnumerable<MapModel> Load();
    }
}
