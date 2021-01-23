using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Null.Library
{
    public static class Function
    {
        public static string CombinePath(string path1, string path2)
        {
            if (path1 == null || path1 == string.Empty)
                return path2;
            if (path2 == null || path2 == string.Empty)
                return path1;

            path1 = path1.TrimEnd(Path.DirectorySeparatorChar);
            path2 = path2.TrimStart(Path.DirectorySeparatorChar);

            if (path2.StartsWith($".{Path.DirectorySeparatorChar}"))
                path2 = path2.Substring(2);

            return $"{path1}{Path.DirectorySeparatorChar}{path2}";
        }
        public static string CombinePath(params string[] paths)
        {
            if (paths.Length > 0)
            {
                string result = paths[0];
                for (int i = 1; i < paths.Length; i++)
                {
                    result = CombinePath(result, paths[i]);
                }
                return result;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
