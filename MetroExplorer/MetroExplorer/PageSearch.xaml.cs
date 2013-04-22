using System.Collections.ObjectModel;
using MetroExplorer.core.Utils;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Input;

namespace MetroExplorer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Common;
    using core;
    using core.Objects;

    /// <summary>
    /// Cette page affiche les résultats d'une recherche globale effectuée dans cette application.
    /// </summary>
    public sealed partial class PageSearch : LayoutAwarePage
    {
        private readonly MetroExplorerLocalDataSource _dataSource;
        private readonly ObservableCollection<GroupInfoList<ExplorerItem>> _explorerGroups;

        public PageSearch()
        {
            InitializeComponent();

            _dataSource = Singleton<MetroExplorerLocalDataSource>.Instance;
            _explorerGroups = new ObservableCollection<GroupInfoList<ExplorerItem>>
                {
                    new GroupInfoList<ExplorerItem>()
                        {
                            Key = StringResources.ResourceLoader.GetString("MainPage_UserFolderGroupTitle")
                        },
                    new GroupInfoList<ExplorerItem>()
                        {
                            Key = StringResources.ResourceLoader.GetString("MainPage_SystemFolderGroupTitle")
                        }
                };
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
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var queryText = navigationParameter as String;
            var query = queryText.ToLower();
            var items = await _dataSource.CurrentStorageFolder.GetItemsAsync();
            var itemsFilter = items.Where(item => item.Name.ToLower().Contains(query));
            int count = 0;
            foreach (var item in itemsFilter)
            {
                if (item is StorageFolder)
                    _explorerGroups[0].AddItem(item);
                else if (item is StorageFile)
                    _explorerGroups[1].AddItem(item);
                count++;
            }

            var filterList = new List<Filter>();
            filterList.Add(new Filter("All", count, true));

            // Communiquez les résultats via le modèle d'affichage
            DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';
            DefaultViewModel["Filters"] = filterList;
            DefaultViewModel["ShowFilters"] = filterList.Count > 1;
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

                DefaultViewModel["Results"] = _explorerGroups;

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

        
    }
}
