namespace MetroExplorer.DataSource.DataControllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataModels;
    using DataConfigurations;

    public interface IController<T>
    {
        Task<IEnumerable<T>> GetSources(DataSourceType serviceName);
    }
}
