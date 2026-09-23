using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI通用等待
    /// </summary>
    [ComponentOf(typeof(UIWindowComponent))]
    public class UIWaitComponent : Entity, IAwake<long>, IDestroy, IUIWindowClose
    {
        public bool m_IsWaitCompleted;
        public long m_WaitId;
    }
}