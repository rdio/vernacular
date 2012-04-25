using System;
using System.Collections.Generic;

namespace Vernacular.Tool
{
    public static class StringExtensions
    {
        private static Dictionary<string, string> escapes = new Dictionary<string, string> {
            {"\\", @"\\"},
            {"\"", @"\"""},
            {"\a", @"\a"},
            {"\b", @"\b"},
            {"\f", @"\f"},
            {"\n", @"\n"},
            {"\r", @"\r"},
            {"\t", @"\t"},
            {"\v", @"\v"},
            {"\0", @"\0"}
        };

        public static string Escape (this string value, bool decode = false)
        {
            foreach (var escape in escapes) {
                value = decode
                    ? value.Replace (escape.Value, escape.Key)
                    : value.Replace (escape.Key, escape.Value);
            }

            return value;
        }
    }
}