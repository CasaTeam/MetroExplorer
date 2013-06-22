namespace MetroExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Core;

    /// <summary>
    /// 这个文件操作了一个存在应用自己目录下的一个txt文件
    /// 这个文件用来记录用户是否是第一次使用这个应用
    /// </summary>
    public class FirstUsingRecord
    {
        private readonly string GuidePageFileRecordName = "GuidePageFileRecord.txt";

        private FirstUsingRecord()
        {

        }

        public async Task<bool> IsFirstUsing()
        {
            if (await UserPreferenceRecord.TextFileAccess.GetInstance().GetStorageFile(GuidePageFileRecordName) == null)
                return true;
            else if (await ReadRecordGuidePageFile() == "")
                return true;
            else
                return false;
        }

        public void WriteRecordGuidePageFile(string text = "")
        {
            UserPreferenceRecord.TextFileAccess.GetInstance().CreateWriteText(text, GuidePageFileRecordName);
        }

        public async Task<string> ReadRecordGuidePageFile()
        {
            return await UserPreferenceRecord.TextFileAccess.GetInstance().ReadText(GuidePageFileRecordName);
        }

        public static FirstUsingRecord GetInstance()
        {
            return Singleton<FirstUsingRecord>.Instance;
        }
    }
}
