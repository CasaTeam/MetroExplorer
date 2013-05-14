using MetroExplorer.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroExplorer.Common
{
    public class UserPreferenceRecord
    {
        private readonly string UserPreferenceRecordName = "UserPreferenceRecord.txt";

        private UserPreferenceRecord()
        { 
        
        }

        public async Task<bool> IfListMode()
        {
            if (await Common.TextFileAccess.GetInstance().GetStorageFile(UserPreferenceRecordName) == null)
                return false;
            else if ((await ReadUserPreferenceRecord()).Contains("List"))
                return true;
            else
                return false;
        }

        public void WriteUserPreferenceRecord(string text)
        {
            Common.TextFileAccess.GetInstance().CreateWriteText(text, UserPreferenceRecordName);
        }

        public async Task<string> ReadUserPreferenceRecord()
        {
            return await Common.TextFileAccess.GetInstance().ReadText(UserPreferenceRecordName);
        }

        public static UserPreferenceRecord GetInstance()
        {
            return Singleton<UserPreferenceRecord>.Instance;
        }
    }
}
