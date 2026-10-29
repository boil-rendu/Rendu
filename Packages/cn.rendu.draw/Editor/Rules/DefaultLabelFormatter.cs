using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// <see cref="ILabelFormatter"/> 的默认实现 —— 引擎自带的标签与文案规则。
    ///
    /// 对外仍是纯函数(输入成员/参数, 输出字符串或颜色); 内部只对"纯函数结果"做记忆化:
    ///   - <see cref="FriendlyName"/> 的名字拆分要跑正则, 而名字只跟成员/枚举值有关;
    ///   - <see cref="CountedText"/> 的 "N items" 只跟条数有关。
    /// 两处都是每帧每成员会走到的路径, 现场算等于每帧重跑一遍同样的正则与插值。
    /// 缓存有上限(名字表满即整体清空), 键的数量因此不会随运行时长无界增长。
    /// </summary>
    internal sealed class DefaultLabelFormatter : ILabelFormatter
    {
        /// <summary>FriendlyName 的记忆化表; 键是成员名/枚举值名, 数量级是类型里的名字总数。</summary>
        private static readonly Dictionary<string, string> s_FriendlyNames = new();

        /// <summary>上限只是兜底(动态拼出来的名字不应无界增长), 正常工程远到不了。</summary>
        private const int FriendlyNameCacheLimit = 4096;

        /// <summary>0..127 条的计数文案; 超过就现算, 不为此无限扩表。</summary>
        private static readonly string[] s_CountedText = BuildCountedText(128);

        private static string[] BuildCountedText(int count)
        {
            var texts = new string[count];
            for (var i = 0; i < count; i++)
            {
                texts[i] = i == 0 ? "Empty" : i.ToString(CultureInfo.InvariantCulture) + " items";
            }

            return texts;
        }

        /// <summary>
        /// 字段/属性的显示名, 三级回退:
        ///   1. 成员自身的 [LabelText];
        ///   2. 成员"类型"上的 [LabelText] —— 仅枚举, 见 <see cref="CachedMember.TypeLabel"/>;
        ///   3. FriendlyName(成员名)。
        /// 第 2 级是给"一个泛型基类承载 N 个枚举"用的: 那 N 个字段声明的其实是同一个成员,
        /// 名字只可能来自各自的枚举类型; 枚举自己又写不了成员名, 类型名是它唯一自然的标签。
        /// </summary>
        public string GetLabel(CachedMember m)
        {
            var label = m.GetAttr<LabelTextAttribute>();
            if (label != null && string.IsNullOrEmpty(label.Text) == false)
            {
                return label.Text;
            }

            var typeLabel = m.TypeLabel;
            if (string.IsNullOrEmpty(typeLabel) == false)
            {
                return typeLabel;
            }

            return FriendlyName(m.RawName);
        }

        /// <summary>
        /// 成员标签, 但 [HideLabel] 时为空串 —— 值控件(标量/枚举/字符串/对象预览)统一走这里:
        /// 控件照画, 只是没有名字。
        /// </summary>
        public string LabelOrEmpty(CachedMember m)
        {
            return m.GetAttr<HideLabelAttribute>() != null ? "" : GetLabel(m);
        }

        /// <summary>
        /// 值控件实际要用的标签: 表格单元格里**永远为空**。
        /// 单元格的字段名已经由列名指明了, 再画一遍标签不仅重复, 而且标签会吃掉约一列的宽度 ——
        /// 控件被挤到自己列之外, 看起来就像"值没刷出来"(截图里列名跑进单元格、CodeType 的下拉
        /// 挪到"层级"列, 都是这么来的)。
        /// </summary>
        public string LabelForControl(CachedMember m, bool inTableCell)
        {
            return inTableCell ? "" : LabelOrEmpty(m);
        }

        /// <summary>
        /// 容器头部(列表/字典/集合/表格)的"名字 + 计数"文案。
        /// [HideLabel] 时**只留计数**: 折叠头("▼")必须有一段可点的文字, 整行不画就没法展开了,
        /// 所以这里不能像纯文本头那样直接省略。
        /// </summary>
        public string CountedLabel(CachedMember m, int count)
        {
            // Trim: [LabelText(" ")] 这类空白占位标签视为无名, 走纯计数分支
            var label = LabelOrEmpty(m)?.Trim();
            if (string.IsNullOrEmpty(label) == false) return $"{label} ({count})";

            // 观感要求: 无名列表头计数显示 "N items", 空列表显示 "Empty"
            return CountedText(count);
        }

        /// <summary>纯计数文案: 0 → "Empty", 否则 "N items"(表格表头计数)。</summary>
        public string CountedText(int count)
        {
            if (count >= 0 && count < s_CountedText.Length) return s_CountedText[count];
            return count.ToString(CultureInfo.InvariantCulture) + " items";
        }

        /// <summary>
        /// 非折叠的纯文本头行(只有一行 bold 文字, 没有箭头)该不该画。
        /// 没有名字就整行不画 —— 否则会留下一条什么都不说明的空白带。
        /// </summary>
        public bool ShouldDrawLabel(CachedMember m)
        {
            return string.IsNullOrEmpty(LabelOrEmpty(m)) == false;
        }

        /// <summary>把成员名拆成词: myValue → "my Value"。</summary>
        public string FriendlyName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // 名字是稳定的短字符串(成员名/枚举值名), 同一批名字每帧都要再拆一遍 —— 缓存命中后不再走正则。
            if (s_FriendlyNames.TryGetValue(name, out var cached)) return cached;

            var friendly = Regex.Replace(name, "(?<!^)([A-Z][a-z0-9])", " $1").Trim();
            if (s_FriendlyNames.Count >= FriendlyNameCacheLimit) s_FriendlyNames.Clear();
            s_FriendlyNames[name] = friendly;
            return friendly;
        }

        /// <summary>[GUIColor] 的颜色(支持 "r,g,b[,a]" 字符串或 rgba 分量)。</summary>
        public Color? ResolveGUIColor(CachedMember m)
        {
            var attr = m.GetAttr<GUIColorAttribute>();
            if (attr == null) return null;

            if (string.IsNullOrEmpty(attr.ColorString) == false)
            {
                var parts = attr.ColorString.Split(',');
                if (parts.Length >= 3 &&
                    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var r) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var g) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
                {
                    var a = 1f;
                    if (parts.Length >= 4)
                    {
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out a);
                    }

                    return new Color(r, g, b, a);
                }

                return null;
            }

            return new Color(attr.R, attr.G, attr.B, attr.A);
        }

        /// <summary>[Title]/[TitleGroup] 的对齐枚举 → 绘制层的水平对齐。Split 的主标题仍靠左(副标题才是被"劈"到
        /// 右侧的那半), 无副标题时 Split 退化为 Left。</summary>
        public DrawAlign TitleAnchor(TitleAlignment alignment)
        {
            switch (alignment)
            {
                case TitleAlignment.Centered: return DrawAlign.Center;
                case TitleAlignment.Right:    return DrawAlign.Right;
                default:                       return DrawAlign.Left;
            }
        }
    }
}
