namespace MetroExplorer.Pages.MapPage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using Windows.ApplicationModel;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using DataSource;
    using DataSource.DataConfigurations;
    using DataSource.DataModels;
    using Common;

    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PageMapList : LayoutAwarePage
    {
        private DataAccess<MapModel> _dataSourceMaps;

        public PageMapList()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="navigationParameter">Valeur de paramètre passée à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page.
        /// </param>
        /// <param name="pageState">Dictionnaire d'état conservé par cette page durant une session
        /// antérieure. Null lors de la première visite de la page.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            _dataSourceMaps = new DataAccess<MapModel>();
            DefaultViewModel["Maps"] = await _dataSourceMaps.GetSources(DataSourceType.Sqlite);
        }

        /// <summary>
        /// Conserve l'état associé à cette page en cas de suspension de l'application ou de la
        /// suppression de la page du cache de navigation. Les valeurs doivent être conformes aux
        /// exigences en matière de sérialisation de <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Dictionnaire vide à remplir à l'aide de l'état sérialisable.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {

        }

        private async void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            await _dataSourceMaps.Add(
                DataSourceType.Sqlite, 
                new MapModel(Guid.NewGuid(), "New Map", @"ms-appx:///MetroExplorer.Components.Maps/DesignAssets/MapBackground.bmp"));
        }

        private async void ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            if (MapListElement.SelectedMap != null)
                await _dataSourceMaps.Remove(DataSourceType.Sqlite, MapListElement.SelectedMap);
        }
    }
}
