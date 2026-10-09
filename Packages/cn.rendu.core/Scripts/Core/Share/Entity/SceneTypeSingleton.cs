using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD.Core
{
    /// <summary>
    /// Scene 类型枚举注册中心（单例）：从 <see cref="Awake"/> 接收一个 <see cref="Type"/>，反射读取其 <c>public static int</c> 字段并建立
    /// <c>sceneType ↔ name</c> 双向映射。供 <c>GetSceneName</c>/<c>GetSceneType</c>/<c>GetAll</c> 查询。
    /// </summary>
    public class SceneTypeSingleton: Singleton<SceneTypeSingleton>, ISingletonAwake<Type>
    {
        /// <summary>判断两个 SceneType 是否「同源」（相等或任一为 0 表示「全部」通配）。</summary>
        public static bool IsSame(int a, int b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == 0)
            {
                return true;
            }

            if (b == 0)
            {
                return true;
            }

            return false;
        }

        private readonly DoubleMap<int, string> sceneNames = new();

        /// <summary>从 <paramref name="type"/> 反射读取 <c>public static int</c> 字段并注册到 <c>sceneNames</c>。</summary>
        public void Awake(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo.FieldType != typeof(int))
                {
                    continue;
                }
                this.sceneNames.Add((int)fieldInfo.GetValue(null), fieldInfo.Name);
            }
        }

        /// <summary>根据 <paramref name="sceneType"/> 枚举值取名称（未注册返回 null）。</summary>
        public string GetSceneName(int sceneType)
        {
            return this.sceneNames.GetValueByKey(sceneType);
        }

        /// <summary>根据 <paramref name="sceneName"/> 取枚举值；未注册且名称不是 "All" 时抛异常。</summary>
        public int GetSceneType(string sceneName)
        {
            int type = this.sceneNames.GetKeyByValue(sceneName);
            if (type == 0 && sceneName != "All")
            {
                throw new Exception($"not found scene type: {type} {sceneName}");
            }
            return type;
        }

        /// <summary>取全部已注册映射。</summary>
        public Dictionary<int, string> GetAll()
        {
            return this.sceneNames.GetAll();
        }
    }
}
