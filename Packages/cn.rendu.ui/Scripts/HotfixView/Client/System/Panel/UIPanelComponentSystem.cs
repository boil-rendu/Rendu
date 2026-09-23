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
    [FriendOf(typeof(UIPanelComponent))]
    [EntitySystemOf(typeof(UIPanelComponent))]
    public static partial class UIPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIPanelComponent self)
        {
            self.UIBindVo = self.UIBase.UIBindVo;
        }

        [EntitySystem]
        private static void UIInitialize(this UIPanelComponent self)
        {
            self.InitPanelViewData();
        }

        [EntitySystem]
        private static void Destroy(this UIPanelComponent self)
        {
            self.StopCountDownDestroyPanel();

            EventSystem.Instance?.UIInvokeEntitySyncSafety(self, new UIInvokeEntity_DestroyPanel
            {
                PanelName = self.UIBindVo.ComponentType.Name
            });
        }
    }
}