using RD.Core;
//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace RD.Client
{
    /// <summary>
    /// 当前场景的UI管理器
    /// </summary>
    [ComponentOf]
    public partial class UIRootComponent : Entity, IAwake
    {
        public EntityRef<UIMgrComponent> m_UIMgrRef;
        public UIMgrComponent UIMgr => m_UIMgrRef;
    }
}