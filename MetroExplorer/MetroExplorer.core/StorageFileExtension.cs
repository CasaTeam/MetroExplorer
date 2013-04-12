namespace MetroExplorer.core
{
    using System;
    using Windows.Storage;

    public static class StorageFileExtensions
    {
        public static Boolean IsImageFile(this StorageFile file)
        {
            return file.FileType.ToUpper().Equals(".JPG") ||
                   file.FileType.ToUpper().Equals(".JPEG") ||
                   file.FileType.ToUpper().Equals(".PNG") ||
                   file.FileType.ToUpper().Equals(".BMP");
        }
    }
}
