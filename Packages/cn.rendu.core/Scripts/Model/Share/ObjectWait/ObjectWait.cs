using System;
using System.Collections.Generic;
using RD.Core;
using Object = RD.Core.Object;

namespace RD
{
	/// <summary>等待结果的错误码常量（与 <see cref="EHashWaitError"/> 类似但更轻量：只表示「成功/销毁/取消/超时」）。</summary>
	public static class WaitTypeError
	{
		/// <summary>成功。</summary>
		public const int Success = 0;
		/// <summary>等待结构被销毁。</summary>
		public const int Destroy = 1;
		/// <summary>主动取消。</summary>
		public const int Cancel = 2;
		/// <summary>超时。</summary>
		public const int Timeout = 3;
	}

	/// <summary>带 <see cref="Error"/> 字段的等待结果 marker 接口，配合 <see cref="ResultCallback{K}"/> 使用。</summary>
	public interface IWaitType
	{
		int Error
		{
			get;
			set;
		}
	}


	/// <summary>「等待结束」行为接口，由 <see cref="ResultCallback{K}"/> 在销毁/取消时回退到默认值。</summary>
	public interface IDestroyRun
	{
		void SetResult();
	}

	/// <summary>
	/// 通用等待回调：构造时 <see cref="Promise{K}.Create(bool)"/>（池化）一个 tcs，业务侧 <c>await tcs.Task</c>。
	/// 成功路径：<see cref="SetResult(K)"/> 写入结果；销毁/取消路径：<see cref="SetResult()"/> 写入 <c>Error = WaitTypeError.Destroy</c> 的默认值。
	/// </summary>
	/// <typeparam name="K">返回结构体类型（需实现 <see cref="IWaitType"/>）。</typeparam>
	public class ResultCallback<K>: Object, IDestroyRun where K : struct, IWaitType
	{
		private Promise<K> tcs;

		public ResultCallback()
		{
			this.tcs = Promise<K>.Create(true);
		}

		/// <summary>是否已 Set 过（成功或销毁），用于重复调防护。</summary>
		public bool IsDisposed
		{
			get
			{
				return this.tcs == null;
			}
		}

		/// <summary>对外暴露的 <see cref="Promise{K}"/>：业务 <c>await this.Task</c> 阻塞。</summary>
		public Promise<K> Task => this.tcs;

		/// <summary>成功路径：写 <paramref name="k"/> 到 tcs。</summary>
		public void SetResult(K k)
		{
			var t = tcs;
			this.tcs = null;
			t.SetResult(k);
		}

		/// <summary>销毁/取消路径：写 <c>new K() { Error = WaitTypeError.Destroy }</c> 到 tcs（实现 <see cref="IDestroyRun"/>）。</summary>
		public void SetResult()
		{
			var t = tcs;
			this.tcs = null;
			t.SetResult(new K() { Error = WaitTypeError.Destroy });
		}
	}

	/// <summary>
	/// Type 维度等待容器：与 <see cref="HashWait"/>（hash 维度）平行使用，按 <see cref="Type"/> 为 key 维护一组 <c>object</c> 等待句柄（实际是 <see cref="ResultCallback{K}"/> 等）。
	/// 通常用于「等某个特定类型实例就绪」场景（如某个 Manager、某个 Config）。
	/// </summary>
	[ComponentOf]
	public class ObjectWait: Entity, IAwake, IDestroy
	{
		/// <summary>Type → 等待句柄（一般是 <see cref="ResultCallback{K}"/>）字典。</summary>
		public Dictionary<Type, object> tcss = new Dictionary<Type, object>();
	}
}
