using System;
using RD.Init.Editor;
using UnityEditor;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Protocol 包窗口类型注册器
    /// </summary>
    [InitializeOnLoad]
    public static class ProtocolWindowRegister
    {
        static ProtocolWindowRegister()
        {
            RegisterProtocolWindows();
        }

        private static void RegisterProtocolWindows()
        {
            try
            {
                // Protocol 已改用声明式模块(ProtoListModule / ProtoEditModule) + ProtocolTool 宿主绘制
                WindowsDefine.RegisterWindowType(typeof(ProtocolTool));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ProtocolWindowRegister] 注册工具页签失败: {e.Message}");
            }
        }
    }
}
