using System;
using System.Collections.Generic;
using RD.Core;
using Unity.Mathematics;

namespace RD
{
	/// <summary>
	/// <see cref="MoveComponent"/> 的系统扩展（Hotfix/Share）：路径点移动核心。
	/// <list type="bullet">
	///   <item><see cref="MoveToAsync(List{float3}, float, int)"/>：业务主入口，按 <paramref name="speed"/> m/s 沿 path 移动（<c>turnTime</c> ms 转向），完成后 publish <see cref="MoveStop"/>。</item>
	///   <item><see cref="ChangeSpeed"/>：中途变速（先停再 MoveToAsync）。</item>
	///   <item><see cref="Stop(bool)"/>：强制停止（清路径、SetResult 到 tcs）。</item>
	///   <item><see cref="FlashTo"/>：瞬移（直接改 Unit.Position）。</item>
	///   <item>每帧 <see cref="MoveTimer"/>（<c>[Invoke(TimerInvokeType.MoveTimer)]</c>）调 <see cref="MoveForward(bool)"/> 按 elapsed time 插值推进位置 + slerp 旋转。</item>
	/// </list>
	/// </summary>
	[EntitySystemOf(typeof(MoveComponent))]
	public static partial class MoveComponentSystem
	{
		/// <summary>移动定时器（<c>[Invoke(TimerInvokeType.MoveTimer)]</c>）：每帧调 <see cref="MoveForward(bool)"/> 推进位置。</summary>
		[Invoke(TimerInvokeType.MoveTimer)]
		public class MoveTimer: ATimer<MoveComponent>
		{
			protected override void Run(MoveComponent self)
			{
				try
				{
					self.MoveForward(true);
				}
				catch (Exception e)
				{
					Log.Error($"move timer error: {self.Id}\n{e}");
				}
			}
		}


		[EntitySystem]
		private static void Destroy(this MoveComponent self)
		{
			self.MoveFinish(false);
		}

		[EntitySystem]
		private static void Awake(this MoveComponent self)
		{
			self.StartTime = 0;
			self.StartPos = float3.zero;
			self.NeedTime = 0;
			self.MoveTimer = 0;
			self.tcs = null;
			self.Targets.Clear();
			self.Speed = 0;
			self.N = 0;
			self.TurnTime = 0;
		}

		/// <summary>是否到达：路径点列表为空即已到。</summary>
		public static bool IsArrived(this MoveComponent self)
		{
			return self.Targets.Count == 0;
		}

		/// <summary>中途变速：先 <see cref="MoveForward(bool)"/> 推进到当前位置，构造新 path（首点=unit.Position）后 <see cref="MoveToAsync(List{float3}, float, int)"/> 重新启动。已到达 / 速度非法返回 false。</summary>
		public static bool ChangeSpeed(this MoveComponent self, float speed)
		{
			if (self.IsArrived())
			{
				return false;
			}

			if (speed < 0.0001)
			{
				return false;
			}

			Unit unit = self.GetParent<Unit>();

			using ListComponent<float3> path = ListComponent<float3>.Create();

			self.MoveForward(false);

			path.Add(unit.Position); // 第一个是Unit的pos
			for (int i = self.N; i < self.Targets.Count; ++i)
			{
				path.Add(self.Targets[i]);
			}
			self.MoveToAsync(path, speed).NoContext();
			return true;
		}

		// 该方法不需要用cancelToken的方式取消，因为即使不传入cancelToken，多次调用该方法也要取消之前的移动协程,上层可以stop取消
		/// <summary>业务主入口：先 <see cref="Stop(bool)"/> 取消旧移动 → 写入 Targets + Speed + TurnTime → 创建 tcs → publish <see cref="MoveStart"/> → <see cref="StartMove"/> → <c>await tcs</c> → 完成后 publish <see cref="MoveStop"/>（仅成功时）。</summary>
		public static async Promise<bool> MoveToAsync(this MoveComponent self, List<float3> target, float speed, int turnTime = 100)
		{
			self.Stop(false);

			foreach (float3 v in target)
			{
				self.Targets.Add(v);
			}

			self.IsTurnHorizontal = true;
			self.TurnTime = turnTime;
			self.Speed = speed;
			self.tcs = Promise<bool>.Create(true);

			EventSystem.Instance.Publish(self.Scene(), new MoveStart() {Unit = self.GetParent<Unit>()});

			self.StartMove();

			bool moveRet = await self.tcs;

			if (moveRet)
			{
				EventSystem.Instance.Publish(self.Scene(), new MoveStop() {Unit = self.GetParent<Unit>()});
			}
			return moveRet;
		}

		// ret: 停止的时候，移动协程的返回值
		/// <summary>每帧定时器推进：按 elapsed time 算 amount，用 <c>math.lerp</c> 插值位置 + <c>math.slerp</c> 插值旋转；走完一个点 <see cref="SetNextTarget"/> 切下一个；全部走完 <see cref="MoveFinish(bool)"/>。</summary>
		private static void MoveForward(this MoveComponent self, bool ret)
		{
			Unit unit = self.GetParent<Unit>();

			long timeNow = TimeInfo.Instance.ClientNow();
			long moveTime = timeNow - self.StartTime;

			while (true)
			{
				if (moveTime <= 0)
				{
					return;
				}

				// 计算位置插值
				if (moveTime >= self.NeedTime)
				{
					unit.Position = self.NextTarget;
					if (self.TurnTime > 0)
					{
						unit.Rotation = self.To;
					}
				}
				else
				{
					// 计算位置插值
					float amount = moveTime * 1f / self.NeedTime;
					if (amount > 0)
					{
						float3 newPos = math.lerp(self.StartPos, self.NextTarget, amount);
						unit.Position = newPos;
					}

					// 计算方向插值
					if (self.TurnTime > 0)
					{
						amount = moveTime * 1f / self.TurnTime;
						if (amount > 1)
						{
							amount = 1f;
						}
						quaternion q = math.slerp(self.From, self.To, amount);
						unit.Rotation = q;
					}
				}

				moveTime -= self.NeedTime;

				// 表示这个点还没走完，等下一帧再来
				if (moveTime < 0)
				{
					return;
				}

				// 到这里说明这个点已经走完

				// 如果是最后一个点
				if (self.N >= self.Targets.Count - 1)
				{
					unit.Position = self.NextTarget;
					unit.Rotation = self.To;

					self.MoveFinish(ret);
					return;
				}

				self.SetNextTarget();
			}
		}

		/// <summary>启动移动：记 BeginTime/StartTime + 切到第一个目标 + 注册每帧 <see cref="MoveTimer"/>。</summary>
		private static void StartMove(this MoveComponent self)
		{
			self.BeginTime = TimeInfo.Instance.ClientNow();
			self.StartTime = self.BeginTime;
			self.SetNextTarget();

			self.MoveTimer = self.Root().GetComponent<TimerComponent>().NewFrameTimer(TimerInvokeType.MoveTimer, self);
		}

		/// <summary>切到下一个目标：N++ + 用当前真实位置做 StartPos + 算 NeedTime = distance/speed*1000 + 算 To quaternion（水平转向 IsTurnHorizontal 时把 faceV.y 清 0）。</summary>
		private static void SetNextTarget(this MoveComponent self)
		{

			Unit unit = self.GetParent<Unit>();

			++self.N;

			// 时间计算用服务端的位置, 但是移动要用客户端的位置来插值
			float3 v = self.GetFaceV();
			float distance = math.length(v);

			// 插值的起始点要以unit的真实位置来算
			self.StartPos = unit.Position;

			self.StartTime += self.NeedTime;

			self.NeedTime = (long) (distance / self.Speed * 1000);

			if (self.TurnTime > 0)
			{
				// 要用unit的位置
				float3 faceV = self.GetFaceV();
				if (math.lengthsq(faceV) < 0.0001f)
				{
					return;
				}
				self.From = unit.Rotation;

				if (self.IsTurnHorizontal)
				{
					faceV.y = 0;
				}

				if (Math.Abs(faceV.x) > 0.01 || Math.Abs(faceV.z) > 0.01)
				{
					self.To = quaternion.LookRotation(faceV, math.up());
				}

				return;
			}

			if (self.TurnTime == 0) // turn time == 0 立即转向
			{
				float3 faceV = self.GetFaceV();
				if (self.IsTurnHorizontal)
				{
					faceV.y = 0;
				}

				if (Math.Abs(faceV.x) > 0.01 || Math.Abs(faceV.z) > 0.01)
				{
					self.To = quaternion.LookRotation(faceV, math.up());
					unit.Rotation = self.To;
				}
			}
		}

		/// <summary>取当前段方向向量 = NextTarget - PreTarget。</summary>
		private static float3 GetFaceV(this MoveComponent self)
		{
			return self.NextTarget - self.PreTarget;
		}

		/// <summary>瞬移：直接改 Unit.Position 到 <paramref name="target"/>。不走插值不走 timer。</summary>
		public static bool FlashTo(this MoveComponent self, float3 target)
		{
			Unit unit = self.GetParent<Unit>();
			unit.Position = target;
			return true;
		}

		// ret: 停止的时候，移动协程的返回值
		/// <summary>强制停止：先把当前位置推到当前点（<see cref="MoveForward(bool)"/>），再 <see cref="MoveFinish(bool)"/>。</summary>
		public static void Stop(this MoveComponent self, bool ret)
		{
			if (self.Targets.Count > 0)
			{
				self.MoveForward(ret);
			}

			self.MoveFinish(ret);
		}

		/// <summary>移动收尾：清状态（StartTime/Targets/Speed/N/TurnTime/...）→ 移除 MoveTimer → SetResult(ret) 到 tcs（tcs 为 null 则跳过）。</summary>
		private static void MoveFinish(this MoveComponent self, bool ret)
		{
			if (self.StartTime == 0)
			{
				return;
			}

			self.StartTime = 0;
			self.StartPos = float3.zero;
			self.BeginTime = 0;
			self.NeedTime = 0;
			self.Targets.Clear();
			self.Speed = 0;
			self.N = 0;
			self.TurnTime = 0;
			self.IsTurnHorizontal = false;
			self.Root().GetComponent<TimerComponent>()?.Remove(ref self.MoveTimer);

			if (self.tcs != null)
			{
				var tcs = self.tcs;
				self.tcs = null;
				tcs.SetResult(ret);
			}
		}
	}
}
