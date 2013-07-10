namespace MetroExplorer.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MetroExplorer.Core.Objects;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

    [TestClass]
    public class LevenshteinTest
    {
        [TestMethod]
        public void LevenshteinDistance()
        {
            float distance = Levenshtein.Distance("birthday Cake.bmp", "cake bmp");
            distance = Levenshtein.Distance("vomax", "volmax");
        }
    }
}
