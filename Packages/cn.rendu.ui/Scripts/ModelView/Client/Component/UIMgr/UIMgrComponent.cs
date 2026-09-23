using RD.Core;
//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace RD.Client
{
    /// <summary>
    /// 全局唯一的UI管理器
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public partial class UIMgrComponent : Entity, IAwake, IDestroy
    {
        public EntityRef<UIRootComponent> m_RootRef;

        public UIRootComponent Root => m_RootRef;
    }
}
