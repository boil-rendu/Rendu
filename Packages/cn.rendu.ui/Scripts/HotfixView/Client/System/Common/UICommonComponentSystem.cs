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
    [FriendOf(typeof(UICommonComponent))]
    [EntitySystemOf(typeof(UICommonComponent))]
    public static partial class UICommonComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UICommonComponent self)
        {
        }

        [EntitySystem]
        private static void UIInitialize(this UICommonComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this UICommonComponent self)
        {
        }
    }
}
