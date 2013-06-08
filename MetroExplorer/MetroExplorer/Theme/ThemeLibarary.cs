namespace MetroExplorer.Theme
{
    public class ThemeLibarary
    {
        public static string BackgroundColor { get; set; }
        public static string BottomBarBackground { get; set; }
        public static string TitleForeground { get; set; }
        public static string ItemBackground { get; set; }
        public static string ItemSmallBackground { get; set; }
        public static string ItemBigBackground { get; set; }
        public static string ItemSelectedBorderColor { get; set; }
        public static string ItemTextForeground { get; set; }

        public static void ChangeTheme()
        {
            FillTheme("#FFf2f2f2", "#FF1e90cd", "#FF343029", "#FF1e90cd", "#FF1e90cd", "#FF828174", "#FFFFFFFF", "#FF1e90cd");
        }

        private static void FillTheme(string bakcgroundColor, string bottomBarBackground, string titleForeground, 
                                      string itemBackground, string itemSmallBackground, string itemSelectedBorderColor,
                                      string itemTextForeground, string itemBigBackground)
        {
            BackgroundColor = bakcgroundColor;
            BottomBarBackground = bottomBarBackground;
            TitleForeground = titleForeground;
            ItemBackground = itemBackground;
            ItemSmallBackground = itemSmallBackground;
            ItemSelectedBorderColor = itemSelectedBorderColor;
            ItemTextForeground = itemTextForeground;
            ItemBigBackground = itemBigBackground;
        }
    }
}
