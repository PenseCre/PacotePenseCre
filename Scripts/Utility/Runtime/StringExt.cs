using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class StringExt
{
    public static string InsertStringAt(string str, int[] indexes, string insert)
    {
        if (indexes.Length == 0 || insert.Length == 0)
        {
            return str;
        }

        var indexes2 = indexes.OrderBy(x => x).ToArray();

        var sb = new StringBuilder(str.Length + indexes.Length * insert.Length);

        int j = 0;

        for (int i = 0; i < indexes2.Length; i++)
        {
            if (indexes2[i] < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexes2));
            }

            if (indexes2[i] > str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indexes2));
            }

            sb.Append(str, j, indexes2[i] - j);
            sb.Append(insert);

            j = indexes2[i];
        }

        sb.Append(str, j, str.Length - j);

        return sb.ToString();
    }
}