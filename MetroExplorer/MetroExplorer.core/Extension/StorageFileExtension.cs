namespace MetroExplorer.Core
{
    using System;
    using Windows.Storage;

    public static class StorageFileExtensions
    {
        public static bool IsImageFile(this StorageFile file)
        {
            return file.FileType.ToUpper().Equals(".JPG") ||
                   file.FileType.ToUpper().Equals(".JPEG") ||
                   file.FileType.ToUpper().Equals(".PNG") ||
                   file.FileType.ToUpper().Equals(".BMP");
        }

        public static bool IsMediaFile(this StorageFile file)
        {
            return file.IsImageFile() ||
                file.FileType.ToUpper().EndsWith(".RMVB") ||
                file.FileType.ToUpper().EndsWith(".MP4") ||
                file.FileType.ToUpper().EndsWith(".PNG");
        }
    }
}
