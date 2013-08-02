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
    using Windows.UI.Xaml.Shapes;
    using Bing.Maps.Search;
    using Bing.Maps;
    using Common;
    using Components.Maps;
    using Components.Maps.Objects;
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

        private MapPin _focusedMapPin, _lastFocusedMapPin;

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
            MapView.ViewChangeEnded -= MapViewViewChangeEnded;
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
            if ((grid == null || grid.Name != "MapPinRoot") && _focusedMapPin != null)
            {
                if (!_focusedMapPin.Marked)
                    MapView.Children.Remove(_focusedMapPin);
                else
                    _focusedMapPin.UnFocus();
                _focusedMapPin = null;
            }
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

            _lastFocusedMapPin = _focusedMapPin;

            if (geoCodeLocation != null)
            {
                MapPin existedMapPin = _mapPins.FirstOrDefault(mapPin =>
                    mapPin.Latitude == geoCodeLocation.Location.Latitude.ToString()
                    && mapPin.Longitude == geoCodeLocation.Location.Longitude.ToString());
                if (existedMapPin == null)
                {
                    MapPin mapPinElement = new MapPin(string.Empty, string.Empty,
                        geoCodeLocation.Location.Latitude.ToString(),
                        geoCodeLocation.Location.Longitude.ToString());

                    mapPinElement.MapPinTapped += MapPinElementMapPinTapped;
                    MapView.Children.Add(mapPinElement);
                    MapLayer.SetPosition(mapPinElement, geoCodeLocation.Location);
                    MapView.SetView(geoCodeLocation.Location, 15.0f);
                    _focusedMapPin = mapPinElement;
                }
                else
                {
                    Location location = new Location(double.Parse(existedMapPin.Latitude), double.Parse(existedMapPin.Longitude));
                    MapView.SetView(location, 15.0f);
                    existedMapPin.Focus();
                    _focusedMapPin = existedMapPin;
                }

            }
        }

        private void MapViewViewChangeEnded(object sender, ViewChangeEndedEventArgs e)
        {
            if (_lastFocusedMapPin != null && _lastFocusedMapPin.Focused)
                _lastFocusedMapPin.UnFocus();

            if (_focusedMapPin != null && !_focusedMapPin.Focused)
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
                        Latitude = mapPinElement.Latitude.ToString(),
                        Longitude = mapPinElement.Longitude.ToString(),
                        MapId = _map.ID
                    });

                    MapLocationModel addedLocation = _mapLocations.FirstOrDefault(location =>
                        location.Latitude == mapPinElement.Latitude.ToString() &&
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

            _lastFocusedMapPin = _focusedMapPin;
            _focusedMapPin = mapPinElement;

            if (_lastFocusedMapPin != null)
                _lastFocusedMapPin.UnFocus();

            if (!_focusedMapPin.Focused)
                _focusedMapPin.Focus();
        }

        private void SetLocations(ObservableCollection<MapLocationModel> locations)
        {
            foreach (MapLocationModel mapLocation in locations)
            {
                MapPin mapPinElement = new MapPin(mapLocation.Name,
                    mapLocation.Description,
                    mapLocation.Latitude.ToString(),
                    mapLocation.Longitude.ToString()) { ID = mapLocation.ID };

                mapPinElement.MapPinTapped += MapPinElementMapPinTapped;

                MapView.Children.Add(mapPinElement);
                Location location = new Location(double.Parse(mapLocation.Latitude), double.Parse(mapLocation.Longitude));
                MapLayer.SetPosition(mapPinElement, location);
                mapPinElement.Mark();
                _mapPins.Add(mapPinElement);
            }
            MapView.ViewChanged += MapViewViewChanged;
        }

        private void MapViewViewChanged(object sender, ViewChangedEventArgs e)
        {
            foreach (MapPin mapPin in _mapPins)
                mapPin.Mark();


            MapView.ViewChanged -= MapViewViewChanged;
        }
    }
}
