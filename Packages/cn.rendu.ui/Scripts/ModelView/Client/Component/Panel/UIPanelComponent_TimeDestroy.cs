using System;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 倒计时摧毁
    /// </summary>
    public partial class UIPanelComponent
    {
        public float CachePanelTime = 10;

        public PromiseCancellationToken m_Token;
    }
}
