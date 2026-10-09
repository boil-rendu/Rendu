using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIMgrComponent 扩展（刘海屏 / 安全区适配）</summary>
    public static partial class UIMgrComponentSystem
    {
        /// <summary>
        /// 计算并应用安全区：取屏幕左右/上下安全边距的较大值（保证横竖屏都安全）
        /// 编辑器下直接用 UIConstHelper 里配置的 SafeAreaX / SafeAreaY 方便调试
        /// </summary>
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

        /// <summary>双击（DoubleSafe）模式下安全值需要翻倍</summary>
        private static float GetSafeValue(this UIMgrComponent self, float safeValue)
        {
            return UIConstHelper.Const.DoubleSafe ? safeValue * 2 : safeValue;
        }

        /// <summary>
        /// 把安全区应用到 UI 层级根节点
        /// DoubleSafe（左右双边）时两侧都留边距；否则只做单边适配
        /// </summary>
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
