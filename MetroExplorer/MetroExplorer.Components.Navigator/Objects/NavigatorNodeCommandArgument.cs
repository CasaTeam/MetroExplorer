using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroExplorer.Components.Navigator.Objects
{
    public class NavigatorNodeCommandArgument : EventArgs
    {
        public int Index { get; private set; }

        public string Path { get; private set; }

        public bool FromInner { get; private set; }

        public NavigatorNodeCommandArgument(int index, string path, bool fromInner)
        {
            Index = index;
            Path = path;
            FromInner = fromInner;
        }
    }
}
