namespace MetroExplorer.DataSource.DataControllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Maps.DataModels;
    using DataSource.DataConfigurations;

    public interface IController<T>
    {
        IEnumerable<T> GetSources(DataSourceType serviceName);
    }
}
