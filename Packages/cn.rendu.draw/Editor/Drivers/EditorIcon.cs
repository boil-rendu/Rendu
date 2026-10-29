using UnityEditor;
using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 轻量图标包装（原 EditorIcon）
    /// </summary>
    public struct EditorIcon
    {
        public readonly Texture Icon;

        public EditorIcon(Texture icon)
        {
            Icon = icon;
        }

        public static implicit operator Texture(EditorIcon icon)
        {
            return icon.Icon;
        }

        public static implicit operator GUIContent(EditorIcon icon)
        {
            return new GUIContent(icon.Icon);
        }
    }

    /// <summary>
    /// 编辑器内置图标集合
    /// </summary>
    public static class EditorIcons
    {
        public static EditorIcon Folder = new(FindBuiltinIcon("Folder Icon"));
        public static EditorIcon SettingsCog = new(FindBuiltinIcon("Settings Icon", "Settings"));

        /// <summary>
        /// 把 SdfIconType 解析为可绘制纹理。
        /// 优先映射到 Unity 内置图标；无法精确对应时返回 null(按钮退化为纯文本)。
        /// 后续可替换为自有的图标字形源以获得一致的视觉。
        /// </summary>
        public static Texture FromSdf(SdfIconType type)
        {
            switch (type)
            {
                case SdfIconType.Folder:
                case SdfIconType.FolderFill:
                case SdfIconType.Folder2:
                case SdfIconType.Folder2Open:
                case SdfIconType.FolderOpen:
                    return FindBuiltinIcon("Folder Icon");
                case SdfIconType.Settings:
                case SdfIconType.SettingsFill:
                case SdfIconType.Gear:
                case SdfIconType.GearFill:
                    return FindBuiltinIcon("Settings Icon", "Settings");
                case SdfIconType.Save:
                case SdfIconType.Save2:
                case SdfIconType.Save2Fill:
                    return FindBuiltinIcon("d_SaveAs", "SaveAs");
                case SdfIconType.Play:
                case SdfIconType.PlayFill:
                    return FindBuiltinIcon("d_PlayButton", "PlayButton");
                case SdfIconType.Pause:
                case SdfIconType.PauseFill:
                    return FindBuiltinIcon("d_PauseButton", "PauseButton");
                case SdfIconType.Stop:
                case SdfIconType.StopFill:
                    return FindBuiltinIcon("d_StopButton", "StopButton");
                case SdfIconType.Refresh:
                case SdfIconType.Sync:
                case SdfIconType.ArrowRepeat:
                    return FindBuiltinIcon("d_Refresh", "Refresh");
                case SdfIconType.Search:
                case SdfIconType.SearchHeart:
                    return FindBuiltinIcon("Search Icon", "d_Search Icon");
                case SdfIconType.Plus:
                case SdfIconType.PlusLg:
                case SdfIconType.PlusSquare:
                case SdfIconType.PlusCircle:
                    return FindBuiltinIcon("d_Toolbar Plus", "Toolbar Plus");
                case SdfIconType.Minus:
                case SdfIconType.MinusSquare:
                case SdfIconType.MinusCircle:
                    return FindBuiltinIcon("d_Toolbar Minus", "Toolbar Minus");
                case SdfIconType.Trash:
                case SdfIconType.TrashFill:
                case SdfIconType.Trash2:
                case SdfIconType.Trash2Fill:
                case SdfIconType.Trash3:
                case SdfIconType.Trash3Fill:
                    return FindBuiltinIcon("d_TreeEditor.Trash", "TreeEditor.Trash");
                case SdfIconType.ExclamationTriangle:
                case SdfIconType.ExclamationTriangleFill:
                case SdfIconType.Exclamation:
                    return FindBuiltinIcon("d_console.warnicon", "console.warnicon");
                case SdfIconType.ExclamationCircle:
                case SdfIconType.ExclamationCircleFill:
                case SdfIconType.Info:
                case SdfIconType.InfoCircle:
                    return FindBuiltinIcon("d_console.infoicon", "console.infoicon");
                case SdfIconType.Question:
                case SdfIconType.QuestionCircle:
                    return FindBuiltinIcon("d_console.infoicon", "console.infoicon");
                case SdfIconType.Eye:
                case SdfIconType.EyeFill:
                    return FindBuiltinIcon("d_scenevis_visible", "scenevis_visible");
                case SdfIconType.EyeSlash:
                case SdfIconType.EyeSlashFill:
                    return FindBuiltinIcon("d_scenevis_hidden", "scenevis_hidden");
                case SdfIconType.Check:
                case SdfIconType.Check2:
                case SdfIconType.CheckLg:
                case SdfIconType.CheckCircle:
                    return FindBuiltinIcon("d_FilterSelectedOnly", "FilterSelectedOnly");
                case SdfIconType.ArrowUp:
                    return FindBuiltinIcon("d_ArrowUp", "ArrowUp");
                case SdfIconType.ArrowDown:
                    return FindBuiltinIcon("d_ArrowDown", "ArrowDown");
                case SdfIconType.ArrowLeft:
                    return FindBuiltinIcon("d_ArrowLeft", "ArrowLeft");
                case SdfIconType.ArrowRight:
                    return FindBuiltinIcon("d_ArrowRight", "ArrowRight");
                case SdfIconType.X:
                case SdfIconType.XCircle:
                case SdfIconType.XSquare:
                    return FindBuiltinIcon("d_winbtn_mac_close", "winbtn_mac_close");
                case SdfIconType.Calendar:
                case SdfIconType.Calendar2:
                    return FindBuiltinIcon("d_Button Icon", "Button Icon");
                case SdfIconType.Link:
                case SdfIconType.Link45deg:
                case SdfIconType.Link2:
                case SdfIconType.Link2Fill:
                case SdfIconType.LinkBreak:
                case SdfIconType.LinkBreakFill:
                case SdfIconType.Unlink:
                    return LinkGlyph;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 自绘的"链环"字形(45° 摆放的两个交叠椭圆环)。
        /// Unity 内置图标里没有链接图标, 而 FontAwesome 的 Link45deg 是"文档"按钮想要的图标,
        /// 所以这里自己画一张 20×20 的白色贴图(靠 5× 超采样抗锯齿, 颜色交给 GUI 侧)。
        /// </summary>
        private static Texture2D s_LinkGlyph;

        /// <summary>链环字形贴图(首次调用时生成, 之后复用)。</summary>
        public static Texture LinkGlyph
        {
            get
            {
                if (s_LinkGlyph == null) s_LinkGlyph = BuildLinkGlyph();
                return s_LinkGlyph;
            }
        }

        private static Texture2D BuildLinkGlyph()
        {
            // 图标按按钮的行高画(20px), 不是 32px: 按钮不会缩放贴图, 32px 的图塞进
            // 30px 高的按钮里会被裁成一条白斑; 16px 又装不下两个环的孔(糊成一坨)。
            const int size = 20;

            // 每像素 5×5 子采样: 环只有 1.2px 粗, 不超采样边缘全是锯齿
            const int samples = 5;

            // 环的参数: 沿链方向的半长轴 / 垂直方向的半短轴 / 两个环心的间距 / 笔画宽度
            //
            // 这组值是"20px 下还认得出是两个环"的折中(在几组候选里比对着挑的):
            // 环心间距要小于半长轴(两环得咬合), 但每个环的孔要留得住 —— 半短轴放大、笔画收细,
            // 孔才有 5~6px 的净空, 缩到 14px 显示时也不糊。
            const float semiMajor = 4.8f;
            const float semiMinor = 3.6f;
            const float centerGap = 4.0f;
            const float stroke    = 1.2f;

            const float cos45 = 0.70710678f;
            var half = (size - 1) * 0.5f;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name       = "RD.LinkGlyph",
                hideFlags  = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode   = TextureWrapMode.Clamp,
            };

            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var coverage = 0f;
                    for (var sy = 0; sy < samples; sy++)
                    {
                        for (var sx = 0; sx < samples; sx++)
                        {
                            // 像素内偏移: 以像素中心为 0, 覆盖 [-0.5, 0.5)
                            var cx = x + (sx + 0.5f) / samples - 0.5f - half;
                            var cy = y + (sy + 0.5f) / samples - 0.5f - half;

                            // 旋转 45°: 链环的长轴变成水平(u 轴), v 轴是垂直方向
                            var u = (cx + cy) * cos45;
                            var v = (cy - cx) * cos45;

                            // 到两条环线的距离取绝对值: 要的是"环"而不是"实心椭圆"(否则环内一律算满,
                            // 会变成两个实心椭圆叠成的斜放白斑)。
                            var dA = Mathf.Abs(RingDistance(u, v, +centerGap, semiMajor, semiMinor));
                            var dB = Mathf.Abs(RingDistance(u, v, -centerGap, semiMajor, semiMinor));

                            var alphaA = Mathf.Clamp01(stroke * 0.5f + 0.5f - dA);
                            var alphaB = Mathf.Clamp01(stroke * 0.5f + 0.5f - dB);

                            // 两环交叠处按上下半区交错, 画出"扣链"的观感
                            float alpha;
                            if (alphaA > 0f && alphaB > 0f)
                            {
                                alpha = v >= 0f ? alphaA : alphaB;
                            }
                            else
                            {
                                alpha = Mathf.Max(alphaA, alphaB);
                            }

                            coverage += alpha;
                        }
                    }

                    coverage /= samples * samples;
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(coverage * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>点到椭圆环轮廓的近似距离(像素), 环内为负值。</summary>
        private static float RingDistance(float u, float v, float centerU, float semiMajor, float semiMinor)
        {
            var du = (u - centerU) / semiMajor;
            var dv = v / semiMinor;
            var r  = Mathf.Sqrt(du * du + dv * dv);
            return (r - 1f) * semiMinor;
        }

        private static Texture FindBuiltinIcon(params string[] candidates)
        {
            foreach (var name in candidates)
            {
                if (string.IsNullOrEmpty(name)) continue;
                try
                {
                    var content = EditorGUIUtility.IconContent(name);
                    if (content != null && content.image != null)
                    {
                        return content.image;
                    }
                }
                catch (System.Exception)
                {
                    // ignore
                }
            }

            return null;
        }
    }
}
