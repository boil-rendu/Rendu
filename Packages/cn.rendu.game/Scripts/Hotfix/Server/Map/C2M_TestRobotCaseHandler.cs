using System;
using RD.Core;

namespace RD.Server
{
	[MessageHandler(SceneType.Map)]
	public class C2M_TestRobotCaseHandler : MessageLocationHandler<Unit, C2M_TestRobotCase, M2C_TestRobotCase>
	{
		protected override async Promise Run(Unit unit, C2M_TestRobotCase request, M2C_TestRobotCase response)
		{
			response.N = request.N;
			await Promise.CompletedTask;
		}
	}
}