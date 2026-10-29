using System;

namespace RD.Draw
{
    /// <summary>
    /// 分组的渲染容器类型。由 <see cref="PropertyGroupAttribute.Kind"/> 各特性自行声明
    /// (BoxGroup→Box、FoldoutGroup→Foldout…), 引擎侧只读不做按类型 switch。
    /// 根节点(未分组成员)用 None。
    /// </summary>
    public enum GroupKind
    {
        None,
        Box,
        Foldout,
        Vertical,
        Horizontal,
        Title,
        ButtonGroup,
    }
}
