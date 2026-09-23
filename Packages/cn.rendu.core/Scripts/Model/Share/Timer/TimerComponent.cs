using System;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
    /// <summary>
    /// 定时器类别。区分一次性、等待型一次性、重复三种触发语义，
    /// 由 <see cref="TimerComponent"/> 在到期时按类别决定：仅触发、触发并等待结果、还是按周期重新入队。
    /// </summary>
    public enum TimerClass
    {
        /// <summary>未指定/无效类别。</summary>
        None,
        /// <summary>一次性定时器：到期后触发回调并移除。</summary>
        OnceTimer,
        /// <summary>等待型一次性定时器：到期后触发并等待结果（常用于 RPC/请求超时等待）。</summary>
        OnceWaitTimer,
        /// <summary>重复定时器：每次到期后按周期重新入队，直到被主动取消。</summary>
        RepeatedTimer,
    }

    /// <summary>
    /// 单次定时任务的参数包。由 <see cref="TimerComponent"/> 创建并存入 timerActions，
    /// 到期时据此还原触发语义（类别、回调类型、绑定对象、起止时间）。
    /// </summary>
    public struct TimerAction
    {
        public TimerAction(TimerClass timerClass, long startTime, long time, int type, object obj)
        {
            this.TimerClass = timerClass;
            this.StartTime = startTime;
            this.Object = obj;
            this.Time = time;
            this.Type = type;
        }

        /// <summary>定时器类别（一次性/等待型一次性/重复），决定到期行为。</summary>
        public TimerClass TimerClass;

        /// <summary>回调类型，用于定位具体要执行的定时回调。</summary>
        public int Type;

        /// <summary>绑定到该定时器的业务对象/上下文。</summary>
        public object Object;

        /// <summary>定时器起始时间（注册时刻）。</summary>
        public long StartTime;

        /// <summary>绝对到期时间；与 <see cref="TimerComponent"/> 的 minTime 比较判断是否到期。</summary>
        public long Time;
    }

    /// <summary>
    /// 定时器回调的上下文参数包（当前未承载实际字段，保留用于后续扩展回调入参）。
    /// </summary>
    public struct TimerCallback
    {
        /// <summary>回调附带的参数。</summary>
        public object Args;
    }

    /// <summary>
    /// 场景级定时器组件（挂在 Scene 上，由同命名空间的 <c>TimerComponentSystem</c> 驱动 Update）。
    /// 负责注册、推进与触发定时任务，内部索引结构如下：
    /// <list type="bullet">
    ///   <item><see cref="timeId"/>：到期时间(time) -> 该时间点上所有 timerId 的多重映射。</item>
    ///   <item><see cref="timerActions"/>：timerId -> 定时器参数包（类别/回调类型/对象/起止时间）。</item>
    ///   <item><see cref="minTime"/>：当前所有定时器中最早到期时间，避免每帧遍历 timeId 取最小。</item>
    ///   <item><see cref="timeOutTime"/> / <see cref="timeOutTimerIds"/>：Update 时临时存放已到期的时间点与 timerId，供触发后清理。</item>
    /// </list>
    /// 本类仅持有数据结构；唤醒、到期判定与回调触发由 TimerComponentSystem 实现。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TimerComponent: Entity, IAwake, IUpdate
    {
        /// <summary>到期时间(time) -> 该时间点上所有 timerId 的多重映射。</summary>
        public readonly MultiMap<long, long> timeId = new();

        /// <summary>本轮 Update 中已到期的时间点（临时缓冲，处理后清空）。</summary>
        public readonly Queue<long> timeOutTime = new();

        /// <summary>本轮 Update 中已到期的 timerId（临时缓冲，处理后清空）。</summary>
        public readonly Queue<long> timeOutTimerIds = new();

        /// <summary>timerId -> 定时器参数包（类别、回调类型、绑定对象、起止时间）。</summary>
        public readonly Dictionary<long, TimerAction> timerActions = new();

        /// <summary>自增的 timerId 生成器，保证每个定时器全局唯一。</summary>
        public long idGenerator;

        /// <summary>当前最早到期的绝对时间；新注册更早的定时器时更新，避免每帧遍历 timeId 取最小。</summary>
        public long minTime = long.MaxValue;
    }
}