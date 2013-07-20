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
        Task<IEnumerable<MapModel>> Load();

        Task Add(MapModel map);

        Task Remove(MapModel map);

        Task Update(MapModel map);
    }
}
