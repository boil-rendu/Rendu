using RD.Client;
using RD.Core;

namespace RD.Client
{
    [Invoke(EUIInvokeType.Async)]
    public class UIInvokeTimerComponentWaitFrameAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_WaitFrameAsync, Promise>
    {
        public override async Promise Handle(Entity entity, UIInvokeEntity_WaitFrameAsync args)
        {
            await entity.Root().GetComponent<TimerComponent>().WaitFrameAsync();
        }
    }

    [Invoke(EUIInvokeType.Async)]
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

    [Invoke(EUIInvokeType.Async)]
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