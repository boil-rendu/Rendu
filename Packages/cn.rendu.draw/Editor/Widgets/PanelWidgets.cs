using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>表头横条: 对应 <c>IDriverPanel.PanelHeader</c>。</summary>
    internal sealed class PanelHeaderWidget : Widget
    {
        public string    Text;
        public DrawAlign Align;
        public bool      Bold;

        public override void Reset()
        {
            Text  = null;
            Align = default;
            Bold  = false;
        }
    }

    /// <summary>表格列竖分隔线: 对应 <c>IDriverPanel.ColumnSeparator</c>。</summary>
    internal sealed class ColumnSeparatorWidget : Widget
    {
        public float Height;   // >0 定高, <=0 撑满行

        public override void Reset()
        {
            Height = 0f;
        }
    }

    /// <summary>可拖拽列边界: 对应 <c>IDriverPanel.ColumnResizer</c>。</summary>
    internal sealed class ColumnResizerWidget : Widget
    {
        public float Height;
        public float Delta;   // 输出: 解释器回填本帧横向拖拽增量

        public override void Reset()
        {
            Height = 0f;
            Delta  = 0f;
        }
    }

    /// <summary>列表条目卡片(包裹节点): 对应 <c>BeginCard/EndCard</c>。</summary>
    internal sealed class CardWidget : Widget
    {
    }

    /// <summary>
    /// 拖拽排序(包裹列表子树): 对应 <c>IDriverReorder.BeginReorder/PollReorder</c>。
    /// <see cref="ListId"/> 是引擎给的稳定标识。每个条目内再挂 <see cref="ReorderHandleWidget"/>。
    /// </summary>
    internal sealed class ReorderWidget : Widget
    {
        public int         ListId;
        public ReorderDrag Result = ReorderDrag.None;   // 解释器回填, 产树方消费(挪数据)

        // Result 的初值是字段初始化器: 池化实例不跑构造, 必须在这里补回 None ——
        // default(ReorderDrag) 的 Source 是 0, 会被当成"正在拖第 0 条"。
        public override void Reset()
        {
            ListId = 0;
            Result = ReorderDrag.None;
        }
    }

    /// <summary>
    /// 拖拽列表里的**一个条目**(包裹节点): 对应 <c>BeginReorder</c> 之后每个条目收尾时的
    /// <c>EndReorderItem</c>。条目区间要在子节点渲染完之后才量得到, 所以它必须是一个节点 ——
    /// 产树方没法在"建完"的那一刻调用后端。
    /// </summary>
    internal sealed class ReorderItemWidget : Widget
    {
        public int ListId;
        public int Index;

        public override void Reset()
        {
            ListId = 0;
            Index  = 0;
        }
    }

    /// <summary>拖拽手柄: 对应 <c>IDriverReorder.ReorderHandle</c>。</summary>
    internal sealed class ReorderHandleWidget : Widget
    {
        public int    ListId;
        public int    Index;
        public float  Width;
        public string Ghost;

        public override void Reset()
        {
            ListId = 0;
            Index  = 0;
            Width  = 0f;
            Ghost  = null;
        }
    }

    /// <summary>
    /// 外观作用域(包裹节点): 对应 <c>PushColor/PushEnabled/PushLabelWidth</c> 的一段子树。
    /// 字段为 null 表示"不改这一项", 渲染器进入时压栈、离开时弹栈 —— 与现在的词法压弹同义。
    ///
    /// 三个字段是互斥的(每次只设一个), 所以池化复用**必须**把另外两个清回 null。
    /// </summary>
    internal sealed class StyledScopeWidget : Widget
    {
        public Color? Color;        // null = 不变
        public bool?  Enabled;      // null = 不变
        public float? LabelWidth;   // null = 不变

        public override void Reset()
        {
            Color      = null;
            Enabled    = null;
            LabelWidth = null;
        }
    }
}
