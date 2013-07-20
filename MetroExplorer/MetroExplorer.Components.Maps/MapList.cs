namespace MetroExplorer.Components.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Documents;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using DataSource;
    using DataSource.DataControllers;
    using DataSource.DataModels;
    using DataSource.DataConfigurations;
    using Windows.ApplicationModel;

    [TemplatePart(Name = GridViewMapListElement, Type = typeof(GridView))]
    public sealed class MapList : Control
    {
        #region Contents

        internal const string GridViewMapListElement = "GridViewMapList";

        #endregion

        #region Fields

        private IEnumerable<MapModel> _dataSource;

        private GridView _gridViewMapList;

        #endregion

        #region Constructors

        public MapList()
        {
            this.DefaultStyleKey = typeof(MapList);
        }

        #endregion

        #region EventHandlers

        public event SelectionChangedEventHandler SelectionChanged;

        #endregion

        #region Override Methods

        protected override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DataAccess<MapModel> dataAccess = new DataAccess<MapModel>();

            _dataSource = await dataAccess.GetSources(
                DesignMode.DesignModeEnabled ? DataSourceType.Design : DataSourceType.Sqlite);

            _gridViewMapList = (GridView)GetTemplateChild(GridViewMapListElement);
            if (_gridViewMapList != null)
            {
                _gridViewMapList.ItemsSource = _dataSource;
                _gridViewMapList.SelectionChanged += GridViewMapListSelectionChanged;
            }
        }

        void GridViewMapListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        #endregion
    }
}
