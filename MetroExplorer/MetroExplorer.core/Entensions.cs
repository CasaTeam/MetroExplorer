using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MetroExplorer.core
{
    public static class Entensions
    {
        public static DependencyObject GetParentByName(this DependencyObject obj, string name)
        {
            DependencyObject objFind = VisualTreeHelper.GetParent(obj);
            if (((FrameworkElement)objFind).Name == name)
                return objFind;
            else
                return objFind.GetParentByName(name);
        }
    }
}
