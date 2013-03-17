using MetroExplorer.Components.Navigator.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MetroExplorer.Components.Navigator
{
    public sealed class Navigator : ItemsControl
    {
        #region Constants

        #endregion

        #region Fields

        private IEnumerable<NavigatorNode> _path;

        #endregion

        #region EventHandlers

        public event EventHandler<string> NPathChanged;

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty PathProperty
            = DependencyProperty.Register("Path", typeof(string), typeof(Navigator),
            new PropertyMetadata(string.Empty, PathChanged));

        private static void PathChanged(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs e
            )
        {
            Navigator navigator = (Navigator)obj;
            if (navigator != null)
                navigator.NavigatorPathChanged(e);
        }

        public void NavigatorPathChanged(
            DependencyPropertyChangedEventArgs e)
        {
            string path = (string)e.NewValue;
            _path = path.Split('\\')
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value =>
                {
                    NavigatorNodeCommand command = new NavigatorNodeCommand();
                    command.Command += (sender, args) =>
                    {
                        string newPath = Path.Substring(0, Path.IndexOf(args) + args.Length);
                        Path = newPath;
                    };
                    return new NavigatorNode(value, command);
                });
            ItemsSource = _path;
            if (NPathChanged != null)
                NPathChanged(this, Path);
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        #endregion

        #region Constructors

        public Navigator()
        {
            this.DefaultStyleKey = typeof(Navigator);
        }

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        #endregion

        #region Events

        #endregion
    }
}
