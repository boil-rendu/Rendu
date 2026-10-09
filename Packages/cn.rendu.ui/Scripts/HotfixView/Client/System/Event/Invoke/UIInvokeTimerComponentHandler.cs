using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>异步等待一帧（TimerComponent.WaitFrameAsync）</summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeTimerComponentWaitFrameAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_WaitFrameAsync, Promise>
    {
        public override async Promise Handle(Entity entity, UIInvokeEntity_WaitFrameAsync args)
        {
            await entity.Root().GetComponent<TimerComponent>().WaitFrameAsync();
        }
    }

    /// <summary>异步等待指定毫秒；带 CancellationToken 时挂到该上下文上以便取消</summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeTimerComponentWaitAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_WaitAsync, Promise>
    {
        public override async Promise Handle(Entity entity, UIInvokeEntity_WaitAsync args)
        {
            if (args.CancellationToken == null)
            {
                await entity.Root().GetComponent<TimerComponent>().WaitAsync(args.Time);
            }
            else
            {
                await entity.Root().GetComponent<TimerComponent>().WaitAsync(args.Time).NewContext(args.CancellationToken);
            }
        }
    }

    /// <summary>异步等待指定秒数（内部 ×1000 转毫秒）；带 CancellationToken 时支持取消</summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeTimerComponentWaitSecondAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_WaitSecondAsync, Promise>
    {
        public override async Promise Handle(Entity entity, UIInvokeEntity_WaitSecondAsync args)
        {
            if (args.CancellationToken == null)
            {
                await entity.Root().GetComponent<TimerComponent>().WaitAsync((long)(args.Time * 1000));
            }
            else
            {
                await entity.Root().GetComponent<TimerComponent>().WaitAsync((long)(args.Time * 1000)).NewContext(args.CancellationToken);
            }
        }
    }
}