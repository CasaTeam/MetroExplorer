using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MetroExplorer.core
{
    public static class StorageFileExtensions
    {
        public static Boolean IsImageFile(this StorageFile file)
        {
            if (file.FileType.ToUpper().Equals(".JPG") ||
                file.FileType.ToUpper().Equals(".JPEG") ||
                file.FileType.ToUpper().Equals(".PNG") ||
                file.FileType.ToUpper().Equals(".BMP"))
                return true;
            return false;
        }
    }
}
