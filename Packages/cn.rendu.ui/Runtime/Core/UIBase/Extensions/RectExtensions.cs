using System;
using System.Text.RegularExpressions;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>Rect 的字符串解析扩展</summary>
    public static class RectExtensions
    {
        private static Regex parseRegex;

        /// <summary>解析失败直接抛 FormatException</summary>
        public static Rect Parse(string text)
        {
            Rect value;
            if (!TryParse(text, out value))
            {
                var msg = string.Format("The string {0} can not convert to Rect.", text);
                throw new FormatException(msg);
            }

            return value;
        }

        /// <summary>
        /// 按 "$(x:…, y:…, width:…, height:…)" 格式解析 Rect
        /// 正则只构建一次；任一字段解析失败则 rect = Rect.zero 并返回 false
        /// </summary>
        public static bool TryParse(string text, out Rect rect)
        {
            if (parseRegex == null)
            {
                parseRegex = new Regex(@"^\(x:(.*), y:(.*), width:(.*), height:(.*)\)$");
            }

            var match = parseRegex.Match(text);
            if (!match.Success || match.Groups.Count != 5)
            {
                rect = Rect.zero;
                return false;
            }

            float x;
            if (!float.TryParse(match.Groups[1].Value, out x))
            {
                rect = Rect.zero;
                return false;
            }

            float y;
            if (!float.TryParse(match.Groups[2].Value, out y))
            {
                rect = Rect.zero;
                return false;
            }

            float width;
            if (!float.TryParse(match.Groups[3].Value, out width))
            {
                rect = Rect.zero;
                return false;
            }

            float height;
            if (!float.TryParse(match.Groups[4].Value, out height))
            {
                rect = Rect.zero;
                return false;
            }

            rect = new Rect(x, y, width, height);
            return true;
        }
    }
}
