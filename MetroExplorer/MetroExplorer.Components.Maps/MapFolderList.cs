namespace MetroExplorer.Components.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Documents;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using DataSource;
    using DataSource.DataModels;
    using DataSource.DataConfigurations;

    [TemplatePart(Name = ListBoxElement, Type = typeof(ListBox))]
    public sealed class MapFolderList : Control
    {
        internal const string ListBoxElement = "ListBoxElement";

        private ListBox _listBox;

        public event EventHandler SelectionChanged;

        #region properties

        public MapLocationFolderModel SelectedItem { get; private set; }

        public ObservableCollection<MapLocationFolderModel> MapLocationFolderSource
        {
            get
            {
                return (ObservableCollection<MapLocationFolderModel>)GetValue(MapLocationFolderSourceProperty);
            }
            set
            {
                SetValue(MapLocationFolderSourceProperty, value);
            }
        }

        #endregion

        #region Dependency Properties

        public static DependencyProperty MapLocationFolderSourceProperty = DependencyProperty
            .Register("MapLocationFolderSource", typeof(ObservableCollection<MapLocationFolderModel>), typeof(MapFolderList), null);

        #endregion

        public MapFolderList()
        {
            this.DefaultStyleKey = typeof(MapFolderList);
            SelectedItem = null;
        }

        protected async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listBox = (ListBox)GetTemplateChild(ListBoxElement);

            if (_listBox != null)
            {
                if (DesignMode.DesignModeEnabled)
                {
                    DataAccess<MapLocationFolderModel> dataAccess = new DataAccess<MapLocationFolderModel>();

                    _listBox.ItemsSource = await dataAccess.GetSources(DataSourceType.Design);
                }

                _listBox.SelectionChanged += ListBoxSelectionChanged;
            }
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = (MapLocationFolderModel)e.AddedItems.FirstOrDefault();
            if (SelectionChanged != null)
                SelectionChanged(this, new EventArgs());
        }
    }
}
