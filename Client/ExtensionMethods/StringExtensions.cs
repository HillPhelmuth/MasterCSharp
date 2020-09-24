using System;
using System.Collections.Generic;

namespace BlazorApp.Client.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string GetVideoId(this string url)
        {
            return url.TrimEnd().Length >= 11 ? url.Substring(url.TrimEnd().Length - 11) : null;
        }
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                return new List<int>();
            var indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
    }
}
