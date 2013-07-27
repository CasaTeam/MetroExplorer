namespace MetroExplorer.DataSource.DataControllers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataModels;
    using DataConfigurations;

    public interface IController<T>
    {
        Task<ObservableCollection<T>> GetSources(DataSourceType serviceName);

        Task Add(DataSourceType serviceName, T source);

        Task Remove(DataSourceType serviceName, T source);

        Task Update(DataSourceType serviceName, T source);
    }
}
