#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>在 Hierarchy 窗口给带 UIBindCDETable 的节点右侧画一个 UI 图标</summary>
    [InitializeOnLoad]
    public static class HierarchyIcons
    {
        private static Texture2D m_UILogo;
        private static bool      m_UILogoLoaded;

        /// <summary>UI 图标（首次访问时从 Resources 加载并缓存，只尝试一次）</summary>
        public static Texture2D UILogo
        {
            get
            {
                if (m_UILogoLoaded)
                {
                    return m_UILogo;
                }

                m_UILogoLoaded = true;
                return m_UILogo = Resources.Load<Texture2D>("UIIcon");
            }
        }

        static HierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += ShowIcons;
        }

        /// <summary>Hierarchy 每项绘制回调：是 UI 节点则在最右侧画图标</summary>
        private static void ShowIcons(int ID, Rect r)
        {
            var go = EditorUtility.InstanceIDToObject(ID) as GameObject;
            if (go == null)
            {
                return;
            }

            if (go.GetComponent<UIBindCDETable>() != null)
            {
                var logo = UILogo;
                if (logo == null)
                {
                    return;
                }

                r.x     = r.xMax - 16;
                r.width = 16;
                GUI.DrawTexture(r, logo);
            }
        }
    }
}

#endif