using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using MetroExplorer.core.Objects;

namespace MetroExplorer.core.Utils
{
    public class EventLogger
    {
        private static readonly String UMENG_APP_KEY = "5151b11f56240bba2a002fbd";

        public static readonly String LABEL_HOME_PAGE = "home_page";//
        public static readonly String LABEL_EXPLORER_PAGE = "explorer_page";//

        public static readonly String ADD_FOLDER_CLICK = "add_folder_click";//
        public static readonly String ADD_FOLDER_DONE = "add_folder_done";//
        public static readonly String ADD_FOLDER_CANCEL = "add_folder_cancel";//

        public static readonly String FOLDER_OPENED = "folder_opened";//
        public static readonly String FILE_OPENED = "file_opened";//

        public static readonly String PHOTO_VIEWED = "photo_viewed";//

        public static readonly String SUPPORT_US = "support_us";//
        public static readonly String LANGUAGES_SETTINGS = "languages_settings";//

        public static readonly String PARAM_LANGUAGES_EN = "en";//
        public static readonly String PARAM_LANGUAGES_FR = "fr";//
        public static readonly String PARAM_LANGUAGES_ZH = "zh";//


        public static void onLaunch()
        {
            #if DEBUG
            UmengSDK.UmengAnalytics.setDebug(true);
            #endif
            UmengSDK.UmengAnalytics.setSessionContinueInterval(TimeSpan.FromSeconds(30));
            UmengSDK.UmengAnalytics.onLaunching(UMENG_APP_KEY);
        }

        public static void onActionEvent(String event_id)
        {
            UmengSDK.UmengAnalytics.onEvent(event_id);
        }

        public static void onActionEvent(String event_id, String label)
        {
            UmengSDK.UmengAnalytics.onEvent(event_id, label);
        }
    }
}
