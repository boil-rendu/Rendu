using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Server
{
    [CodeProcess]
    public class ConsoleDispatcher: Singleton<ConsoleDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<string, IConsoleHandler> handlers = new();
        
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof (ConsoleHandlerAttribute));

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(ConsoleHandlerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                ConsoleHandlerAttribute consoleHandlerAttribute = (ConsoleHandlerAttribute)attrs[0];

                object obj = Activator.CreateInstance(type);

                IConsoleHandler iConsoleHandler = obj as IConsoleHandler;
                if (iConsoleHandler == null)
                {
                    throw new Exception($"ConsoleHandler handler not inherit IConsoleHandler class: {obj.GetType().FullName}");
                }
                this.handlers.Add(consoleHandlerAttribute.Mode, iConsoleHandler);
            }
        }

        /// <summary>
        /// 取指定模式的处理器，未注册时返回 null。
        /// 原实现直接索引，模式不存在会抛 KeyNotFoundException，而调用方的异常又被门控日志吞掉，
        /// 表现为"敲了命令完全没反应"，故改为返回 null，由调用方明确提示。
        /// </summary>
        public IConsoleHandler Get(string key)
        {
            return this.handlers.TryGetValue(key, out IConsoleHandler handler) ? handler : null;
        }
    }
}