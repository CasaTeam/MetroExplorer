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
using Windows.UI.ApplicationSettings;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MetroExplorer.LayoutsBar
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SupportUs : MetroExplorer.Common.LayoutAwarePage
    {
        const int ContentAnimationOffset = 100;
        DispatcherTimer dispatcherTimer = null;
        public SupportUs()
        {
            this.InitializeComponent();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);

            ChangeTheme(Theme.ThemeLibarary.CurrentTheme);
            this.Loaded += SupportUs_Loaded;
            this.Unloaded += SupportUs_Unloaded;
        }

        void SupportUs_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Start();
        }

        void SupportUs_Unloaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            AdControl1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            AdControl2.Visibility = Windows.UI.Xaml.Visibility.Visible;
            dispatcherTimer.Stop();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
          
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

            AdControl1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            AdControl2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        #region propertychanged
        private void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }

    public sealed partial class SupportUs : MetroExplorer.Common.LayoutAwarePage, INotifyPropertyChanged
    {
        private string _backgroundColor = "WHITE";
        public string BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                NotifyPropertyChanged("BackgroundColor");
            }
        }

        private string _bottomBarBackground = "WHITE";
        public string BottomBarBackground
        {
            get
            {
                return _bottomBarBackground;
            }
            set
            {
                _bottomBarBackground = value;
                NotifyPropertyChanged("BottomBarBackground");
            }
        }

        private string _titleForeground = Theme.ThemeLibarary.TitleForeground;
        public string TitleForeground
        {
            get
            {
                return _titleForeground;
            }
            set
            {
                _titleForeground = value;
                NotifyPropertyChanged("TitleForeground");
            }
        }

        private string _itemBackground = "WHITE";
        public string ItemBackground
        {
            get
            {
                return _itemBackground;
            }
            set
            {
                _itemBackground = value;
                NotifyPropertyChanged("ItemBackground");
            }
        }

        private string _itemSmallBackground = "WHITE";
        public string ItemSmallBackground
        {
            get
            {
                return _itemSmallBackground;
            }
            set
            {
                _itemSmallBackground = value;
                NotifyPropertyChanged("ItemSmallBackground");
            }
        }

        private string _itemSelectedBorderColor = "WHITE";
        public string ItemSelectedBorderColor
        {
            get
            {
                return _itemSelectedBorderColor;
            }
            set
            {
                _itemSelectedBorderColor = value;
                NotifyPropertyChanged("ItemSelectedBorderColor");
            }
        }

        private string _itemTextForeground = "WHITE";
        public string ItemTextForeground
        {
            get
            {
                return _itemTextForeground;
            }
            set
            {
                _itemTextForeground = value;
                NotifyPropertyChanged("ItemTextForeground");
            }
        }

        private string _itemBigBackground = "WHITE";
        public string ItemBigBackground
        {
            get
            {
                return _itemBigBackground;
            }
            set
            {
                _itemBigBackground = value;
                NotifyPropertyChanged("ItemBigBackground");
            }
        }

        private void ChangeTheme(Theme.Themes themeYouWant)
        {
            Theme.ThemeLibarary.ChangeTheme(themeYouWant);
            MainBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(System.Convert.ToByte("FF", 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(3, 2), 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(5, 2), 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(7, 2), 16)));
            MainGrid.Background = new SolidColorBrush(Color.FromArgb(System.Convert.ToByte("FF", 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(3, 2), 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(5, 2), 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(7, 2), 16)));
            FlyoutContent.Background = new SolidColorBrush(Color.FromArgb(System.Convert.ToByte("FF", 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(3, 2), 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(5, 2), 16),
                                                                       System.Convert.ToByte(Theme.ThemeLibarary.ItemBigBackground.Substring(7, 2), 16)));
            //BottomBarBackground = Theme.ThemeLibarary.BottomBarBackground;
            //TitleForeground = Theme.ThemeLibarary.TitleForeground;
            //ItemBackground = Theme.ThemeLibarary.ItemBackground;
            //ItemSmallBackground = Theme.ThemeLibarary.ItemSmallBackground;
            //ItemSelectedBorderColor = Theme.ThemeLibarary.ItemSelectedBorderColor;
            //ItemTextForeground = Theme.ThemeLibarary.ItemTextForeground;
            //ItemBigBackground = Theme.ThemeLibarary.ItemBigBackground;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
