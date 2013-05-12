using MetroExplorer.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroExplorer
{
    public class FirstUsingRecord
    {
        private readonly string GuidePageFileRecordName = "GuidePageFileRecord.txt";

        private FirstUsingRecord()
        { 
        
        }

        public async Task<bool> IsFirstUsing()
        {
            if (await Common.TextFileAccess.GetInstance().GetStorageFile(GuidePageFileRecordName) == null)
                return true;
            else if (await ReadRecordGuidePageFile() == "")
                return true;
            else
                return false;
        }

        //
        public void WriteRecordGuidePageFile(string text)
        {
            Common.TextFileAccess.GetInstance().CreateWriteText(text, GuidePageFileRecordName);
        }

        public async Task<string> ReadRecordGuidePageFile()
        {
            return await Common.TextFileAccess.GetInstance().ReadText(GuidePageFileRecordName);
        }

        public static FirstUsingRecord GetInstance()
        {
            return Singleton<FirstUsingRecord>.Instance;
        }
    }
}
