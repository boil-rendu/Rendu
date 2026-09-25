using System;

namespace RD.Core
{
    /// <summary>
    /// InvokeEntity事件
    /// </summary>
    public partial class EventSystem
    {
        public void InvokeEntity<A>(Entity entity, long type, A args) where A : struct
        {
            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error1: {type} {typeof(A).FullName}");
            }

            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error2: {type} {typeof(A).FullName}");
            }

            var aInvokeHandler = invokeHandler as AInvokeEntityHandler<A>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error3, not AInvokeHandler: {type} {typeof(A).FullName}");
            }

            aInvokeHandler.Handle(entity, args);
        }

        public T InvokeEntity<A, T>(Entity entity, long type, A args) where A : struct
        {
            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error4: {type} {typeof(A).FullName}");
            }

            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error5: {type} {typeof(A).FullName}");
            }

            var aInvokeHandler = invokeHandler as AInvokeEntityHandler<A, T>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error6, not AInvokeHandler: {type} {typeof(A).FullName} {typeof(T).FullName} ");
            }

            return aInvokeHandler.Handle(entity, args);
        }

        public void InvokeEntity<A>(Entity entity, A args) where A : struct
        {
            InvokeEntity(entity, 0, args);
        }

        public T InvokeEntity<A, T>(Entity entity, A args) where A : struct
        {
            return InvokeEntity<A, T>(entity, 0, args);
        }

        /// <summary>
        /// 与 <see cref="InvokeEntity{A}(Entity, long, A)"/> 对应但不抛异常：没有实现时返回 false。
        /// UI 派发链路（UIEventHandle / UITaskEventHandle）统一走这里，避免缺实现时抛异常打断一帧。
        /// </summary>
        public bool TryInvokeEntity<A>(Entity entity, long type, A args) where A : struct
        {
            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                return false;
            }

            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                return false;
            }

            if (invokeHandler is not AInvokeEntityHandler<A> aInvokeHandler)
            {
                return false;
            }

            aInvokeHandler.Handle(entity, args);
            return true;
        }

        public bool TryInvokeEntity<A, T>(Entity entity, long type, A args, out T result) where A : struct
        {
            result = default;

            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                return false;
            }

            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                return false;
            }

            if (invokeHandler is not AInvokeEntityHandler<A, T> aInvokeHandler)
            {
                return false;
            }

            result = aInvokeHandler.Handle(entity, args);
            return true;
        }
    }
}
