using MetroExplorer.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroExplorer.UserPreferenceRecord;

namespace MetroExplorer.UserPreferenceRecord
{
    /// <summary>
    /// 记录用户的上次使用习惯。
    /// 目前主要是：
    /// 如果用户上一次离开程序，保持的是列表显示文件的模式，那么再次打开时就是用列表显示，否则，用其他显示
    /// </summary>
    public class UserPreferenceRecord
    {
        private readonly string UserPreferenceRecordName = "UserPreferenceRecord.txt";

        private UserPreferenceRecord()
        { 
        
        }

        public async Task<bool> IfListMode()
        {
            if (await TextFileAccess.GetInstance().GetStorageFile(UserPreferenceRecordName) == null)
                return false;
            else if ((await ReadUserPreferenceRecord()).Contains("List"))
                return true;
            else
                return false;
        }

        public void WriteUserPreferenceRecord(string text)
        {
            TextFileAccess.GetInstance().CreateWriteText(text, UserPreferenceRecordName);
        }

        public async Task<string> ReadUserPreferenceRecord()
        {
            return await TextFileAccess.GetInstance().ReadText(UserPreferenceRecordName);
        }

        public static UserPreferenceRecord GetInstance()
        {
            return Singleton<UserPreferenceRecord>.Instance;
        }
    }
}
