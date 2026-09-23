namespace RD.Core
{
    public abstract class UIInvokeCommonHandler<T, P1, P2, P3, P4, P5, R> :
            IUIInvokeHandler,
            IUIInvokeHandler<P1>,
            IUIInvokeHandler<P1, P2>,
            IUIInvokeHandler<P1, P2, P3>,
            IUIInvokeHandler<P1, P2, P3, P4>,
            IUIInvokeHandler<P1, P2, P3, P4, P5>,
            IUIInvokeReturnHandler<R>,
            IUIInvokeReturnHandler<P1, R>,
            IUIInvokeReturnHandler<P1, P2, R>,
            IUIInvokeReturnHandler<P1, P2, P3, R>,
            IUIInvokeReturnHandler<P1, P2, P3, P4, R>,
            IUIInvokeReturnHandler<P1, P2, P3, P4, P5, R>
            where T : Entity
    {
        public string InvokeType { get; set; }

        #region 无返回值

        void IUIInvokeHandler.Invoke(Entity self)
        {
            this.InvokeParams(self);
        }

        void IUIInvokeHandler<P1>.Invoke(Entity self, P1 p1)
        {
            this.InvokeParams(self, p1);
        }

        void IUIInvokeHandler<P1, P2>.Invoke(Entity self, P1 p1, P2 p2)
        {
            this.InvokeParams(self, p1, p2);
        }

        void IUIInvokeHandler<P1, P2, P3>.Invoke(Entity self, P1 p1, P2 p2, P3 p3)
        {
            this.InvokeParams(self, p1, p2, p3);
        }

        void IUIInvokeHandler<P1, P2, P3, P4>.Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            this.InvokeParams(self, p1, p2, p3, p4);
        }

        void IUIInvokeHandler<P1, P2, P3, P4, P5>.Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            this.InvokeParams(self, p1, p2, p3, p4, p5);
        }

        protected abstract void InvokeParams(Entity self, params object[] paramVo);

        #endregion

        #region 有返回值

        R IUIInvokeReturnHandler<R>.Invoke(Entity self)
        {
            return this.InvokeReturnParams(self);
        }

        R IUIInvokeReturnHandler<P1, R>.Invoke(Entity self, P1 p1)
        {
            return this.InvokeReturnParams(self, p1);
        }

        R IUIInvokeReturnHandler<P1, P2, R>.Invoke(Entity self, P1 p1, P2 p2)
        {
            return this.InvokeReturnParams(self, p1, p2);
        }

        R IUIInvokeReturnHandler<P1, P2, P3, R>.Invoke(Entity self, P1 p1, P2 p2, P3 p3)
        {
            return this.InvokeReturnParams(self, p1, p2, p3);
        }

        R IUIInvokeReturnHandler<P1, P2, P3, P4, R>.Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            return this.InvokeReturnParams(self, p1, p2, p3, p4);
        }

        R IUIInvokeReturnHandler<P1, P2, P3, P4, P5, R>.Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            return this.InvokeReturnParams(self, p1, p2, p3, p4, p5);
        }

        protected abstract R InvokeReturnParams(Entity self, params object[] paramVo);

        #endregion
    }
}