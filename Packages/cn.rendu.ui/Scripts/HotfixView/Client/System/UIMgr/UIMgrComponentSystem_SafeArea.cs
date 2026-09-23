using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    public static partial class UIMgrComponentSystem
    {
        internal static void InitSafeArea(this UIMgrComponent self)
        {
            var safeAreaX = Math.Max(Screen.safeArea.x, Screen.width - Screen.safeArea.xMax);
            var safeAreaY = Math.Max(Screen.safeArea.y, Screen.height - Screen.safeArea.yMax);

            #if UNITY_EDITOR //调试用 有其他需求自行修改
            safeAreaX = UIConstHelper.Const.SafeAreaX;
            safeAreaY = UIConstHelper.Const.SafeAreaY;
            #endif

            UIMgrComponent.g_SafeArea = new Rect(safeAreaX,
                                                   safeAreaY,
                                                   UIConstHelper.Const.DesignScreenWidth_F - self.GetSafeValue(safeAreaX),
                                                   UIConstHelper.Const.DesignScreenHeight_F - self.GetSafeValue(safeAreaY));

            self.InitUISafeArea();
        }

        private static float GetSafeValue(this UIMgrComponent self, float safeValue)
        {
            return UIConstHelper.Const.DoubleSafe ? safeValue * 2 : safeValue;
        }

        private static void InitUISafeArea(this UIMgrComponent self)
        {
            self.UILayerRoot.anchoredPosition = new Vector2(UIMgrComponent.g_SafeArea.x, -UIMgrComponent.g_SafeArea.y);
            if (UIConstHelper.Const.DoubleSafe)
            {
                self.UILayerRoot.offsetMax = new Vector2(-UIMgrComponent.g_SafeArea.x, self.UILayerRoot.offsetMax.y);
                self.UILayerRoot.offsetMin = new Vector2(self.UILayerRoot.offsetMin.x, UIMgrComponent.g_SafeArea.y);
            }
            else
            {
                //TODO 单边时需要考虑手机是左还是右
                self.UILayerRoot.offsetMax = new Vector2(0, self.UILayerRoot.offsetMax.y);
                self.UILayerRoot.offsetMin = new Vector2(self.UILayerRoot.offsetMin.x, 0);
            }
        }
    }
}
