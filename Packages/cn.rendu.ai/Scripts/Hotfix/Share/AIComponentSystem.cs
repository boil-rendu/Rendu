using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="AIComponent"/> 的系统扩展（Hotfix/Share 层）：负责 AI 周期检测、行为切换、协程取消。
	/// <list type="bullet">
	///   <item><see cref="Awake"/>：创建 1s 周期的 <see cref="AITimer"/> 定时器。</item>
	///   <item><see cref="Destroy"/>：移除定时器 + 取消当前行为协程。</item>
	///   <item><see cref="Check"/>：按 <see cref="AIConfigCategory.AIConfigs"/> 顺序遍历子节点，<c>Check()</c> 通过且不是当前节点则切到该节点执行。</item>
	/// </list>
	/// </summary>
	[EntitySystemOf(typeof(AIComponent))]
	[FriendOf(typeof(AIComponent))]
	[FriendOf(typeof(AIDispatcherComponent))]
	public static partial class AIComponentSystem
	{
		/// <summary>AI 周期检测定时器（<c>[Invoke(TimerInvokeType.AITimer)]</c>）：每 1s 调一次 <see cref="Check"/>。</summary>
		[Invoke(TimerInvokeType.AITimer)]
		public class AITimer: ATimer<AIComponent>
		{
			protected override void Run(AIComponent self)
			{
				try
				{
					self.Check();
				}
				catch (Exception e)
				{
					Log.Error($"move timer error: {self.Id}\n{e}");
				}
			}
		}

		[EntitySystem]
		private static void Awake(this AIComponent self, int aiConfigId)
		{
			self.AIConfigId = aiConfigId;
			self.Timer = self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(1000, TimerInvokeType.AITimer, self);
		}

		[EntitySystem]
		private static void Destroy(this AIComponent self)
		{
			self.Root().GetComponent<TimerComponent>()?.Remove(ref self.Timer);
			self.CancellationToken?.Cancel();
			self.CancellationToken = null;
			self.Current = 0;
		}

		/// <summary>AI 行为检测：遍历 <see cref="AIConfigCategory.AIConfigs"/>[<see cref="AIComponent.AIConfigId"/>] 所有子节点，<c>Check()</c> 通过且 <c>id != Current</c> 则切换过去（先 <see cref="Cancel"/> 旧行为再 <c>Execute().WithContext(token)</c>）。</summary>
		private static void Check(this AIComponent self)
		{
			Fiber fiber = self.Fiber();
			if (self.Parent == null)
			{
				fiber.Root.GetComponent<TimerComponent>().Remove(ref self.Timer);
				return;
			}

			var oneAI = AIConfigCategory.Instance.AIConfigs[self.AIConfigId];

			foreach (AIConfig aiConfig in oneAI)
			{

				AAIHandler aaiHandler = AIDispatcherComponent.Instance.Get(aiConfig.Name);

				if (aaiHandler == null)
				{
					Log.Error($"not found aihandler: {aiConfig.Name}");
					continue;
				}

				int ret = aaiHandler.Check(self, aiConfig);
				if (ret != 0)
				{
					continue;
				}

				if (self.Current == aiConfig.Id)
				{
					break;
				}

				self.Cancel(); // 取消之前的行为
				PromiseCancellationToken cancellationToken = new();
				self.CancellationToken = cancellationToken;
				self.Current = aiConfig.Id;

				aaiHandler.Execute(self, aiConfig).WithContext(cancellationToken);
				return;
			}

		}

		/// <summary>取消当前 AI 行为协程并清状态。</summary>
		private static void Cancel(this AIComponent self)
		{
			self.CancellationToken?.Cancel();
			self.Current = 0;
			self.CancellationToken = null;
		}
	}
}
