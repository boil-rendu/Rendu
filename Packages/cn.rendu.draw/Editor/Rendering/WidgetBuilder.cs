using System;
using System.Collections.Generic;
using UnityEngine;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 产树器: 把渲染链"过程式调用"翻译成 <see cref="Widget"/> 树。
    ///
    /// 它与 <see cref="IDriverControls"/> 同形(Begin/End、Label、控件……), 区别是这些调用不落屏,
    /// 而是 append 到当前节点的 <see cref="Widget.Children"/>。渲染链从"调 Driver"改成"调 builder",
    /// 结构不变, 只是"往哪画"从"立即落屏"变成"先攒成一棵树"。
    ///
    /// 三类"写"必须延迟到渲染之后(解释器渲染后才知道新值/点击), 因此这里只**记录**, 不执行:
    ///   - 值变更(<see cref="Changes"/>) —— 控件产节点时记下"旧值 + 宿主 + 成员", 渲染后由
    ///     <c>ChangeApplier</c> 读新值、比较、Mutate;
    ///   - 状态变更(折叠 / 页码 / 拖拽落点 / 列宽增量) —— 同一个 <see cref="ValueChange"/>,
    ///     但新值交给 <see cref="ValueChange.OnValue"/> 回调, 由产树方自己落库;
    ///   - 按钮点击(<see cref="Buttons"/>) —— 记下"Key + 动作", 渲染后按 <see cref="RenderContext.ClickedButtonKeys"/>
    ///     匹配执行。
    ///
    /// 节点从 <see cref="WidgetPool"/> 借出、渲染完归还(设计决策 4); 借出的节点字段一定是默认值,
    /// 所以这里各方法都把自己的字段写全 —— 新增字段时也要在这里补一行。
    /// 异常时用 <see cref="Mark"/>/<see cref="Restore"/> 把容器栈弹回去, 避免后面的节点挂错父级。
    /// </summary>
    internal sealed class WidgetBuilder
    {
        private readonly Widget m_Root;
        private Widget m_Current;
        private readonly Stack<Widget> m_Stack = new();

        /// <summary>值变更记录(渲染后由 ChangeApplier 应用)。</summary>
        public readonly List<ValueChange> Changes = new();

        /// <summary>按钮点击记录(渲染后按 Key 匹配执行)。</summary>
        public readonly List<ButtonAction> Buttons = new();

        private int m_NextButtonKey = 1;

        public WidgetBuilder(Widget root)
        {
            m_Root    = root ?? throw new ArgumentNullException(nameof(root));
            m_Current = root;
        }

        /// <summary>本次产出的根节点。</summary>
        public Widget Root => m_Root;

        /// <summary>从池里借一个节点(字段保证是默认值)。</summary>
        private static T New<T>() where T : Widget, new() => WidgetPool.Rent<T>();

        //-------------------------------------- 容器 --------------------------------------

        public void BeginVertical(bool boxed = false, float width = 0f, bool expandHeight = false,
                                  bool expandWidth = false)
        {
            var node = New<ColumnWidget>();
            node.Boxed        = boxed;
            node.Width        = width;
            node.ExpandHeight = expandHeight;
            node.ExpandWidth  = expandWidth;
            Push(node);
        }

        public void EndVertical() => Pop();

        public void BeginHorizontal(bool boxed = false)
        {
            var node = New<RowWidget>();
            node.Boxed = boxed;
            Push(node);
        }

        public void EndHorizontal() => Pop();

        public void BeginIndent() => Push(New<IndentWidget>());

        public void EndIndent() => Pop();

        /// <summary>
        /// 开一个滚动区。返回节点本身: 表格这类"要在滚动区里量视口宽"的产树方需要挂钩子
        /// (见 <see cref="ScrollWidget.BeforeBegin"/> 等)。
        /// </summary>
        public ScrollWidget BeginScroll(ref Vector2 scroll, float maxHeight)
        {
            var node = New<ScrollWidget>();
            node.Scroll    = scroll;
            node.MaxHeight = maxHeight;
            Push(node);
            return node;
        }

        public void EndScroll() => Pop();

        public void Space(float pixels)
        {
            var node = New<SpaceWidget>();
            node.Pixels = pixels;
            Add(node);
        }

        public void FlexibleSpace() => Add(New<FlexibleSpaceWidget>());

        public void Separator(float thickness, Color color)
        {
            var node = New<SeparatorWidget>();
            node.Thickness = thickness;
            node.Color     = color;
            Add(node);
        }

        //-------------------------------------- 文本 --------------------------------------

        public void Label(string text, in DrawTextSpec spec)
        {
            var node = New<LabelWidget>();
            node.Text = text;
            node.Spec = spec;
            Add(node);
        }

        public void KeyValueLabel(string label, string value)
        {
            var node = New<KeyValueLabelWidget>();
            node.Label = label;
            node.Value = value;
            Add(node);
        }

        public void HelpBox(string message, DrawMessageKind kind)
        {
            var node = New<HelpBoxWidget>();
            node.Message = message;
            node.Kind    = kind;
            Add(node);
        }

        public void ProgressBar(float value, string title, float height)
        {
            var node = New<ProgressBarWidget>();
            node.Value  = value;
            node.Title  = title;
            node.Height = height;
            Add(node);
        }

        //-------------------------------------- 可选能力 --------------------------------------

        public void PanelHeader(string text, DrawAlign align, bool bold)
        {
            var node = New<PanelHeaderWidget>();
            node.Text  = text;
            node.Align = align;
            node.Bold  = bold;
            Add(node);
        }

        public void ColumnSeparator(float height = 0f)
        {
            var node = New<ColumnSeparatorWidget>();
            node.Height = height;
            Add(node);
        }

        public void BeginCard() => Push(New<CardWidget>());

        public void EndCard() => Pop();

        public void ColumnResizer(float height)
        {
            var node = New<ColumnResizerWidget>();
            node.Height = height;
            Add(node);
        }

        /// <summary>可拖拽列边界: 渲染后把本帧拖拽增量交给 <paramref name="onDrag"/>。</summary>
        public void ColumnResizer(float height, Action<float> onDrag)
        {
            var node = New<ColumnResizerWidget>();
            node.Height = height;
            Add(node);
            Changes.Add(new ValueChange
            {
                Widget          = node,
                OldValue        = 0f,
                ExtractNewValue = w => ((ColumnResizerWidget)w).Delta,
                OnValue         = onDrag == null ? null : v => onDrag((float)v),
            });
        }

        //-------------------------------------- 拖拽排序 --------------------------------------

        /// <summary>开一张可拖拽排序的列表; 收尾用 <see cref="EndReorder"/>。</summary>
        public ReorderWidget BeginReorder(int listId)
        {
            var node = New<ReorderWidget>();
            node.ListId = listId;
            Push(node);
            return node;
        }

        /// <summary>结束列表体: 渲染后把本帧拖拽结果交给 <paramref name="onResult"/>。</summary>
        public void EndReorder(Action<ReorderDrag> onResult)
        {
            var node = (ReorderWidget)m_Current;
            Pop();
            Changes.Add(new ValueChange
            {
                Widget          = node,
                OldValue        = ReorderDrag.None,
                ExtractNewValue = w => ((ReorderWidget)w).Result,
                OnValue         = onResult == null ? null : v => onResult((ReorderDrag)v),
            });
        }

        /// <summary>开一个条目(渲染完后才登记它的区间, 见 <see cref="ReorderItemWidget"/>)。</summary>
        public void BeginReorderItem(int listId, int index)
        {
            var node = New<ReorderItemWidget>();
            node.ListId = listId;
            node.Index  = index;
            Push(node);
        }

        public void EndReorderItem() => Pop();

        public void ReorderHandle(int listId, int index, float width, string ghost)
        {
            var node = New<ReorderHandleWidget>();
            node.ListId = listId;
            node.Index  = index;
            node.Width  = width;
            node.Ghost  = ghost;
            Add(node);
        }

        //-------------------------------------- 外观作用域 --------------------------------------

        public void PushColor(Color color)
        {
            var node = New<StyledScopeWidget>();
            node.Color = color;
            Push(node);
        }

        public void PushEnabled(bool enabled)
        {
            var node = New<StyledScopeWidget>();
            node.Enabled = enabled;
            Push(node);
        }

        public void PushLabelWidth(float? width)
        {
            var node = New<StyledScopeWidget>();
            node.LabelWidth = width;
            Push(node);
        }

        public void PopScope() => Pop();

        //-------------------------------------- 控件 --------------------------------------

        /// <summary>按钮: 记录 Key → 动作, 渲染后匹配 <see cref="RenderContext.ClickedButtonKeys"/> 执行。</summary>
        public void Button(string text, in DrawButtonSpec spec, Action action)
        {
            var key  = m_NextButtonKey++;
            var node = New<ButtonWidget>();
            node.Text = text;
            node.Spec = spec;
            node.Key  = key;
            Add(node);

            Buttons.Add(new ButtonAction { Key = key, Action = action });
        }

        public void Foldout(bool expanded, string label, Action<bool> onChanged = null)
        {
            var node = New<FoldoutWidget>();
            node.Expanded = expanded;
            node.Label    = label;
            Add(node);

            Changes.Add(new ValueChange
            {
                Widget          = node,
                OldValue        = expanded,
                ExtractNewValue = w => ((FoldoutWidget)w).Expanded,
                OnValue         = onChanged == null ? null : v => onChanged((bool)v),
            });
        }

        public void TextField(object owner, CachedMember member, string label, string value, float width) =>
            AddField(owner, member, NewTextField(label, value, width), value);

        /// <summary>
        /// 只读显示的文本框(列表里直接编辑元素、字典的键值之类): 值不属于任何 <see cref="CachedMember"/>,
        /// 新值直接交给 <paramref name="onChanged"/>, 不走成员写回。传 null = 纯显示(编辑结果丢弃)。
        /// </summary>
        public void TextField(string label, string value, float width, Action<string> onChanged)
        {
            var node = NewTextField(label, value, width);
            Add(node);
            if (onChanged == null) return;

            Changes.Add(new ValueChange
            {
                Widget          = node,
                OldValue        = value,
                ExtractNewValue = w => ((TextFieldWidget)w).Value,
                OnValue         = v => onChanged((string)v),
            });
        }

        private static TextFieldWidget NewTextField(string label, string value, float width)
        {
            var node = New<TextFieldWidget>();
            node.Label = label;
            node.Value = value;
            node.Width = width;
            return node;
        }

        public void IntegerField(object owner, CachedMember member, string label, long value, float width) =>
            AddField(owner, member, NewIntegerField(label, value, width), value);

        /// <summary>同 <see cref="TextField(string, string, float, Action{string})"/>: 新值走回调, 不写成员。</summary>
        public void IntegerField(string label, long value, float width, Action<long> onChanged)
        {
            var node = NewIntegerField(label, value, width);
            Add(node);
            if (onChanged == null) return;

            Changes.Add(new ValueChange
            {
                Widget          = node,
                OldValue        = value,
                ExtractNewValue = w => ((IntegerFieldWidget)w).Value,
                OnValue         = v => onChanged((long)v),
            });
        }

        private static IntegerFieldWidget NewIntegerField(string label, long value, float width)
        {
            var node = New<IntegerFieldWidget>();
            node.Label = label;
            node.Value = value;
            node.Width = width;
            return node;
        }

        public void FloatField(object owner, CachedMember member, string label, double value, float width)
        {
            var node = New<FloatFieldWidget>();
            node.Label = label;
            node.Value = value;
            node.Width = width;
            AddField(owner, member, node, value);
        }

        public void Slider(object owner, CachedMember member, string label, float value, float min, float max, float width)
        {
            var node = New<SliderWidget>();
            node.Label = label;
            node.Value = value;
            node.Min   = min;
            node.Max   = max;
            node.Width = width;
            AddField(owner, member, node, value);
        }

        public void ToggleField(object owner, CachedMember member, string label, bool value, float width)
        {
            var node = New<ToggleWidget>();
            node.Label = label;
            node.Value = value;
            node.Width = width;
            AddField(owner, member, node, value);
        }

        public void Popup(object owner, CachedMember member, string label, int selected, string[] options, float width,
                          object oldValue, Func<Widget, object> extract)
        {
            var node = New<PopupWidget>();
            node.Label    = label;
            node.Selected = selected;
            node.Options  = options;
            node.Width    = width;
            AddField(owner, member, node, oldValue, extract);
        }

        public void EnumFlagsField(object owner, CachedMember member, string label, Enum value, float width)
        {
            var node = New<EnumFlagsWidget>();
            node.Label = label;
            node.Value = value;
            node.Width = width;
            AddField(owner, member, node, value);
        }

        public void Toolbar(object owner, CachedMember member, int selected, string[] options, float width,
                            object oldValue, Func<Widget, object> extract)
        {
            var node = New<ToolbarWidget>();
            node.Selected = selected;
            node.Options  = options;
            node.Width    = width;
            AddField(owner, member, node, oldValue, extract);
        }

        /// <summary>
        /// 多选按钮组([Flags] + [EnumToggleButtons(MultiSelect = true)]): 节点存 <see cref="ToolbarMultiWidget.States"/>
        /// (当前亮灭) 与 <see cref="ToolbarMultiWidget.ClickedIndex"/> (被点击下标); 成员值由 <paramref name="extract"/>
        /// 按 None/All/普通位语义合成。
        /// </summary>
        public void ToolbarMulti(object owner, CachedMember member, bool[] states, string[] options, float width,
                                 object oldValue, Func<Widget, object> extract)
        {
            var node = New<ToolbarMultiWidget>();
            node.States  = states;
            node.Options = options;
            node.Width   = width;
            AddField(owner, member, node, oldValue, extract);
        }

        public void ObjectField(object owner, CachedMember member, string label, UnityEngine.Object value, Type type, float width)
        {
            var node = New<ObjectFieldWidget>();
            node.Label = label;
            node.Value = value;
            node.Type  = type;
            node.Width = width;
            AddField(owner, member, node, value);
        }

        /// <summary>只读显示的 Unity 对象引用(列表元素): 不写回任何成员, 编辑结果丢弃。</summary>
        public void ObjectField(string label, UnityEngine.Object value, Type type, float width = 0f)
        {
            var node = New<ObjectFieldWidget>();
            node.Label = label;
            node.Value = value;
            node.Type  = type;
            node.Width = width;
            Add(node);
        }

        public void StructField(object owner, CachedMember member, string label, object value, Type type, float width)
        {
            var node = New<StructFieldWidget>();
            node.Label = label;
            node.Value = value;
            node.Type  = type;
            node.Width = width;
            AddField(owner, member, node, value);
        }

        //-------------------------------------- 内部 --------------------------------------

        private void AddField(object owner, CachedMember member, ControlWidget widget, object oldValue,
                              Func<Widget, object> extract = null)
        {
            Add(widget);
            Changes.Add(new ValueChange
            {
                Widget          = widget,
                OldValue        = oldValue,
                Owner           = owner,
                Member          = member,
                ExtractNewValue = extract,
            });
        }

        //-------------------------------------- 异常回滚 --------------------------------------

        /// <summary>
        /// 记录当前挂载点(容器栈深), 供 <see cref="Restore"/> 在异常时回滚。
        ///
        /// 建树是"当前节点"栈, 一个成员/条目抛异常时它可能还开着两三层容器; 不弹回去,
        /// 同一帧后面所有节点都会挂进那个半开的容器里 —— 表现为"整块面板缩进错位"。
        /// </summary>
        public int Mark() => m_Stack.Count;

        /// <summary>
        /// 回滚到 <see cref="Mark"/> 的位置: 把异常时还开着的容器逐层弹掉。
        /// 已产出的节点保留 —— 半棵树也比整块消失更容易看懂哪里出了问题。
        /// </summary>
        public void Restore(int mark)
        {
            while (m_Stack.Count > mark) Pop();
        }

        private void Add(Widget node)
        {
            m_Current.Add(node);
        }

        private void Push(Widget node)
        {
            m_Current.Add(node);
            m_Stack.Push(m_Current);
            m_Current = node;
        }

        private void Pop()
        {
            m_Current = m_Stack.Pop();
        }
    }

    /// <summary>一次值变更的记录: 渲染后读 <see cref="Widget"/> 新值、与 <see cref="OldValue"/> 比较。</summary>
    internal sealed class ValueChange
    {
        public Widget       Widget;       // 渲染后其 Value/Selected/Expanded 被解释器回填
        public object       OldValue;     // 产树时读到的旧值
        public object       Owner;        // 宿主对象
        public CachedMember Member;       // 成员元数据(写回用)

        /// <summary>
        /// 状态型变更的去处: 有它就不写成员, 新值直接交给它(折叠态、页码、拖拽结果、列宽增量)。
        /// </summary>
        public Action<object> OnValue;

        /// <summary>
        /// 从节点读"已回填的新值"并转回成员类型的转换器(可为 null)。
        ///
        /// 标量控件(TextField/Integer/Float/Slider/Toggle/EnumFlags/Object/Struct)由
        /// <see cref="ChangeApplier"/> 内置的按类型读取; Popup / Toolbar / ToolbarMulti 这类
        /// "节点存下标或位数组、成员是枚举/字符串"的控件, 由产树方提供闭包转回成员值。
        /// </summary>
        public Func<Widget, object> ExtractNewValue;
    }

    /// <summary>一次按钮点击的记录: 渲染后按 <see cref="Key"/> 匹配 <see cref="RenderContext.ClickedButtonKeys"/>。</summary>
    internal sealed class ButtonAction
    {
        public int    Key;
        public Action Action;
    }
}
