using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>垂直容器: 对应 <c>BeginVertical/EndVertical</c>。</summary>
    internal sealed class ColumnWidget : Widget
    {
        public bool  Boxed;
        public float Width;
        public bool  ExpandHeight;
        public bool  ExpandWidth;

        public override void Reset()
        {
            Boxed        = false;
            Width        = 0f;
            ExpandHeight = false;
            ExpandWidth  = false;
        }
    }

    /// <summary>水平容器(一行): 对应 <c>BeginHorizontal/EndHorizontal</c>。boxed = 表头行。</summary>
    internal sealed class RowWidget : Widget
    {
        public bool Boxed;

        public override void Reset()
        {
            Boxed = false;
        }
    }

    /// <summary>一级缩进: 对应 <c>BeginIndent/EndIndent</c>。</summary>
    internal sealed class IndentWidget : Widget
    {
    }

    /// <summary>
    /// 滚动区: 对应 <c>BeginScroll/EndScroll</c>。<see cref="Scroll"/> 由会话持久化。
    ///
    /// 三个钩子是"产树与解释分离"里唯一必须在**渲染途中**跑的逻辑: 表格要量滚动区视口宽,
    /// 而那个矩形只有在 <c>BeginScroll</c> 之后才存在。建树发生在渲染之前, 量不到 ——
    /// 所以把这一刻借给产树方, 而不是让它去直接调后端。
    ///
    /// 池化注意: 钩子闭包会捕获宿主对象, 复用前必须清空(见 <see cref="Reset"/>)。
    /// </summary>
    internal sealed class ScrollWidget : Widget
    {
        public Vector2 Scroll;
        public float   MaxHeight;

        /// <summary>进入滚动区**之前**(量当前纵向位置这类探测)。</summary>
        public System.Action<Rendering.RenderContext> BeforeBegin;

        /// <summary>进入滚动区**之后**、渲染子节点之前(量视口宽)。</summary>
        public System.Action<Rendering.RenderContext> AfterBegin;

        /// <summary>离开滚动区之后(把滚动位置写回会话)。</summary>
        public System.Action<Vector2> End;

        public override void Reset()
        {
            Scroll      = default;
            MaxHeight   = 0f;
            BeforeBegin = null;
            AfterBegin  = null;
            End         = null;
        }
    }

    /// <summary>固定空白(像素): 对应 <c>Space</c>。</summary>
    internal sealed class SpaceWidget : Widget
    {
        public float Pixels;

        public override void Reset()
        {
            Pixels = 0f;
        }
    }

    /// <summary>吃掉剩余空间: 对应 <c>FlexibleSpace</c>。</summary>
    internal sealed class FlexibleSpaceWidget : Widget
    {
    }

    /// <summary>一条横线: 对应 <c>Separator</c>。</summary>
    internal sealed class SeparatorWidget : Widget
    {
        public float Thickness;
        public Color Color;

        public override void Reset()
        {
            Thickness = 0f;
            Color     = default;
        }
    }
}
