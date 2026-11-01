using System;
using RD.Draw.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 编辑器窗口的绘制<strong>宿主</strong>(host): 把引擎的绘制能力(RD.Draw)接入 UIElements 窗口。
    ///
    /// 宿主只管三件事, 与检视器宿主(RD.Inspector)共用同一套契约:
    ///   1) <strong>会话</strong> <c>m_Session</c>: 每窗口一份, 存折叠 / 滚动位置等绘制态, 窗口之间互不干扰;
    ///   2) <strong>后端</strong> <see cref="Driver"/>: 挂在一块 VisualElement 上, 把引擎的调用序列落成控件树;
    ///   3) <strong>重绘</strong>: 内容变了就再画一遍(<see cref="OnDrawContent"/> → <see cref="DrawObject"/>)。
    ///
    /// 引导顺序固定为: <c>Initialize</c>(一次) → <see cref="SetupUI"/>(外壳) → 挂驱动 → 首帧绘制。
    /// 驱动挂好之前不允许绘制: 那时尚无落点, 引擎会按位置复用把内容塞进整窗 root 里
    /// (表现是布局根多出一列挤扁的控件), 见 <see cref="ContentDriverReady"/>。
    /// </summary>
    public abstract class DrawWindow : EditorWindow
    {
        /// <summary>本窗口的绘制会话(HostState): 生命周期与窗口一致, 各窗口互不干扰。</summary>
        protected readonly HostState m_Session = Engine.CreateHostState();

        /// <summary>
        /// 本窗口的内容驱动: 默认绑到整窗 root; 自定义布局(如菜单窗口)时绑到右内容面板。
        /// 两种方式不会同时生效, 因此只需一个字段; 首次绘制和后续事件回调都复用它。
        /// </summary>
        private Driver m_Driver;

        private bool m_Initialized;

        /// <summary>内容驱动是否已挂好: 挂好后 <see cref="DrawObject"/> 才有落点。</summary>
        protected bool ContentDriverReady => m_Driver != null;

        /// <summary>
        /// 请求一次内容重绘: 走驱动的"一帧一次"合并(见 <see cref="Driver.RequestRepaint"/>),
        /// 避免收敛窗口里叠出多帧冗余重绘。驱动未挂好时为空操作。
        /// </summary>
        protected void RequestRedraw() => m_Driver?.RequestRepaint();

        //-------------------------------------- 生命周期 --------------------------------------

        /// <summary>子类重写做一次性初始化(首次绘制前调用, 异常会被捕获, 不会让窗口崩溃)。</summary>
        protected virtual void Initialize() { }

        /// <summary>窗口销毁时调用, 子类重写做清理。</summary>
        protected virtual void OnWindowDestroy() { }

        // Unity 在定义了 CreateGUI 后只会调用它, 不会再调用窗口自身的 OnGUI;
        // 本类在内部重建 UIElements 树和引擎驱动, 子类只需关心布局(SetupUI)与内容(OnDrawContent)。
        // 注意: CreateGUI 是普通方法而非 virtual, 不能写 override(否则 CS0115)。
        protected void CreateGUI()
        {
            EnsureInitialize();
            Rebuild();
        }

        private void EnsureInitialize()
        {
            if (m_Initialized) return;
            m_Initialized = true;
            try { Initialize(); }
            catch (Exception e) { Debug.LogError($"[Draw] 窗口初始化失败 {GetType().Name}\n{e}"); }
        }

        /// <summary>重建整个 UI: 清空窗体、新建根容器, 再交给子类布局。</summary>
        private void Rebuild()
        {
            rootVisualElement.Clear();
            m_Driver = null;

            var root = new VisualElement { style = { flexGrow = 1f } };
            rootVisualElement.Add(root);
            SetupUI(root);
        }

        //-------------------------------------- 布局与驱动 --------------------------------------

        /// <summary>
        /// 布局钩子: 子类重写以定义窗口的 UIElements 结构, 并把引擎内容驱动绑到某个容器。
        /// 默认实现: 在整窗 root 上挂驱动并立即绘制一次。
        /// 需要自定义布局(如菜单 + 分割条)时重写本方法排布 UIElements,
        /// 再用 <see cref="CreateContentArea"/> 创建一块引擎内容容器。
        /// </summary>
        protected virtual void SetupUI(VisualElement root)
        {
            MountDriver(root);
            Redraw();
        }

        /// <summary>把引擎后端挂到容器上并接上重绘链; 一个窗口同时只有一处内容驱动。</summary>
        protected void MountDriver(VisualElement host, Action draw = null)
        {
            m_Driver = new Driver(host, draw ?? Redraw);
        }

        /// <summary>
        /// 在自定义布局里创建一块引擎内容容器(保留模式)并挂上驱动。
        /// <paramref name="scroll"/> 为 false 时不带外层滚动视图。返回的内容容器即绘制目标的挂载点。
        /// 驱动挂上后会立即绘制一次, 因此调用点必须是布局已经排好的时刻。
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

            MountDriver(host, draw);
            draw();                     // 立即绘制一次
            return host;
        }

        //-------------------------------------- 绘制 --------------------------------------

        /// <summary>重绘入口: 由 Driver 的重绘回调触发, 调用 OnDrawContent 完成实际绘制。</summary>
        private void Redraw()
        {
            if (m_Driver == null) return;
            OnDrawContent();
        }

        /// <summary>
        /// 内容钩子: 决定窗口画什么。默认绘制窗口自身成员;
        /// 子类(如菜单窗口)可重写以绘制自己的内容。默认布局和自定义布局的内容区都会调用它。
        /// </summary>
        protected virtual void OnDrawContent()
        {
            DrawObject(this);
        }

        /// <summary>
        /// 用本窗口的会话与驱动绘制任意对象, 是对 <see cref="Engine.DrawObject"/> 的封装。
        /// 驱动还没挂好时直接跳过: 挂好之后本就会再画一遍, 不存在漏画。
        /// </summary>
        /// <remarks>
        /// 默认窗口宿主只改内存状态(不编辑可序列化资产), 故 onBeforeChange/onChanged 传 null;
        /// 若子类要编辑可序列化对象, 应改为包 <see cref="Engine.BeginChange"/> / <see cref="Engine.EndChange"/>。
        /// </remarks>
        protected void DrawObject(object target)
        {
            if (m_Driver == null) return;
            Engine.DrawObject(target, this, null, null, m_Session, m_Driver);
        }

        private void OnDestroy() => OnWindowDestroy();
    }
}
