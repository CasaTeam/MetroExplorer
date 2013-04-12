namespace MetroExplorer.core
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class VariableGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var viewModel = item as IResizable;

            element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, viewModel.Width);
            element.SetValue(VariableSizedWrapGrid.RowSpanProperty, viewModel.Height);
            base.PrepareContainerForItemOverride(element, item);
        }
    }

    public interface IResizable
    {
        int Width { get; set; }
        int Height { get; set; }
    }
}
