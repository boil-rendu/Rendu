#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [InitializeOnLoad]
    public static class UIHierarchyIcons
    {
        private static Texture2D m_UILogo;
        private static bool      m_UILogoLoaded;

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

        static UIHierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += ShowIcons;
        }

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