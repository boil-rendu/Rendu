using System;
using System.Collections.Generic;

namespace RD.Core
{
    public partial class UIInvokeSystem
    {
        public R InvokeReturn<T, R>(T self, string invokeType, List<R> returnList = null) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return default;
            }

            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IUIInvokeReturnHandler<R>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, R>(T self, string invokeType, T1 arg1, List<R> returnList = null) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return default;
            }

            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, R>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, T2, R>(T self, string invokeType, T1 arg1, T2 arg2, List<R> returnList = null) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return default;
            }

            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, T2, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, T2, R>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1, arg2);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1, arg2);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1, arg2);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, T2, T3, R>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, List<R> returnList = null) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return default;
            }

            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, T2, T3, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, T2, T3, R>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1, arg2, arg3);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1, arg2, arg3);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1, arg2, arg3);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, T2, T3, T4, R>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, List<R> returnList = null) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return default;
            }

            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, T2, T3, T4, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, T2, T3, T4, R>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1, arg2, arg3, arg4);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1, arg2, arg3, arg4);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1, arg2, arg3, arg4);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }

            return default;
        }

        public R InvokeReturn<T, T1, T2, T3, T4, T5, R>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, List<R> returnList = null) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return default;
            }

            returnList?.Clear();

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, T2, T3, T4, T5, R>>(invokeType);
                if (invoker == null) return default;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, T2, T3, T4, T5, R>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    var beforeResult = listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                    if (returnList != null)
                    {
                        returnList.Add(beforeResult);
                    }
                }

                var result = invoker.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                if (returnList != null)
                {
                    returnList.Add(result);
                }

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    var afterResult = listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                    if (returnList != null)
                    {
                        returnList.Add(afterResult);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }

            return default;
        }

        public async Promise InvokeTask<T>(T self, string invokeType) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<Promise>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeReturnHandler<Promise>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self);
                }

                await invoker.Invoke(self);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public async Promise InvokeTask<T, T1>(T self, string invokeType, T1 arg1) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, Promise>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, Promise>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1);
                }

                await invoker.Invoke(self, arg1);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public async Promise InvokeTask<T, T1, T2>(T self, string invokeType, T1 arg1, T2 arg2) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, T2, Promise>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, T2, Promise>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2);
                }

                await invoker.Invoke(self, arg1, arg2);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public async Promise InvokeTask<T, T1, T2, T3>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, T2, T3, Promise>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, T2, T3, Promise>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3);
                }

                await invoker.Invoke(self, arg1, arg2, arg3);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public async Promise InvokeTask<T, T1, T2, T3, T4>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, T2, T3, T4, Promise>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, T2, T3, T4, Promise>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3, arg4);
                }

                await invoker.Invoke(self, arg1, arg2, arg3, arg4);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3, arg4);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }

        public async Promise InvokeTask<T, T1, T2, T3, T4, T5>(T self, string invokeType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) where T : Entity
        {
            if (self == null || self.IsDisposed)
            {
                Log.Error($"self is null or disposed,{invokeType}");
                return;
            }

            try
            {
                var invoker = GetInvoker<IUIInvokeReturnHandler<T1, T2, T3, T4, T5, Promise>>(invokeType);
                if (invoker == null) return;

                using var list = ListComponent<IUIInvokeReturnHandler<T1, T2, T3, T4, T5, Promise>>.Create();
                UIListenerInvokeSystem.Instance.GetListenerInvokerBefore(invokeType, list);

                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                }

                await invoker.Invoke(self, arg1, arg2, arg3, arg4, arg5);

                UIListenerInvokeSystem.Instance.GetListenerInvokerAfter(invokeType, list);
                foreach (var listener in list)
                {
                    await listener.Invoke(self, arg1, arg2, arg3, arg4, arg5);
                }
            }
            catch (Exception e)
            {
                Log.Error($"UIInvoke执行错误请检查{self.GetType().Name} >> {invokeType} 类型:{typeof(T).Name}, {e}");
            }
        }
    }
}