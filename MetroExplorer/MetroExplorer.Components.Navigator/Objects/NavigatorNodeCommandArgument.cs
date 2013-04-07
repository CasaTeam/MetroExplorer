namespace MetroExplorer.Components.Navigator.Objects
{
    using System;

    public enum NavigatorNodeCommandType
    {
        None = 0,
        Reduce = 1,
        ShowList = 2,
        Change = 3
    }

    public class NavigatorNodeCommandArgument : EventArgs
    {
        public int Index { get; private set; }

        public string Path { get; private set; }

        public NavigatorNodeCommandType CommandType { get; private set; }

        public double PointerPositionX { get; private set; }

        public NavigatorNodeCommandArgument(
            int index,
            string path,
            NavigatorNodeCommandType commandType)
        {
            Index = index;
            Path = path;
            CommandType = commandType;
        }

        public NavigatorNodeCommandArgument(
            int index,
            string path,
            NavigatorNodeCommandType commandType,
            double positionX)
            : this(index, path, commandType)
        {
            PointerPositionX = positionX;
        }
    }
}
