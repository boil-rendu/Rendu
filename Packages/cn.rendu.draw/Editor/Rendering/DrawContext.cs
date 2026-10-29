using System;
using System.Collections.Generic;
using RD.Draw.Editor.Tables;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 一次绘制的上下文: 绘制后端、状态会话、变更回调, 以及绘制过程中随位置变化的状态。
    ///
    /// 以前这些都挂在 Engine 的静态字段上(进入时存、退出时还原):
    ///   - 后端 / 会话 / 回调: 靠"手工 save/restore"保证嵌套调用不出错;
    ///   - "当前在表格单元格里吗""单元格按钮该占多宽": 靠全局开关硬传, 嵌套内容还得手工还原。
    /// 改成对象之后:
    ///   - 上下文随绘制调用往下传, 引擎里不再有进程级的可变状态;
    ///   - 嵌套绘制(列表项里的子表格)天然是另一个上下文, 不必手工还原;
    ///   - 可测试: 造一个上下文就能单独驱动渲染逻辑。
    ///
    /// 位置相关的状态(单元格上下文)走**栈**: 进入时压栈, 退出时弹栈,
    /// 于是"嵌套单元格"与"嵌套内容不继承外层"这两种情形都由结构保证, 不靠人记着手工还原。
    ///
    /// 现在只有一个后端(<see cref="Driver"/>, 具体类), 所以不再有"按能力降级"的视图
    /// (度量 / 状态栈 / 面板装饰 / 排序 / 诊断都直接走 <see cref="Driver"/> 上的 virtual 原语),
    /// 这里只持有一个后端实例。
    /// </summary>
    internal sealed class DrawContext
    {
        /// <summary>表格单元格作用域栈: 栈顶即"当前所在的单元格"。</summary>
        private readonly List<float> m_CellButtonShares = new();

        private DrawContext(Driver driver, HostSession session, Action<object> beforeChange, Action<object> changed,
                            bool isPlaying)
        {
            Driver       = driver;
            Session      = session;
            BeforeChange = beforeChange;
            Changed      = changed;
            IsPlaying    = isPlaying;
            Renderer     = new Renderer();
        }

        /// <summary>
        /// 构造一次绘制的上下文。
        /// </summary>
        public static DrawContext Create(Driver driver, HostSession session, Action<object> beforeChange,
                                         Action<object> changed, bool isPlaying = false)
        {
            if (driver == null) throw new ArgumentNullException(nameof(driver));
            return new DrawContext(driver, session, beforeChange, changed, isPlaying);
        }

        /// <summary>本次绘制使用的后端(所有画的地方都经由它, 不直接碰任何本地 UI API)。</summary>
        public Driver Driver { get; }

        /// <summary>本次绘制的树解释器: 把规则层产出的 <see cref="Widget"/> 树翻译成后端原语。</summary>
        public Renderer Renderer { get; }

        /// <summary>状态会话: 折叠状态、滚动位置、分页都存在这里。</summary>
        public HostSession Session { get; }

        /// <summary>数据写入前回调(宿主在此刻拍 Undo 快照)。</summary>
        public Action<object> BeforeChange { get; }

        /// <summary>数据已被修改后的回调(宿主标记脏 / 请求重绘)。</summary>
        public Action<object> Changed { get; }

        /// <summary>
        /// 本次绘制所处的播放态。
        ///
        /// 它是**入参**而不是一个全局开关: [VisibleIf] 之类条件在编辑器里用 isPlaying 判分支,
        /// 播放与否只有宿主知道。宿主在调用 DrawObject 时直接传进来, 测试也能自由指定。
        /// </summary>
        public bool IsPlaying { get; }

        /// <summary>
        /// 本次绘制在**根节点**量到的可用宽; 0 = 还没量到(Layout 事件里量不到真值)。
        /// 表格按它均分列宽(见 <see cref="TableRenderer"/> 的表格部分)。
        /// </summary>
        public float RootWidth { get; set; }

        /// <summary>本次绘制的临时列宽缓冲(表格列宽分发用), 见 <see cref="ColumnWidthScratch"/>。</summary>
        private float[] m_ColumnWidthScratch;

        /// <summary>
        /// 借一块本次绘制内复用的浮点缓冲。
        ///
        /// 列数是常数级(个位到十几), 但一张面板里通常有好几张表; 挂在上下文上才是真的按绘制复用。
        /// 缓冲只增不减, 使用方负责整段覆盖。嵌套表格是安全的: 分发在把缓冲读完之前就结束, 内层再借会整段重写。
        /// </summary>
        internal float[] ColumnWidthScratch(int count)
        {
            if (m_ColumnWidthScratch == null || m_ColumnWidthScratch.Length < count)
            {
                m_ColumnWidthScratch = new float[count];
            }

            return m_ColumnWidthScratch;
        }

        /// <summary>当前是否在表格单元格内(任意嵌套层级)。</summary>
        public bool InTableCell => m_CellButtonShares.Count > 0;

        /// <summary>
        /// 单元格按钮的宽度上下文:
        ///   0  = 不在单元格内(紧凑自然宽);
        ///  &gt;0 = 按钮组内平分的共享宽(几个同组按钮各占一份);
        ///  &lt;0 = 独立按钮, 撑满所在列宽。
        /// </summary>
        public float CellButtonShare => m_CellButtonShares.Count > 0 ? m_CellButtonShares[m_CellButtonShares.Count - 1] : 0f;

        /// <summary>
        /// 进入一个表格单元格作用域, 返回的句柄释放时自动退出。
        ///
        /// 用 using 而不是"手工置位 + finally 还原": 嵌套绘制(单元格里的子表格)时,
        /// 退出顺序由栈保证, 不依赖调用点记得还原。
        /// </summary>
        /// <param name="buttonShare">该单元格内按钮的宽度上下文(见 <see cref="CellButtonShare"/>)。</param>
        public CellScope EnterTableCell(float buttonShare = 0f)
        {
            m_CellButtonShares.Add(buttonShare);
            return new CellScope(this);
        }

        internal void ExitTableCell()
        {
            if (m_CellButtonShares.Count > 0) m_CellButtonShares.RemoveAt(m_CellButtonShares.Count - 1);
        }

        /// <summary>
        /// 执行一次可能修改数据的操作, 并把"变更前 / 变更后"通知给宿主。
        /// 引擎里所有写数据的地方(成员赋值、集合增删改、按钮方法调用)都走这里,
        /// 这样宿主(UnityEditor.Editor 宿主)才能统一做 Undo 快照与标记脏。
        /// </summary>
        public void Mutate(object owner, Action action)
        {
            BeforeChange?.Invoke(owner);
            action();
            Changed?.Invoke(owner);
        }
    }

    /// <summary>
    /// 表格单元格作用域句柄: 用 using 结束, 不必手工 save/restore。
    /// </summary>
    internal readonly struct CellScope : IDisposable
    {
        private readonly DrawContext m_Ctx;

        internal CellScope(DrawContext ctx)
        {
            m_Ctx = ctx;
        }

        public void Dispose()
        {
            m_Ctx.ExitTableCell();
        }
    }

    /// <summary>
    /// 只按引用比较对象身份, 不受业务类型重写 Equals/GetHashCode 的影响。
    /// 绘制环检测若用默认比较器, 会把"两个 Equals 判定相等、但并非同一个"的实例误判为
    /// 同一个对象正在绘制, 从而整块跳过绘制。
    /// </summary>
    internal sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceComparer Instance = new();

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
