using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MetroExplorer
{
    /// <summary>
    /// Cette page affiche les résultats d'une recherche globale effectuée dans cette application.
    /// </summary>
    public sealed partial class PageSearch : MetroExplorer.Common.LayoutAwarePage
    {

        public PageSearch()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="navigationParameter">Valeur de paramètre passée à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page.
        /// </param>
        /// <param name="pageState">Dictionnaire d'état conservé par cette page durant une session
        /// antérieure. Null lors de la première visite de la page.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var queryText = navigationParameter as String;

            // TODO: logique de recherche spécifique à l'application. Le processus de recherche est chargé de la
            //       création d'une liste de catégories de résultats sélectionnables par l'utilisateur :
            //
            //       filterList.Add(new Filter("<filter name>", <result count>));
            //
            //       Seul le premier filtre (en général « Tout ») doit transmettre la valeur True comme troisième argument
            //       afin de démarrer avec l'état actif. Les résultats du filtre actif sont fournis
            //       dans Filter_SelectionChanged ci-dessous.

            var filterList = new List<Filter>();
            filterList.Add(new Filter("All", 0, true));

            // Communiquez les résultats via le modèle d'affichage
            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';
            this.DefaultViewModel["Filters"] = filterList;
            this.DefaultViewModel["ShowFilters"] = filterList.Count > 1;
        }

        /// <summary>
        /// Invoqué lorsqu'un filtre est sélectionné à l'aide d'un contrôle ComboBox avec l'état d'affichage Snapped.
        /// </summary>
        /// <param name="sender">Instance ComboBox.</param>
        /// <param name="e">Données d'événement décrivant la façon dont le filtre sélectionné a été modifié.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Déterminez quel filtre a été sélectionné
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Reflétez les résultats dans l'objet Filter correspondant pour permettre à
                // la représentation RadioButton utilisée avec un état d'affichage autre que Snapped de refléter les modifications apportées
                selectedFilter.Active = true;

                // TODO: répondez à la modification du filtre actif en associant this.DefaultViewModel["Results"]
                //       à une collection d'éléments avec des propriétés Image, Title, Subtitle et Description pouvant être liées

                // Vérifie que des résultats sont trouvés
                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Affiche des informations lorsqu'il n'y aucun résultat de recherche.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        /// <summary>
        /// Invoqué lorsqu'un filtre est sélectionné à l'aide d'un RadioButton, lorsque l'état d'affichage n'est pas Snapped.
        /// </summary>
        /// <param name="sender">Instance RadioButton sélectionnée.</param>
        /// <param name="e">Données d'événement décrivant la façon dont le RadioButton a été sélectionné.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Reflétez la modification dans le CollectionViewSource utilisé par le contrôle ComboBox correspondant
            // pour garantir que la modification soit reflétée lorsque l'état d'affichage a la valeur Snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        /// <summary>
        /// Modèle d'affichage décrivant l'un des filtres disponibles pour l'affichage des résultats de recherche.
        /// </summary>
        private sealed class Filter : MetroExplorer.Common.BindableBase
        {
            private String _name;
            private int _count;
            private bool _active;

            public Filter(String name, int count, bool active = false)
            {
                this.Name = name;
                this.Count = count;
                this.Active = active;
            }

            public override String ToString()
            {
                return Description;
            }

            public String Name
            {
                get { return _name; }
                set { if (this.SetProperty(ref _name, value)) this.OnPropertyChanged("Description"); }
            }

            public int Count
            {
                get { return _count; }
                set { if (this.SetProperty(ref _count, value)) this.OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                get { return _active; }
                set { this.SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }
        }
    }
}
