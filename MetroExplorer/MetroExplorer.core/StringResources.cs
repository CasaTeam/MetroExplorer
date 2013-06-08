namespace MetroExplorer.core
{
    using Windows.ApplicationModel.Resources;

    /// <summary>
    /// 这个服务于多语言功能
    /// 使用此类可以在.cs文件中帮助提取strings resource里面的值
    /// </summary>
    public class StringResources
    {
        private static ResourceLoader _resourceLoader;
        public static ResourceLoader ResourceLoader
        {
            get { return _resourceLoader ?? (_resourceLoader = new ResourceLoader()); }
        }
    }
}
