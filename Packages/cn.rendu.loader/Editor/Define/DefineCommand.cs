using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 宏定义命令工具类
    /// </summary>
    public static class DefineCommand
    {
        /// <summary>
        /// 启用或禁用指定的宏定义
        /// </summary>
        /// <param name="defineSymbol">宏定义符号</param>
        /// <param name="enable">是否启用</param>
        public static void EnableDefineSymbols(string defineSymbol, bool enable)
        {
            if (string.IsNullOrEmpty(defineSymbol))
            {
                Debug.LogWarning("[DefineCommand] 宏定义符号为空");
                return;
            }

            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            var defineList = new System.Collections.Generic.List<string>(defines.Split(';'));
            var hasDefine = defineList.Contains(defineSymbol);

            if (enable && !hasDefine)
            {
                defineList.Add(defineSymbol);
                Debug.Log($"[DefineCommand] 启用宏定义: {defineSymbol}");
            }
            else if (!enable && hasDefine)
            {
                defineList.Remove(defineSymbol);
                Debug.Log($"[DefineCommand] 禁用宏定义: {defineSymbol}");
            }
            else
            {
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", defineList.ToArray()));
        }
    }
}
