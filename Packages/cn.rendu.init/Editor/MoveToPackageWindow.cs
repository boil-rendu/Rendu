using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RD.Init.Editor
{
    /// <summary>
    /// "RD/MoveToPackage" 菜单窗口: 扫描 Library/PackageCache 中的本地包,
    /// 按筛选词过滤(留空=全部), 用勾选列表多选, 一键把选中的包从 PackageCache 移入 Packages/
    /// (真正干活的是 <see cref="DependencyResolver.MoveToPackage"/>)。
    ///
    /// Unity 菜单方法只接受"无参"或"单个 MenuCommand", 所以这里用窗口收集选择, 再转发。
    /// </summary>
    public class MoveToPackageWindow : EditorWindow
    {
        private Vector2 m_Scroll;
        private string m_Filter = "";
        private string m_LastFilter = null;
        private string[] m_Packages = new string[0];
        private bool[] m_Selected = new bool[0];

        [MenuItem("RD/Init/MoveToPackage",false, 10)]
        static void Open()
        {
            GetWindow<MoveToPackageWindow>("Move To Package").Show();
        }

        void OnEnable()
        {
            Scan();
        }

        void OnGUI()
        {
            // 筛选词变化即重新扫描; 留空表示不过滤(扫描全部)
            m_Filter = EditorGUILayout.TextField("Filter", m_Filter);
            if (m_Filter != m_LastFilter)
            {
                Scan();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Scan", GUILayout.Width(80)))
            {
                Scan();
            }
            if (m_Packages.Length > 0 && GUILayout.Button("Select All", GUILayout.Width(80)))
            {
                for (int i = 0; i < m_Selected.Length; i++) m_Selected[i] = true;
            }
            if (m_Packages.Length > 0 && GUILayout.Button("Clear", GUILayout.Width(80)))
            {
                for (int i = 0; i < m_Selected.Length; i++) m_Selected[i] = false;
            }
            EditorGUILayout.EndHorizontal();

            if (m_Packages.Length == 0)
            {
                GUILayout.Label("Library/PackageCache 中暂无匹配的包, 清空筛选词或点击 Scan 刷新。");
                return;
            }

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            for (int i = 0; i < m_Packages.Length; i++)
            {
                m_Selected[i] = EditorGUILayout.ToggleLeft(m_Packages[i], m_Selected[i]);
            }
            EditorGUILayout.EndScrollView();

            int count = m_Selected.Count(x => x);
            if (GUILayout.Button($"Move Selected ({count})") && count > 0)
            {
                MoveSelected();
            }
        }

        private void Scan()
        {
            m_LastFilter = m_Filter;

            if (!Directory.Exists("Library/PackageCache"))
            {
                m_Packages = new string[0];
                m_Selected = new bool[0];
                return;
            }

            var dirs = Directory.GetDirectories("Library/PackageCache")
                                .Select(Path.GetFileName)
                                .Where(n => !string.IsNullOrEmpty(n));

            string filter = m_Filter.Trim();
            if (filter.Length > 0)
            {
                dirs = dirs.Where(n => n.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            m_Packages = dirs.OrderBy(n => n).ToArray();
            m_Selected = new bool[m_Packages.Length];
        }

        private void MoveSelected()
        {
            for (int i = 0; i < m_Packages.Length; i++)
            {
                if (!m_Selected[i]) continue;

                string[] ss = m_Packages[i].Split('@');
                string name = ss[0];
                #if UNITY_6000_0_OR_NEWER
                string version = "";
                #else
                string version = ss.Length > 1 ? ss[1] : "";
                #endif

                DependencyResolver.MoveToPackage(name, version);
            }

            AssetDatabase.Refresh();
            Scan(); // 移动后刷新列表
        }
    }
}
