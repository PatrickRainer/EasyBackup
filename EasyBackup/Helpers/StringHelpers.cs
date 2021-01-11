using System;

namespace EasyBackup.Helpers
{
    public static class StringHelpers
    {
        public static string ExtractLastFolder(string path)
        {
            string result;

            if (string.IsNullOrEmpty(path)) return string.Empty;

            try
            {
                result = path.Substring(path.LastIndexOf(@"\", StringComparison.Ordinal));
            }
            catch (ArgumentOutOfRangeException e)
            {
                return string.Empty;
            }

            return result;
        }
    }
}