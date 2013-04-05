using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroExplorer.Theme
{
    public class ThemeLibarary
    {
        public static Themes CurrentTheme { get; set; }
        public static string BackgroundColor { get; set; }
        public static string BottomBarBackground { get; set; }
        public static string TitleForeground { get; set; }
        public static string ItemBackground { get; set; }
        public static string ItemSmallBackground { get; set; }
        public static string ItemSelectedBorderColor { get; set; }
        public static string ItemTextForeground { get; set; }
        public static void ChangeTheme(Themes themeyouwant)
        {
            switch (themeyouwant)
            { 
                case Themes.Bo:
                    FillTheme("#FF1E2647", "#FF0E5480", "#DDFFFFFF", "#FF1C6E62", "#FF0E5480", "#FF6285A2", "#FFFFFFFF");
                    break;
                case Themes.E2E3DA:
                    FillTheme("#FFE2E3DA", "#FF64675F", "#FF343029", "#FF8EBAB6", "#FF64675F", "#FF618089", "#FFFFFFFF");
                    break;
                case Themes.E4DFD1:
                    FillTheme("#FFE4DFD1", "#FF8C796A", "#FF8C796A", "#FF8FA08E", "#FF8C796A", "#FF618089", "#FFFFFFFF");
                    break;
                case Themes.E4E8E8:
                    FillTheme("#FFE4E8E8", "#FF607075", "#FF607075", "#FF90B1B2", "#FF607075", "#FF6285A2", "#FFFFFFFF");
                    break;
                case Themes.EAE9E5:
                    FillTheme("#FFEAE9E5", "#FF9E403E", "#FF343029", "#FFAEAEA1", "#FF9E403E", "#FF828174", "#FFFFFFFF");
                    break;
                case Themes.FFF1DD:
                    FillTheme("#FFFFF1DD", "#FFC7AE86", "#FFB39667", "#FFE1988C", "#FFC7AE86", "#FF83BDB0", "#FFFFFFFF");
                    break;
            }
            CurrentTheme = themeyouwant;
        }

        private static void FillTheme(string bakcgroundColor, string bottomBarBackground, string titleForeground, string itemBackground, string itemSmallBackground, string itemSelectedBorderColor, string itemTextForeground)
        {
            BackgroundColor = bakcgroundColor;
            BottomBarBackground = bottomBarBackground;
            TitleForeground = titleForeground;
            ItemBackground = itemBackground;
            ItemSmallBackground = itemSmallBackground;
            ItemSelectedBorderColor = itemSelectedBorderColor;
            ItemTextForeground = itemTextForeground;
        }
    }

    public enum Themes
    { 
        Bo,
        E4E8E8,
        E4DFD1,
        FFF1DD,
        EAE9E5,
        E2E3DA
    }
}
