using UnityEditor;
using UnityEngine;

namespace RD.EditorKit.Editor
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
    public static class Icons
    {
        public static EditorIcon UnityFolderIcon = new(FindBuiltinIcon("Folder Icon"));
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
                default:
                    return null;
            }
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
