using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    public static class UILoadHelperStatic
    {
        [StaticField]
        public static Dictionary<UnityEngine.Object, UnityEngine.Object> g_ObjectMap = new();
    }
}
