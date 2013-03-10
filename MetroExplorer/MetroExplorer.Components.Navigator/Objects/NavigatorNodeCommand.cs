using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroExplorer.Components.Navigator.Objects
{
    public sealed class NavigatorNodeCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler<string> Command;

        public void Execute(object parameter)
        {
            if (Command != null)
                Command(this, parameter.ToString());
        }
    }
}
