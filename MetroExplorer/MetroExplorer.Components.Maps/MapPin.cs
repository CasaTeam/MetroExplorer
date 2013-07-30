namespace MetroExplorer.Components.Maps
{
    using MetroExplorer.Components.Maps.Objects;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Documents;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;

    public sealed class MapPin : Control
    {
        public Guid ID { get; set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public bool Marked { get; private set; }

        public bool Focused { get; private set; }

        public event EventHandler<MapPinTappedEventArgs> MapPinTapped;

        public MapPin()
        {
            this.DefaultStyleKey = typeof(MapPin);
        }

        public MapPin(
            string name,
            string description,
            double latitude,
            double longitude)
            : this()
        {
            Name = name;
            Description = description;
            Latitude = latitude;
            Longitude = longitude;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Tapped += OnTapped;
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Focused)
            {
                VisualStateManager.GoToState(this, Marked ? "UnMarked" : "Marked", true);
                Marked = !Marked;
            }
            if (MapPinTapped != null)
                MapPinTapped(this, new MapPinTappedEventArgs(Marked));

        }

        public void Focus()
        {
            if (!Focused)
            {
                Focused = true;
                VisualStateManager.GoToState(this, "Focused", true);
            }
        }

        public void UnFocus()
        {
            if (Focused)
            {
                Focused = false;
                VisualStateManager.GoToState(this, "UnFocused", true);
            }
        }

        public void Mark()
        {
            Marked = true;
            VisualStateManager.GoToState(this, "Marked", true);
        }
    }
}
