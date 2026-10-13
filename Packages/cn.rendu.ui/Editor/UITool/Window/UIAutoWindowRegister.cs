#if UNITY_EDITOR

using System;
using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.UI.Editor
{
    /// <summary>
    /// ui 包窗口类型注册器: 把 UITool 登记进 WindowsDefine 的停靠组,
    /// 与 Excel View / 批量导出导航网格 / Loader Tool 等窗口同组 ——
    /// 菜单打开时互相停靠成同一个窗口下的不同页签, 不再各自飘独立窗口。
    /// </summary>
    [InitializeOnLoad]
    public static class UIAutoWindowRegister
    {
        static UIAutoWindowRegister()
        {
            RegisterUIWindows();
        }

        private static void RegisterUIWindows()
        {
            try
            {
                WindowsDefine.RegisterWindowType(typeof(UITool));
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAutoWindowRegister] 注册窗口类型失败: {e.Message}");
            }
        }
    }
}

#endif
