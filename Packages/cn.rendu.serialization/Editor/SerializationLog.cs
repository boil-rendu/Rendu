using System.Collections.Generic;
using UnityEngine;

namespace RD.Serialization.Editor
{
    /// <summary>
    /// 本包的日志出口: 给 Debug.* 包一层, 并支持"同一件事只报一次"。
    ///
    /// 为什么不复用绘制引擎的 DrawLog: 本包是**独立序列化器**, 位于绘制引擎之下,
    /// 反向引用 draw 会让依赖成环。判定可以下沉共用(那是数据事实), 日志通道不行 —— 各包自己有一层。
    ///
    /// 为什么要包: Unity 的 Debug.* 是内部调用, 在非 Unity 进程里调用会抛 SecurityException;
    /// 告警路径本身不该把功能打断, 包起来之后这些分支才能被普通 .NET 的校验工程覆盖。
    ///
    /// 为什么要 WarnOnce: 序列化/反序列化跑在域重载与每次存取上, 一个"没有可序列化成员"的类型
    /// 若每次都报, 控制台会被刷爆, 用户反而看不到第一条。
    /// </summary>
    public static class SerializationLog
    {
        /// <summary>去重表的容量上限。键来自写死的类型名/成员名, 正常到不了这儿; 到了就清空重来。</summary>
        private const int MaxRemembered = 512;

        private static readonly HashSet<string> s_Warned = new HashSet<string>();

        public static void Warn(string message)
        {
            try
            {
                Debug.LogWarning(message);
            }
            catch
            {
                // 日志通道不可用就算了, 调用方的行为不受影响
            }
        }

        /// <summary>按 key 去重地记一条警告: 同一个 key 只报一次。</summary>
        public static void WarnOnce(string key, string message)
        {
            if (s_Warned.Count >= MaxRemembered) s_Warned.Clear();
            if (s_Warned.Add(key)) Warn(message);
        }

        public static void Error(string message)
        {
            try
            {
                Debug.LogError(message);
            }
            catch
            {
            }
        }
    }
}
