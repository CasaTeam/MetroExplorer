namespace MetroExplorer.core
{
    using Windows.ApplicationModel.Resources;

    public class StringResources
    {
        private static ResourceLoader _resourceLoader;
        public static ResourceLoader ResourceLoader
        {
            get { return _resourceLoader ?? (_resourceLoader = new ResourceLoader()); }
        }
    }
}
