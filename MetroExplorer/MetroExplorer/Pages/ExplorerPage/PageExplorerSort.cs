namespace MetroExplorer.Pages.ExplorerPage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.Storage.FileProperties;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Imaging;
    using Core;
    using Core.Objects;

    public sealed partial class PageExplorer
    {
        private Dictionary<SortType, SortOrderType> _sortedRecoder = new Dictionary<SortType, SortOrderType>();

        private void Button_Sort_Click(object sender, RoutedEventArgs e)
        {
            Popup_Sort.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_Sort.Margin = new Thickness(0, 0, 268, 285);
            Popup_Sort.IsOpen = true;
        }

        private void ListBox_Sorte_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) return;
            Popup_Sort.IsOpen = false;
            Popup_Sort.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 0)
                SortItems(SortType.Date);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 1)
                SortItems(SortType.Name);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 2)
                SortItems(SortType.Size);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 3)
                SortItems(SortType.Type);
            if (ListBox_Sorte.Items.IndexOf((sender as ListBox).SelectedItem) == 4)
                SortItems(SortType.None);
            ListBox_Sorte.SelectedItem = null;
        }

        private void SortItems(SortType sortType)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource = null;
            if (sortType == SortType.Date)
                sortedSource = SortByDate(ExplorerItems);
            else if (sortType == SortType.Name)
                sortedSource = SortByName(ExplorerItems);
            else if (sortType == SortType.Size)
                sortedSource = SortBySize(ExplorerItems);
            else if (sortType == SortType.Type)
                sortedSource = SortByType(ExplorerItems);
            else if (sortType == SortType.None)
                return;
            if (sortedSource != null)
            {
                RerangeDataSource(sortedSource);
                _counterForLoadUnloadedItems = 0;
            }
        }

        private IOrderedEnumerable<ExplorerItem> SortByDate(IEnumerable<ExplorerItem> items)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource;
            if (_sortedRecoder.Keys.Contains(SortType.Date))
            {
                if (_sortedRecoder[SortType.Date] == SortOrderType.Ascend)
                {
                    sortedSource = items.OrderByDescending(p => p.ModifiedDateTime);
                    _sortedRecoder[SortType.Date] = SortOrderType.Descend;
                }
                else
                {
                    sortedSource = items.OrderBy(p => p.ModifiedDateTime);
                    _sortedRecoder[SortType.Date] = SortOrderType.Ascend;
                }
            }
            else
            {
                sortedSource = items.OrderBy(p => p.ModifiedDateTime);
                _sortedRecoder.Add(SortType.Date, SortOrderType.Ascend);
            }
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByName(IEnumerable<ExplorerItem> items)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource;
            if (_sortedRecoder.Keys.Contains(SortType.Name))
            {
                if (_sortedRecoder[SortType.Name] == SortOrderType.Ascend)
                {
                    sortedSource = items.OrderByDescending(p => p.Name);
                    _sortedRecoder[SortType.Name] = SortOrderType.Descend;
                }
                else
                {
                    sortedSource = items.OrderBy(p => p.Name);
                    _sortedRecoder[SortType.Name] = SortOrderType.Ascend;
                }
            }
            else
            {
                sortedSource = items.OrderBy(p => p.Name);
                _sortedRecoder.Add(SortType.Name, SortOrderType.Ascend);
            }
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortBySize(IEnumerable<ExplorerItem> items)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource;
            if (_sortedRecoder.Keys.Contains(SortType.Size))
            {
                if (_sortedRecoder[SortType.Size] == SortOrderType.Ascend)
                {
                    sortedSource = items.OrderByDescending(p => p.Size);
                    _sortedRecoder[SortType.Size] = SortOrderType.Descend;
                }
                else
                {
                    sortedSource = items.OrderBy(p => p.Size);
                    _sortedRecoder[SortType.Size] = SortOrderType.Ascend;
                }
            }
            else
            {
                sortedSource = items.OrderBy(p => p.Size);
                _sortedRecoder.Add(SortType.Size, SortOrderType.Ascend);
            }
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByType(IEnumerable<ExplorerItem> items)
        {
            IOrderedEnumerable<ExplorerItem> sortedSource;
            if (_sortedRecoder.Keys.Contains(SortType.Type))
            {
                if (_sortedRecoder[SortType.Type] == SortOrderType.Ascend)
                {
                    sortedSource = items.OrderByDescending(p => p.Type);
                    _sortedRecoder[SortType.Type] = SortOrderType.Descend;
                }
                else
                {
                    sortedSource = items.OrderBy(p => p.Type);
                    _sortedRecoder[SortType.Type] = SortOrderType.Ascend;
                }
            }
            else
            {
                sortedSource = items.OrderBy(p => p.Type);
                _sortedRecoder.Add(SortType.Type, SortOrderType.Ascend);
            }
            return sortedSource;
        }

        private IOrderedEnumerable<ExplorerItem> SortByNone(IEnumerable<ExplorerItem> items)
        {
            var sortedSource = items as IOrderedEnumerable<ExplorerItem>;
            return sortedSource;
        }

        private void RerangeDataSource(IOrderedEnumerable<ExplorerItem> sortedSource)
        {
            if (ExplorerItems == null ) return;
            List<ExplorerItem> sortedItems = new List<ExplorerItem>();
            foreach (var item in sortedSource)
            {
                sortedItems.Add(item);
            }
            ExplorerItems.Clear();
            foreach (var item in sortedItems)
            {
                ExplorerItems.Add(item);
            }
        }
    }

    public enum SortType
    {
        Date,
        Name,
        Size,
        Type,
        None
    }

    public enum SortOrderType
    {
        Ascend,
        Descend
    }
}
