using System.Collections.Generic;
using System.Reflection;
using System;

namespace RD.Core
{
    /// <summary>
    /// 运行时类型表：从一组 <c>Assembly</c> 中按全名索引所有 <see cref="Type"/>，并按 <see cref="BaseAttribute"/> 派生特性（<see cref="EventAttribute"/>、<see cref="EntitySystemAttribute"/>、<see cref="CodeProcessAttribute"/> 等）做反向索引。
    /// <see cref="EventSystem"/> 等带 <c>[CodeProcess]</c> 标注的单例在 <see cref="CodeProcess"/> 阶段调用 <c>CodeTypes.GetTypes(typeof(EventAttribute))</c> 取得全部 handler 反射构造。
    /// </summary>
    public class CodeTypes: Singleton<CodeTypes>, ISingletonAwake<Assembly[]>
    {
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types = new();

        /// <summary>扫描传入的 <paramref name="assemblies"/>：填充 <see cref="allTypes"/> 全名索引，并按 <see cref="BaseAttribute"/> 派生特性把类型分组到 <see cref="types"/>。</summary>
        /// <param name="assemblies">要扫描的程序集（通常是 Hotfix/Model/ModelView/HotfixView 4 个热更 dll + 已加载的 AOT dll）。</param>
        public void Awake(Assembly[] assemblies)
        {
            using var _ = Profiler.Once("CodeTypes.Awake");

            // 细分测量：程序集扫描 与 属性分类 各占多少，用数据决定优化点，不凭感觉
            Dictionary<string, Type> addTypes;
            using (Profiler.Once("CodeTypes.Awake/GetAssemblyTypes"))
            {
                addTypes = AssemblyHelper.GetAssemblyTypes(assemblies);
            }
            foreach ((string fullName, Type type) in addTypes)
            {
                this.allTypes[fullName] = type;

                if (type.IsAbstract)
                {
                    continue;
                }

                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

                foreach (object o in objects)
                {
                    this.types.Add(o.GetType(), type);
                }
            }
        }

        /// <summary>按 <see cref="BaseAttribute"/> 派生特性取所有标注了该特性的类型集合（如 <c>GetTypes(typeof(EventAttribute))</c> 取所有事件 handler）。</summary>
        /// <param name="systemAttributeType">要查询的 <see cref="BaseAttribute"/> 派生特性 Type。</param>
        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }

        /// <summary>返回全名 → Type 的字典（直接引用，不复制），用于运行时按字符串查类型（如反射创建 Entity 实例）。</summary>
        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        /// <summary>按全名查 Type，未找到返回 <c>null</c>。</summary>
        public Type GetType(string typeName)
        {
            Type type = null;
            this.allTypes.TryGetValue(typeName, out type);
            return type;
        }

        /// <summary>把所有 <c>[CodeProcess]</c> 标注的单例 <c>new</c> 出来 + <c>Awake()</c> + 注册到 <see cref="World"/>：用于把 <see cref="EventSystem"/> 之类的「需要程序集扫描结果」的单例串成启动链。</summary>
        public void CodeProcess()
        {
            using var _ = Profiler.Once("CodeTypes.CodeProcess");
            var hashSet = this.GetTypes(typeof (CodeProcessAttribute));
            foreach (Type type in hashSet)
            {
                object obj = Activator.CreateInstance(type);
                ((ISingletonAwake)obj).Awake();
                World.Instance.AddSingleton((ASingleton)obj);
            }
        }
    }
}
