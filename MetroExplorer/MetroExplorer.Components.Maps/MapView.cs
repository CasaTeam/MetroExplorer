using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// Pour en savoir plus sur le modèle d'élément Contrôle basé sur un modèle, consultez la page http://go.microsoft.com/fwlink/?LinkId=234235

namespace MetroExplorer.Components.Maps
{
    public sealed class MapView : Control
    {
        public MapView()
        {
            this.DefaultStyleKey = typeof(MapView);
        }
    }
}
