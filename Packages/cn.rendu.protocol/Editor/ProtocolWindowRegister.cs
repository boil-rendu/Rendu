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
                // 注册 Protocol 包中的窗口类型
                WindowsDefine.RegisterWindowType(typeof(ProtoViewWindow));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ProtocolWindowRegister] 注册窗口类型失败: {e.Message}");
            }
        }
    }
}
