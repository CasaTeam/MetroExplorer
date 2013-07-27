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

    [TemplatePart(Name = ButtonPinElement, Type = typeof(Button))]
    public sealed class MapPin : Control
    {
        internal const string ButtonPinElement = "ButtonPinElement";

        private Button _buttonPin;

        public MapPin()
        {
            this.DefaultStyleKey = typeof(MapPin);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _buttonPin = (Button)GetTemplateChild(ButtonPinElement);
            if (_buttonPin != null)
            {
                _buttonPin.Click += ButtonPinClick;
            }
        }

        private void ButtonPinClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
