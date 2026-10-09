using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>UILoad 模块的静态数据（与 UILoadHelper 实例数据分离，避免热重载影响）</summary>
    public static class UILoadHelperStatic
    {
        /// <summary>原始资源对象 → 其持有对象的映射（用于反查引用关系）</summary>
        [StaticField]
        public static Dictionary<UnityEngine.Object, UnityEngine.Object> g_ObjectMap = new();
    }
}
