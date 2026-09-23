#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;

namespace RD.Init.Editor
{
    /// <summary>
    /// 停靠窗口类型集合
    /// 支持扩展窗口类型列表
    /// </summary>
    public static class WindowsDefine
    {
        private static List<Type> _additionalWindowTypes = new List<Type>();

        /// <summary>
        /// 注册额外的窗口类型
        /// </summary>
        public static void RegisterWindowType(Type windowType)
        {
            if (windowType != null && !_additionalWindowTypes.Contains(windowType))
            {
                _additionalWindowTypes.Add(windowType);
            }
        }

        /// <summary>
        /// 注册多个额外的窗口类型
        /// </summary>
        public static void RegisterWindowTypes(params Type[] windowTypes)
        {
            if (windowTypes == null) return;

            foreach (var type in windowTypes)
            {
                RegisterWindowType(type);
            }
        }

        /// <summary>
        /// 获取所有窗口类型
        /// </summary>
        public static Type[] GetAllWindowTypes()
        {
            return _additionalWindowTypes.ToArray();
        }

        /// <summary>
        /// 获取停靠窗口类型集合（向后兼容）
        /// </summary>
        public static Type[] DockedWindowTypes => GetAllWindowTypes();
    }
}
#endif

