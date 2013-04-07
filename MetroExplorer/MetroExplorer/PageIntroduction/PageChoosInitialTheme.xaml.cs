using MetroExplorer.Theme;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MetroExplorer.PageIntroduction
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class PageChoosInitialTheme : MetroExplorer.Common.LayoutAwarePage
    {
        public PageChoosInitialTheme()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void Rectangle_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            ThemeLibarary.CurrentTheme = Themes.Bo;
            this.Frame.Navigate(typeof(PageMain), Themes.Bo);
        }

        private void Rectangle_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            ThemeLibarary.CurrentTheme = Themes.E2E3DA;
            this.Frame.Navigate(typeof(PageMain));
        }

        private void Rectangle_Tapped_3(object sender, TappedRoutedEventArgs e)
        {
            ThemeLibarary.CurrentTheme = Themes.E4DFD1;
            this.Frame.Navigate(typeof(PageMain));
        }

        private void Rectangle_Tapped_4(object sender, TappedRoutedEventArgs e)
        {
            ThemeLibarary.CurrentTheme = Themes.E4E8E8;
            this.Frame.Navigate(typeof(PageMain));
        }

        private void Rectangle_Tapped_5(object sender, TappedRoutedEventArgs e)
        {
            ThemeLibarary.CurrentTheme = Themes.EAE9E5;
            this.Frame.Navigate(typeof(PageMain));
        }

        private void Rectangle_Tapped_6(object sender, TappedRoutedEventArgs e)
        {
            ThemeLibarary.CurrentTheme = Themes.FFF1DD;
            this.Frame.Navigate(typeof(PageMain));
        }
    }
}
