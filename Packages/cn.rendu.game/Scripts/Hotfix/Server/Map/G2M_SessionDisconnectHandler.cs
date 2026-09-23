using RD.Core;

namespace RD.Server
{
	[MessageHandler(SceneType.Map)]
	public class G2M_SessionDisconnectHandler : MessageLocationHandler<Unit, G2M_SessionDisconnect>
	{
		protected override async Promise Run(Unit unit, G2M_SessionDisconnect message)
		{
			await Promise.CompletedTask;
		}
	}
}