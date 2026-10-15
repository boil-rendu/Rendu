using System;
using RD.Draw.Editor;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 轻量编辑器窗口基类
    /// 默认在 OnGUI 中依据 Inspector 属性自动绘制窗口自身成员
    ///
    /// 绘制后端由 <see cref="WindowBackend"/> 在包内切换(与检视器侧的 InspectorBackend 同一套用法):
    ///   - IMGUI(默认): 走 OnGUI 逐帧绘制 —— 与改造前一致;
    ///   - UI Toolkit: 引擎画进 rootVisualElement 里的一棵 VisualElement 树, 重绘由绘制回调驱动。
    /// 两条分支都汇到 DrawObjectPanel / DrawTargetObject 两个入口, 子类的写法不变。
    /// </summary>
    public class EditorWindowBase : UnityEditor.EditorWindow
    {
        private bool m_Initialized;
        private Vector2 m_ScrollPosition;

        /// <summary>本窗口的状态句柄: 生命周期与窗口一致, 窗口关闭即释放。</summary>
        private readonly HostState m_Session = Engine.CreateHostState();

#if UNITY_2021_2_OR_NEWER
        /// <summary>UI Toolkit 模式下的绘制后端(与内容容器同生共死)。</summary>
        private UitkDriver m_Driver;

        /// <summary>UI Toolkit 模式的重绘体: 数据变化 → 驱动排一次重绘 → 再调它。</summary>
        private Action m_Draw;

        /// <summary>本窗口是否已经按 UI Toolkit 模式搭好树(开关变化时重建 / 拆掉)。</summary>
        private bool m_ToolkitBuilt;

        /// <summary>正在搭树: 子类在 BuildToolkitUI 里又走到 EnsureBackend 时的防重入标记。</summary>
        private bool m_ToolkitBuilding;
#endif

        /// <summary>本窗口这次是否在用 UI Toolkit 绘制。子类据此决定要不要画自己那段 IMGUI。</summary>
        protected bool UseToolkitBackend
        {
            get
            {
#if UNITY_2021_2_OR_NEWER
                return m_ToolkitBuilt;
#else
                return false;   // 本包要求 2022.3; 老版本 Unity 上永远走 IMGUI
#endif
            }
        }

        /// <summary>
        /// 首次显示前调用一次 子类可重写做初始化
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// 窗口关闭销毁时调用 子类可重写做清理
        /// </summary>
        protected virtual void OnWindowDestroy()
        {
        }

        private void EnsureInitialize()
        {
            if (m_Initialized) return;
            m_Initialized = true;

            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Inspector] 窗口初始化失败 {GetType().Name}\n{e}");
            }
        }

        /// <summary>
        /// 子类 OnGUI 覆盖时也需先完成惰性初始化
        /// </summary>
        protected void EnsureWindowInitialized()
        {
            EnsureInitialize();
            EnsureBackend();   // 顺带把绘制后端对齐到当前开关(重写 OnGUI 的窗口靠这一句才会跟着切)
        }

        protected virtual void OnGUI()
        {
            EnsureInitialize();
            EnsureBackend();

            // UI Toolkit 模式: 内容已经画在 rootVisualElement 上了, IMGUI 这一遍什么都不画。
            // OnGUI 仍会被每帧调用 —— 开关切换就是靠它在下一帧生效的。
            if (UseToolkitBackend) return;

            DrawObjectPanel();
        }

        /// <summary>
        /// 以滚动视图绘制宿主窗口对象
        /// (IMGUI 模式下滚动区在这里开; UI Toolkit 模式下内容区自带滚动, 见 BuildToolkitUI)
        /// </summary>
        protected void DrawObjectPanel()
        {
#if UNITY_2021_2_OR_NEWER
            if (UseToolkitBackend)
            {
                Engine.DrawObject(this, this, null, null, m_Session, m_Driver);
                return;
            }
#endif

            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            Engine.DrawObject(this, this, null, null, m_Session);
            GUILayout.EndScrollView();
        }

        /// <summary>用本窗口的会话绘制任意对象, 供子类复用。</summary>
        protected void DrawTargetObject(object target)
        {
#if UNITY_2021_2_OR_NEWER
            if (UseToolkitBackend)
            {
                Engine.DrawObject(target, this, null, null, m_Session, m_Driver);
                return;
            }
#endif

            Engine.DrawObject(target, this, null, null, m_Session);
        }

        private void OnEnable()
        {
            // 消息占位:统一走 EnsureInitialize 惰性初始化
        }

        private void OnDestroy()
        {
            try
            {
                OnWindowDestroy();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Inspector] 窗口销毁失败 {GetType().Name}\n{e}");
            }
        }

        //-------------------------------------- 绘制后端 --------------------------------------

        /// <summary>
        /// 按 <see cref="WindowBackend"/> 的开关把本窗口切到目标后端。
        /// 每帧 OnGUI 都会问一次: 开关是在外面(菜单/代码)改的, 不这样窗口就得关掉重开才生效。
        /// 子类重写 OnGUI 时也要走一趟(EnsureWindowInitialized 里已经带了)。
        /// </summary>
        protected void EnsureBackend()
        {
#if UNITY_2021_2_OR_NEWER
            var wantToolkit = WindowBackend.UseUIToolkit;
            if (wantToolkit == m_ToolkitBuilt) return;

            if (wantToolkit) BuildToolkit();
            else TearDownToolkit();
#endif
        }

#if UNITY_2021_2_OR_NEWER
        private void BuildToolkit()
        {
            if (m_ToolkitBuilding) return;   // 防重入: 子类在 BuildToolkitUI 里再走一次 EnsureBackend 也不能又进来

            m_ToolkitBuilding = true;

            var root = rootVisualElement;
            root.Clear();

            try
            {
                BuildToolkitUI(root);
                m_ToolkitBuilt = true;
            }
            catch (Exception e)
            {
                // 结构没搭起来就继续走 IMGUI —— 总比留半棵树 + 面板空白强
                Debug.LogError($"[Inspector] 窗口 UI Toolkit 布局构建失败, 本窗口退回 IMGUI {GetType().Name}\n{e}");
                m_Driver = null;
                m_Draw = null;
                root.Clear();
            }
            finally
            {
                m_ToolkitBuilding = false;
            }

            if (m_ToolkitBuilt) RedrawToolkit();   // 搭好就画一次, 不等下次数据变化
        }

        private void TearDownToolkit()
        {
            m_Driver = null;
            m_Draw = null;
            m_ToolkitBuilt = false;
            rootVisualElement.Clear();
        }

        /// <summary>
        /// UI Toolkit 模式的界面骨架。基类默认: 整窗一个滚动区, 引擎内容画进去。
        /// 自己有外壳的窗口(如 MenuEditorWindow 的左树 + 分割条)重写它, 用
        /// <see cref="CreateToolkitContent"/> 把引擎内容摆进自己的布局里。
        /// </summary>
        protected virtual void BuildToolkitUI(VisualElement root)
        {
            CreateToolkitContent(root, DrawObjectPanel);
        }

        /// <summary>
        /// 开一块"引擎内容区"并接上重绘链: 数据变化 → 驱动排一次重绘 → 重新调 <paramref name="draw"/>。
        /// 驱动按位置复用元素, 重画不会整树重建, 正在输入的输入框也不丢焦点。
        /// <paramref name="scroll"/> 为 true 时自带滚动区(内容比窗口高时能滚)。
        /// </summary>
        protected VisualElement CreateToolkitContent(VisualElement parent, Action draw, bool scroll = true)
        {
            VisualElement container;

            if (scroll)
            {
                var scrollView = new ScrollView();
                scrollView.style.flexGrow = 1f;
                parent.Add(scrollView);
                container = scrollView.contentContainer;
            }
            else
            {
                container = new VisualElement();
                container.style.flexGrow = 1f;
                parent.Add(container);
            }

            m_Draw   = draw;
            m_Driver = new UitkDriver(container, RedrawToolkit);
            return container;
        }

        /// <summary>UI Toolkit 模式下重画一次内容: 首次搭建与驱动请求的重绘都汇到这里。</summary>
        private void RedrawToolkit()
        {
            if (m_ToolkitBuilt == false || m_Draw == null) return;

            try
            {
                m_Draw();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Inspector] 窗口绘制失败 {GetType().Name}\n{e}");
            }
        }
#endif
    }
}
