using System;
using UnityEngine;

namespace RD.Panel.Editor
{
    /// <summary>
    /// 面板绘制后端: 把引擎"画什么"的决策翻译成本地 UI 系统的一次次绘制调用。
    ///
    /// 这是一套**立即模式**契约 —— 引擎按成员顺序调用这些方法, 后端负责把调用序列落到自己的
    /// UI 树上。两个实现:<see cref="ImGuiDriver"/>(EditorGUILayout)与 <see cref="UitkDriver"/>(UI Toolkit),
    /// 通过 <see cref="DrawObject(object, UnityEditor.EditorWindow, Action{object}, Action{object}, IPanelDriver)"/>
    /// 或 <see cref="Drawing.DefaultFactory"/> 选择。
    ///
    /// 约定:
    /// - Begin/End 必须成对; 引擎保证每一分支 toolkit 都配平(与改造前 IMGUI 的写法一致);
    /// - "不画"= 不调用, 不是画空盒子 —— 折叠 / 条件隐藏的子成员一律整棵子树都不进来,
    ///   因此保留模式的实现(UIToolkit)看到的就是一棵已经剪好的树;
    /// - 控件返回"用户当前输入的值", 是否真的变更由 <see cref="BeginCheck"/> / <see cref="EndCheck"/> 判定,
    ///   不靠引擎拿返回值去比 —— 两者的时机语义不同(后者会在每帧重入时误判)。
    /// </summary>
    public interface IPanelDriver
    {
        //-------------------------------------- 度量 --------------------------------------

        /// <summary>单行高度(表格高度 formula 与占位用)。</summary>
        float LineHeight { get; }

        /// <summary>横向滚动条厚度: 表格自己开滚动区时要把它的高度算进去。</summary>
        float ScrollbarHeight { get; }

        /// <summary>当前视图宽度: 分隔线要按它取一整行的矩形。</summary>
        float ViewWidth { get; }

        //-------------------------------------- 生命周期 --------------------------------------

        /// <summary>一次面板绘制开始。宿主/引擎会把后续所有绘制限定在这一次 Begin/End 之间。</summary>
        void BeginPanel();

        /// <summary>一次面板绘制结束; 后端在此提交自己攒下的树/状态。</summary>
        void EndPanel();

        /// <summary>请求宿主重绘。IMGUI 需要显式 Repaint; 保留模式(UIToolkit)自身会刷新, 实现为空即可。</summary>
        void RequestRepaint();

        //-------------------------------------- 容器 --------------------------------------

        /// <summary>开启垂直容器。<paramref name="boxed"/> 表示要画外框(背景) —— 对应分组的 Box/Title/Foldout。</summary>
        void BeginVertical(bool boxed, float width = 0f);

        void EndVertical();

        /// <summary>开启水平容器(一行): 成员"标签 + 控件 + 行内按钮"以及表格的一行。</summary>
        void BeginHorizontal(bool boxed = false);

        void EndHorizontal();

        /// <summary>嵌套对象的一级缩进。</summary>
        void BeginIndent();

        void EndIndent();

        /// <summary>开启滚动区。<paramref name="scroll"/> 是引擎侧持久化的滚动位置(存取宿主会话), 由这里更新。</summary>
        void BeginScroll(ref Vector2 scroll, float maxHeight);

        void EndScroll();

        /// <summary>固定空白(像素)。</summary>
        void Space(float pixels);

        /// <summary>吃掉剩余空间, 把后面的东西推到另一端。</summary>
        void FlexibleSpace();

        /// <summary>一条横线: <see cref="HorizontalLine"/> 的粗细与颜色由调用方给。</summary>
        void Separator(float thickness, Color color);

        //-------------------------------------- 状态栈 --------------------------------------

        /// <summary>之后绘制的内容按该颜色着色([GUIColor]); 必须配一个 <see cref="PopColor"/>。</summary>
        void PushColor(Color color);

        void PopColor();

        /// <summary>之后绘制的控件禁用交互(只读 / [DisableIf]); 必须配一个 <see cref="PopEnabled"/>。</summary>
        void PushEnabled(bool enabled);

        void PopEnabled();

        /// <summary>
        /// 之后控件的标签宽度([LabelWidth]; 表格单元格传 0)。传 null 表示"不变", 沿用外层 ——
        /// 没有 [LabelWidth] 的成员不能把上层设好的宽度顶掉。
        /// </summary>
        void PushLabelWidth(float? width);

        void PopLabelWidth();

        /// <summary>开始一段"用户有没有改过值"的观察。</summary>
        void BeginCheck();

        /// <summary>结束观察并返回这段区间内是否发生过变更。</summary>
        bool EndCheck();

        //-------------------------------------- 文本 --------------------------------------

        /// <summary>一行文本。<paramref name="text"/> 为空时占位但不显示内容。</summary>
        void Label(string text, in PanelTextSpec spec);

        /// <summary>"标签 + 值"两栏的一行: 左窄右宽(列表/字典的只读项与 (null) 占位)。</summary>
        void KeyValueLabel(string label, string value);

        /// <summary>提示框([Required] 校验与 [InfoBox])。</summary>
        void HelpBox(string message, PanelMessageKind kind);

        /// <summary>
        /// 进度条([ProgressBar]): <paramref name="value"/> 按 0~1 归一, <paramref name="height"/> 为 0 时用默认行高。
        /// </summary>
        void ProgressBar(float value, string title, float height);

        //-------------------------------------- 控件 --------------------------------------

        /// <summary>按钮。返回是否被点击。</summary>
        bool Button(string text, in PanelButtonSpec spec);

        /// <summary>折叠头: 返回用户期望的展开状态(引擎负责存进会话)。</summary>
        bool Foldout(bool expanded, string label);

        string TextField(string label, string value, float width = 0f);

        long IntegerField(string label, long value, float width = 0f);

        double FloatField(string label, double value, float width = 0f);

        /// <summary>滑杆: [MinValue] + [MaxValue] 同时存在且都给了数值时使用。</summary>
        float Slider(string label, float value, float min, float max, float width = 0f);

        /// <summary>带前缀标签的复选框(bool 成员)。</summary>
        bool ToggleField(string label, bool value, float width = 0f);

        /// <summary>下拉框([ValueDropdown] 与不带 [EnumToggleButtons] 的枚举)。返回选中的下标。</summary>
        int Popup(string label, int selected, string[] options, float width = 0f);

        /// <summary>[Flags] 枚举的多选控件, 返回当前位组合(后端不支持时返回原值)。</summary>
        Enum EnumFlagsField(string label, Enum value, float width = 0f);

        /// <summary>按钮组([EnumToggleButtons] 的枚举)。返回选中的下标。</summary>
        int Toolbar(int selected, string[] options, float width = 0f);

        /// <summary>Unity 对象引用。</summary>
        UnityEngine.Object ObjectField(string label, UnityEngine.Object value, Type type, float width = 0f);

        /// <summary>
        /// Unity 复合值类型的控件: Vector2/3/4、Vector2Int/3Int、Quaternion、Rect/RectInt、
        /// Bounds/BoundsInt、LayerMask、AnimationCurve、Gradient。
        /// 不支持的类型返回原值(与改造前一致)。
        /// </summary>
        object StructField(string label, object value, Type type, float width = 0f);
    }
}
