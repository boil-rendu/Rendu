using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    public static partial class UIMgrComponentSystem
    {
        /// <summary>
        /// 场景里 UIRoot 是 UIRoot.prefab 的实例 直接挂在 Global 下 路径 /Global/UIRoot
        /// Global 已由 Init.cs 设置 DontDestroyOnLoad, UIRoot 挂在其下即可跨场景常驻
        /// 兜底: 场景里没摆时运行时实例化出来的挂在 /Global 下
        /// </summary>
        private const string UIRootPath = "/Global/UIRoot";
        private const string GlobalPath = "/Global";

        internal static async Promise<bool> InitRoot(this UIMgrComponent self)
        {
            #region UICanvasRoot 查找各种组件

            EntityRef<UIMgrComponent> selfRef = self;
            // 场景里摆的是 /Global/UIRoot 预制体实例 按路径优先找 找不到再退回按名字找
            self.UIRoot = GameObject.Find(UIRootPath);
            if (self.UIRoot == null)
            {
                self.UIRoot = GameObject.Find(UIConstHelper.Const.UIRootName);
            }

            if (self.UIRoot == null)
            {
                self.UIRoot = await self.GetComponent<UILoadComponent>().LoadAssetAsyncInstantiate(UIConstHelper.Const.UIRootPkgName, UIConstHelper.Const.UIRootName);
            }

            self = selfRef;
            if (self.UIRoot == null)
            {
                Debug.LogError($"初始化错误 没有找到UIRoot");
                return false;
            }

            self.UIRoot.name = self.UIRoot.name.Replace("(Clone)", "");
            self.InitUIRootSlot();

            //root可修改位置防止与世界3D场景重叠导致不好编辑
            self.UIRoot.transform.localPosition = new Vector3(UIConstHelper.Const.RootPosOffset, UIConstHelper.Const.RootPosOffset, 0);

            self.UICanvas = self.UIRoot.GetComponentInChildren<Canvas>();
            if (self.UICanvas == null)
            {
                Debug.LogError($"初始化错误 没有找到Canvas");
                return false;
            }

            self.UICanvasRoot = self.UICanvas.gameObject;

            self.UILayerRoot = self.UICanvasRoot.transform.FindChildByName(UIConstHelper.Const.UILayerRootName)?.GetComponent<RectTransform>();
            if (self.UILayerRoot == null)
            {
                Debug.LogError($"初始化错误 没有找到UILayerRoot");
                return false;
            }

            self.UICamera = self.UICanvasRoot.GetComponentInChildren<Camera>();
            if (self.UICamera == null)
            {
                Debug.LogError($"初始化错误 没有找到UICamera");
                return false;
            }

            var canvas = self.UICanvasRoot.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError($"初始化错误 没有找到UICanvasRoot - Canvas");
                return false;
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = self.UICamera;

            var canvasScaler = self.UICanvasRoot.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                Debug.LogError($"初始化错误 没有找到UICanvasRoot - CanvasScaler");
                return false;
            }

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(UIConstHelper.Const.DesignScreenWidth, UIConstHelper.Const.DesignScreenHeight);

            #endregion

            //分层
            const int len = (int)EPanelLayer.Count;
            self.m_AllPanelLayer.Clear();
            for (var i = len - 1; i >= 0; i--)
            {
                var layer = new GameObject($"Layer{i}-{(EPanelLayer)i}");
                var rect = layer.AddComponent<RectTransform>();
                rect.SetParent(self.UILayerRoot);
                rect.localScale = Vector3.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchorMax = Vector2.one;
                rect.anchorMin = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localPosition = new Vector3(0, 0, i * UIConstHelper.Const.LayerDistance);
                var rectDic = new Dictionary<RectTransform, List<PanelInfo>> { { rect, new List<PanelInfo>() } };
                self.m_AllPanelLayer.Add((EPanelLayer)i, rectDic);
            }

            self.GetLayerRect(EPanelLayer.Cache).gameObject.SetActive(false);

            self.InitAddUIBlock(); //所有层级初始化后添加一个终极屏蔽层 可根据API 定时屏蔽UI操作

            // ReSharper disable once Unity.InefficientPropertyAccess
            self.UICamera.transform.localPosition = new Vector3(self.UILayerRoot.localPosition.x, self.UILayerRoot.localPosition.y, -1000);

            // URP 不支持 CameraClearFlags.Depth(会被当作 SolidColor 用背景色清屏, 盖掉主相机画面);
            // Nothing 在桌面端只清深度, 保留主相机输出. 移动端(GLES)会连颜色一起清, 届时需改用相机堆栈(Overlay)
            self.UICamera.clearFlags = CameraClearFlags.Nothing;
            self.UICamera.orthographic = true;

            //根据需求可以修改摄像机的远裁剪平面大小 没必要设置的很大
            //UICamera.farClipPlane = ((len + 1) * UIMgrComponent.LayerDistance) * UICanvasRoot.transform.localScale.x;

            return true;
        }

        /// <summary>
        /// 场景里已摆好的不动(本来就在 /Global 下); 运行时实例化出来的(没有父节点)才挂到 /Global
        /// 不再对 UIRoot 单独 DontDestroyOnLoad: 非根对象调用无效, 且会把它搬出场景层级
        /// </summary>
        internal static void InitUIRootSlot(this UIMgrComponent self)
        {
            var root = self.UIRoot.transform;
            if (root.parent != null) return;

            var global = GameObject.Find(GlobalPath);
            if (global == null)
            {
                Debug.LogWarning($"场景中没有找到 {GlobalPath} 节点, UIRoot 将保持在场景根节点下");
                return;
            }

            root.SetParent(global.transform, false);
        }

        internal static void ResetRoot(this UIMgrComponent self)
        {
            if (self.UILayerRoot == null) return;
            for (int i = self.UILayerRoot.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(self.UILayerRoot.transform.GetChild(i).gameObject);
            }
        }

        internal static bool ContainsLayerPanelInfo(this UIMgrComponent self, EPanelLayer panelLayer, PanelInfo panelInfo)
        {
            var list = self.GetLayerPanelInfoList(panelLayer);
            return list.Contains(panelInfo);
        }

        internal static bool RemoveLayerPanelInfo(this UIMgrComponent self, EPanelLayer panelLayer, PanelInfo panelInfo)
        {
            var list = self.GetLayerPanelInfoList(panelLayer);
            return list != null && list.Remove(panelInfo);
        }

        internal static List<PanelInfo> GetLayerPanelInfoList(this UIMgrComponent self, EPanelLayer panelLayer)
        {
            self.m_AllPanelLayer.TryGetValue(panelLayer, out var rectDic);
            if (rectDic == null)
            {
                Debug.LogError($"没有这个层级 请检查 {panelLayer}");
                return null;
            }

            //只能有一个所以返回第一个
            foreach (var infoList in rectDic.Values)
            {
                return infoList;
            }

            return null;
        }

        /// <summary>
        /// 获取一个层级对象
        /// </summary>
        /// <param name="self"></param>
        /// <param name="panelLayer"></param>
        /// <returns></returns>
        public static RectTransform GetLayerRect(this UIMgrComponent self, EPanelLayer panelLayer)
        {
            self.m_AllPanelLayer.TryGetValue(panelLayer, out var rectDic);
            if (rectDic == null)
            {
                Debug.LogError($"没有这个层级 请检查 {panelLayer}");
                return null;
            }

            //只能有一个所以返回第一个
            foreach (var rect in rectDic.Keys)
            {
                return rect;
            }

            return null;
        }
    }
}