using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>同步添加倒计时：只做参数校验后 Add，不返回是否成功</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeCountDownAddSyncHandler : AInvokeEntityHandler<UIInvokeEntity_CountDownAdd>
    {
        public override void Handle(Entity entity, UIInvokeEntity_CountDownAdd args)
        {
            if (args.TotalTime < 0)
            {
                Log.Error($"总时长必须>= 0  (0=无限)");
                return;
            }

            if (args.Interval <= 0)
            {
                if (args.TotalTime <= 0)
                {
                    Log.Error($"没有间隔,总时长不能<=0");
                    return;
                }

                //没有间隔则默认使用一次性回调
                args.Interval = args.TotalTime;
            }

            entity?.UICountDown()?.Add(args.TimerCallback, args.TotalTime, args.Interval, args.Forever, args.StartCallback);
        }
    }

    /// <summary>同步添加倒计时（返回是否成功的变体）：逻辑同上，返回 Add 的结果</summary>
    [Invoke(EInvokeType.SyncHandler_1)]
    public class UIInvokeCountDownAddSyncHandler_1 : AInvokeEntityHandler<UIInvokeEntity_CountDownAdd, bool>
    {
        public override bool Handle(Entity entity, UIInvokeEntity_CountDownAdd args)
        {
            if (args.TotalTime < 0)
            {
                Log.Error($"总时长必须>= 0  (0=无限)");
                return false;
            }

            if (args.Interval <= 0)
            {
                if (args.TotalTime <= 0)
                {
                    Log.Error($"没有间隔,总时长不能<=0");
                    return false;
                }

                //没有间隔则默认使用一次性回调
                args.Interval = args.TotalTime;
            }

            return entity.UICountDown().Add(args.TimerCallback, args.TotalTime, args.Interval, args.Forever, args.StartCallback);
        }
    }

    /// <summary>同步移除倒计时</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeCountDownRemoveSyncHandler : AInvokeEntityHandler<UIInvokeEntity_CountDownRemove>
    {
        public override void Handle(Entity entity, UIInvokeEntity_CountDownRemove args)
        {
            entity?.UICountDown()?.Remove(args.TimerCallback);
        }
    }

    /// <summary>同步移除倒计时（返回是否成功的变体）</summary>
    [Invoke(EInvokeType.SyncHandler_1)]
    public class UIInvokeCountDownRemoveSyncHandler_1 : AInvokeEntityHandler<UIInvokeEntity_CountDownRemove, bool>
    {
        public override bool Handle(Entity entity, UIInvokeEntity_CountDownRemove args)
        {
            return entity?.UICountDown()?.Remove(args.TimerCallback) ?? false;
        }
    }
}