using System;
using System.Collections.Generic;
using RD.Core;
using Unity.Mathematics;

namespace RD
{
	/// <summary>
	/// Unit 移动组件：挂在 <see cref="Unit"/> 上，按 <see cref="Targets"/> 路径点序列匀速移动，过程中通过 <see cref="MoveTimer"/> 定时器轮转推进。
	/// 业务侧 <c>await moveComponent.MoveTo(targets)</c> 等 <see cref="tcs"/> 完成。
	/// </summary>
	[ComponentOf(typeof(Unit))]
	public class MoveComponent: Entity, IAwake, IDestroy
	{
		/// <summary>当前路径中上一个目标点（<c>Targets[N-1]</c>，当 <c>N = 0</c> 时抛 <see cref="ArgumentOutOfRangeException"/>）。</summary>
		public float3 PreTarget
		{
			get
			{
				return this.Targets[this.N - 1];
			}
		}

		/// <summary>当前路径中下一个目标点（<c>Targets[N]</c>）。</summary>
		public float3 NextTarget
		{
			get
			{
				return this.Targets[this.N];
			}
		}

		// 开启移动协程的时间
		/// <summary>开启移动协程的时间（Unix 毫秒）。</summary>
		public long BeginTime;

		// 每个点的开始时间
		/// <summary>当前路径点的开始时间（Unix 毫秒），用于每段速度计算。</summary>
		public long StartTime { get; set; }

		// 开启移动协程的Unit的位置
		/// <summary>移动开始时 Unit 的位置（即 <c>Targets[0]</c>，方便回放/校正）。</summary>
		public float3 StartPos;

		/// <summary>当前位置（<c>Targets[0]</c>，首点 = 真实位置）。</summary>
		public float3 RealPos
		{
			get
			{
				return this.Targets[0];
			}
		}

		private long needTime;

		/// <summary>完成整段路径所需的总时长（毫秒）。</summary>
		public long NeedTime
		{
			get
			{
				return this.needTime;
			}
			set
			{
				this.needTime = value;
			}
		}

		/// <summary>移动定时器句柄（按 <see cref="TimerInvokeType.MoveTimer"/> 周期推进）。</summary>
		public long MoveTimer;

		/// <summary>移动速度（米/秒）。</summary>
		public float Speed; // m/s

		/// <summary>完成等待句柄：业务 <c>await this.tcs</c> 阻塞到路径走完/被打断。</summary>
		public Promise<bool> tcs;

		/// <summary>路径点列表（首点 = 当前位置）。</summary>
		public List<float3> Targets = new List<float3>();

		/// <summary>最终目标点（<c>Targets[Targets.Count-1]</c>）。</summary>
		public float3 FinalTarget
		{
			get
			{
				return this.Targets[this.Targets.Count - 1];
			}
		}

		/// <summary>当前路径点索引（0 表示在首点）。</summary>
		public int N;

		/// <summary>每段路径之间转向所需时间（毫秒）。</summary>
		public int TurnTime;

		/// <summary>转向是否仅绕 Y 轴（水平转）。</summary>
		public bool IsTurnHorizontal;

		/// <summary>起始旋转。</summary>
		public quaternion From;

		/// <summary>目标旋转。</summary>
		public quaternion To;
	}
}
