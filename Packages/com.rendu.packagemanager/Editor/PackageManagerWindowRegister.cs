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
                // 注册 PackageManager 包中的窗口类型
                WindowsDefine.RegisterWindowType(typeof(PackageCreateView));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[PackageManagerWindowRegister] 注册窗口类型失败: {e.Message}");
            }
        }
    }
}
