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
    using System.Collections.ObjectModel;

    [TemplatePart(Name = GridViewMapListElement, Type = typeof(GridView))]
    public sealed class MapList : Control
    {
        #region Contents

        internal const string GridViewMapListElement = "GridViewMapList";

        #endregion

        #region Fields

        private GridView _gridViewMapList;

        #endregion

        #region Properties

        public MapModel SelectedMap
        {
            get
            {
                if (_gridViewMapList != null && _gridViewMapList.SelectedItem != null)
                    return (MapModel)_gridViewMapList.SelectedItem;
                return null;
            }
        }

        public Visibility RemoveVisibility
        {
            get
            {
                return Visibility.Collapsed;
                //return SelectedMap != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ObservableCollection<MapModel> MapSource
        {
            get
            {
                return (ObservableCollection<MapModel>)GetValue(MapSourceProperty);
            }
            set
            {
                SetValue(MapSourceProperty, value);
            }
        }

        #endregion

        #region Dependency Properties

        public static DependencyProperty MapSourceProperty = DependencyProperty
            .Register("MapSource", typeof(ObservableCollection<MapModel>), typeof(MapList), null);

        #endregion

        #region Constructors

        public MapList()
        {
            this.DefaultStyleKey = typeof(MapList);
        }

        #endregion

        #region Override Methods

        protected async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _gridViewMapList = (GridView)GetTemplateChild(GridViewMapListElement);
            if (_gridViewMapList != null)
            {
                if (DesignMode.DesignModeEnabled)
                {
                    DataAccess<MapModel> dataAccess = new DataAccess<MapModel>();

                    _gridViewMapList.ItemsSource = await dataAccess.GetSources(DataSourceType.Design);
                }
            }
        }

        #endregion
    }
}
