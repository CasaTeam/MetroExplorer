using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MetroExplorer.core.Behavior
{
    public class ImageSourceChangedBehavior : Behavior<Image>
    {
        public static readonly DependencyProperty SourceProperty =
           DependencyProperty.RegisterAttached("SSource",
           typeof(ImageSource),
           typeof(ImageSourceChangedBehavior),
           new PropertyMetadata(null, SourceChanged));

        public ImageSource SSource
        {
            get 
            { 
                return (ImageSource)this.GetValue(SourceProperty); 
            }
            set 
            { 
                this.SetValue(SourceProperty, value);
            }
        }

        static void SourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var image = (obj as ImageSourceChangedBehavior).AssociatedObject as Image;
            if (image == null) return;
            image.Source = e.NewValue as BitmapImage;
            image.FadeInCustom(new TimeSpan(0, 0, 0, 0, 700));
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
