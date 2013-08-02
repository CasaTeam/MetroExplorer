namespace MetroExplorer.Components.Maps
{
    using System;
    using System.Collections.Generic;
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

        public MapFolderList()
        {
            this.DefaultStyleKey = typeof(MapFolderList);
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
            }
        }
    }
}
