using System;
using UnityEditor;

namespace RD.Panel.Editor
{
    /// <summary>
    /// 绘制后端的选择与切换。
    ///
    /// 三种粒度, 从粗到细:
    /// 1. 全局默认: <see cref="UseIMGUI"/> / <see cref="UseUIToolkit"/> —— 所有没显式指定的绘制都走它;
    /// 2. 宿主指定: <see cref="Engine.DrawObject(object, EditorWindow, Action{object}, Action{object}, IPanelDriver, HostState)"/>
    ///    多传入一个 driver, 这一个窗口走它(见
    ///    <see cref="Engine.DrawObject(object, EditorWindow, Action{object}, Action{object}, HostState, IPanelDriver)"/>);
    /// 3. 自己实现 <see cref="IPanelDriver"/> 并传进去 —— 想接别的 UI 系统不需要改引擎。
    /// </summary>
    public static class Drawing
    {
        private static Func<EditorWindow, IPanelDriver> s_Factory = CreateImGui;

        /// <summary>本次绘制生效的后端。由 DrawObject 在进入/退出时挂载与还原, 与会话是同一套 save/restore。</summary>
        private static IPanelDriver s_Current;

        /// <summary>引擎内部取值用: 不在 DrawObject 里时没有"当前绘制"这回事, 取它即为用错。 </summary>
        internal static IPanelDriver Current => s_Current;

        /// <summary>注册全局默认后端。</summary>
        public static void SetDefaultFactory(Func<EditorWindow, IPanelDriver> factory)
        {
            s_Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>全局切到 IMGUI(包默认)。</summary>
        public static void UseIMGUI()
        {
            SetDefaultFactory(CreateImGui);
        }

        /// <summary>
        /// 全局切到 UI Toolkit。宿主窗口自绘时挂到 host.rootVisualElement;
        /// 检视器宿主(cn.rendu.inspector)不经过这里 —— 它自己持有开关并直接构造 <see cref="UitkDriver"/>,
        /// 因为它没有 EditorWindow 可给(host 为 null)。
        /// </summary>
        public static void UseUIToolkit()
        {
#if UNITY_2021_2_OR_NEWER
            SetDefaultFactory(host =>
            {
                // UI Toolkit 必须有可挂树的容器: 检视器宿主传的 host 是 null, 它自己没有为 Uitk 准备宿主 UI,
                // 这时退回 IMGUI 并说明原因, 总比挂到一棵游离的树上更好定位
                if (host == null)
                {
                    Log.Warn("[Inspector] UI Toolkit 后端需要一个宿主窗口(EditorWindow)来挂 VisualElement 树, "
                           + "当前宿主为空, 这次退回到 IMGUI。检视器宿主要用 UIToolkit 需重写 CreateInspectorGUI。");
                    return new ImGuiDriver(null);
                }

                return new UitkDriver(host.rootVisualElement);
            });
#else
            Log.Warn("[Inspector] UI Toolkit 后端需要 Unity 2021.2+(本包要求 2022.3), 当前保持 IMGUI。");
#endif
        }

        /// <summary>按当前全局默认后端为宿主创建一份绘制后端。</summary>
        public static IPanelDriver CreateDriver(EditorWindow host)
        {
            return (s_Factory ?? CreateImGui)(host);
        }

        private static IPanelDriver CreateImGui(EditorWindow host)
        {
            return new ImGuiDriver(host);
        }

        /// <summary>挂载本次绘制的后端, 返回上一个(供 DrawObject 在 finally 里还原)。</summary>
        internal static IPanelDriver Attach(IPanelDriver driver)
        {
            var previous = s_Current;
            s_Current    = driver;
            return previous;
        }
    }
}
