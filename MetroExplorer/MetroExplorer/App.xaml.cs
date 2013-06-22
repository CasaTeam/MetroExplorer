namespace MetroExplorer
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.ApplicationModel.Search;
    using Windows.Foundation;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Media.Animation;
    using Windows.ApplicationModel.Resources;
    using Windows.System.Threading;
    using Pages.ExplorerPage;
    using Pages.MainPage;
    using Pages.GuidePage;
    using Core.Utils;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

            InitSearchDispatcher();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }
                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested; // 初始化右侧菜单命令
                if (await FirstUsingRecord.GetInstance().IsFirstUsing())   // 如果第一次打开应用，跳转到用户向导页面，如不是，直接进入主页
                {
                    FirstUsingRecord.GetInstance().WriteRecordGuidePageFile();
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    if (!rootFrame.Navigate(typeof(PageUserGuide), args.Arguments))
                        throw new Exception("Failed to create initial page");
                }
                else if (!rootFrame.Navigate(typeof(PageMain), args.Arguments))
                    throw new Exception("Failed to create initial page");
            }

            InitSearchPanel();

            // Ensure the current window is active
            Window.Current.Activate();

            EventLogger.onLaunch();
        }



        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Appelé lorsque l'application est activée pour afficher les résultats de la recherche.
        /// </summary>
        /// <param name="args">Détails relatifs à la requête d'activation.</param>
        protected async override void OnSearchActivated(SearchActivatedEventArgs args)
        {

            // TODO: enregistrez l'événement Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
            // dans OnWindowCreated pour accélérer les recherches une fois l'application exécutée

            // Si la fenêtre n'utilise pas encore la navigation Frame, insérez votre propre Frame
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            // Si l'application ne contient pas de frame de niveau supérieur, il se peut qu'il s'agisse 
            // du lancement initial de l'application. En général, cette méthode et OnLaunched 
            // dans App.xaml.cs peuvent appeler une méthode commune.
            if (frame == null)
            {
                // Créez un Frame utilisable en tant que contexte de navigation et associez-lui une
                // clé SuspensionManager
                frame = new Frame();
                MetroExplorer.Common.SuspensionManager.RegisterFrame(frame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restaurez l'état de la session enregistrée uniquement lorsque cela est nécessaire
                    try
                    {
                        await MetroExplorer.Common.SuspensionManager.RestoreAsync();
                    }
                    catch (MetroExplorer.Common.SuspensionManagerException)
                    {
                        //Un problème est survenu lors de la restauration de l'état.
                        //Partez du principe que l'état est absent et poursuivez
                    }
                }
            }

            Window.Current.Content = frame;

            // Vérifiez que la fenêtre actuelle est active
            Window.Current.Activate();
        }
    }


    /// <summary>
    /// 负责share的部分
    /// </summary>
    sealed partial class App : Application
    {
        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                rootFrame = new Frame();
            rootFrame.Navigate(typeof(PageMain), args.ShareOperation);
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }
    }


    /// <summary>
    /// 负责search的部分
    /// </summary>
    sealed partial class App : Application
    {
        public static string CurrentQuery;
        public static string LastQuery;
        private DispatcherTimer _dispatcher;

        private void InitSearchDispatcher()
        {
            LastQuery = CurrentQuery = string.Empty;
            _dispatcher = new DispatcherTimer();
            _dispatcher.Tick += (sender, e) =>
            {
                var previousContent = Window.Current.Content;
                var frame = previousContent as Frame;
                if (frame.CurrentSourcePageType == typeof(PageExplorer))
                {
                    if (LastQuery.Equals(CurrentQuery))
                    {
                        frame.Navigate(typeof(PageExplorer), LastQuery);
                        _dispatcher.Stop();
                    }
                }

                LastQuery = CurrentQuery;
            };
            _dispatcher.Interval = TimeSpan.FromMilliseconds(500);
        }

        private void InitSearchPanel()
        {
            SearchPane searchPane = SearchPane.GetForCurrentView();
            searchPane.SearchHistoryEnabled = false;
            searchPane.ShowOnKeyboardInput = true;
            searchPane.QueryChanged += SearchPaneQueryChanged;
            searchPane.SuggestionsRequested += PageExplorerSuggestionsRequested;
        }

        void SearchPaneQueryChanged(SearchPane sender,
            SearchPaneQueryChangedEventArgs args)
        {
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;
            if (frame.CurrentSourcePageType == typeof(PageExplorer))
            {
                CurrentQuery = args.QueryText;
                if (!_dispatcher.IsEnabled)
                    _dispatcher.Start();
            }
        }

        void PageExplorerSuggestionsRequested(
           SearchPane sender,
           SearchPaneSuggestionsRequestedEventArgs args)
        {
            string query = args.QueryText.ToLower();
            if (PageExplorer.CurrentItems == null) return;
            foreach (string item in PageExplorer.CurrentItems)
                if (item.ToLower().Contains(query))
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(item);
        }
    }


    /// <summary>
    /// 负责command的部分 CommandsRequested
    /// </summary>
    sealed partial class App : Application
    {
        void OnCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            UICommandInvokedHandler handler = new UICommandInvokedHandler(OnSettingsCommand);

            SettingsCommand preferenceCommand = new SettingsCommand("SupportUs", (new ResourceLoader()).GetString("SettingCommand_SupportUs"), handler);
            eventArgs.Request.ApplicationCommands.Add(preferenceCommand);

            SettingsCommand languageCommand = new SettingsCommand("LanguageSetting", (new ResourceLoader()).GetString("SettingCommand_LanguageSetting"), handler);
            eventArgs.Request.ApplicationCommands.Add(languageCommand);

            SettingsCommand contactUsCommand = new SettingsCommand("ContactUs", (new ResourceLoader()).GetString("SettingCommand_ContactUs"), handler);
            eventArgs.Request.ApplicationCommands.Add(contactUsCommand);

            SettingsCommand policyCommand = new SettingsCommand("PrivacyPolicy", (new ResourceLoader()).GetString("SettingCommand_Policy"), handler);
            eventArgs.Request.ApplicationCommands.Add(policyCommand);

            SettingsCommand userGuideCommand = new SettingsCommand("UserGuide", (new ResourceLoader()).GetString("SettingCommand_UserGuide"), handler);
            eventArgs.Request.ApplicationCommands.Add(userGuideCommand);
        }

        void OnSettingsCommand(IUICommand command)
        {
            SettingsCommand settingsCommand = (SettingsCommand)command;
            SupportSettingCommand(settingsCommand);
            LanguageSettingCommand(settingsCommand);
            ContactUsCommand(settingsCommand);
            PrivacyPolicyCommand(settingsCommand);
            UserGuideCommand(settingsCommand);
        }

        #region Right Command Layout
        private Popup settingsPopup;
        private double settingsWidth = 500;
        private Rect windowBounds;

        void SupportSettingCommand(SettingsCommand settingsCommand)
        {
            windowBounds = Window.Current.Bounds;
            if (settingsCommand.Id.ToString() == "SupportUs")
            {
                CreatePopupWindowContainsFlyout("SupportUs");
            }
        }

        void LanguageSettingCommand(SettingsCommand settingsCommand)
        {
            if (settingsCommand.Id.ToString() == "LanguageSetting")
            {
                CreatePopupWindowContainsFlyout("LanguageSetting");
            }
        }

        void ContactUsCommand(SettingsCommand settingsCommand)
        {
            windowBounds = Window.Current.Bounds;
            if (settingsCommand.Id.ToString() == "ContactUs")
            {
                CreatePopupWindowContainsFlyout("ContactUs");
            }
        }

        void PrivacyPolicyCommand(SettingsCommand settingsCommand)
        {
            windowBounds = Window.Current.Bounds;
            if (settingsCommand.Id.ToString() == "PrivacyPolicy")
            {
                CreatePopupWindowContainsFlyout("PrivacyPolicy");
                //var mailto = new Uri("http://www.comiscience.info/privacy/pushthemonmapsprivacypolicy.txt");
                //await Windows.System.Launcher.LaunchUriAsync(mailto);
            }
        }

        void UserGuideCommand(SettingsCommand settingsCommand)
        {
            windowBounds = Window.Current.Bounds;
            if (settingsCommand.Id.ToString() == "UserGuide")
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(PageUserGuide));
            }
        }

        private void CreatePopupWindowContainsFlyout(string option)
        {
            CreateSettingsPopup();
            AddProperAnimationForPanel();
            if (option == "SupportUs")
            {
                settingsPopup.IsLightDismissEnabled = false;
                RightMenuLayoutBars.SupportUs mypane = new RightMenuLayoutBars.SupportUs();
                settingsWidth = 400;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                settingsPopup.Child = mypane;
            }
            else if (option == "LanguageSetting")
            {
                RightMenuLayoutBars.LanguagesSetting mypane = new RightMenuLayoutBars.LanguagesSetting();
                settingsWidth = 400;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                settingsPopup.Child = mypane;
            }
            else if (option == "ContactUs")
            {
                RightMenuLayoutBars.ContactUs mypane = new RightMenuLayoutBars.ContactUs();
                settingsWidth = 400;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                settingsPopup.Child = mypane;
            }
            else if (option == "PrivacyPolicy")
            {
                RightMenuLayoutBars.Policy mypane = new RightMenuLayoutBars.Policy();
                settingsWidth = 400;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                settingsPopup.Child = mypane;
            }
            DefineLocationOfOurPopup();
        }

        private void CreateSettingsPopup()
        {
            settingsPopup = new Popup();
            settingsPopup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            settingsPopup.IsLightDismissEnabled = true;
            settingsPopup.Width = settingsWidth;
            settingsPopup.Height = windowBounds.Height;
        }

        private void AddProperAnimationForPanel()
        {
            settingsPopup.ChildTransitions = new TransitionCollection();
            settingsPopup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                       EdgeTransitionLocation.Right :
                       EdgeTransitionLocation.Left,
            });
        }

        private void DefineLocationOfOurPopup()
        {
            settingsPopup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (windowBounds.Width - settingsWidth) : 0);
            settingsPopup.SetValue(Canvas.TopProperty, 0);
            settingsPopup.IsOpen = true;
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                settingsPopup.IsOpen = false;
            }
        }

        void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnWindowActivated;
        }
        #endregion
    }
}
