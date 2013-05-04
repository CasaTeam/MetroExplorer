using Windows.UI.Xaml.Media;

namespace MetroExplorer.Components.Navigator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Objects;

    public sealed class Navigator : ItemsControl
    {
        #region Constants

        private const string PopupListElement = "PopupList";

        private const string ListBoxDropDownElement = "ListBoxDropDown";

        #endregion

        #region Fields

        private IEnumerable<NavigatorNode> _path;
        private int _currentIndex;
        private NavigatorNodeCommandType _commandType;

        private Popup _popupList;
        private ListBox _listBoxDropDown;

        #endregion

        #region EventHandlers

        public event EventHandler<NavigatorNodeCommandArgument> NPathChanged;

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(Navigator),
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
            NavigatorNodeCommandArgument argument =
                new NavigatorNodeCommandArgument(
                    _currentIndex,
                    (string)e.NewValue,
                    _commandType);

            int index = 0;
            List<NavigatorNode> nodes = new List<NavigatorNode>();
            foreach (string value in argument.Path.Split('\\').Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                NavigatorNodeCommand command = new NavigatorNodeCommand();
                command.Command += (sender, args) =>
                {
                    _currentIndex = args.Index;

                    _commandType = args.CommandType;
                    switch (_commandType)
                    {
                        case NavigatorNodeCommandType.Reduce:
                            string newPath = Path.Substring(0, Path.IndexOf(args.Path, StringComparison.Ordinal) + args.Path.Length);
                            Path = newPath;
                            break;
                        case NavigatorNodeCommandType.ShowList:
                            double positionX = args.PointerPositionX;
                            if (_popupList != null)
                            {
                                _listBoxDropDown.ItemsSource = ItemListArray[_currentIndex];
                                _popupList.Margin = new Thickness(positionX - _popupList.Width, ActualHeight + 5.0, 0, -342.0);
                                _popupList.IsOpen = true;
                            }
                            break;
                    }

                };
                if (index < ItemListArray.Count())
                    nodes.Add(new NavigatorNode(index, value, command, Background, ItemListArray[index]));
                index++;
            }

            _path = nodes;
            ItemsSource = _path;
            if (NPathChanged != null)
                NPathChanged(this, argument);
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }


        public static readonly DependencyProperty DropBackgroundProperty =
            DependencyProperty.Register("DropBackground", typeof(Brush), typeof(NavigatorItem),
                                        new PropertyMetadata(new SolidColorBrush()));

        public Brush DropBackground
        {
            get { return (Brush)GetValue(DropBackgroundProperty); }
            set { SetValue(DropBackgroundProperty, value); }
        }

        #endregion

        #region Properties

        public List<string>[] ItemListArray { get; set; }

        #endregion

        #region Constructors

        public Navigator()
        {
            DefaultStyleKey = typeof(Navigator);
            _commandType = NavigatorNodeCommandType.None;
        }

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _popupList = (Popup)GetTemplateChild(PopupListElement);
            _listBoxDropDown = (ListBox)GetTemplateChild(ListBoxDropDownElement);
            if (_listBoxDropDown == null) return;
            _listBoxDropDown.SelectionChanged += ListBoxDropDownSelectionChanged;
        }

        #endregion

        #region Events

        private void ListBoxDropDownSelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            _commandType = NavigatorNodeCommandType.Change;

            IEnumerable<string> splitedPath = Path.Split('\\').Take(_currentIndex + 1);
            string newPath = splitedPath.Aggregate(string.Empty, (current, next) => next + "\\")
                + (string.IsNullOrWhiteSpace(((string)e.AddedItems.FirstOrDefault())) ? string.Empty : (string)(e.AddedItems.FirstOrDefault()));
            NavigatorNodeCommandArgument argument =
                new NavigatorNodeCommandArgument(_currentIndex, newPath, _commandType);
            if (NPathChanged != null)
                NPathChanged(this, argument);
        }

        #endregion
    }
}
