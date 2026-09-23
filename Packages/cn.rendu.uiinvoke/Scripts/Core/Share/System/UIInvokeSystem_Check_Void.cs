namespace RD.Core
{
    public partial class UIInvokeSystem
    {
        public bool CheckInvoke(string invokeType)
        {
            var invoker = GetInvoker<IUIInvokeHandler>(invokeType);
            return invoker != null;
        }

        public bool CheckInvoke<T1>(string invokeType)
        {
            var invoker = GetInvoker<IUIInvokeHandler<T1>>(invokeType, false);
            return invoker != null;
        }

        public bool CheckInvoke<T1, T2>(string invokeType)
        {
            var invoker = GetInvoker<IUIInvokeHandler<T1, T2>>(invokeType, false);
            return invoker != null;
        }

        public bool CheckInvoke<T1, T2, T3>(string invokeType)
        {
            var invoker = GetInvoker<IUIInvokeHandler<T1, T2, T3>>(invokeType, false);
            return invoker != null;
        }

        public bool CheckInvoke<T1, T2, T3, T4>(string invokeType)
        {
            var invoker = GetInvoker<IUIInvokeHandler<T1, T2, T3, T4>>(invokeType, false);
            return invoker != null;
        }

        public bool CheckInvoke<T1, T2, T3, T4, T5>(string invokeType)
        {
            var invoker = GetInvoker<IUIInvokeHandler<T1, T2, T3, T4, T5>>(invokeType, false);
            return invoker != null;
        }
    }
}