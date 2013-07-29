namespace MetroExplorer.Pages.MapPage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using Windows.ApplicationModel.Search;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using Bing.Maps.Search;
    using Bing.Maps;
    using Common;
    using Components.Maps;
    using DataSource;
    using DataSource.DataConfigurations;
    using DataSource.DataModels;
    using MainPage;

    public sealed partial class PageMap : LayoutAwarePage
    {
        private SearchPane _searchPane;
        private SearchManager _searchManager;
        private LocationDataResponse _searchResponse;

        private MapModel _map;
        private ObservableCollection<MapLocationModel> _mapLocations;

        private DataAccess<MapModel> _mapDataAccess;
        private DataAccess<MapLocationModel> _mapLocationAccess;

        private ObservableCollection<MapPin> _mapPins;

        private MapPin _focusedMapPin;

        public PageMap()
        {
            this.InitializeComponent();

            _searchPane = SearchPane.GetForCurrentView();
            _searchPane.PlaceholderText = "Please enter your address";
            _searchPane.ShowOnKeyboardInput = true;
            _searchPane.SearchHistoryEnabled = false;

            _mapDataAccess = new DataAccess<MapModel>();
            _mapLocationAccess = new DataAccess<MapLocationModel>();

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
            ObservableCollection<MapModel> maps = await _mapDataAccess.GetSources(DataSourceType.Sqlite);
            _map = maps.First();
            _mapLocationAccess.MapId = _map.ID;
            _mapLocations = await _mapLocationAccess.GetSources(DataSourceType.Sqlite);

            SetLocations(_mapLocations);

            _searchPane.QuerySubmitted += SearchPaneQuerySubmitted;
            _searchPane.SuggestionsRequested += SearchPaneSuggestionsRequested;
        }


        /// <summary>
        /// Conserve l'état associé à cette page en cas de suspension de l'application ou de la
        /// suppression de la page du cache de navigation. Les valeurs doivent être conformes aux
        /// exigences en matière de sérialisation de <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Dictionnaire vide à remplir à l'aide de l'état sérialisable.</param>
        protected override void SaveState(
            Dictionary<String, Object> pageState)
        {
            _searchPane.QuerySubmitted -= SearchPaneQuerySubmitted;
            _searchPane.SuggestionsRequested -= SearchPaneSuggestionsRequested;
        }

        private void MapTapped(
            object sender,
            TappedRoutedEventArgs e)
        {
            Grid grid = e.OriginalSource as Grid;
            if ((grid == null || grid.Name != "MapPinRoot") && _focusedMapPin != null && !_focusedMapPin.Marked)
                MapView.Children.Remove(_focusedMapPin);
        }

        private async void SearchPaneSuggestionsRequested(
            SearchPane sender,
            SearchPaneSuggestionsRequestedEventArgs args)
        {
            SearchPaneSuggestionsRequestDeferral deferral = args.Request.GetDeferral();

            GeocodeRequestOptions requests = new GeocodeRequestOptions(args.QueryText);
            _searchManager = MapView.SearchManager;
            _searchResponse = await _searchManager.GeocodeAsync(requests);
            foreach (GeocodeLocation locationData in _searchResponse.LocationData)
                args.Request.SearchSuggestionCollection.AppendQuerySuggestion(locationData.Address.FormattedAddress);

            deferral.Complete();
        }

        private void SearchPaneQuerySubmitted(
            SearchPane sender,
            SearchPaneQuerySubmittedEventArgs args)
        {
            GeocodeLocation geoCodeLocation = _searchResponse.LocationData
                .FirstOrDefault(locationData => locationData.Address.FormattedAddress == args.QueryText) ??
                _searchResponse.LocationData.FirstOrDefault();

            if (geoCodeLocation != null)
            {
                MapPin mapPinElement = new MapPin("Pin", "Desc Pin",
                    geoCodeLocation.Location.Latitude,
                    geoCodeLocation.Location.Longitude);
                mapPinElement.MapPinTapped += MapPinElementMapPinTapped;
                MapView.Children.Add(mapPinElement);
                MapLayer.SetPosition(mapPinElement, geoCodeLocation.Location);
                MapView.SetView(geoCodeLocation.Location, 15.0f);
                MapView.ViewChangeEnded += MapViewViewChangeEnded;
                _focusedMapPin = mapPinElement;
            }
        }

        private void MapViewViewChangeEnded(object sender, ViewChangeEndedEventArgs e)
        {
            _focusedMapPin.Focus();
        }

        private async void MapPinElementMapPinTapped(object sender, Components.Maps.Objects.MapPinTappedEventArgs e)
        {
            MapPin mapPinElement = (MapPin)sender;
            if (e.Marked)
            {
                await _mapLocationAccess.Add(
                    DataSourceType.Sqlite,
                    new MapLocationModel
                {
                    ID = Guid.NewGuid(),
                    Name = mapPinElement.Name,
                    Description = mapPinElement.Description,
                    Latitude = mapPinElement.Latitude,
                    Longitude = mapPinElement.Longitude,
                    MapId = _map.ID
                });

                MapLocationModel addedLocation = _mapLocations.FirstOrDefault(location =>
                    location.Latitude == mapPinElement.Latitude &&
                    location.Longitude == location.Longitude);
                if (addedLocation != null)
                    mapPinElement.ID = addedLocation.ID;
            }
            else
            {
                MapLocationModel deleteLocation = _mapLocations.FirstOrDefault(location =>
                    location.ID.Equals(mapPinElement.ID));
                if (deleteLocation != null)
                    await _mapLocationAccess.Remove(DataSourceType.Sqlite, deleteLocation);
            }

            _focusedMapPin = mapPinElement;
        }

        private void MapPanelLink(object sender, EventArgs e)
        {
            Frame.Navigate(typeof(PageMain), null);
        }

        private void SetLocations(ObservableCollection<MapLocationModel> locations)
        {
            foreach (MapLocationModel mapLocation in locations)
            {
                MapPin mapPinElement = new MapPin(mapLocation.Name,
                    mapLocation.Description,
                    mapLocation.Latitude,
                    mapLocation.Longitude) { ID = mapLocation.ID };

                MapView.Children.Add(mapPinElement);
                Location location = new Location(mapLocation.Latitude, mapLocation.Longitude);
                MapLayer.SetPosition(mapPinElement, location);
                mapPinElement.Mark();
            }
        }
    }
}
