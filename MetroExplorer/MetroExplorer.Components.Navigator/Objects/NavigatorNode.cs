using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroExplorer.Components.Navigator.Objects
{
    public sealed class NavigatorNode
    {
        public string NodeName { get; set; }
        public ICommand NodeAction { get; private set; }

        public NavigatorNode(string name,ICommand action)
        {
            NodeName = name;
            NodeAction = action;
        }
    }
}
