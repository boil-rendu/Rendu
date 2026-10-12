using UnityEngine;
using UnityEditor;

namespace RD.Inspector.Editor
{
    /// <summary>
    /// 检视器绘制后端的包级开关: IMGUI(默认) 或 UI Toolkit。
    ///
    /// 业务侧不用改任何代码 —— 所有继承 <see cref="InspectorEditor"/> 的检视器都跟着这个开关走,
    /// 用菜单 Tools/RD Inspector/UI Toolkit 绘制 切换, 或在任意编辑器代码里
    /// <c>InspectorBackend.UseUIToolkit = true</c>。选择持久化在 EditorPrefs, 重启后保持。
    ///
    /// 切换的生效时机: 检视器重建时(CreateInspectorGUI 被再次调用)。切换菜单会尝试重绘所有视图,
    /// 多数情况立即生效; 个别没有立刻变的, 重选一下对象即可。
    /// </summary>
    public static class InspectorBackend
    {
        private const string PrefsKey = "RD.Inspector.UseUIToolkit";
        private const string MenuPath = "Tools/RD Inspector/UI Toolkit 绘制";

        /// <summary>当前是否用 UI Toolkit 绘制检视器(全工程生效, 持久化)。</summary>
        public static bool UseUIToolkit
        {
            get => EditorPrefs.GetBool(PrefsKey, false);
            set => EditorPrefs.SetBool(PrefsKey, value);
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            UseUIToolkit = UseUIToolkit == false;

            // 尝试让已打开的检视器按新后端重建; 没重建的(如多窗口)重选对象即可
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            Debug.Log($"[Inspector] 检视器绘制后端切换为 {(UseUIToolkit ? "UI Toolkit" : "IMGUI")}");
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            // validate 必须返回 bool: 返回 false 菜单就置灰; 勾选态在这里同步
            Menu.SetChecked(MenuPath, UseUIToolkit);
            return true;
        }
    }
}
