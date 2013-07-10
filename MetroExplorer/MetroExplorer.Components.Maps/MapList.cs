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

    public sealed class MapList : Control
    {
        #region Fields

        private MapController _mapController;

        private IEnumerable<MapModel> _dataSource;

        #endregion

        #region Constructors

        public MapList()
        {
            this.DefaultStyleKey = typeof(MapList);
            _mapController = DataAccess.GetMapController();
            _dataSource = _mapController.GetSources("MapServiceDesign");
        }

        #endregion
    }
}
