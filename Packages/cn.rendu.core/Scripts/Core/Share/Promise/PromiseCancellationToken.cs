using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RD.Core
{
    /// <summary>
    /// Promise 取消令牌：<see cref="Cancel"/> 一次性触发所有 <see cref="Add"/> 注册的回调，回调内部抛出的异常走 <see cref="Promise.ExceptionHandler"/>。
    /// 与 BCL <see cref="System.Threading.CancellationToken"/> 的区别：一次性（Cancel 后 <see cref="IsDispose"/> = true，不可复用），由本仓库协程体系统一消费。
    /// </summary>
    public class PromiseCancellationToken
    {
        private HashSet<Action> actions = new();

        /// <summary>注册一个取消回调：<c>null</c> 是非法状态（业务侧必须显式给一个 cleanup），底层 <see cref="HashSet{T}"/> 遇到 <c>null</c> 会抛 <see cref="ArgumentNullException"/> 用于早发现协程泄漏。</summary>
        public void Add(Action callback)
        {
            // 如果action是null，绝对不能添加,要抛异常，说明有协程泄漏
            this.actions.Add(callback);
        }

        /// <summary>移除一个已注册的取消回调（<c>null</c> 安全的空操作）。</summary>
        public void Remove(Action callback)
        {
            this.actions?.Remove(callback);
        }

        /// <summary>token 是否已触发取消（即不可再用的「已释放」状态）。</summary>
        public bool IsDispose()
        {
            return this.actions == null;
        }

        /// <summary>触发取消：一次性把当前 <see cref="actions"/> 全部调用，然后置 <c>null</c> 标记 <see cref="IsDispose"/>。重复调用安全（短路返回）。</summary>
        public void Cancel()
        {
            if (this.actions == null)
            {
                return;
            }

            this.Invoke();
        }

        private void Invoke()
        {
            HashSet<Action> runActions = this.actions;
            this.actions = null;
            try
            {
                foreach (Action action in runActions)
                {
                    action.Invoke();
                }
            }
            catch (Exception e)
            {
                Promise.ExceptionHandler.Invoke(e);
            }
        }
    }
}
