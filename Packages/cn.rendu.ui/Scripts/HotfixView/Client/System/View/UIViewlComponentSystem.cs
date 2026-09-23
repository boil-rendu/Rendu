using RD.Core;
//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace RD.Client
{
    /// <summary>
    /// UI面板组件
    /// </summary>
    [FriendOf(typeof(UIViewComponent))]
    [EntitySystemOf(typeof(UIViewComponent))]
    public static partial class UIViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIViewComponent self)
        {
        }

        [EntitySystem]
        private static void UIInitialize(this UIViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this UIViewComponent self)
        {
        }
    }
}
