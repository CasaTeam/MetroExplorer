namespace MetroExplorer.Components.Navigator
{
    using System.Collections.Generic;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Objects;
    using Core;
    using System.Windows.Input;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media.Animation;

    public sealed class NavigatorButton : Control
    {
        #region Constents

        private const string LayoutRootElement = "LayoutRoot";

        private const string ArrowTopToButtomStoryboard = "StoryboardArrowTopToButtom";

        private const string ArrowButtomToTopStoryboard = "StoryboardArrowButtomToTop";

        #endregion

        #region Fields

        private Border _layoutRoot;

        private Storyboard _arrowTopToButtomStoryboard;

        private Storyboard _arrowButtomToTopStoryboard;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(NavigatorButton),
            new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(NavigatorButton),
            new PropertyMetadata(-1));

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly DependencyProperty ItemListProperty =
            DependencyProperty.Register("ItemList", typeof(IEnumerable<string>), typeof(NavigatorButton),
            new PropertyMetadata(null));

        public IEnumerable<string> ItemList
        {
            get { return (IEnumerable<string>)GetValue(ItemListProperty); }
            set { SetValue(ItemListProperty, value); }
        }

        #endregion

        #region Constructors

        public NavigatorButton()
        {
            DefaultStyleKey = typeof(NavigatorButton);
        }

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _layoutRoot = (Border)GetTemplateChild(LayoutRootElement);
            if (_layoutRoot != null)
            {
                _arrowButtomToTopStoryboard = (Storyboard)_layoutRoot.Resources[ArrowButtomToTopStoryboard];
                _arrowTopToButtomStoryboard = (Storyboard)_layoutRoot.Resources[ArrowTopToButtomStoryboard];
            }

            PointerPressed += NavigatorButtonPointerPressed;
            PointerExited += NavigatorButtonPointerExited;
            PointerMoved += NavigatorButtonPointerMoved;
            PointerReleased += NavigatorButtonPointerReleased;
            PointerCaptureLost += NavigatorButtonPointerCaptureLost;
        }

        #endregion

        #region Events

        private void NavigatorButtonPointerMoved(
            object sender,
            PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Released", true);
        }

        private void NavigatorButtonPointerExited(
            object sender,
            PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Released", true);
        }

        private void NavigatorButtonPointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Pressed", true);
        }

        private void NavigatorButtonPointerReleased(
           object sender,
           PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Released", true);
            ScrollViewer parent = (ScrollViewer)((DependencyObject)sender).GetParentByName("NavigatorScrollViewer");
            Command.Execute(
                new NavigatorNodeCommandArgument(
                    Index, string.Empty,
                    NavigatorNodeCommandType.ShowList,
                    e.GetCurrentPoint(parent).Position.X + ActualWidth - e.GetCurrentPoint(this).Position.X,
                    this));
        }

        private void NavigatorButtonPointerCaptureLost(
            object sender,
            PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Released", true);
        }

        #endregion

        #region Public Methods

        public void BeginShowAnimation()
        {
            if (_arrowButtomToTopStoryboard != null)
                _arrowButtomToTopStoryboard.Begin();
        }

        public void BeginHideAnimation()
        {
            if (_arrowTopToButtomStoryboard != null)
                _arrowTopToButtomStoryboard.Begin();
        }

        #endregion
    }
}
