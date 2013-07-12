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
    using DataSource.Maps.DataControllers;
    using DataSource.Maps.DataModels;
    using DataSource.DataConfigurations;

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

        #region Override Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DataAccess<MapModel> dataAccess = new DataAccess<MapModel>();
            _dataSource = dataAccess.GetSources(DataSourceType.Design);

            _gridViewMapList = (GridView)GetTemplateChild(GridViewMapListElement);
            if (_gridViewMapList != null)
                _gridViewMapList.ItemsSource = _dataSource;
        }

        #endregion
    }
}
