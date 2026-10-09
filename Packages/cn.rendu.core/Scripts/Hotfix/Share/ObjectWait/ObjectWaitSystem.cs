using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="ObjectWait"/> 的系统扩展（Hotfix/Share）：按 Type 维度等待某事件完成。
	/// 行为：<see cref="Wait{T}"/> 注册 (Type → <see cref="ResultCallback{T}"/>)；<see cref="Notify{T}(ObjectWait, T)"/> 找到并 SetResult；Destroy 时所有等待者按 <see cref="WaitTypeError.Destroy"/> 唤醒。
	/// 关联 <see cref="PromiseCancellationToken"/> 实现可取消（Cancel 时回 <see cref="WaitTypeError.Cancel"/>）。
	/// </summary>
	[EntitySystemOf(typeof(ObjectWait))]
	public static partial class ObjectWaitSystem
	{
		[EntitySystem]
		private static void Awake(this ObjectWait self)
		{
			self.tcss.Clear();
		}

		[EntitySystem]
		private static void Destroy(this ObjectWait self)
		{
			foreach (object v in self.tcss.Values.ToArray())
			{
				((IDestroyRun) v).SetResult();
			}
		}

		/// <summary>等待 Type = <typeparamref name="T"/> 的事件：建 <see cref="ResultCallback{T}"/> + 注册到字典 + 关联当前 <see cref="PromiseCancellationToken"/> → <c>await tcs.Task</c>。Cancel 时回 <see cref="WaitTypeError.Cancel"/>。</summary>
		public static async Promise<T> Wait<T>(this ObjectWait self) where T : struct, IWaitType
		{
			ResultCallback<T> tcs = new ResultCallback<T>();
			PromiseCancellationToken cancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
			self.tcss.Add(typeof (T), tcs);

			void CancelAction()
			{
				self.Notify(new T() { Error = WaitTypeError.Cancel });
			}

			T ret;
			try
			{
				cancellationToken?.Add(CancelAction);
				ret = await tcs.Task;
			}
			finally
			{
				cancellationToken?.Remove(CancelAction);
			}
			return ret;
		}

		/// <summary>触发 Type = <typeparamref name="T"/> 的等待者：按 Type 查字典 → 移除 → 写结果。无等待者静默 return。</summary>
		public static void Notify<T>(this ObjectWait self, T obj) where T : struct, IWaitType
		{
			Type type = typeof (T);
			if (!self.tcss.TryGetValue(type, out object tcs))
			{
				return;
			}

			self.tcss.Remove(type);
			((ResultCallback<T>) tcs).SetResult(obj);
		}
	}
}
