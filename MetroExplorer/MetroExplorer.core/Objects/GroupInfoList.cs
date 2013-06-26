namespace MetroExplorer.Core.Objects
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Windows.UI.Xaml;

    public class GroupInfoList<T> : ObservableCollection<T>
    {
        public string Key { get; set; }

        public new IEnumerator<T> GetEnumerator()
        {
            return base.GetEnumerator();
        }
    }
}
