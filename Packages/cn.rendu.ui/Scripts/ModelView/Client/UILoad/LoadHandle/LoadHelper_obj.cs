using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>LoadHelper 的对象反查部分（资源对象 → 加载句柄）</summary>
    public static partial class LoadHelper
    {
        /// <summary>资源对象 → 加载句柄</summary>
        [StaticField]
        private static readonly Dictionary<UnityObject, LoadHandle> m_ObjLoadHandle = new();

        /// <summary>
        /// 登记对象与句柄的对应关系；同一对象若已绑定不同句柄则报错并返回 false
        /// （同一资源重复创建多个句柄属于使用错误）
        /// </summary>
        public static bool AddLoadHandle(UnityObject obj, LoadHandle handle)
        {
            if (m_ObjLoadHandle.TryGetValue(obj, out LoadHandle value))
            {
                if (value != handle)
                {
                    Debug.LogError($"此obj {obj.name} Handle 已存在 且前后不一致 请检查 请勿创建多个");
                    return false;
                }

                return true;
            }

            m_ObjLoadHandle.Add(obj, handle);
            return true;
        }

        /// <summary>按句柄内的对象反查并移除；对象为空返回 false</summary>
        private static bool RemoveLoadHandle(LoadHandle handle)
        {
            var obj = handle.Object;
            if (obj == null)
            {
                return false;
            }

            return RemoveLoadHandle(obj);
        }

        /// <summary>移除指定对象的登记；未登记则报错并返回 false</summary>
        private static bool RemoveLoadHandle(UnityObject obj)
        {
            if (!m_ObjLoadHandle.ContainsKey(obj))
            {
                Debug.LogError($"此obj {obj.name} Handle 不存在 请检查 请先创建设置");
                return false;
            }

            return m_ObjLoadHandle.Remove(obj);
        }

        /// <summary>取对象对应的加载句柄；未登记则报错并返回 null</summary>
        public static LoadHandle GetLoadHandle(UnityObject obj)
        {
            if (!m_ObjLoadHandle.TryGetValue(obj, out LoadHandle handle))
            {
                Debug.LogError($"此obj {obj.name} Handle 不存在 请检查 请先创建设置");
                return null;
            }

            return handle;
        }
    }
}
