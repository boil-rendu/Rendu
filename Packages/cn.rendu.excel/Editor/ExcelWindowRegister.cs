using System;
using RD.EditTool.Editor;
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
                // Excel 已改用声明式模块(ExcelToolModule) + ExcelTool 宿主绘制, 不再注册 UXML 页签;
                // 这里只把宿主窗口登记到停靠组, 与 Loader Tool / UI Tool 合成同一个窗口下的页签
                WindowsDefine.RegisterWindowType(typeof(ExcelTool));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ExcelWindowRegister] 注册工具页签失败: {e.Message}");
            }
        }
    }
}
