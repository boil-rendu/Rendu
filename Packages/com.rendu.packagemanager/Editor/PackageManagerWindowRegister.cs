using System;
using UnityEditor;
using RD.Init.Editor;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// PackageManager 包窗口类型注册器
    /// </summary>
    [InitializeOnLoad]
    public static class PackageManagerWindowRegister
    {
        static PackageManagerWindowRegister()
        {
            RegisterPackageManagerWindows();
        }

        private static void RegisterPackageManagerWindows()
        {
            try
            {
                // 注册为 Rendu 工具窗口的一个页签(不再单独开窗口)
                ToolTabRegistry.Register(PackageCreateContent.TabName, 1000, PackageCreateContent.CreateContent);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[PackageManagerWindowRegister] 注册工具页签失败: {e.Message}");
            }
        }
    }
}
