using System.Collections.Generic;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIMgrComponent 的根节点引用与层级结构部分</summary>
    public partial class UIMgrComponent
    {
        /// <summary>UI 世界根节点</summary>
        public GameObject UIRoot { get; set; }
        /// <summary>Canvas 根节点</summary>
        public GameObject UICanvasRoot { get; set; }
        /// <summary>层级根节点（安全区适配的作用对象）</summary>
        public RectTransform UILayerRoot { get; set; }
        /// <summary>UI 专用相机</summary>
        public Camera UICamera { get; set; }
        /// <summary>UI 根 Canvas</summary>
        public Canvas UICanvas { get; set; }

        //K1 = 层级枚举 V1 = 层级对应的rect
        //List = 当前层级中的当前所有UI 前面的代表这个UI在前面以此类推
        /// <summary>
        /// 全部层级数据：层级枚举 → （层级 Rect → 该层级下的 UI 列表）
        /// 列表顺序即显示顺序，靠前的排在更前面
        /// </summary>
        public readonly Dictionary<EPanelLayer, Dictionary<RectTransform, List<PanelInfo>>> m_AllPanelLayer = new();

        #region 快捷获取层级

        private RectTransform m_UICache;

        /// <summary>缓存层级的 Rect（首次访问时从层级字典里取并缓存）</summary>
        public RectTransform UICache
        {
            get
            {
                if (m_UICache == null)
                {
                    m_AllPanelLayer.TryGetValue(EPanelLayer.Cache, out var rectDic);
                    if (rectDic == null)
                    {
                        Log.Error($"没有这个层级 请检查 {EPanelLayer.Cache}");
                        return null;
                    }

                    foreach (var rect in rectDic)
                    {
                        m_UICache = rect.Key;
                        break;
                    }

                    return m_UICache;
                }

                return m_UICache;
            }
        }

        private RectTransform m_UIPanel;

        /// <summary>面板层级的 Rect（首次访问时从层级字典里取并缓存）</summary>
        public RectTransform UIPanel
        {
            get
            {
                if (m_UIPanel == null)
                {
                    m_AllPanelLayer.TryGetValue(EPanelLayer.Panel, out var rectDic);
                    if (rectDic == null)
                    {
                        Log.Error($"没有这个层级 请检查 {EPanelLayer.Panel}");
                        return null;
                    }

                    foreach (var rect in rectDic)
                    {
                        m_UIPanel = rect.Key;
                        break;
                    }

                    return m_UIPanel;
                }

                return m_UIPanel;
            }
        }

        #endregion
    }
}