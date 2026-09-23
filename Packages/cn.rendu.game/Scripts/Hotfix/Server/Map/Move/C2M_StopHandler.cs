using RD.Core;

namespace RD.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_StopHandler: MessageLocationHandler<Unit, C2M_Stop>
    {
        protected override async Promise Run(Unit unit, C2M_Stop message)
        {
            unit.Stop(1);
            await Promise.CompletedTask;
        }
    }
}