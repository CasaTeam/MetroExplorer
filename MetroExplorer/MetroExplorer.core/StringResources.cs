using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace MetroExplorer.core
{
    public class StringResources
    {
        private static ResourceLoader resourceLoader;
        public static ResourceLoader ResourceLoader
        {
            get
            {
                if (resourceLoader == null)
                    resourceLoader = new ResourceLoader();
                return resourceLoader;
            }
        }
    }
}
