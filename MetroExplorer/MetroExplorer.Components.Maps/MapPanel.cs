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

    [TemplatePart(Name = MapListElement, Type = typeof(MapList))]
    public sealed class MapPanel : Control
    {
        #region Contents

        internal const string MapListElement = "MapList";

        #endregion

        #region Fields

        private MapList _mapList;

        #endregion

        #region Constructors
        
        public MapPanel()
        {
            this.DefaultStyleKey = typeof(MapPanel);
        }

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _mapList = (MapList)GetTemplateChild(MapListElement);
            if(_mapList!=null)
                _mapList.SelectionChanged += MapListSelectionChanged;
        }

        void MapListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _mapList.Height = 200.0;
            VisualStateManager.GoToState(this, "ShowMap", true);
        }

        #endregion
    }
}
