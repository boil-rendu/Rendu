using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 轻量菜单树工具窗口: 左侧绘制 <see cref="BuildMenuTree"/> 返回的菜单树, 右侧用引擎后端绘制当前选中项。
    ///
    /// 本文件是<strong>宿主 + 外壳(UI 布局)</strong>; 左树自身的构建与交互按主题放在同名的 partial 文件里,
    /// 四个文件合起来才是这个类:
    ///   - <c>MenuEditorWindow.cs</c>          常量 / 字段 / 菜单树 / 选中调度 / 外壳布局 / 内容区;
    ///   - <c>MenuEditorWindow.MenuUI.cs</c>   左树的行构建、点击、搜索过滤、键盘导航;
    ///   - <c>MenuEditorWindow.Drag.cs</c>     菜单项拖拽重排与外部对象拖入;
    ///   - <c>MenuEditorWindow.Splitter.cs</c> 左树与内容区之间的可拖分割条。
    /// </summary>
    public partial class MenuEditorWindow : DrawWindow
    {
        //-------------------------------------- 常量 --------------------------------------

        /// <summary>菜单栏最小宽: 同时也是 MenuWidth 的默认值与拖拽下限。</summary>
        private const float MinMenuWidth = 150f;

        /// <summary>折叠箭头(foldout)控件固定宽, 叶子项用等量 Space 对齐文字起点。</summary>
        private const float FoldoutWidth = 16f;

        /// <summary>子级菜单相对父级额外缩进(每层叠加一次)。</summary>
        private const float MenuIndentWidth = 12f;

        /// <summary>菜单/内容区上下留白, 以及菜单项文字左右内边距。</summary>
        private const int MenuPadding = 4;

        /// <summary>未选中菜单项顶部的空态提示距顶间距。</summary>
        private const float EmptyHintTopMargin = 30f;

        /// <summary>分割条颜色(纯用 background 绘制)。</summary>
        private static readonly Color SplitterColor = new(0.15f, 0.15f, 0.15f, 1f);

        /// <summary>选中菜单项底纹色(低饱和蓝, 避免暗色面板过跳)。</summary>
        private static readonly Color MenuSelectColor = new(0.20f, 0.36f, 0.56f, 1f);

        /// <summary>
        /// 非选中菜单项文字色: 取编辑器默认标签色(跟随深浅皮肤)。
        /// 不能写 <c>StyleColor(StyleKeyword.Undefined)</c> —— 它在 color 上不会落回继承色, 而是解析成
        /// 初始的透明色, 结果只有被选中项(显式白字)可见、其余菜单项"消失"。
        /// </summary>
        private static Color NormalMenuTextColor
        {
            get
            {
                    return EditorGUIUtility.isProSkin
                        ? new Color(0.78f, 0.78f, 0.78f)
                        : new Color(0.12f, 0.12f, 0.12f);
            }
        }

        //-------------------------------------- 字段 --------------------------------------

        /// <summary>本窗口的菜单树(首次访问时自动构建)</summary>
        protected MenuTree m_MenuTree;

        private bool m_MenuTreeBuilt;

        /// <summary>
        /// 左树默认宽: 工具窗口的模块名普遍是 2~4 个字, 用户仍可拖分割条改宽,
        /// 下限见 <see cref="MenuWidth"/>。
        /// </summary>
        private float m_MenuWidth = MinMenuWidth;

        /// <summary>承载左侧菜单树的面板(宽度由 MenuWidth 决定, 可拖分割条改)。</summary>
        private VisualElement m_MenuPanel;

        /// <summary>菜单项 -> 其行元素引用 的映射, 用于选中态高亮刷新(避免整树重建)。</summary>
        private Dictionary<MenuTreeItem, MenuRowRef> m_MenuRowMap;

        /// <summary>菜单搜索框(顶栏过滤, 只改 display 不重建树)。</summary>
        private TextField m_SearchFilter;

        /// <summary>承载菜单行的滚动视图(供键盘导航自动滚动)。</summary>
        private ScrollView m_MenuScroll;

        /// <summary>当前展开/过滤态下的可见有序项(供键盘导航跳过隐藏项)。</summary>
        private List<MenuTreeItem> m_VisibleOrder;

        /// <summary>当前搜索关键字(空串表示未过滤)。</summary>
        private string m_Filter = "";

        //-------------------------------------- 内容区字段 --------------------------------------
        /// <summary>空态提示(没选中项时显示); 放在驱动容器**外面**, 免得被按位置复用换掉。</summary>
        private HelpBox m_ContentEmptyHint;

        /// <summary>右侧内容区的引擎宿主(驱动挂在这里): 换绘制目标时清空它, 强制整树重建。</summary>
        private VisualElement m_ContentHost;

        /// <summary>上一次画进内容区的目标对象: 与本次不同 = 换了页签/模块。</summary>
        private object m_LastDrawnTarget;

        /// <summary>上一次报给引擎的内容区下沿(面板坐标), 用来判断外框是否真的变了。</summary>
        private float m_LastContentBottom;

        /// <summary>内容宿主上一次的宽度: 宽度变了要补一次重绘(见 BuildContentScroll 里的兜底)。</summary>
        private float m_LastContentHostWidth;

        /// <summary>临时诊断用: 累计 RedrawContent 调用次数, 用于识别重绘连环触发(事后删除)。</summary>
        private int m_JitterRedrawCount;

        //-------------------------------------- 菜单树 --------------------------------------

        /// <summary>菜单栏宽度</summary>
        public virtual float MenuWidth
        {
            get => m_MenuWidth;
            set => m_MenuWidth = Mathf.Max(MinMenuWidth, value);
        }

        /// <summary>构建菜单树, 返回 null 表示无菜单</summary>
        protected virtual MenuTree BuildMenuTree()
        {
            return null;
        }

        /// <summary>当前菜单树(未构建时自动构建)</summary>
        protected MenuTree MenuTree
        {
            get
            {
                EnsureMenuTree();
                return m_MenuTree;
            }
        }

        private void EnsureMenuTree()
        {
            if (m_MenuTreeBuilt) return;
            m_MenuTreeBuilt = true;

            try
            {
                m_MenuTree = BuildMenuTree();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditTool] 菜单树构建失败 {GetType().Name}\n{e}");
                m_MenuTree = null;
            }

            // 无选中时默认选中第一项, 触发派生类的 SelectionChanged 恢复逻辑
            if (m_MenuTree != null && m_MenuTree.Selection.Count == 0)
            {
                try
                {
                    m_MenuTree.SelectFirstMenuItem();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EditTool] 菜单默认选中失败 {GetType().Name}\n{e}");
                }
            }
        }

        /// <summary>强制重建菜单树(例如构建条件变化后调用): 重订阅选中事件并刷新左侧菜单 UI。</summary>
        protected void RebuildMenuTree()
        {
            m_MenuTreeBuilt = false;
            m_MenuTree = null;
            EnsureMenuTree();
            SubscribeSelectionEvents();   // 新建的树已在 BuildMenuTree 内订阅, 此处幂等补一次

            if (m_MenuPanel != null)
            {
                m_MenuPanel.Clear();
                BuildMenuUI(m_MenuPanel);

                // 重建后搜索框是新建的(空), 但过滤关键字 m_Filter 仍在:
                // 回填关键字并重套过滤, 否则"框里没字、树却被过滤着"两种状态会脱节。
                if (m_SearchFilter != null)
                {
                    m_SearchFilter.value = m_Filter;   // 回填(变更回调会顺带重套一次过滤)
                    ApplyFilter(m_Filter);             // 幂等, 显式保证树的显隐与关键字一致
                }

                RefreshMenuSelection();
            }
        }

        //-------------------------------------- 选中调度 --------------------------------------

        /// <summary>
        /// 统一订阅"菜单选中变化"事件的两个处理器(幂等, 可多处复用):
        /// - <see cref="OnSelectionChanged"/>    (业务/状态层, 由子类实现): 恢复上次选中、初始化模块、落盘最近选中;
        /// - <see cref="OnMenuSelectionChanged"/> (UI 层): 刷新左侧高亮 + 重绘右侧内容。
        /// 先退订再订阅, 因此无论"首次构建 / 外壳(UI 布局)重建 / 强制重建"都只挂一份回调, 不会重复触发。
        /// </summary>
        protected void SubscribeSelectionEvents()
        {
            if (m_MenuTree == null) return;
            m_MenuTree.Selection.SelectionChanged -= OnSelectionChanged;
            m_MenuTree.Selection.SelectionChanged -= OnMenuSelectionChanged;
            m_MenuTree.Selection.SelectionChanged += OnSelectionChanged;
            m_MenuTree.Selection.SelectionChanged += OnMenuSelectionChanged;

            // 拖拽重排后要重建左侧 UI; 同样先退订再订阅, 重建多次也只挂一份
            m_MenuTree.ItemsReordered -= OnMenuItemsReordered;
            m_MenuTree.ItemsReordered += OnMenuItemsReordered;
        }

        /// <summary>基类钩子: 选中变化时的业务/状态处理(恢复选中、初始化模块、记忆最近选中)。子类重写。</summary>
        protected virtual void OnSelectionChanged(SelectionChangedType obj) { }

        /// <summary>选中变化(UI 层): 左树高亮 + 滚入视口 + 右侧重绘。</summary>
        private void OnMenuSelectionChanged(SelectionChangedType type)
        {
            RefreshMenuSelection();
            ScrollSelectedIntoView();
            RedrawContent();
        }

        //-------------------------------------- 外壳 --------------------------------------

        /// <summary>排布: 左树(菜单) + 可拖分割条 + 右内容(引擎后端)。</summary>
        protected override void SetupUI(VisualElement root)
        {
            // 走到这里时 Initialize 已经跑过(基类 DrawWindow.CreateGUI 先 EnsureInitialize 再构建), 直接建树
            EnsureMenuTree();

            if (m_MenuTree == null)
            {
                base.SetupUI(root);   // 没有菜单: 退化成普通窗口
                return;
            }

            root.style.flexDirection = FlexDirection.Row;

            m_MenuPanel = BuildMenuPanel();
            root.Add(m_MenuPanel);
            root.Add(BuildSplitter());
            root.Add(BuildContentScroll());

            BuildMenuUI(m_MenuPanel);
            RefreshMenuSelection();   // 把初始选中项的高亮画上

            // 键盘导航: 在根容器注册; 搜索框输入中(evt.target 为 TextField)不拦截
            root.RegisterCallback<KeyDownEvent>(OnMenuKeyDown);

            // 菜单选中项变了: 右侧内容要重画、菜单高亮要刷新。
            // 统一在 SubscribeSelectionEvents 里幂等订阅(首次构建 / 外壳(UI 布局)重建 / 强制重建共用), 避免重复回调。
            SubscribeSelectionEvents();
        }

        private VisualElement BuildMenuPanel()
        {
            return new VisualElement
            {
                style =
                {
                    width      = MenuWidth,
                    flexGrow   = 0f,
                    flexShrink = 0f,
                },
            };
        }

        private ScrollView BuildContentScroll()
        {
            var scroll = new ScrollView { style = { flexGrow = 1f } };
            // 竖向滚动条常驻: 预留滚动条宽度, 内容宽度不随"滚动条出现/消失"跳变。
            // 否则内容高度贴着视口时滚动条会在有无之间反复跳: 宽度随之变 → 表格列重排 → 行高变 →
            // 高度变 → 滚动条又消失…形成持续重绘反馈环(切页签后尤为明显, 直接进来因首画在建树前
            // 没激活探测而躲过)。常驻也顺带让"直接进来"与"切页签"的宽度基准一致。
            scroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            // 横向滚动条同样常驻: 否则窗口被拉窄到内容(工具条等)略微超出视口时, 横向条出现 →
            // 可用宽骤减 → 内容回流又能装下 → 条消失 → 宽回涨 → 又超出…缩放时每帧反复跳(颤)。
            // 常驻预留出横向 gutter, 内容宽恒定, 缩放不再触发这条反馈环。
            scroll.horizontalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            // ScrollView 的内容组默认按"内容高"收缩: 内容比外框短时下方就留一条空带, 内容框贴不住外框。
            // 显式撑满视口, 里面的引擎内容区(flexGrow=1)才可能跟着外框一起拉伸/收缩。
            scroll.contentContainer.style.flexGrow = 1f;
            // 但"撑满"不能连收缩一起允许: 内容组是 flex 项, flexShrink 默认 1 —— 页面比视口高时
            // 它会被压回视口高, 里面的内容再按比例分掉"差的那几像素", 定高的行全被压扁、
            // 溢出后叠成一片(筛选区几条工具条糊在一起就是这么来的)。归零之后内容组保持内容高,
            // 超出的部分由 ScrollView 自己滚动。
            scroll.contentContainer.style.flexShrink = 0f;

            // 外框尺寸变了(窗口拉伸 / 隐藏后重开): 把内容区可用下沿报给引擎,
            // 表格数据区据此填满外框剩下的高度(见 HostState.SetContentBottom)。
            scroll.RegisterCallback<GeometryChangedEvent>(_ => ReportContentBottom(scroll));

            m_ContentEmptyHint = new HelpBox("请选择左侧菜单项", HelpBoxMessageType.Info);
            m_ContentEmptyHint.style.marginTop = EmptyHintTopMargin;
            scroll.contentContainer.Add(m_ContentEmptyHint);

            // 内容区挂上驱动后即画第一遍; 这之前的所有 RedrawContent(建外壳时的默认选中)
            // 都被 ContentDriverReady 挡住, 不会画到整窗 root 上。
            m_ContentHost     = CreateContentArea(scroll.contentContainer, RedrawContent, false);
            m_LastDrawnTarget = null;   // 宿主是新的, 上次目标作废: 下一画必走整树重建

            var host = m_ContentHost;

            // 首遍绘制必然发生在布局完成前: ViewWidth 量不到宿主自己的宽, 会沿祖先猜到整窗宽,
            // 工具条这类按宽均分的控件就以错误度量落下(页签折行、压住下面的行)。三个自愈通道
            // (OnFirstGeometry 一次性 / EndDraw 只在完全量不到宽时重试 / ReportContentBottom 只在
            // 下沿变化时)在"量到了宽、但量的是祖先的宽"场景都不触发, 坏布局会卡住。
            // 这里兜底: 宿主宽度真变了(含首帧 0 → 实际宽)就排一次延后重绘; 重绘是全量复用, 多排无害。
            host.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var width = evt.newRect.width;
                if (Mathf.Abs(width - m_LastContentHostWidth) <= 0.5f) return;
                Debug.Log($"[ContentJitter] width {m_LastContentHostWidth:F2} -> {width:F2} (Δ{(width - m_LastContentHostWidth):F2}) => RequestRedraw");
                m_LastContentHostWidth = width;
                RequestRedraw();   // 合并到驱动的"一帧一次"重绘, 收敛窗口里不叠多帧
            });
            return scroll;
        }

        //-------------------------------------- 内容区 --------------------------------------

        /// <summary>右侧内容入口: 选中项有值就画它, 否则只留空态提示。驱动没挂好时不画。</summary>
        private void RedrawContent()
        {
            Debug.Log($"[ContentJitter] RedrawContent #{++m_JitterRedrawCount}");
            var target = m_MenuTree?.Selection?.SelectedValue?.Value;

            UpdateEmptyHint(target);

            // 驱动没挂好时不画: 那时没有落点, 引擎会按位置复用把模块内容塞进布局根容器,
            // 视觉上就是左树左边多出一条挤扁的按钮条。挂好之后自然会有第一遍绘制。
            if (target != null && ContentDriverReady)
                DrawSelectedTarget(target);
        }

        /// <summary>空态提示: 没选中项就显示, 选中项就隐藏(提示放在驱动容器外面, 不被复用换掉)。</summary>
        private void UpdateEmptyHint(object target)
        {
            if (m_ContentEmptyHint != null)
                m_ContentEmptyHint.style.display = target == null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// 画当前选中目标: 换了目标(切页签 / 模块)就清空整树 + 排一次布局完成后重画;
        /// 同一目标则直接画(按位置复用, 不丢输入焦点)。
        /// </summary>
        private void DrawSelectedTarget(object target)
        {
            // 切页签(绘制目标变了): 与"直接进来就是这一页"走同一条画界面通道。
            // 先清空整树再画(上一模块留在同型元素上的创建期样式不会被 Styler 重刷, 不清会压盖),
            // 再排一次布局完成后的修正重绘 —— 直接进来靠首帧后的 GeometryChangedEvent 补画,
            // 切页签时宿主宽度没变、那个事件不触发, 所以这里手动兜底。
            // 抖动不会回来了: 内容区竖向滚动条已 AlwaysVisible, 宽度不再随滚动条跳变,
            // 不会触发 host 宽度变化 → 不会反复重绘。
            if (ReferenceEquals(target, m_LastDrawnTarget) == false)
            {
                Debug.Log($"[ContentJitter] TargetChanged {m_LastDrawnTarget?.GetType().Name} -> {target?.GetType().Name} (hash {target?.GetHashCode()})");
                m_LastDrawnTarget = target;
                m_ContentHost?.Clear();
                RequestRedraw();   // 延后到布局完成后画(合并到驱动重绘, 不叠多帧机颤)
                return;
            }

            DrawObject(target);
        }

        /// <summary>
        /// 把内容区的可用下沿交给引擎: 表格的数据区按"可用下沿 − 当前位置"填满外框剩下的高度,
        /// 底边贴着外框下沿, 窗口拉高时跟着拉长(见 <c>HostState.SetContentBottom</c> / <c>TableFillHeight</c>)。
        ///
        /// 只在值变化时补一次重绘: 重绘不会改变视口高度, 不会自己喂自己。
        /// 滚动位置要折算掉 —— 否则一滚滚轮"还剩多少高"就变, 表格会跟着长/缩。
        /// </summary>
        private void ReportContentBottom(ScrollView scroll)
        {
            var bottom = scroll.worldBound.yMax - scroll.scrollOffset.y;
            if (bottom <= 1f) return;
            if (Mathf.Abs(bottom - m_LastContentBottom) <= 0.5f) return;

            m_LastContentBottom = bottom;
            Debug.Log($"[ContentJitter] bottom {m_LastContentBottom:F2} -> {bottom:F2} (Δ{(bottom - m_LastContentBottom):F2}, scrollOffsetY={scroll.scrollOffset.y:F2}) => RequestRedraw");
            m_Session.SetContentBottom(bottom);

            // 延后重绘: 回调发生在布局阶段, 当场重画等于在事件处理里重入布局。
            // 走驱动的"一帧一次"合并, 与上面两处来源一起压掉收敛窗口里的冗余帧。
            RequestRedraw();
        }
    }
}
