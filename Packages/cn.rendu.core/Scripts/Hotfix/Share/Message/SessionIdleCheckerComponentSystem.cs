using System;
using RD.Core;

namespace RD
{
    [EntitySystemOf(typeof(SessionIdleCheckerComponent))]
    public static partial class SessionIdleCheckerComponentSystem
    {
        /// <summary>
        /// 定时器回调，每 CheckInternal 毫秒触发一次
        /// </summary>
        [Invoke(TimerCoreInvokeType.SessionIdleChecker)]
        public class SessionIdleChecker: ATimer<SessionIdleCheckerComponent>
        {
            protected override void Run(SessionIdleCheckerComponent self)
            {
                try
                {
                    self.Check();
                }
                catch (Exception e)
                {
                    Log.Error($"session idle checker timer error: {self.Id}\n{e}");
                }
            }
        }
    
        [EntitySystem]
        private static void Awake(this SessionIdleCheckerComponent self)
        {
            // 启动循环定时器，每 CheckInternal 毫秒检查一次连接空闲状态
            self.RepeatedTimer = self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(CheckInternal, TimerCoreInvokeType.SessionIdleChecker, self);
        }
        
        [EntitySystem]
        private static void Destroy(this SessionIdleCheckerComponent self)
        {
            // 组件销毁时移除定时器
            self.Root().GetComponent<TimerComponent>()?.Remove(ref self.RepeatedTimer);
        }

        /// <summary>检查间隔：2秒</summary>
        private const int CheckInternal = 2000;

        /// <summary>
        /// 空闲超时阈值
        /// 编辑器调试模式 400 秒，正式环境 40 秒
        /// </summary>
#if UNITY_EDITOR && DEBUG
        public const int SessionTimeoutTime = 400000;
#else
        public const int SessionTimeoutTime = 40000;
#endif

        /// <summary>
        /// 检查连接是否空闲超时。
        /// 使用 && 而非 ||，只有收和发两个方向都超时才会断开连接，
        /// 避免单向流量（如心跳、服务端推送）被误判为死连接。
        /// </summary>
        private static void Check(this SessionIdleCheckerComponent self)
        {
            Session session = self.GetParent<Session>();
            long timeNow = TimeInfo.Instance.ClientNow();

            // 收发包任一方向仍有活动，连接正常
            if (timeNow - session.LastRecvTime < SessionTimeoutTime && timeNow - session.LastSendTime < SessionTimeoutTime)
            {
                return;
            }

            // 双方都超时，断开连接
            Log.Info($"session timeout: {session.Id} {timeNow} {session.LastRecvTime} {session.LastSendTime} {timeNow - session.LastRecvTime} {timeNow - session.LastSendTime}");
            session.Error = ErrorCode.ERR_SessionSendOrRecvTimeout;

            session.Dispose();
        }
    }
}