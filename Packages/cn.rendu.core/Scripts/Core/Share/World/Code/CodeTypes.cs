using System.Collections.Generic;
using System.Reflection;
using System;

namespace RD.Core
{
    public class CodeTypes: Singleton<CodeTypes>, ISingletonAwake<Assembly[]>
    {
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types = new();
        
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

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }
        
        public Type GetType(string typeName)
        {
            Type type = null;
            this.allTypes.TryGetValue(typeName, out type);
            return type;
        }
        
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