using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI通用加载组件
    /// 全局唯一 挂在UI管理器下
    /// </summary>
    [ComponentOf(typeof(UIMgrComponent))]
    public partial class UILoadComponent : Entity, IAwake, IDestroy
    {
    }
}
