using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetroExplorer.core.Objects;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace MetroExplorer.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            float distance = Levenshtein.Distance("user", "us");
            distance = Levenshtein.Distance("vomax", "volmax");
        }
    }
}
