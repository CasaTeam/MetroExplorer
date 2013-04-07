using MetroExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using MetroExplorer.core.Utils;
// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace MetroExplorer
{
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
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            EventLogger.onLaunch();
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
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested; // TODO: INIT Command Request 
                Theme.ThemeLibarary.CurrentTheme = Theme.Themes.EAE9E5; // TODO: INIT DEFAUT Theme
                Theme.ThemeLibarary.ChangeTheme(Theme.ThemeLibarary.CurrentTheme);

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(PageMain), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
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
    }


    /// <summary>
    /// CommandsRequested
    /// </summary>
    sealed partial class App : Application
    {
        void OnCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            UICommandInvokedHandler handler = new UICommandInvokedHandler(OnSettingsCommand);

            //SettingsCommand preferenceCommand = new SettingsCommand("Preference", (new ResourceLoader()).GetString("SettingCommand_Preference"), handler);
            //eventArgs.Request.ApplicationCommands.Add(preferenceCommand);
        }

        void OnSettingsCommand(IUICommand command)
        {
            SettingsCommand settingsCommand = (SettingsCommand)command;
            PreferenceSettingCommand(settingsCommand);
        }

        #region Right Command Layout
        private Popup settingsPopup;
        private double settingsWidth = 500;
        private Rect windowBounds;

        void PreferenceSettingCommand(SettingsCommand settingsCommand)
        {
            windowBounds = Window.Current.Bounds;
            if (settingsCommand.Id.ToString() == "Preference")
            {
                CreatePopupWindowContainsFlyout("Preference");
            }
        }

        private void CreatePopupWindowContainsFlyout(string option)
        {
            CreateSettingsPopup();
            AddProperAnimationForPanel();
            if (option == "Preference")
            {
                LayoutsBar.Preference mypane = new LayoutsBar.Preference();
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
                       EdgeTransitionLocation.Left
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
