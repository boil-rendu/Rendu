using RD.Core;

namespace RD.Server
{
    // 离开视野
    [Event(SceneType.Map)]
    public class UnitLeaveSightRange_NotifyClient: AEvent<Scene, UnitLeaveSightRange>
    {
        protected override async Promise Run(Scene scene, UnitLeaveSightRange args)
        {
            await Promise.CompletedTask;
            AOIEntity a = args.A;
            AOIEntity b = args.B;
            if (a.Unit.Type() != UnitType.Player)
            {
                return;
            }

            MapMessageHelper.NoticeUnitRemove(a.GetParent<Unit>(), b.GetParent<Unit>());
        }
    }
}