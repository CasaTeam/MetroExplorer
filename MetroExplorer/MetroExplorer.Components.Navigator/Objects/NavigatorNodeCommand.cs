namespace MetroExplorer.Components.Navigator.Objects
{
    using System;
    using System.Windows.Input;

    public sealed class NavigatorNodeCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler<NavigatorNodeCommandArgument> Command;

        public void Execute(object parameter)
        {
            if (Command != null)
                Command(this, (NavigatorNodeCommandArgument)parameter);

            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }
    }
}
