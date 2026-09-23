using System.Collections.Generic;
using System.Numerics;
using RD.Core;
using Unity.Mathematics;

namespace RD.Client
{
    public static partial class MoveHelper
    {
        // 可以多次调用，多次调用的话会取消上一次的协程
        public static async Promise<int> MoveToAsync(this Unit unit, float3 targetPos)
        {
            C2M_PathfindingResult msg = C2M_PathfindingResult.Create();
            msg.Position = targetPos;
            unit.Root().GetComponent<ClientSenderComponent>().Send(msg);

            ObjectWait objectWait = unit.GetComponent<ObjectWait>();
            
            // 要取消上一次的移动协程
            objectWait.Notify(new Wait_UnitStop() { Error = WaitTypeError.Cancel });
            
            // 一直等到unit发送stop
            Wait_UnitStop waitUnitStop = await objectWait.Wait<Wait_UnitStop>();
            PromiseCancellationToken cancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
            if (cancellationToken.IsCancel())
            {
                return WaitTypeError.Cancel;
            }
            return waitUnitStop.Error;
        }
        
        public static async Promise MoveToAsync(this Unit unit, List<float3> path)
        {
            float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
            await moveComponent.MoveToAsync(path, speed);
        }
    }
}