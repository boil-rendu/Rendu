using RD.Core;
using Unity.Mathematics;

namespace RD
{
    [Event(SceneType.StateSync)]
    public class ShareInitEvent: AEvent<Scene, InitShare>
    {
        protected override async Promise Run(Scene root, InitShare args)
        {
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ObjectWait>();
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ProcessInnerSender>();
            
            MongoRegister.RegisterStruct<float2>();
            MongoRegister.RegisterStruct<float3>();
            MongoRegister.RegisterStruct<float4>();
            MongoRegister.RegisterStruct<quaternion>();
            
            await Promise.CompletedTask;
        }
    }
}