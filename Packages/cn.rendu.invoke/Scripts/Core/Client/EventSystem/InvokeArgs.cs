namespace RD.Core
{
    /// <summary>
    /// Invoke 统一参数容器（0 参）。
    /// <para>
    /// 原先 UI invoke 用 <c>IUIInvokeHandler&lt;P1..P5&gt;</c> 一整套接口族承载「0~5 个入参」，
    /// 现在统一并入 core 的 struct 参数派发：由源生成器按方法签名选一个 <see cref="InvokeArgs"/>，
    /// handler 直接继承 <see cref="AInvokeEntityHandler{A}"/> / <see cref="AInvokeEntityHandler{A,T}"/>，
    /// 派发表与 Invoke / InvokeEntity 共用一套（按 argsType + long id 索引），不再有第二套注册表。
    /// </para>
    /// </summary>
    public struct InvokeArgs
    {
    }

    public struct InvokeArgs<P1>
    {
        public P1 Item1;

        public InvokeArgs(P1 item1)
        {
            this.Item1 = item1;
        }
    }

    public struct InvokeArgs<P1, P2>
    {
        public P1 Item1;
        public P2 Item2;

        public InvokeArgs(P1 item1, P2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }

    public struct InvokeArgs<P1, P2, P3>
    {
        public P1 Item1;
        public P2 Item2;
        public P3 Item3;

        public InvokeArgs(P1 item1, P2 item2, P3 item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }
    }

    public struct InvokeArgs<P1, P2, P3, P4>
    {
        public P1 Item1;
        public P2 Item2;
        public P3 Item3;
        public P4 Item4;

        public InvokeArgs(P1 item1, P2 item2, P3 item3, P4 item4)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
        }
    }

    public struct InvokeArgs<P1, P2, P3, P4, P5>
    {
        public P1 Item1;
        public P2 Item2;
        public P3 Item3;
        public P4 Item4;
        public P5 Item5;

        public InvokeArgs(P1 item1, P2 item2, P3 item3, P4 item4, P5 item5)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
        }
    }
}
