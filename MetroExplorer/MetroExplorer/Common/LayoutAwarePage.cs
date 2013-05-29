using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MetroExplorer.Common
{
    /// <summary>
    /// Implémentation standard de page qui offre plusieurs avantages importants :
    /// <list type="bullet">
    /// <item>
    /// <description>Mappage de l'état de l'application à l'état visuel</description>
    /// </item>
    /// <item>
    /// <description>Gestionnaires d'événements GoBack, GoForward et GoHome</description>
    /// </item>
    /// <item>
    /// <description>Raccourcis souris et clavier pour la navigation</description>
    /// </item>
    /// <item>
    /// <description>Gestion d'état pour la navigation et gestion de la durée de vie des processus</description>
    /// </item>
    /// <item>
    /// <description>Modèle d'affichage par défaut</description>
    /// </item>
    /// </list>
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public class LayoutAwarePage : Page
    {
        /// <summary>
        /// Identifie la propriété de dépendance <see cref="DefaultViewModel"/>.
        /// </summary>
        public static readonly DependencyProperty DefaultViewModelProperty =
            DependencyProperty.Register("DefaultViewModel", typeof(IObservableMap<String, Object>),
            typeof(LayoutAwarePage), null);

        private List<Control> _layoutAwareControls;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="LayoutAwarePage"/>.
        /// </summary>
        public LayoutAwarePage()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;

            // Crée un modèle d'affichage par défaut vide
            this.DefaultViewModel = new ObservableDictionary<String, Object>();

            // Lorsque cette page fait partie de l'arborescence d'éléments visuels, effectue deux modifications :
            // 1) Mappe l'état d'affichage de l'application à l'état visuel pour la page
            // 2) Traite les requêtes de navigation à l'aide du clavier ou de la souris
            this.Loaded += (sender, e) =>
            {
                this.StartLayoutUpdates(sender, e);

                // La navigation à l'aide du clavier et de la souris s'applique uniquement lorsque la totalité de la fenêtre est occupée
                if (this.ActualHeight == Window.Current.Bounds.Height &&
                    this.ActualWidth == Window.Current.Bounds.Width)
                {
                    // Écoute directement la fenêtre, ce qui ne requiert pas le focus
                    Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
                        CoreDispatcher_AcceleratorKeyActivated;
                    Window.Current.CoreWindow.PointerPressed +=
                        this.CoreWindow_PointerPressed;
                }
            };

            // Annule les mêmes modifications lorsque la page n'est plus visible
            this.Unloaded += (sender, e) =>
            {
                this.StopLayoutUpdates(sender, e);
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
                    CoreDispatcher_AcceleratorKeyActivated;
                Window.Current.CoreWindow.PointerPressed -=
                    this.CoreWindow_PointerPressed;
            };
        }

        /// <summary>
        /// Implémentation de <see cref="IObservableMap&lt;String, Object&gt;"/> conçue pour être
        /// utilisée en tant que modèle d'affichage trivial.
        /// </summary>
        protected IObservableMap<String, Object> DefaultViewModel
        {
            get
            {
                return this.GetValue(DefaultViewModelProperty) as IObservableMap<String, Object>;
            }

            set
            {
                this.SetValue(DefaultViewModelProperty, value);
            }
        }

        #region Prise en charge de la navigation

        /// <summary>
        /// Invoqué en tant que gestionnaire d'événements pour naviguer vers l'arrière dans le
        /// <see cref="Frame"/> associé de la page jusqu'à ce qu'il atteigne le haut de la pile de navigation.
        /// </summary>
        /// <param name="sender">Instance qui a déclenché l'événement.</param>
        /// <param name="e">Données d'événement décrivant les conditions ayant déclenché l'événement.</param>
        protected virtual void GoHome(object sender, RoutedEventArgs e)
        {
            // Utilisez le cadre de navigation pour revenir à la page la plus en haut
            if (this.Frame != null)
            {
                while (this.Frame.CanGoBack) this.Frame.GoBack();
            }
        }

        /// <summary>
        /// Invoqué en tant que gestionnaire d'événements pour naviguer vers l'arrière dans la pile de navigation
        /// associé au <see cref="Frame"/> de cette page.
        /// </summary>
        /// <param name="sender">Instance qui a déclenché l'événement.</param>
        /// <param name="e">Données d'événement décrivant les conditions ayant déclenché cet
        /// événement.</param>
        protected virtual void GoBack(object sender, RoutedEventArgs e)
        {
            // Utilisez le cadre de navigation pour revenir à la page précédente
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }

        /// <summary>
        /// Invoqué en tant que gestionnaire d'événements pour naviguer vers l'avant dans la pile de navigation
        /// associé au <see cref="Frame"/> de cette page.
        /// </summary>
        /// <param name="sender">Instance qui a déclenché l'événement.</param>
        /// <param name="e">Données d'événement décrivant les conditions ayant déclenché cet
        ///.</param>
        protected virtual void GoForward(object sender, RoutedEventArgs e)
        {
            // Utilise le frame de navigation pour atteindre la page suivante
            if (this.Frame != null && this.Frame.CanGoForward) this.Frame.GoForward();
        }

        /// <summary>
        /// Invoqué à chaque séquence de touches, notamment les touches système comme les combinaisons utilisant la touche Alt, lorsque
        /// cette page est active et occupe la totalité de la fenêtre. Utilisé pour détecter la navigation à l'aide du clavier
        /// entre les pages, même lorsque la page elle-même n'a pas le focus.
        /// </summary>
        /// <param name="sender">Instance qui a déclenché l'événement.</param>
        /// <param name="args">Données d'événement décrivant les conditions ayant déclenché l'événement.</param>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender,
            AcceleratorKeyEventArgs args)
        {
            var virtualKey = args.VirtualKey;

            // Approfondit les recherches uniquement lorsque les touches Gauche, Droite ou les touches Précédent et Suivant dédiées
            // sont actionnées
            if ((args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
                args.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
                (virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
                (int)virtualKey == 166 || (int)virtualKey == 167))
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;
                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
                bool noModifiers = !menuKey && !controlKey && !shiftKey;
                bool onlyAlt = menuKey && !controlKey && !shiftKey;

                if (((int)virtualKey == 166 && noModifiers) ||
                    (virtualKey == VirtualKey.Left && onlyAlt))
                {
                    // Lorsque la touche Précédent ou les touches Alt+Gauche sont actionnées, navigue vers l'arrière
                    args.Handled = true;
                    this.GoBack(this, new RoutedEventArgs());
                }
                else if (((int)virtualKey == 167 && noModifiers) ||
                    (virtualKey == VirtualKey.Right && onlyAlt))
                {
                    // Lorsque la touche Suivant ou les touches Alt+Droite sont actionnées, navigue vers l'avant
                    args.Handled = true;
                    this.GoForward(this, new RoutedEventArgs());
                }
            }
        }

        /// <summary>
        /// Invoqué sur chaque clic de souris, pression d'écran tactile ou interaction équivalente lorsque cette
        /// page est active et occupe la totalité de la fenêtre. Utilisé pour détecter les clics de souris Suivant et Précédent
        /// de style navigateur pour naviguer entre les pages.
        /// </summary>
        /// <param name="sender">Instance qui a déclenché l'événement.</param>
        /// <param name="args">Données d'événement décrivant les conditions ayant déclenché l'événement.</param>
        private void CoreWindow_PointerPressed(CoreWindow sender,
            PointerEventArgs args)
        {
            var properties = args.CurrentPoint.Properties;

            // Ignore les pressions simultanées sur les boutons droit, gauche et central
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed) return;

            // Si les boutons Précédent ou Suivant sont utilisés (mais pas les deux à la fois) navigue en conséquence
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                args.Handled = true;
                if (backPressed) this.GoBack(this, new RoutedEventArgs());
                if (forwardPressed) this.GoForward(this, new RoutedEventArgs());
            }
        }

        #endregion

        #region Changement d'état visuel

        /// <summary>
        /// Invoqué en tant que gestionnaire d'événements, en général sur l'événement <see cref="FrameworkElement.Loaded"/>
        /// d'un <see cref="Control"/> dans la page, pour indiquer que l'expéditeur doit
        /// commencer à recevoir les modifications de gestion de l'état visuel correspondant aux modifications de l'état d'affichage de
        /// l'application.
        /// </summary>
        /// <param name="sender">Instance de <see cref="Control"/> prenant en charge la gestion de l'état visuel
        /// correspondant aux états d'affichage.</param>
        /// <param name="e">Données d'événement qui décrivent comment la requête a été effectuée.</param>
        /// <remarks>L'état d'affichage actuel sera immédiatement utilisé pour définir l'état visuel correspondant
        /// lorsque des mises à jour de la disposition sont demandées. Un
        /// gestionnaire d'événements <see cref="FrameworkElement.Unloaded"/> correspondant connecté à
        /// <see cref="StopLayoutUpdates"/> est fortement recommandé. Les instances de
        /// <see cref="LayoutAwarePage"/> appellent automatiquement ces gestionnaires dans leurs événements Loaded et
        /// Unloaded.</remarks>
        /// <seealso cref="DetermineVisualState"/>
        /// <seealso cref="InvalidateVisualState"/>
        public void StartLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null) return;
            if (this._layoutAwareControls == null)
            {
                // Commence à écouter les changements d'état d'affichage lorsque des contrôles demandent des mises à jour
                Window.Current.SizeChanged += this.WindowSizeChanged;
                this._layoutAwareControls = new List<Control>();
            }
            this._layoutAwareControls.Add(control);

            // Définit l'état visuel initial du contrôle
            VisualStateManager.GoToState(control, DetermineVisualState(ApplicationView.Value), false);
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            this.InvalidateVisualState();
        }

        /// <summary>
        /// Invoqué en tant que gestionnaire d'événements, en général sur l'événement <see cref="FrameworkElement.Unloaded"/>
        /// d'un <see cref="Control"/>, pour indiquer que l'expéditeur doit commencer à recevoir
        /// des changements de gestion de l'état visuel correspondant aux changements de l'état d'affichage de l'application.
        /// </summary>
        /// <param name="sender">Instance de <see cref="Control"/> prenant en charge la gestion de l'état visuel
        /// correspondant aux états d'affichage.</param>
        /// <param name="e">Données d'événement qui décrivent comment la requête a été effectuée.</param>
        /// <remarks>L'état d'affichage actuel sera immédiatement utilisé pour définir l'état visuel correspondant
        /// l'état visuel lorsque des mises à jour de la disposition sont demandées.</remarks>
        /// <seealso cref="StartLayoutUpdates"/>
        public void StopLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null || this._layoutAwareControls == null) return;
            this._layoutAwareControls.Remove(control);
            if (this._layoutAwareControls.Count == 0)
            {
                // Arrête d'écouter les modifications de l'état d'affichage lorsque les contrôles ne demandent plus de mises à jour
                this._layoutAwareControls = null;
                Window.Current.SizeChanged -= this.WindowSizeChanged;
            }
        }

        /// <summary>
        /// Convertit les valeurs <see cref="ApplicationViewState"/> en chaînes pour la gestion de l'état visuel
        /// dans la page. L'implémentation par défaut utilise les noms de valeurs enum.
        /// Les sous-classes peuvent substituer cette méthode pour contrôler le schéma de mappage utilisé.
        /// </summary>
        /// <param name="viewState">État d'affichage pour lequel un état visuel est requis.</param>
        /// <returns>Nom d'état visuel utilisé pour piloter le
        /// <see cref="VisualStateManager"/></returns>
        /// <seealso cref="InvalidateVisualState"/>
        protected virtual string DetermineVisualState(ApplicationViewState viewState)
        {
            return viewState.ToString();
        }

        /// <summary>
        /// Met à jour tous les contrôles qui écoutent les modifications de l'état visuel avec
        /// l'état visuel approprié.
        /// </summary>
        /// <remarks>
        /// Généralement utilisé avec le <see cref="DetermineVisualState"/> de substitution pour
        /// indique qu'une autre valeur peut être retournée bien que l'état visuel n'ait pas
        /// changé.
        /// </remarks>
        public void InvalidateVisualState()
        {
            if (this._layoutAwareControls != null)
            {
                string visualState = DetermineVisualState(ApplicationView.Value);
                foreach (var layoutAwareControl in this._layoutAwareControls)
                {
                    VisualStateManager.GoToState(layoutAwareControl, visualState, false);
                }
            }
        }

        #endregion

        #region Gestion de la durée de vie des processus

        private String _pageKey;

        /// <summary>
        /// Invoqué lorsque cette page est sur le point d'être affichée dans un frame.
        /// </summary>
        /// <param name="e">Données d'événement décrivant la manière dont l'utilisateur a accédé à cette page. La propriété Parameter
        /// fournit le groupe devant être affiché.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Le retour à une page mise en cache via la navigation ne devrait pas déclencher le chargement de l'état
            if (this._pageKey != null) return;

            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            this._pageKey = "Page-" + this.Frame.BackStackDepth;

            if (e.NavigationMode == NavigationMode.New)
            {
                // Efface l'état existant pour la navigation avant lors de l'ajout d'une nouvelle page à la
                // pile de navigation
                var nextPageKey = this._pageKey;
                int nextPageIndex = this.Frame.BackStackDepth;
                while (frameState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }
                // Passe le paramètre de navigation à la nouvelle page
                this.LoadState(e.Parameter, null);
            }
            else
            {
                // Passe le paramètre de navigation et conserve l'état de page de la page, en utilisant
                // la même stratégie pour charger l'état suspendu et recréer les pages supprimées
                // du cache
                this.LoadState(e.Parameter, (Dictionary<String, Object>)frameState[this._pageKey]);
            }
        }

        /// <summary>
        /// Invoqué lorsque cette page n'est plus affichée dans un frame.
        /// </summary>
        /// <param name="e">Données d'événement décrivant la manière dont l'utilisateur a accédé à cette page. La propriété Parameter
        /// fournit le groupe devant être affiché.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            var pageState = new Dictionary<String, Object>();
            this.SaveState(pageState);
            frameState[_pageKey] = pageState;
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
        protected virtual void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Conserve l'état associé à cette page en cas de suspension de l'application ou de la
        /// suppression de la page du cache de navigation. Les valeurs doivent être conformes aux
        /// exigences en matière de sérialisation de <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Dictionnaire vide à remplir à l'aide de l'état sérialisable.</param>
        protected virtual void SaveState(Dictionary<String, Object> pageState)
        {
        }

        #endregion

        /// <summary>
        /// Implémentation de IObservableMap qui prend en charge la réentrance pour une utilisation en tant que modèle d'affichage
        /// par défaut.
        /// </summary>
        private class ObservableDictionary<K, V> : IObservableMap<K, V>
        {
            private class ObservableDictionaryChangedEventArgs : IMapChangedEventArgs<K>
            {
                public ObservableDictionaryChangedEventArgs(CollectionChange change, K key)
                {
                    this.CollectionChange = change;
                    this.Key = key;
                }

                public CollectionChange CollectionChange { get; private set; }
                public K Key { get; private set; }
            }

            private Dictionary<K, V> _dictionary = new Dictionary<K, V>();
            public event MapChangedEventHandler<K, V> MapChanged;

            private void InvokeMapChanged(CollectionChange change, K key)
            {
                var eventHandler = MapChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new ObservableDictionaryChangedEventArgs(change, key));
                }
            }

            public void Add(K key, V value)
            {
                this._dictionary.Add(key, value);
                this.InvokeMapChanged(CollectionChange.ItemInserted, key);
            }

            public void Add(KeyValuePair<K, V> item)
            {
                this.Add(item.Key, item.Value);
            }

            public bool Remove(K key)
            {
                if (this._dictionary.Remove(key))
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                    return true;
                }
                return false;
            }

            public bool Remove(KeyValuePair<K, V> item)
            {
                V currentValue;
                if (this._dictionary.TryGetValue(item.Key, out currentValue) &&
                    Object.Equals(item.Value, currentValue) && this._dictionary.Remove(item.Key))
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
                    return true;
                }
                return false;
            }

            public V this[K key]
            {
                get
                {
                    return this._dictionary[key];
                }
                set
                {
                    this._dictionary[key] = value;
                    this.InvokeMapChanged(CollectionChange.ItemChanged, key);
                }
            }

            public void Clear()
            {
                var priorKeys = this._dictionary.Keys.ToArray();
                this._dictionary.Clear();
                foreach (var key in priorKeys)
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                }
            }

            public ICollection<K> Keys
            {
                get { return this._dictionary.Keys; }
            }

            public bool ContainsKey(K key)
            {
                return this._dictionary.ContainsKey(key);
            }

            public bool TryGetValue(K key, out V value)
            {
                return this._dictionary.TryGetValue(key, out value);
            }

            public ICollection<V> Values
            {
                get { return this._dictionary.Values; }
            }

            public bool Contains(KeyValuePair<K, V> item)
            {
                return this._dictionary.Contains(item);
            }

            public int Count
            {
                get { return this._dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            {
                return this._dictionary.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this._dictionary.GetEnumerator();
            }

            public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
            {
                int arraySize = array.Length;
                foreach (var pair in this._dictionary)
                {
                    if (arrayIndex >= arraySize) break;
                    array[arrayIndex++] = pair;
                }
            }
        }
    }
}
