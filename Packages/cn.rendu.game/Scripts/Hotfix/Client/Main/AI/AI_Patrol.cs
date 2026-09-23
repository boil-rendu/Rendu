using RD.Core;
using Unity.Mathematics;

namespace RD.Client
{
    public class AI_Patrol: AAIHandler
    {
        public override int Check(AIComponent aiComponent, AIConfig aiConfig)
        {
            long sec = TimeInfo.Instance.ClientNow() / 1000 % 15;
            if (sec < 10)
            {
                return 0;
            }
            return 1;
        }

        public override async Promise Execute(AIComponent aiComponent, AIConfig aiConfig)
        {
            Scene root = aiComponent.Root();

            Unit myUnit = UnitHelper.GetMyUnitFromClientScene(root);
            if (myUnit == null)
            {
                return;
            }
            
            Log.Debug("开始巡逻");

            while (true)
            {
                PatrolPathComponent patrolPathComponent = myUnit.GetComponent<PatrolPathComponent>();
                float3 nextTarget = patrolPathComponent.GetCurrent();
                await myUnit.MoveToAsync(nextTarget);
                
                PromiseCancellationToken cancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
                if (cancellationToken.IsCancel())
                {
                    return;
                }
                patrolPathComponent.MoveNext();
            }
        }
    }
}