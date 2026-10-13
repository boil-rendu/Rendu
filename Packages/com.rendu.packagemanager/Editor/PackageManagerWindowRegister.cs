using System;
using UnityEditor;
using RD.EditTool.Editor;

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
                // PackageManager 已改用声明式模块(PackageCreateModule) + PackageManagerTool 宿主绘制
                WindowsDefine.RegisterWindowType(typeof(PackageManagerTool));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[PackageManagerWindowRegister] 注册工具页签失败: {e.Message}");
            }
        }
    }
}
