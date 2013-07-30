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
    using Components.Maps.Objects;
    using DataSource;
    using DataSource.DataConfigurations;
    using DataSource.DataModels;
    using MainPage;
    using Windows.UI.Xaml.Shapes;

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

            _mapPins = new ObservableCollection<MapPin>();
        }

        protected override async void LoadState(
            Object navigationParameter,
            Dictionary<String, Object> pageState)
        {
            ObservableCollection<MapModel> maps = await _mapDataAccess.GetSources(DataSourceType.Sqlite);
            _map = maps.First();
            _mapLocationAccess.MapId = _map.ID;
            _mapLocations = await _mapLocationAccess.GetSources(DataSourceType.Sqlite);

            SetLocations(_mapLocations);

            MapView.ViewChangeEnded += MapViewViewChangeEnded;

            _searchPane.QuerySubmitted += SearchPaneQuerySubmitted;
            _searchPane.SuggestionsRequested += SearchPaneSuggestionsRequested;
        }

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
            if (grid == null && e.OriginalSource != null)
                grid = (e.OriginalSource as FrameworkElement).Parent as Grid;
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
                MapPin exsitedMapPin = _mapPins.FirstOrDefault(mapPin =>
                    mapPin.Latitude == geoCodeLocation.Location.Latitude
                    && mapPin.Longitude == geoCodeLocation.Location.Longitude);
                if (exsitedMapPin == null)
                {
                    MapPin mapPinElement = new MapPin(string.Empty, string.Empty,
                        geoCodeLocation.Location.Latitude,
                        geoCodeLocation.Location.Longitude);

                    mapPinElement.MapPinTapped += MapPinElementMapPinTapped;
                    MapView.Children.Add(mapPinElement);
                    MapLayer.SetPosition(mapPinElement, geoCodeLocation.Location);
                    MapView.SetView(geoCodeLocation.Location, 15.0f);
                    _focusedMapPin = mapPinElement;
                }
                else
                    _focusedMapPin = exsitedMapPin;
            }
        }

        private void MapViewViewChangeEnded(object sender, ViewChangeEndedEventArgs e)
        {
            foreach (MapPin mapPin in _mapPins)
                mapPin.UnFocus();

            if (_focusedMapPin != null)
                _focusedMapPin.Focus();
        }

        private async void MapPinElementMapPinTapped(object sender, MapPinTappedEventArgs e)
        {
            MapPin mapPinElement = (MapPin)sender;
            if (mapPinElement.Focused)
            {
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

                    _mapPins.Add(mapPinElement);
                }
                else
                {
                    MapLocationModel deleteLocation = _mapLocations.FirstOrDefault(location =>
                        location.ID.Equals(mapPinElement.ID));
                    if (deleteLocation != null)
                        await _mapLocationAccess.Remove(DataSourceType.Sqlite, deleteLocation);

                    _mapPins.Remove(mapPinElement);
                }
            }

            _focusedMapPin = mapPinElement;

            foreach (MapPin mapPin in _mapPins)
                mapPin.UnFocus();

            _focusedMapPin.Focus();
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

                mapPinElement.MapPinTapped += MapPinElementMapPinTapped;

                MapView.Children.Add(mapPinElement);
                Location location = new Location(mapLocation.Latitude, mapLocation.Longitude);
                MapLayer.SetPosition(mapPinElement, location);
                MapView.ViewChanged += MapViewViewChanged;
                _mapPins.Add(mapPinElement);
            }
        }

        private void MapViewViewChanged(object sender, ViewChangedEventArgs e)
        {
            foreach (MapPin mapPin in _mapPins)
                mapPin.Mark();
        }
    }
}
