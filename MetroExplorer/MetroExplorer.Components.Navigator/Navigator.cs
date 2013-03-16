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

        private bool _isPointerPressed;
        private bool _update = false;

        private double _pointerPressedX;
        private double _totalWidth;
        private IEnumerable<NavigatorNode> _path;

        private StackPanel _navigatorItemStack;
        private ScrollViewer _navigatorScrollViewer;

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
            _update = true;
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

            _isPointerPressed = false;

            _pointerPressedX = 0.0;
            _totalWidth = 0.0;
        }

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _navigatorScrollViewer = (ScrollViewer)GetTemplateChild("NavigatorScrollViewer");
            _navigatorItemStack = (StackPanel)GetTemplateChild("NavigatorItemStack");

            if (_navigatorItemStack != null)
            {
                _navigatorItemStack.PointerEntered += NavigatorItemStackPointerEntered;
                _navigatorItemStack.PointerExited += NavigatorItemStackPointerExited;
                _navigatorItemStack.AddHandler(PointerPressedEvent, new PointerEventHandler(NavigatorItemStackPointerPressed), true);
                _navigatorItemStack.AddHandler(PointerReleasedEvent, new PointerEventHandler(NavigatorItemStackPointerReleased), true);
                _navigatorItemStack.PointerMoved += NavigatorItemStackPointerMoved;
                _navigatorItemStack.LayoutUpdated += NavigatorItemStackLayoutUpdated;
            }
        }


        #endregion

        #region Events

        private void NavigatorItemStackPointerEntered(
            object sender,
            PointerRoutedEventArgs e)
        {
        }

        private void NavigatorItemStackPointerExited(
            object sender,
            PointerRoutedEventArgs e)
        {
            _isPointerPressed = false;
        }

        private void NavigatorItemStackPointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            _isPointerPressed = true;
            _pointerPressedX = e.GetCurrentPoint(_navigatorItemStack).Position.X;
        }

        private void NavigatorItemStackPointerReleased(
            object sender,
            PointerRoutedEventArgs e)
        {
            _isPointerPressed = false;
        }

        private void NavigatorItemStackPointerMoved(
            object sender,
            PointerRoutedEventArgs e)
        {
            double interval = e.GetCurrentPoint(_navigatorItemStack).Position.X - _pointerPressedX,
                   marginLeft = _navigatorItemStack.Margin.Left,
                   navigatorScrollViewerWidth = _navigatorScrollViewer.ActualWidth,
                   minMarginLeft = navigatorScrollViewerWidth - _totalWidth;

            if (_isPointerPressed && _totalWidth > navigatorScrollViewerWidth)
            {
                if (marginLeft + interval < minMarginLeft)
                    marginLeft = minMarginLeft;
                else if (marginLeft + interval > 0)
                    marginLeft = 0;
                else
                    marginLeft += interval;

                _navigatorItemStack.Margin = new Thickness(marginLeft, 0, 0, 0);
            }
        }

        private void NavigatorItemStackLayoutUpdated(
            object sender,
            object e)
        {
            _totalWidth = ((ItemsPresenter)_navigatorItemStack.Children.First()).DesiredSize.Width;
            double marginLeft = _navigatorItemStack.Margin.Left;
            marginLeft = _totalWidth <= _navigatorScrollViewer.ActualWidth ? 0.0 : _navigatorScrollViewer.ActualWidth - _totalWidth;
            if (_navigatorItemStack.Margin.Left != marginLeft && _update)
            {
                _navigatorItemStack.Margin = new Thickness(marginLeft, 0, 0, 0);
                _update = false;
            }
        }

        #endregion
    }
}
