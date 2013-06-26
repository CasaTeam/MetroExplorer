namespace MetroExplorer.Pages.GuidePage
{
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
    using MainPage;

    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PageUserGuide : Page
    {
        public PageUserGuide()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoqué lorsque cette page est sur le point d'être affichée dans un frame.
        /// </summary>
        /// <param name="e">Données d'événement décrivant la manière dont l'utilisateur a accédé à cette page. La propriété Parameter
        /// est généralement utilisée pour configurer la page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Button_ToGuidPage2_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PageUserGuidePage2));
        }

        private void Button_ToMainPage_Click(object sender, RoutedEventArgs e)
        {
            FirstUsingRecord.GetInstance().WriteRecordGuidePageFile("1");
            this.Frame.Navigate(typeof(PageMain));
        }
    }
}
