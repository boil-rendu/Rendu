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
                // 注册为 Rendu 工具窗口的一个页签(不再单独开窗口)
                ToolTabRegistry.Register(ProtoViewContent.TabName, 1030, ProtoViewContent.CreateContent);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ProtocolWindowRegister] 注册工具页签失败: {e.Message}");
            }
        }
    }
}
