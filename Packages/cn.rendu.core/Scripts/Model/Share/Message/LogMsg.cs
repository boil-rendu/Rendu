using System.Collections.Generic;
using System.Diagnostics;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 消息日志门面（<see cref="Singleton{T}"/>）：在 DEBUG 编译下按 opcode 把 <see cref="object"/> 消息打到 <see cref="Fiber.Log"/>，
	/// 业务侧可调 <see cref="AddIgnore(ushort)"/> 把高频包（Ping/Benchmark）排除。
	/// </summary>
	public class LogMsg: Singleton<LogMsg>, ISingletonAwake
	{
		private readonly HashSet<ushort> ignore = new();
			//OuterMessage.C2G_Ping,
			//OuterMessage.G2C_Ping,
			//OuterMessage.C2G_Benchmark,
			//OuterMessage.G2C_Benchmark,

		public void Awake()
		{
		}

		/// <summary>把 <paramref name="opcode"/> 加入忽略列表：之后该 opcode 不再打日志。</summary>
		public void AddIgnore(ushort opcode)
		{
			this.ignore.Add(opcode);
		}

		/// <summary>DEBUG 编译下生效：取 <paramref name="msg"/> 的 opcode，若在忽略列表则跳过；否则 <c>fiber.Log.Debug(msg.ToString())</c>。</summary>
		[Conditional("DEBUG")]
		public void Debug(Fiber fiber, object msg)
		{
			ushort opcode = OpcodeType.Instance.GetOpcode(msg.GetType());
			if (this.ignore.Contains(opcode))
			{
				return;
			}
			fiber.Log.Debug(msg.ToString());
		}
	}
}
