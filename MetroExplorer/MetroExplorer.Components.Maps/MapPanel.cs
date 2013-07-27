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

    [TemplatePart(Name = ButtonLinkElement, Type = typeof(Button))]
    public sealed class MapPanel : Control
    {
        internal const string ButtonLinkElement = "ButtonLinkElement";

        private Button _buttonLink;

        public event EventHandler Link;

        public MapPanel()
        {
            this.DefaultStyleKey = typeof(MapPanel);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _buttonLink = (Button)GetTemplateChild(ButtonLinkElement);
            if (_buttonLink != null)
            {
                _buttonLink.Click += ButtonLinkClick;
            }
        }

        private void ButtonLinkClick(object sender, RoutedEventArgs e)
        {
            if (Link != null)
                Link(this, new EventArgs());
        }
    }
}
