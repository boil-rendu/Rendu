using System;
using RD.Draw.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 编辑器窗口的绘制宿主基类。
    /// 负责把引擎的绘制能力（RD.Draw）接入 UIElements 窗口：管理内容驱动（Driver）与每窗口的绘制状态。
    /// </summary>
    public abstract class DrawWindow : EditorWindow
    {
        /// <summary>本窗口的绘制状态（折叠 / 滚动位置等），生命周期与窗口一致，各窗口互不干扰。</summary>
        protected readonly HostState m_Session = Engine.CreateHostState();

        // 本窗口内容区的引擎驱动：默认绑到整窗 root；自定义布局（如菜单窗口）时绑到右内容面板。
        // 两种方式不会同时生效，因此只需一个字段；首次绘制和后续事件回调都复用它。
        private Driver m_Driver;

        private bool m_Initialized;

        /// <summary>子类重写做一次性初始化（首次绘制前调用，异常会被捕获，不会让窗口崩溃）。</summary>
        protected virtual void Initialize() { }

        /// <summary>窗口销毁时调用，子类重写做清理。</summary>
        protected virtual void OnWindowDestroy() { }

        private void EnsureInitialize()
        {
            if (m_Initialized) return;
            m_Initialized = true;
            try { Initialize(); }
            catch (Exception e) { Debug.LogError($"[Draw] 窗口初始化失败 {GetType().Name}\n{e}"); }
        }

        //-------------------------------------- 生命周期 --------------------------------------
        // Unity 在定义了 CreateGUI 后只会调用它，不会再调用窗口自身的 OnGUI；
        // 本类在内部重建 UIElements 树和引擎驱动，子类只需关心内容怎么画（OnDrawContent），不感知底层后端。
        // 注意：CreateGUI 是普通方法而非 virtual，不能写 override（否则 CS0115）。

        protected void CreateGUI()
        {
            EnsureInitialize();
            Rebuild();
        }

        /// <summary>重建整个 UI：清空窗体、新建根容器，再交给子类布局。</summary>
        private void Rebuild()
        {
            rootVisualElement.Clear();
            m_Driver = null;

            var root = new VisualElement { style = { flexGrow = 1f } };
            rootVisualElement.Add(root);
            SetupUI(root);
        }

        /// <summary>
        /// 布局钩子：子类重写以定义窗口的 UIElements 结构，并把引擎内容驱动绑到某个容器。
        /// 默认实现：在整窗 root 上挂一个滚动区作为内容驱动，并立即绘制一次。
        /// 需要自定义布局（如菜单 + 分割条）时，重写本方法排布 UIElements，
        /// 再用 <see cref="CreateContentArea"/> 创建一块引擎内容容器。
        /// </summary>
        protected virtual void SetupUI(VisualElement root)
        {
            m_Driver = new Driver(root, Redraw);
            Redraw();
        }

        /// <summary>
        /// 在自定义布局里创建一块引擎内容容器（保留模式），并接上重绘链。
        /// <paramref name="scroll"/> 为 false 时不带外层滚动视图。返回的内容容器即绘制目标的挂载点。
        /// </summary>
        protected VisualElement CreateContentArea(VisualElement parent, Action draw, bool scroll = true)
        {
            VisualElement host;
            if (scroll)
            {
                var sv = new ScrollView { style = { flexGrow = 1f } };
                parent.Add(sv);
                host = sv.contentContainer;
            }
            else
            {
                host = new VisualElement { style = { flexGrow = 1f } };
                parent.Add(host);
            }

            var driver = new Driver(host, draw);
            m_Driver = driver;          // 本窗口内容区驱动；绘制与事件回调共用
            draw();                     // 立即绘制一次
            return host;
        }

        /// <summary>重绘入口：由 Driver 的重绘回调触发，调用 OnDrawContent 完成实际绘制。</summary>
        private void Redraw()
        {
            if (m_Driver == null) return;
            OnDrawContent();
        }

        /// <summary>
        /// 内容钩子：决定窗口画什么。默认绘制窗口自身成员；
        /// 子类（如菜单窗口）可重写以绘制自己的内容。
        /// 默认布局和自定义布局的内容区都会调用它。
        /// </summary>
        protected virtual void OnDrawContent()
        {
            DrawObject(this);
        }

        /// <summary>用本窗口的会话与驱动绘制任意对象，是对 Engine.DrawObject 的封装。</summary>
        /// <remarks>
        /// 默认窗口宿主只改内存状态（不编辑可序列化资产），故 onBeforeChange/onChanged 传 null；
        /// 若子类要编辑可序列化对象，应改为包 <see cref="Engine.BeginChange"/> / <see cref="Engine.EndChange"/>。
        /// </remarks>
        protected void DrawObject(object target)
        {
            var driver = m_Driver ?? new Driver(rootVisualElement);
            Engine.DrawObject(target, this, null, null, m_Session, driver);
        }

        private void OnDestroy() => OnWindowDestroy();
    }
}
