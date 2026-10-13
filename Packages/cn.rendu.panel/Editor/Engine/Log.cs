using System.Collections.Generic;
using UnityEngine;

namespace RD.Panel.Editor
{
    /// <summary>
    /// 日志出口: 统一给 Debug.* 包一层, 并支持"同一件事只报一次"。
    ///
    /// 为什么要包: Unity 的 Debug.* 是内部调用, 在非 Unity 进程(包内的纯 .NET 校验工程)里调用会直接抛
    /// SecurityException。告警路径本身不该把功能打断 —— 包起来之后这些分支才能在普通 .NET 下被测试覆盖。
    ///
    /// 为什么要 WarnOnce: 条件显隐、表达式求值都是每帧重算的, 一个拼错的名字/表达式若每帧各报一条,
    /// 控制台会被刷爆, 用户反而看不到第一条。只报一次既留下线索, 也不至于淹掉日志。
    ///
    /// 注: 窗口宿主(cn.rendu.edittool)里的日志直接走 Debug.* —— 那些回调只可能在编辑器里跑, 没有第二套宿主。
    /// </summary>
    public static class Log
    {
        /// <summary>去重表的容量上限。键来自写死的成员名/表达式, 正常到不了这儿; 到了就清空重来。</summary>
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
