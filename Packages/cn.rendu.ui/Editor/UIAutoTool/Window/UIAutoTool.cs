//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using RD.Inspector;
using RD.Inspector.Editor;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// UI 自动化工具
    /// </summary>
    public class UIAutoTool : InspectorMenuEditorWindow
    {
        [MenuItem("Rendu/UI 自动化工具")]
        public static void OpenWindow()
        {
            var window = GetWindow<UIAutoTool>("UI");
            if (window != null)
            {
                window.Show();
            }
        }

        //[MenuItem("RD/关闭 UI 自动化工具")]
        //错误时使用的 面板出现了错误 会导致如何都打不开 就需要先关闭
        public static void CloseWindow()
        {
            GetWindow<UIAutoTool>()?.Close();
        }

        //关闭后刷新资源
        public static void CloseWindowRefresh()
        {
            CloseWindow();
            AssetDatabase.SaveAssets();

            //AssetDatabase.Refresh();//下面的刷新更NB
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }

        private RD.Inspector.Editor.FloatPrefs m_MenuWidthPrefs = new("UIAutoTool_MenuWidth", null, 300f);

        public override float MenuWidth
        {
            get { return m_MenuWidth; }
            set { m_MenuWidth = value; }
        }

        private InspectorMenuTree m_MenuTree;
        private List<BaseTreeMenuItem> m_AllMenuItem = new List<BaseTreeMenuItem>();

        protected override InspectorMenuTree BuildMenuTree()
        {
            m_MenuTree = new InspectorMenuTree();
            m_MenuTree.Selection.SelectionChanged += OnSelectionChanged;

            m_AllMenuItem.Clear();

            m_AllMenuItem.Add(new TreeMenuItem<UIPublishModule>(this, m_MenuTree, UIPublishModule.m_PublishName, InspectorIcons.UnityFolderIcon));

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allAutoMenus = new List<UIAutoMenuData>();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type.IsDefined(typeof(UIAutoMenuAttribute), false))
                    {
                        UIAutoMenuAttribute attribute = (UIAutoMenuAttribute)Attribute.GetCustomAttribute(type, typeof(UIAutoMenuAttribute));
                        allAutoMenus.Add(new UIAutoMenuData
                        {
                            Type = type,
                            MenuName = attribute.MenuName,
                            Order = attribute.Order
                        });
                    }
                }
            }

            allAutoMenus.Sort((a, b) => a.Order.CompareTo(b.Order));

            foreach (var attribute in allAutoMenus)
            {
                m_AllMenuItem.Add(NewTreeMenuItem(attribute.Type, attribute.MenuName));
            }

            m_MenuTree.Add("全局设置", this, InspectorIcons.SettingsCog);

            return m_MenuTree;
        }

        private BaseTreeMenuItem NewTreeMenuItem(Type moduleType, string moduleName)
        {
            var treeMenuItemType = typeof(TreeMenuItem<>);

            var specificTreeMenuItemType = treeMenuItemType.MakeGenericType(moduleType);

            var constructor = specificTreeMenuItemType.GetConstructor(new Type[]
            {
                typeof(InspectorMenuEditorWindow),
                typeof(InspectorMenuTree),
                typeof(string)
            });

            object treeMenuItem = constructor.Invoke(new object[]
            {
                this,
                m_MenuTree,
                moduleName
            });

            return (BaseTreeMenuItem)treeMenuItem;
        }

        private bool m_FirstInit = true;
        private RD.Inspector.Editor.StringPrefs m_LastSelectMenuPrefs = new RD.Inspector.Editor.StringPrefs("UIAutoTool_LastSelectMenu", null, "全局设置");
        private RD.Inspector.Editor.StringPrefs m_LastSelectToolModulePathPrefs = new RD.Inspector.Editor.StringPrefs("UIAutoTool_LastSelectToolModulePath", null, "");
        private readonly HashSet<InspectorMenuItem> m_FirstSelect = new();
        private bool m_RestoringSelection;

        private void OnSelectionChanged(SelectionChangedType obj)
        {
            if (m_RestoringSelection)
            {
                return;
            }

            if (obj != SelectionChangedType.ItemAdded)
            {
                return;
            }

            if (m_FirstInit)
            {
                m_FirstInit = false;

                if (!string.IsNullOrEmpty(m_LastSelectToolModulePathPrefs.Value))
                {
                    m_RestoringSelection = true;
                    var restored = SelectChainByPath(m_LastSelectToolModulePathPrefs.Value);
                    m_RestoringSelection = false;
                    if (restored != null)
                    {
                        return;
                    }
                }

                foreach (var menu in m_MenuTree.MenuItems)
                {
                    if (menu.Name != m_LastSelectMenuPrefs.Value) continue;
                    menu.Select();
                    menu.Toggled = true;
                    break;
                }

                return;
            }

            if (m_MenuTree.Selection.Count > 1)
            {
                Debug.LogError($"不可能同时选多个: {m_MenuTree.Selection.Count}");
                return;
            }

            var selectedMenuItem = m_MenuTree.Selection[0];

            if (selectedMenuItem.Value is BaseTreeMenuItem menuItem)
            {
                menuItem.SelectionMenu();
                m_LastSelectToolModulePathPrefs.Value = menuItem.ModuleName;
            }
            else if (selectedMenuItem.Value is BaseUIToolModule module)
            {
                module.SelectionMenu();
                m_LastSelectToolModulePathPrefs.Value = module.ModuleName;
            }

            if (m_FirstSelect.Add(selectedMenuItem))
            {
                selectedMenuItem.Toggled = true;
            }
            else
            {
                selectedMenuItem.Toggled = !selectedMenuItem.Toggled;
            }

            foreach (var menu in m_MenuTree.MenuItems)
            {
                if (!menu.IsSelected) continue;
                m_LastSelectMenuPrefs.Value = menu.Name;
                break;
            }

            var root = selectedMenuItem;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            m_LastSelectMenuPrefs.Value = root.Name;
        }

        private InspectorMenuItem FindMenuItemByModulePath(string modulePath)
        {
            foreach (var root in m_MenuTree.MenuItems)
            {
                var result = FindRecursive(root, modulePath);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private InspectorMenuItem FindRecursive(InspectorMenuItem item, string modulePath)
        {
            if (item.Value is BaseTreeMenuItem bt && bt.ModuleName == modulePath) return item;
            if (item.Value is BaseUIToolModule bm && bm.ModuleName == modulePath) return item;
            foreach (var child in item.ChildMenuItems)
            {
                var res = FindRecursive(child, modulePath);
                if (res != null) return res;
            }

            return null;
        }

        private void ExpandParents(InspectorMenuItem item)
        {
            var cur = item.Parent;
            while (cur != null)
            {
                cur.Toggled = true;
                cur = cur.Parent;
            }

            item.Toggled = true;
        }

        private InspectorMenuItem SelectChainByPath(string modulePath)
        {
            if (string.IsNullOrEmpty(modulePath))
            {
                return null;
            }

            var segments = modulePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return null;
            }

            InspectorMenuItem current = null;

            foreach (var root in m_MenuTree.MenuItems)
            {
                if (root.Name == segments[0])
                {
                    current = root;
                    break;
                }
            }

            if (current == null)
            {
                return null;
            }

            SelectAndInit(current);
            ExpandParents(current);

            for (int i = 1; i < segments.Length; i++)
            {
                var nextName = segments[i];
                InspectorMenuItem next = null;
                foreach (var child in current.ChildMenuItems)
                {
                    if (child.Name == nextName)
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null)
                {
                    SelectAndInit(current);
                    foreach (var child in current.ChildMenuItems)
                    {
                        if (child.Name == nextName)
                        {
                            next = child;
                            break;
                        }
                    }
                }

                if (next == null)
                {
                    return null;
                }

                current = next;
                SelectAndInit(current);
                ExpandParents(current);
            }

            return current;
        }

        public void SelectModule(string moduleName)
        {
            if (m_MenuTree == null) return;

            foreach (var root in m_MenuTree.MenuItems)
            {
                if (root.Name == moduleName)
                {
                    SelectAndInit(root);
                    ExpandParents(root);
                    break;
                }
            }
        }

        private void SelectAndInit(InspectorMenuItem item)
        {
            item.Select();
            item.Toggled = true;
            if (item.Value is BaseTreeMenuItem bt)
            {
                bt.SelectionMenu();
            }
            else if (item.Value is BaseUIToolModule bm)
            {
                bm.SelectionMenu();
            }
        }

        public static RD.Inspector.Editor.StringPrefs UserNamePrefs = new RD.Inspector.Editor.StringPrefs("UIAutoTool_UserName", null, "UI");

        [LabelText("用户名")]
        [Required("请填写用户名")]
        [ShowInInspector]
        private static string m_Author;

        public static string Author
        {
            get
            {
                if (string.IsNullOrEmpty(m_Author))
                {
                    m_Author = UserNamePrefs.Value;
                }

                return m_Author;
            }
        }

        [HideLabel]
        [HideReferenceObjectPicker]
        [ShowInInspector]
        private BaseCreateModule m_UIBaseModule;

        [BoxGroup("全局图集设置", centerLabel: true)]
        [ShowInInspector]
        internal UIAtlasModule AtlasModule;

        [BoxGroup("其他设置", centerLabel: true)]
        [ShowInInspector]
        internal UIOtherModule OtherModule;

        protected override void Initialize()
        {
            base.Initialize();
            m_UIBaseModule = new CreateUIBaseModule();
            AtlasModule = new();
            OtherModule = new();
            m_Author = UserNamePrefs.Value;
            m_UIBaseModule?.Initialize();
            AtlasModule?.Initialize();
            OtherModule?.Initialize();
            m_MenuWidth = MathF.Max(m_MenuWidthPrefs.Value, 200f);
        }

        protected override void OnWindowDestroy()
        {
            base.OnWindowDestroy();
            UserNamePrefs.Value = Author;
            m_UIBaseModule?.OnDestroy();
            AtlasModule?.OnDestroy();
            OtherModule?.OnDestroy();
            m_MenuWidthPrefs.Value = MathF.Min(MathF.Max(m_MenuWidth, 200f), position.width - 100f);

            foreach (var menuItem in m_AllMenuItem)
            {
                menuItem.OnDestroy();
            }
        }
    }
}
#endif
