namespace MetroExplorer.Components.Navigator
{
    using System.Windows.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Objects;

    public sealed class NavigatorItem : Control
    {
        #region TemplateParts

        internal const string TextBlockContentElement = "TextBlockContent";

        #endregion

        #region Fields

        private bool _pressed;

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(NavigatorItem),
            new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(NavigatorItem),
            new PropertyMetadata(string.Empty));

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(NavigatorItem),
            new PropertyMetadata(-1));

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        #endregion

        #region Constructors

        public NavigatorItem()
        {
            DefaultStyleKey = typeof(NavigatorItem);
        }

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PointerEntered += NavigatorItemPointerEntered;
            PointerExited += NavigatorItemPointerExited;
            PointerPressed += NavigatorItemPointerPressed;
            PointerReleased += NavigatorItemPointerReleased;
            PointerMoved += NavigatorItemPointerMoved;
            PointerCaptureLost += NavigatorItemPointerCaptureLost;
        }

        #endregion

        #region Events

        private void NavigatorItemPointerEntered(
            object sender,
            PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Entered", true);
        }

        private void NavigatorItemPointerExited(
            object sender,
            PointerRoutedEventArgs e)
        {
            _pressed = false;
            VisualStateManager.GoToState(this, "Released", true);
        }

        private void NavigatorItemPointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            _pressed = true;
            VisualStateManager.GoToState(this, "Pressed", true);
        }

        private void NavigatorItemPointerReleased(
            object sender,
            PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Released", true);
            if (_pressed && Command != null)
            {
                Command.Execute(
                    new NavigatorNodeCommandArgument(Index, Content, NavigatorNodeCommandType.Reduce));
                _pressed = false;
            }
        }

        private void NavigatorItemPointerMoved(
            object sender,
            PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Released", true);
        }

        private void NavigatorItemPointerCaptureLost(
            object sender,
            PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Released", true);
        }

        #endregion
    }
}
