using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
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