using System;

namespace RD.Core
{
    public partial class UIInvokeSystem
    {
        public void Invoke<T>(T self, string invokeType) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeHandler>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeHandler>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    listener.Invoke(self);
                }

                invoker.Invoke(self);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    listener.Invoke(self);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public void Invoke<T, T1>(T self, string invokeType, T1 arg1) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeHandler<T1>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeHandler<T1>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1);
                }

                invoker.Invoke(self, arg1);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public void Invoke<T, T1, T2>(T self, string invokeType, T1 arg1, T2 arg2) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeHandler<T1, T2>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeHandler<T1, T2>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1, arg2);
                }

                invoker.Invoke(self, arg1, arg2);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1, arg2);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public void Invoke<T, T1, T2, T3>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeHandler<T1, T2, T3>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeHandler<T1, T2, T3>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1, arg2, arg3);
                }

                invoker.Invoke(self, arg1, arg2, arg3);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1, arg2, arg3);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public void Invoke<T, T1, T2, T3, T4>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeHandler<T1, T2, T3, T4>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeHandler<T1, T2, T3, T4>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1, arg2, arg3, arg4);
                }

                invoker.Invoke(self, arg1, arg2, arg3, arg4);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1, arg2, arg3, arg4);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public void Invoke<T, T1, T2, T3, T4, T5>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeHandler<T1, T2, T3, T4, T5>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeHandler<T1, T2, T3, T4, T5>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                }

                invoker.Invoke(self, arg1, arg2, arg3, arg4, arg5);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }
    }
}