using System;
using System.IO;
using RD.Inspector;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 一条预制体检查结果：记录模块名 / 资源名与各类脚本路径，
    /// 并附带「选中 / 删除 / 忽略 / 取消忽略」四个按钮列，供 CheckPrefabModule 的表格展示
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    public class CheckPrefabData
    {
        #region 信息

        /// <summary>模块名</summary>
        [TableColumnWidth(300, Resizable = false)]
        [VerticalGroup("信息")]
        [LabelText("模块名")]
        [LabelWidth(50)]
        [ReadOnly]
        public string PkgName;

        /// <summary>资源名</summary>
        [TableColumnWidth(300, Resizable = false)]
        [VerticalGroup("信息")]
        [LabelText("资源名")]
        [LabelWidth(50)]
        [ReadOnly]
        public string ResName;

        #endregion

        #region 路径

        /// <summary>手写 Component 脚本路径</summary>
        [TableColumnWidth(400)]
        [VerticalGroup("路径")]
        [ShowInInspector]
        [LabelText("C")]
        [LabelWidth(50)]
        [ReadOnly]
        [RD.Inspector.FilePath]
        public string Component;

        /// <summary>自动生成的 ComponentGen 脚本路径</summary>
        [TableColumnWidth(400)]
        [VerticalGroup("路径")]
        [ShowInInspector]
        [LabelText("CGen")]
        [LabelWidth(50)]
        [ReadOnly]
        [RD.Inspector.FilePath]
        public string ComponentGen;

        /// <summary>手写 System 脚本路径</summary>
        [TableColumnWidth(400)]
        [VerticalGroup("路径")]
        [ShowInInspector]
        [LabelText("S")]
        [LabelWidth(50)]
        [ReadOnly]
        [RD.Inspector.FilePath]
        public string System;

        /// <summary>自动生成的 SystemGen 脚本路径</summary>
        [TableColumnWidth(400)]
        [VerticalGroup("路径")]
        [ShowInInspector]
        [LabelText("SGen")]
        [LabelWidth(50)]
        [ReadOnly]
        [RD.Inspector.FilePath]
        public string SystemGen;

        /// <summary>预制体资源路径</summary>
        [TableColumnWidth(400)]
        [VerticalGroup("路径")]
        [ShowInInspector]
        [LabelText("预制")]
        [LabelWidth(50)]
        [ReadOnly]
        [RD.Inspector.FilePath]
        public string PrefabPath;

        #endregion

        /// <summary>构造：先按资源名查找各类脚本路径，再加载预制体并校正模块名 / 资源名</summary>
        public CheckPrefabData(string prefabPath, string pkgName, string resName)
        {
            PrefabPath = prefabPath;
            PkgName    = pkgName;
            ResName    = resName;
            UpdateData(ResName);
            LoadGameObject();
        }

        /// <summary>在 Project 面板中定位并选中该预制体</summary>
        [VerticalGroup("操作")]
        [HideLabel]
        [TableColumnWidth(100, Resizable = false)]
        [Button(30)]
        [ShowIf("ShowIfSelectPrefab")]
        private void SelectPrefab()
        {
            if (m_GameObject == null) return;
            EditorGUIUtility.PingObject(m_GameObject);
            Selection.activeObject = m_GameObject;
        }

        /// <summary>「选中」按钮的显示条件：预制体已成功加载</summary>
        private bool ShowIfSelectPrefab()
        {
            return m_GameObject != null;
        }

        private bool m_IsDelete;

        /// <summary>表格里的删除按钮（删除后刷新窗口）</summary>
        [VerticalGroup("操作")]
        [GUIColor(1, 0, 0)]
        [HideLabel]
        [TableColumnWidth(100, Resizable = false)]
        [Button(50)]
        [ShowIf("ShowIfIgonre")]
        private void Delete()
        {
            Delete(true, true);
        }

        /// <summary>
        /// 是否可删除：已删过、路径为空、预制体加载失败返回 false；
        /// 没有 CDE 或没有手写 Component 时返回 true（属于冗余资源）
        /// </summary>
        public bool ShowIfDelete()
        {
            if (m_IsDelete)
            {
                return false;
            }

            if (string.IsNullOrEmpty(PrefabPath))
            {
                return false;
            }

            if (m_GameObject == null)
            {
                return false;
            }

            if (m_CDETable == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(Component))
            {
                return true;
            }

            return false;
        }

        /// <summary>删除预制体文件（可控制是否刷新窗口与弹提示），并把 PrefabPath 清空</summary>
        public void Delete(bool refresh, bool tips)
        {
            m_IsDelete = true;

            DeleteFile(PrefabPath);

            if (tips)
            {
                UnityTipsHelper.Show($"删除成功: {PrefabPath}");
            }

            if (refresh)
            {
                AutoTool.CloseWindowRefresh();
            }

            PrefabPath = "";
        }

        /// <summary>删除文件及其 .meta；文件不存在直接返回，IO/权限异常只记日志</summary>
        private void DeleteFile(string path)
        {
            if (!File.Exists(path)) return;
            try
            {
                File.Delete(path);

                var metaPath = $"{path}.meta";
                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
            }
            catch (IOException e)
            {
                Debug.LogError($"无法删除文件: \n{path}\n{e}");
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError($"没有权限删除文件: \n{path}\n{e}");
            }
        }

        /// <summary>
        /// 按资源名搜索同名脚本，按文件名后缀归类：
        /// SystemGen / System / Gen 分别对应生成与手写版本，其余归到手写 Component
        /// </summary>
        private void UpdateData(string resName)
        {
            Component    = "";
            ComponentGen = "";
            System       = "";
            SystemGen    = "";
            var componentName = $"{resName}Component";
            foreach (string guid in AssetDatabase.FindAssets($"{componentName} t:Script", null))
            {
                var path        = AssetDatabase.GUIDToAssetPath(guid);
                var fileNameAll = Path.GetFileName(path);
                if (string.IsNullOrEmpty(fileNameAll)) continue;

                var fileName = fileNameAll.Split('.')[0];

                if (fileName.EndsWith("SystemGen"))
                {
                    SystemGen = path;
                }
                else if (fileName.EndsWith("System"))
                {
                    System = path;
                }
                else if (fileName.EndsWith("Gen"))
                {
                    ComponentGen = path;
                }
                else
                {
                    Component = path;
                }
            }
        }

        private GameObject     m_GameObject;
        private UIBindCDETable m_CDETable;
        /// <summary>该预制体上是否挂了 UIBindCDETable</summary>
        public  bool           IsCDETable => m_CDETable != null;

        /// <summary>加载预制体；若挂有 UIBindCDETable，则以组件上的模块名 / 资源名为准并提示不一致</summary>
        private void LoadGameObject()
        {
            m_CDETable   = null;
            m_GameObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (m_GameObject != null)
            {
                m_CDETable = m_GameObject.GetComponent<UIBindCDETable>();
                if (m_CDETable != null)
                {
                    if (PkgName != m_CDETable.PkgName)
                    {
                        Debug.LogError($"{PrefabPath} PkgName不匹配 {PkgName} != {m_CDETable.PkgName}");
                        PkgName = m_CDETable.PkgName;
                    }

                    if (ResName != m_CDETable.ResName)
                    {
                        Debug.LogError($"{PrefabPath} ResName不匹配 {ResName} != {m_CDETable.ResName}");
                        ResName = m_CDETable.ResName;
                    }
                }
            }
        }

        private bool m_IsIgnore;

        /// <summary>忽略该检查项（从「可删除」结果中移出）</summary>
        [VerticalGroup("操作")]
        [HideLabel]
        [TableColumnWidth(100, Resizable = false)]
        [Button(50)]
        [ShowIf("ShowIfIgonre")]
        private void Igonre()
        {
            m_IsIgnore = true;
        }

        /// <summary>「忽略」按钮与「删除」按钮的显示条件：可删除且尚未被忽略</summary>
        public bool ShowIfIgonre()
        {
            return ShowIfDelete() && !m_IsIgnore;
        }

        /// <summary>取消忽略</summary>
        [VerticalGroup("操作")]
        [HideLabel]
        [TableColumnWidth(100, Resizable = false)]
        [Button(50)]
        [ShowIf("ShowIfReIgonre")]
        private void ReIgonre()
        {
            m_IsIgnore = false;
        }

        /// <summary>「取消忽略」按钮的显示条件</summary>
        public bool ShowIfReIgonre()
        {
            return m_IsIgnore;
        }
    }
}