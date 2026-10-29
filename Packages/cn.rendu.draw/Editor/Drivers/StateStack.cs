using System.Collections.Generic;
using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 后端的外观/交互状态栈: 颜色、启用态、标签列宽。
    ///
    /// 它单独成类, 是因为这三种状态**不是只服务于某一个控件**, 而是被两处同时需要:
    ///   - <see cref="Styler"/> 在复用每个元素时按栈顶重刷外观(元素按位置复用, 不重刷就会残留上一次的值);
    ///   - 标签列宽还要被文本绘制路径直接读到(标题/表格头那一列要对齐)。
    /// 留在 <see cref="Driver"/> 里就只能靠共享私有字段互访, 两边谁也说不清对方在什么时候读。
    /// </summary>
    internal sealed class StateStack 
    {
        private readonly Stack<Color>  m_Colors  = new();
        private readonly Stack<bool>   m_Enabled = new();
        private readonly Stack<float?> m_Labels  = new();

        //-------------------------------------- 颜色 --------------------------------------

        public void PushColor(Color color) => m_Colors.Push(color);

        public void PopColor()
        {
            if (m_Colors.Count > 0) m_Colors.Pop();
        }

        /// <summary>当前生效的颜色; null = 不在任何一个 PushColor 区间内, 调用方须显式清回默认。</summary>
        public Color? CurrentColor => m_Colors.Count > 0 ? m_Colors.Peek() : null;

        //-------------------------------------- 启用态 ------------------------------------

        public void PushEnabled(bool enabled) => m_Enabled.Push(enabled);

        public void PopEnabled()
        {
            if (m_Enabled.Count > 0) m_Enabled.Pop();
        }

        /// <summary>当前生效的启用态; 不在任何区间内时为 true(须显式启用, 否则复用的元素会永久禁用)。</summary>
        public bool CurrentEnabled => m_Enabled.Count > 0 ? m_Enabled.Peek() : true;

        //-------------------------------------- 标签列宽 ------------------------------------

        public void PushLabelWidth(float? width)
        {
            m_Labels.Push(LabelWidth);
            if (width.HasValue) LabelWidth = width.Value;
        }

        public void PopLabelWidth()
        {
            LabelWidth = m_Labels.Count > 0 ? m_Labels.Pop() : null;
        }

        /// <summary>当前生效的标签列宽; null = 不在任何区间内, 由调用方取默认或清回默认。</summary>
        public float? LabelWidth { get; private set; }

        /// <summary>
        /// 归位: 上一轮若被异常打断(引擎只在最外层 catch), 三个栈会留着半截, 之后整块面板都会
        /// 用错色/错禁用态, 而且再也回不来。每轮开头必须清干净 —— 最坏只影响这一轮的余下部分。
        /// </summary>
        public void Reset()
        {
            m_Colors.Clear();
            m_Enabled.Clear();
            m_Labels.Clear();
            LabelWidth = null;
        }
    }
}
