using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD.Core
{
    [CodeProcess]
    public partial class UIInvokeSystem : Singleton<UIInvokeSystem>, ISingletonAwake
    {
        private readonly Dictionary<string, IUIInvokeBaseHandler> m_AllInvokers = new();

        public void Awake()
        {
            CodeTypes codeTypes = CodeTypes.Instance;

            foreach (Type type in codeTypes.GetTypes(typeof(UIInvokeSystemAttribute)))
            {
                var obj = Activator.CreateInstance(type);
                if (obj is not IUIInvokeBaseHandler handler)
                {
                    Log.Error($"类型{type.Name}不是IUIInvokeBaseHandler的实现");
                    continue;
                }

                var attribute = (UIInvokeSystemAttribute)type.GetCustomAttribute(typeof(UIInvokeSystemAttribute), true);
                var invokeType = attribute.InvokeType;
                handler.InvokeType = invokeType;

                if (!m_AllInvokers.TryAdd(invokeType, handler))
                {
                    Log.Error($"重复添加UIInvoke请检查\ntype:{type.Name}\ninvokeType:{invokeType}");
                }
            }
        }

        private T GetInvoker<T>(string invokeType, bool isThrowError = true)
        {
            if (m_AllInvokers.TryGetValue(invokeType, out var invoker))
            {
                if (invoker is T tInvoker)
                {
                    return tInvoker;
                }

                if (isThrowError)
                {
                    Log.Error($"找到UIInvoke实现请 但类型不一致请检查 {invokeType}  需求:{typeof(T).Name} 实际:{invoker.GetType().Name}");
                }

                return default;
            }

            if (isThrowError)
            {
                Log.Error($"未找到UIInvoke实现请检查 {invokeType}  类型:{typeof(T).Name}");
            }

            return default;
        }
    }
}