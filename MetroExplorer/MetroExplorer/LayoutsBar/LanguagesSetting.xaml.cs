using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using MetroExplorer.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MetroExplorer.LayoutsBar
{
    using core.Utils;
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LanguagesSetting : Common.LayoutAwarePage
    {
        public LanguagesSetting()
        {
            this.InitializeComponent();


            //FlyoutContent.Transitions = new TransitionCollection();
            //FlyoutContent.Transitions.Add(new EntranceThemeTransition()
            //{
            //    FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            //});
        }

        private void MySettingsBackClicked(object sender, RoutedEventArgs e)
        {
            // First close our Flyout.
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }

            // If the app is not snapped, then the back button shows the Settings pane again.
            if (Windows.UI.ViewManagement.ApplicationView.Value != Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                SettingsPane.Show();
            }
        }

        private void Button_Germain(object sender, TappedRoutedEventArgs e)
        {
            NotificationHelper.CreateToastNotifications("");
        }

        private void Button_Italie(object sender, TappedRoutedEventArgs e)
        {
            NotificationHelper.CreateToastNotifications("");
        }

        private async void Button_English(object sender, TappedRoutedEventArgs e)
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            NotificationHelper.CreateToastNotifications("You have just set the application language to English");
            MessageDialog dialog = new MessageDialog("You need to quit and restart the application to change the language", "Change language to English");
            dialog.Commands.Add(new UICommand("Shut up app for restart", p =>
            {
                App.Current.Exit();
            }));
            dialog.Commands.Add(new UICommand("Later"));
            EventLogger.onActionEvent(EventLogger.LANGUAGES_SETTINGS, EventLogger.PARAM_LANGUAGES_EN);
            await dialog.ShowAsync();
        }

        private async void Button_Zhongwen(object sender, TappedRoutedEventArgs e)
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "zh-CN";
            NotificationHelper.CreateToastNotifications("您刚刚将应用程序语言改为中文");
            MessageDialog dialog = new MessageDialog("您需要退出，重新进入应用程序，才可以切换程序语言!", "切换程序系统语言提示");
            dialog.Commands.Add(new UICommand("关闭程序，以便重新启动", p =>
            {
                App.Current.Exit();
            }));
            dialog.Commands.Add(new UICommand("待会儿重启"));
            EventLogger.onActionEvent(EventLogger.LANGUAGES_SETTINGS, EventLogger.PARAM_LANGUAGES_ZH);
            await dialog.ShowAsync();
        }

        private async void Button_Francais(object sender, TappedRoutedEventArgs e)
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "fr-FR";
            NotificationHelper.CreateToastNotifications("Vous venez de choisir la langue français pour être la langue d'application!");
            MessageDialog dialog = new MessageDialog("Pour changer la langue, vous avez besoin de rédémarrer l'application!", "Changer la langue");
            dialog.Commands.Add(new UICommand("Fermer Pour rédémarrer", p =>
            {
                App.Current.Exit();
            }));
            dialog.Commands.Add(new UICommand("Reporting"));
            EventLogger.onActionEvent(EventLogger.LANGUAGES_SETTINGS, EventLogger.PARAM_LANGUAGES_FR);
            await dialog.ShowAsync();
        }

        private void Button_Arabe(object sender, TappedRoutedEventArgs e)
        {
            NotificationHelper.CreateToastNotifications("");
        }

        private void Button_Espanol(object sender, TappedRoutedEventArgs e)
        {
            NotificationHelper.CreateToastNotifications("");
        }

        private void Button_Russia(object sender, TappedRoutedEventArgs e)
        {
            NotificationHelper.CreateToastNotifications("");
        }

        private void Button_Naruto(object sender, TappedRoutedEventArgs e)
        {
            NotificationHelper.CreateToastNotifications("");
        }
    }
}
