using System;
using RD.Draw.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 编辑器窗口的绘制宿主基类（宿主集成层，住在 cn.rendu.edittool，不是 RD.Draw）
    /// </summary>
    public abstract class DrawWindow : EditorWindow
    {
        /// <summary>本窗口的状态句柄：折叠 / 滚动状态，生命周期与窗口一致，各窗口互不串台。</summary>
        public readonly HostState m_Session = Engine.CreateHostState();

        private Driver m_Driver;        // 默认外壳分支的 Driver
        private Driver m_ActiveDriver;  // 绘制进行中：当前嵌套内容区正在用的驱动（DrawTarget 优先用它）
        private Driver m_ContentDriver; // 自定义外壳里某块内容区的常驻驱动（绘制之外被事件回调时靠它）

        private bool m_Initialized;

        /// <summary>子类重写做一次性初始化（首次绘制前调用，异常被接住不会打炸窗口）。</summary>
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

        //-------------------------------------- 生命周期：CreateGUI 是 Unity 入口 --------------------------------------
        // 定义了 CreateGUI 后，Unity 只调它、不会调窗口自身的 OnGUI；本类据此在内部建 Driver 树。
        // 子类只需重写 OnDrawContent，不感知后端。
        // CreateGUI 是普通方法：基类非 virtual，不能写 override（否则 CS0115）。

        protected void CreateGUI()
        {
            EnsureInitialize();
            Rebuild();
        }

        private void Rebuild()
        {
            rootVisualElement.Clear();
            m_Driver = null;
            m_ActiveDriver = null;
            m_ContentDriver = null;

            var root = new VisualElement { style = { flexGrow = 1f } };
            rootVisualElement.Add(root);
            BuildToolkitUI(root);
        }

        /// <summary>
        ///  外壳入口：默认整窗一个滚动区画窗口自身；有自定义外壳（菜单 + 分割条）的窗口重写它，
        /// 用 <see cref="CreateToolkitContent"/> 开一块引擎内容区。
        /// </summary>
        protected virtual void BuildToolkitUI(VisualElement root)
        {
            m_Driver = new Driver(root, Redraw);
            Redraw();
        }

        /// <summary>
        /// 在自定义外壳里"开一块引擎内容区"（保留模式），接上重绘链。
        /// <paramref name="scroll"/> = false 时外层不带滚动视图。返回的 host 即内容容器。
        /// </summary>
        protected VisualElement CreateToolkitContent(VisualElement parent, Action draw, bool scroll = true)
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

            Driver driver = null;
            driver = new Driver(host, () => RunContent(driver, draw));
            m_ContentDriver = driver;   // 绘制之外（如  选中事件）被回调时也要用它
            RunContent(driver, draw);   // 立刻画一次
            return host;
        }

        private void RunContent(Driver driver, Action draw)
        {
            m_ActiveDriver = driver;
            try { draw(); }
            finally { m_ActiveDriver = null; }
        }

        /// <summary>默认  分支的绘制体：再把内容画一遍（由 Driver 重绘回调触发）。走 OnDrawContent 以尊重子类的重写。</summary>
        private void Redraw()
        {
            if (m_Driver == null) return;
            OnDrawContent();
        }

        /// <summary>
        /// 内容钩子：默认画窗口自身成员；子类（如菜单窗口）重写为自己的外壳。
        /// 默认外壳在 <see cref="Redraw"/> 里调它；嵌套内容区在 <see cref="RunContent"/> 里调它。
        /// </summary>
        protected virtual void OnDrawContent()
        {
            DrawTarget(this);
        }

        /// <summary>用本窗口会话 + 当前后端画任意对象（承载"画什么"的入口）。</summary>
        protected void DrawTarget(object target)
        {
            var driver = m_ActiveDriver ?? m_ContentDriver ?? m_Driver ?? new Driver(rootVisualElement);
            Engine.DrawObject(target, this, null, null, m_Session, driver);
        }

        /// <summary>兼容旧名：画指定对象（见 <see cref="DrawTarget"/>）。</summary>
        protected void DrawTargetObject(object target) => DrawTarget(target);

        private void OnDestroy() => OnWindowDestroy();
    }
}
