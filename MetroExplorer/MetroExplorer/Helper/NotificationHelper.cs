using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace MetroExplorer.Helper
{
    public class NotificationHelper
    {
        /// <summary>
        /// 创建本地toast 推送
        /// </summary>
        /// <param name="content"></param>
        public static void CreateToastNotifications(string content)
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            if (toastNotifier.Setting == NotificationSetting.Enabled)
            {
                var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                var tileAttributes = template.GetElementsByTagName("text")[0];
                tileAttributes.AppendChild(template.CreateTextNode(content));
                var notifications1 = new ToastNotification(template);
                toastNotifier.Show(notifications1);
            }
        }

        public async static void ShowError(Exception ex)
        {
            MessageDialog dialog = new MessageDialog(ex.Message, ":( ");
            await dialog.ShowAsync();
        }

        public async static void ShowInfo(string info)
        {
            MessageDialog dialog = new MessageDialog(info);
            await dialog.ShowAsync();
        }
    }
}
