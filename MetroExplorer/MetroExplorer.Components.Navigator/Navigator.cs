using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using MetroExplorer.Components.Navigator.Objects;

namespace MetroExplorer.Components.Navigator
{
    public sealed class Navigator : ItemsControl
    {
        #region Constants

        private const string GridItemListElement = "GridItemList";

        private const string ListBoxDropDownElement = "ListBoxDropDown";

        #endregion

        #region Fields

        private IEnumerable<NavigatorNode> _path;
        private int _currentIndex;
        private NavigatorNodeCommandType _commandType;

        private Grid _gridItemList;
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
                            string newPath = Path.Substring(0, Path.IndexOf(args.Path) + args.Path.Length);
                            Path = newPath;
                            break;
                        case NavigatorNodeCommandType.ShowList:
                            double positionX = args.PointerPositionX;
                            if (_gridItemList != null)
                            {
                                ((ListBox)_gridItemList.Children[0]).ItemsSource = ItemListArray[_currentIndex];
                                _gridItemList.Margin = new Thickness(positionX - _gridItemList.Width, 80.0, 0, -342.0);
                                VisualStateManager.GoToState(this, "Pressed", true);
                            }
                            break;
                    }

                };
                nodes.Add(new NavigatorNode(index, value, command, ItemListArray[index]));
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

        #endregion

        #region Properties

        public IReadOnlyList<string>[] ItemListArray { get; set; }

        #endregion

        #region Constructors

        public Navigator()
        {
            this.DefaultStyleKey = typeof(Navigator);
            _commandType = NavigatorNodeCommandType.None;
        }

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _gridItemList = (Grid)GetTemplateChild(GridItemListElement);
            _listBoxDropDown = (ListBox)GetTemplateChild(ListBoxDropDownElement);
            if (_listBoxDropDown != null)
            {
                _listBoxDropDown.SelectionChanged += ListBoxDropDownSelectionChanged;
            }
        }

        #endregion

        #region Events

        private void ListBoxDropDownSelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            _commandType = NavigatorNodeCommandType.Change;

            IEnumerable<string> splitedPath = Path.Split('\\').Take(_currentIndex + 1);
            string newPath = splitedPath.Aggregate(string.Empty, (current, next) => current += next + "\\")
                + (string.IsNullOrWhiteSpace(((string)e.AddedItems.FirstOrDefault())) ? string.Empty : (string)(e.AddedItems.FirstOrDefault())); ;
            NavigatorNodeCommandArgument argument =
                new NavigatorNodeCommandArgument(
                    _currentIndex,
                    newPath,
                    _commandType);
            if (NPathChanged != null)
                NPathChanged(this, argument);
        }

        #endregion
    }
}
