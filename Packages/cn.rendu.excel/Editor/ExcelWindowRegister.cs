using System;
using RD.Init.Editor;
using UnityEditor;

namespace RD.Excel.Editor
{
    /// <summary>
    /// Excel 包窗口类型注册器
    /// </summary>
    [InitializeOnLoad]
    public static class ExcelWindowRegister
    {
        static ExcelWindowRegister()
        {
            RegisterExcelWindows();
        }

        private static void RegisterExcelWindows()
        {
            try
            {
                // 注册 Excel 包中的窗口类型
                WindowsDefine.RegisterWindowType(typeof(ExcelViewWindow));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ExcelWindowRegister] 注册窗口类型失败: {e.Message}");
            }
        }
    }
}
