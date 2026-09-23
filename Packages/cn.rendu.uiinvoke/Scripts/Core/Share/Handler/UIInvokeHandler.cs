namespace RD.Core
{
    public interface IUIInvokeBaseHandler
    {
        string InvokeType { get; set; }
    }

    public interface IUIInvokeHandler : IUIInvokeBaseHandler
    {
        void Invoke(Entity self);
    }

    public interface IUIInvokeHandler<in P1> : IUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1);
    }

    public interface IUIInvokeHandler<in P1, in P2> : IUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1, P2 p2);
    }

    public interface IUIInvokeHandler<in P1, in P2, in P3> : IUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1, P2 p2, P3 p3);
    }

    public interface IUIInvokeHandler<in P1, in P2, in P3, in P4> : IUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public interface IUIInvokeHandler<in P1, in P2, in P3, in P4, in P5> : IUIInvokeBaseHandler
    {
        void Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }

    public abstract class UIInvokeHandler<T> : SystemObject, IUIInvokeHandler where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self)
        {
            Invoke((T)self);
        }

        protected abstract void Invoke(T self);
    }

    public abstract class UIInvokeHandler<T, P1> : SystemObject, IUIInvokeHandler<P1> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1)
        {
            Invoke((T)self, p1);
        }

        protected abstract void Invoke(T self, P1 p1);
    }

    public abstract class UIInvokeHandler<T, P1, P2> : SystemObject, IUIInvokeHandler<P1, P2> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1, P2 p2)
        {
            Invoke((T)self, p1, p2);
        }

        protected abstract void Invoke(T self, P1 p1, P2 p2);
    }

    public abstract class UIInvokeHandler<T, P1, P2, P3> : SystemObject, IUIInvokeHandler<P1, P2, P3> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1, P2 p2, P3 p3)
        {
            Invoke((T)self, p1, p2, p3);
        }

        protected abstract void Invoke(T self, P1 p1, P2 p2, P3 p3);
    }

    public abstract class UIInvokeHandler<T, P1, P2, P3, P4> : SystemObject, IUIInvokeHandler<P1, P2, P3, P4> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            Invoke((T)self, p1, p2, p3, p4);
        }

        protected abstract void Invoke(T self, P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public abstract class UIInvokeHandler<T, P1, P2, P3, P4, P5> : SystemObject, IUIInvokeHandler<P1, P2, P3, P4, P5> where T : Entity
    {
        public string InvokeType { get; set; }

        public void Invoke(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            Invoke((T)self, p1, p2, p3, p4, p5);
        }

        protected abstract void Invoke(T self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}