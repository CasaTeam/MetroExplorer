namespace MetroExplorer.Core.Objects
{
    using System;

    /// <summary>
    /// 比较两个字符串接近程度
    /// </summary>
    public static class Levenshtein
    {
        public static float Distance(string stra, string strb)
        {
            int n = stra.Length,
                m = strb.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;
            if (m == 0)
                return n;

            for (int i = 0; i <= n; )
                d[i, 0] = i++;

            for (int j = 0; j <= m; )
                d[0, j] = j++;

            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                {
                    int cost = (strb[j - 1] == stra[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1)
                        , d[i - 1, j - 1] + cost);
                }

            return 1 - (float)d[n, m] / (float)Math.Max(m, n);
        }
    }
}
