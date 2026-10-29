using System;
using System.Globalization;
using UnityEngine;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 标量文本 → 值的解析: 集合里的元素用输入框编辑时, 文本要能变回原类型。
    ///
    /// 单独成类是因为它是**纯函数**: 与绘制无关, 与集合无关, 只与"类型 + 文本"有关。
    /// 解析不出来就返回 null, 由调用方决定"保持原值"还是"报错"。
    /// </summary>
    internal static class ScalarParser
    {
        /// <summary>
        /// 把输入框里的文本解析成目标类型的值; 解析不了(或该类型本就不该这样转)时返回 null。
        /// </summary>
        public static object Parse(Type type, string text)
        {
            try
            {
                if (type.IsEnum) return Enum.Parse(type, text, true);
                if (type == typeof(string)) return text;
                if (type == typeof(bool)) return bool.Parse(text);
                if (type == typeof(float)) return float.Parse(text, CultureInfo.InvariantCulture);
                if (type == typeof(double)) return double.Parse(text, CultureInfo.InvariantCulture);
                if (type == typeof(int)) return int.Parse(text);
                if (type == typeof(long)) return long.Parse(text);
                if (type == typeof(Vector2) || type == typeof(Vector3)) return null;
                return Convert.ChangeType(text, type);
            }
            catch
            {
                return null;
            }
        }
    }
}
