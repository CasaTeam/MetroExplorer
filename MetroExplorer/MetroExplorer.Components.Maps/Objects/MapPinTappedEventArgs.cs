namespace MetroExplorer.Components.Maps.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;

    public class MapPinTappedEventArgs : RoutedEventArgs
    {
        public bool Marked { get; private set; }

        public MapPinTappedEventArgs(bool marked)
        {
            Marked = marked;
        }
    }
}
