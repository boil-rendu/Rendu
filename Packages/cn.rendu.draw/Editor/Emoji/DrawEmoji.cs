#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 面板侧的 emoji 渲染支持。
    ///
    /// 编辑器默认字体(Inter + CJK 回退链)里没有 emoji 字形, 直接写 emoji 字符会渲染成"口"方块,
    /// 而 Unity 2022.3 的文本引擎不支持彩色位图字体(Unity 6 才支持)。这里改用"贴图内联"的办法:
    /// 文字按 emoji 切成若干段, 文本段走 Label, emoji 段走 Image, 两个绘制后端都吃这一份逻辑。
    ///
    /// 贴图来源见 Resources/Emoji/README.md(从本机系统字体 Apple Color Emoji 抽取, 仅开发环境使用)。
    /// </summary>
    internal static class DrawEmoji
    {
        /// <summary>emoji 贴图的显示边长(与面板行高对齐)。</summary>
        public const float Size = 16f;

        private const string DirPath = "Packages/cn.rendu.draw/Editor/Resources/Emoji";
        private const string UriRoot = "Packages/cn.rendu.draw/Editor/Resources/Emoji/";

        /// <summary>文字段 / emoji 段。</summary>
        public readonly struct Run
        {
            public readonly bool IsEmoji;
            public readonly string Text;
            public readonly Texture2D Texture;

            public Run(string text)
            {
                IsEmoji = false;
                Text    = text;
                Texture = null;
            }

            public Run(Texture2D texture)
            {
                IsEmoji = true;
                Text    = null;
                Texture = texture;
            }
        }

        private static readonly HashSet<int> s_Available = new HashSet<int>();
        private static readonly Dictionary<int, Texture2D> s_Textures = new Dictionary<int, Texture2D>();
        private static readonly List<Run> s_Runs = new List<Run>();
        private static bool s_Scanned;

        /// <summary>该字符串里有没有"我们有贴图的" emoji。</summary>
        public static bool Contains(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            EnsureScanned();

            for (int i = 0; i < text.Length; i++)
            {
                if (TryTake(text, i, out _, out int length))
                {
                    i += length - 1;
                    return true;
                }
            }

            return false;
        }

        /// <summary>把字符串切成文本段 / emoji 段。返回的列表是复用的, 用完即止, 不要长期持有。</summary>
        public static List<Run> Split(string text)
        {
            s_Runs.Clear();

            if (string.IsNullOrEmpty(text))
            {
                s_Runs.Add(new Run(""));
                return s_Runs;
            }

            EnsureScanned();

            int start = 0;
            for (int i = 0; i < text.Length;)
            {
                if (TryTake(text, i, out int codePoint, out int length))
                {
                    if (i > start) s_Runs.Add(new Run(text.Substring(start, i - start)));

                    var texture = Load(codePoint);
                    if (texture != null)
                    {
                        s_Runs.Add(new Run(texture));
                    }
                    else
                    {
                        // 贴图缺失就退化成原字符, 至少不会静默吞掉内容
                        s_Runs.Add(new Run(text.Substring(i, length)));
                    }

                    i += length;
                    start = i;
                    continue;
                }

                i++;
            }

            if (start < text.Length) s_Runs.Add(new Run(text.Substring(start)));
            if (s_Runs.Count == 0) s_Runs.Add(new Run(""));

            return s_Runs;
        }

        /// <summary>取字符串中第一个 emoji 的贴图(按钮把 emoji 当图标用)。</summary>
        public static Texture2D First(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            EnsureScanned();

            for (int i = 0; i < text.Length; i++)
            {
                if (TryTake(text, i, out int codePoint, out int length)) return Load(codePoint);
            }

            return null;
        }

        /// <summary>去掉字符串里的 emoji(及其变体选择符), 供按钮在把 emoji 转成图标后使用。</summary>
        public static string Strip(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            EnsureScanned();

            var buffer = new System.Text.StringBuilder(text.Length);
            for (int i = 0; i < text.Length;)
            {
                if (TryTake(text, i, out _, out int length))
                {
                    i += length;
                    continue;
                }

                buffer.Append(text[i]);
                i++;
            }

            return buffer.ToString().Trim();
        }

        /// <summary>
        /// 从 i 处取一个 emoji: 命中返回 true 并给出码位与字符长度(含变体选择符 U+FE0F)。
        /// 只认"贴图目录里存在"的码位, 其余字符一律当普通文字, 避免误判 CJK 区段。
        /// </summary>
        private static bool TryTake(string text, int i, out int codePoint, out int length)
        {
            codePoint = 0;
            length    = 0;

            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                int pair = char.ConvertToUtf32(text[i], text[i + 1]);
                if (s_Available.Contains(pair) == false) return false;

                codePoint = pair;
                length    = 2;
            }
            else
            {
                int single = text[i];
                if (s_Available.Contains(single) == false) return false;

                codePoint = single;
                length    = 1;
            }

            // 变体选择符(️): 跟着前一个 emoji 一起吃掉, 否则会渲染成独立字符
            if (i + length < text.Length && text[i + length] == 0xFE0F) length++;

            return true;
        }

        private static Texture2D Load(int codePoint)
        {
            if (s_Textures.TryGetValue(codePoint, out var cached)) return cached;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(UriRoot + codePoint.ToString("x") + ".png");
            s_Textures[codePoint] = texture;   // 缺失也记下, 免得每帧都去查
            return texture;
        }

        /// <summary>扫一次贴图目录, 得到"哪些码位有图"。</summary>
        private static void EnsureScanned()
        {
            if (s_Scanned) return;
            s_Scanned = true;

            if (Directory.Exists(DirPath) == false) return;

            foreach (var file in Directory.GetFiles(DirPath, "*.png"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (int.TryParse(name, System.Globalization.NumberStyles.HexNumber,
                                 System.Globalization.CultureInfo.InvariantCulture, out int codePoint))
                {
                    s_Available.Add(codePoint);
                }
            }
        }
    }
}

#endif
