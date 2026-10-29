using UnityEngine;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// 标签与文案 —— 成员显示名、计数文案、颜色解析的**契约**。
    ///
    /// 这些规则是纯函数(输入成员/参数, 输出字符串或颜色), 本身不持有状态;
    /// 但做成接口而不是静态工具类, 是为了让它可以**被替换**:
    /// 宿主可能想用自己的一套命名规则(比如统一加前缀、或把某些字段名翻成业务名),
    /// 换实现不需要改引擎源码(依赖倒置)。
    /// </summary>
    internal interface ILabelFormatter
    {
        /// <summary>字段/属性的显示名(三级回退, 见 <see cref="DefaultLabelFormatter.GetLabel"/>)。</summary>
        string GetLabel(CachedMember m);

        /// <summary>成员标签, [HideLabel] 时为空串。</summary>
        string LabelOrEmpty(CachedMember m);

        /// <summary>值控件实际要用的标签(表格单元格里恒为空)。</summary>
        string LabelForControl(CachedMember m, bool inTableCell);

        /// <summary>容器头部(列表/字典/集合/表格)的"名字 + 计数"文案。</summary>
        string CountedLabel(CachedMember m, int count);

        /// <summary>纯计数文案: 0 → "Empty", 否则 "N items"。</summary>
        string CountedText(int count);

        /// <summary>非折叠的纯文本头行(只有一行 bold 文字, 没有箭头)该不该画。</summary>
        bool ShouldDrawLabel(CachedMember m);

        /// <summary>把成员名拆成词: myValue → "my Value"。</summary>
        string FriendlyName(string name);

        /// <summary>[GUIColor] 的颜色(支持 "r,g,b[,a]" 字符串或 rgba 分量)。</summary>
        Color? ResolveGUIColor(CachedMember m);

        /// <summary>[Title]/[TitleGroup] 的对齐枚举 → 绘制层的水平对齐。</summary>
        DrawAlign TitleAnchor(TitleAlignment alignment);
    }
}
