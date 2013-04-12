namespace MetroExplorer
{
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;


    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PagePreview : Page
    {
        public PagePreview()
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
    }
}
