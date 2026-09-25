using System;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
    [ComponentOf]
    public class HashWait : Entity, IAwake, IDestroy
    {
        public Dictionary<long, Promise<EHashWaitError>> m_HashWaitTasks = new();
    }
}