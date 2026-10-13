using UnityEditor;
using UnityEngine;

namespace RD.EditorWindows.Editor
{
    /// <summary>
    /// 窗口宿主绘制后端的包级开关: IMGUI(默认) 或 UI Toolkit。
    ///
    /// 与检视器侧的 <c>RD.Inspector.Editor.InspectorBackend</c> 同一套用法, 只是各管各的(互不影响):
    /// 业务代码不用改 —— 所有继承 <see cref="EditorWindowBase"/> 的窗口都跟着这个开关走, 用菜单
    /// Tools/RD EditorWindow/UI Toolkit 绘制 切换, 或在任意编辑器代码里
    /// <c>WindowBackend.UseUIToolkit = true</c>。选择持久化在 EditorPrefs, 重启后保持。
    ///
    /// 生效时机: 切换后已打开的窗口在下一帧自己重建 / 拆掉那棵 UI Toolkit 树(见 EditorWindowBase.EnsureBackend),
    /// 不需要关掉重开。
    /// </summary>
    public static class WindowBackend
    {
        private const string PrefsKey = "RD.EditorWindow.UseUIToolkit";
        private const string MenuPath = "Tools/RD EditorWindow/UI Toolkit 绘制";

        /// <summary>当前是否用 UI Toolkit 绘制窗口(全工程生效, 持久化)。</summary>
        public static bool UseUIToolkit
        {
            get => EditorPrefs.GetBool(PrefsKey, false);
            set => EditorPrefs.SetBool(PrefsKey, value);
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            UseUIToolkit = UseUIToolkit == false;

            // 让已打开的窗口都重绘一遍: 各自在重绘里发现开关变了, 重建/拆掉自己的树
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            Debug.Log($"[Inspector] 窗口绘制后端切换为 {(UseUIToolkit ? "UI Toolkit" : "IMGUI")}");
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
