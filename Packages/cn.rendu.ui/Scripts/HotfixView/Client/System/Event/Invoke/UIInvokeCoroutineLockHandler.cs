using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 协程锁：按 Lock 值在 CoroutineLockComponent 上排队，用于串行化同一资源的加载/切换
    /// LockType &lt;= 0 时使用默认的 CoroutineLockType.UIInvokeCoroutineLock
    /// </summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeCoroutineLockHandler : AInvokeEntityHandler<UIInvokeEntity_CoroutineLock, Promise<Entity>>
    {
        public override async Promise<Entity> Handle(Entity entity, UIInvokeEntity_CoroutineLock args)
        {
            var root = entity?.Root();
            if (root == null)
            {
                Log.Error($"没有找到root {entity.IsDisposed}");
                return null;
            }

            var lockType = args.LockType;
            if (lockType <= 0)
            {
                lockType = CoroutineLockType.UIInvokeCoroutineLock;
            }

            return await root.GetComponent<CoroutineLockComponent>().Wait(lockType, args.Lock);
        }
    }
}