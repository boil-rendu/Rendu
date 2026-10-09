using RD.Core;
using Unity.Mathematics;

namespace RD
{
	/// <summary>
	/// Share 层初始化事件（<c>[Event(SceneType.StateSync)]</c>）：在 <see cref="MainFiberInit"/> 处理 <see cref="InitShare"/> 时触发，
	/// 给 StateSync 类型的 Scene 挂 5 个基础组件（<see cref="TimerComponent"/>/<see cref="CoroutineLockComponent"/>/<see cref="ObjectWait"/>/<see cref="MailBoxComponent"/>/<see cref="ProcessInnerSender"/>）+ 注册 Unity.Mathematics 5 个 struct 的 Bson serializer。
	/// </summary>
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
