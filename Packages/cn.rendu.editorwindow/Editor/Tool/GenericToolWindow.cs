#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using RD.Panel;
using UnityEngine;
using Type = System.Type;

namespace RD.EditorWindows.Editor
{
    /// <summary>
    /// 通用工具窗口基类: 继承 <see cref="MenuEditorWindow"/>, 在子类的 [MenuItem] 入口之外, 自动完成
    /// "反射扫描所有带 [UIAutoMenu] 的模块类 → 挂到左侧菜单树 → 选中时初始化与生命周期调度" 这套套路。
    ///
    /// 子类只需:
    ///   1) 继承本类, 提供一个带 [MenuItem] 的静态 OpenWindow 入口;
    ///   2) (可选) 重写 <see cref="BuildToolMenuTree"/> 在头部插入根节点(如"发布")或尾部追加"全局设置"根;
    ///   3) (可选) 重写 <see cref="OnToolInitialize"/> / <see cref="OnToolDestroy"/> 做自身模块(挂在自身字段
    ///      上的 [ShowInInspector] 面板)的装配 / 清理。
    ///
    /// 模块本身的写法保持不变: 继承 <see cref="BaseToolModule"/>, 挂 [UIAutoMenu("名字", order)]。
    /// </summary>
    public abstract class GenericToolWindow : MenuEditorWindow
    {
        /// <summary>本窗口的菜单树(首次访问时自动构建)</summary>
        protected MenuTree m_MenuTree;

        /// <summary>本窗口持有的所有菜单项(用于窗口销毁时统一 OnDestroy)</summary>
        protected readonly List<BaseTreeMenuItem> m_AllMenuItem = new();

        private bool m_FirstInit = true;
        private EditorPrefsValue<string> m_LastSelectMenuPrefs = new("GenericTool_LastSelectMenu", "");
        private EditorPrefsValue<string> m_LastSelectToolModulePathPrefs = new("GenericTool_LastSelectModulePath", "");
        private readonly HashSet<MenuTreeItem> m_FirstSelect = new();
        private bool m_RestoringSelection;

        /// <summary>
        /// 选中变化时调度: 恢复上次选中 / 调模块 SelectionMenu / 切换 Toggled / 落盘最近选中。
        /// 子类一般不用重写。
        /// </summary>
        private void OnSelectionChanged(SelectionChangedType obj)
        {
            if (m_RestoringSelection) return;
            if (obj != SelectionChangedType.ItemAdded) return;

            if (m_FirstInit)
            {
                m_FirstInit = false;

                if (!string.IsNullOrEmpty(m_LastSelectToolModulePathPrefs.Value))
                {
                    m_RestoringSelection = true;
                    var restored = SelectChainByPath(m_LastSelectToolModulePathPrefs.Value);
                    m_RestoringSelection = false;
                    if (restored != null) return;
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
            else if (selectedMenuItem.Value is BaseToolModule module)
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

        private MenuTreeItem FindMenuItemByModulePath(string modulePath)
        {
            foreach (var root in m_MenuTree.MenuItems)
            {
                var result = FindRecursive(root, modulePath);
                if (result != null) return result;
            }
            return null;
        }

        private static MenuTreeItem FindRecursive(MenuTreeItem item, string modulePath)
        {
            if (item.Value is BaseTreeMenuItem bt && bt.ModuleName == modulePath) return item;
            if (item.Value is BaseToolModule bm && bm.ModuleName == modulePath) return item;
            foreach (var child in item.ChildMenuItems)
            {
                var res = FindRecursive(child, modulePath);
                if (res != null) return res;
            }
            return null;
        }

        private static void ExpandParents(MenuTreeItem item)
        {
            var cur = item.Parent;
            while (cur != null)
            {
                cur.Toggled = true;
                cur = cur.Parent;
            }
            item.Toggled = true;
        }

        private MenuTreeItem SelectChainByPath(string modulePath)
        {
            if (string.IsNullOrEmpty(modulePath)) return null;

            var segments = modulePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0) return null;

            MenuTreeItem current = null;
            foreach (var root in m_MenuTree.MenuItems)
            {
                if (root.Name == segments[0]) { current = root; break; }
            }
            if (current == null) return null;

            SelectAndInit(current);
            ExpandParents(current);

            for (int i = 1; i < segments.Length; i++)
            {
                var nextName = segments[i];
                MenuTreeItem next = null;
                foreach (var child in current.ChildMenuItems)
                {
                    if (child.Name == nextName) { next = child; break; }
                }

                if (next == null)
                {
                    SelectAndInit(current);
                    foreach (var child in current.ChildMenuItems)
                    {
                        if (child.Name == nextName) { next = child; break; }
                    }
                }

                if (next == null) return null;
                current = next;
                SelectAndInit(current);
                ExpandParents(current);
            }

            return current;
        }

        private void SelectAndInit(MenuTreeItem item)
        {
            item.Select();
            item.Toggled = true;
            if (item.Value is BaseTreeMenuItem bt) bt.SelectionMenu();
            else if (item.Value is BaseToolModule bm) bm.SelectionMenu();
        }

        /// <summary>按模块名选中(同名根节点级); 用于外部代码直接跳转到某模块</summary>
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

        /// <summary>
        /// 创建一份 <see cref="TreeMenuItem{T}"/> 包装, 注册到当前菜单树, 并加入生命周期列表。
        /// 子类在 BuildToolMenuTree 里要加新模块时用这个, 不要直接 new TreeMenuItem&lt;T&gt;,
        /// 否则窗口销毁时该模块的 OnDestroy 不会被调用。
        /// </summary>
        protected BaseTreeMenuItem NewTreeMenuItem(Type moduleType, string moduleName)
        {
            var treeMenuItemType = typeof(TreeMenuItem<>);
            var specificTreeMenuItemType = treeMenuItemType.MakeGenericType(moduleType);
            var constructor = specificTreeMenuItemType.GetConstructor(new Type[]
            {
                typeof(MenuEditorWindow),
                typeof(MenuTree),
                typeof(string)
            });
            var treeMenuItem = (BaseTreeMenuItem)constructor.Invoke(new object[] { this, m_MenuTree, moduleName });
            m_AllMenuItem.Add(treeMenuItem);
            return treeMenuItem;
        }

        /// <summary>
        /// 本窗口接受的模块 <see cref="UIAutoMenuAttribute.Scope"/>。null/空 = 接受所有作用域。
        /// 子类(如 LoaderTool)重写它来限定"本窗口只加载某作用域的模块", 与"按包位置拆分模块"配合。
        /// </summary>
        protected virtual string AcceptScope => null;

        /// <summary>
        /// 构建菜单树的扩展点: 默认行为是反射扫描所有程序集中带 [UIAutoMenu] 的模块类, 按 Order 排序后挂树。
        /// 子类重写时, 建议先调 <c>base.BuildToolMenuTree(tree)</c>, 再在头部/尾部追加自己的根节点
        /// (如头部"发布"、尾部"全局设置")。
        /// </summary>
        protected virtual MenuTree BuildToolMenuTree(MenuTree tree)
        {
            var allAutoMenus = new List<UIAutoMenuData>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsDefined(typeof(UIAutoMenuAttribute), false))
                    {
                        var attribute = (UIAutoMenuAttribute)Attribute.GetCustomAttribute(type, typeof(UIAutoMenuAttribute));
                        // 作用域过滤: 不匹配本窗口 AcceptScope 的模块跳过
                        if (!string.IsNullOrEmpty(attribute.Scope) && attribute.Scope != AcceptScope) continue;
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
                NewTreeMenuItem(attribute.Type, attribute.MenuName);
            }
            return tree;
        }

        protected override MenuTree BuildMenuTree()
        {
            m_MenuTree = new MenuTree();
            m_MenuTree.Selection.SelectionChanged += OnSelectionChanged;
            m_AllMenuItem.Clear();
            return BuildToolMenuTree(m_MenuTree);
        }

        /// <summary>首次显示前调用一次; 子类在这里装配自己的内嵌模块(挂在自身 [ShowInInspector] 字段上的面板)。</summary>
        protected override void Initialize()
        {
            base.Initialize();
            OnToolInitialize();
        }

        /// <summary>窗口销毁时调用; 子类在这里释放自己的内嵌模块(基类会统一调所有菜单项的 OnDestroy)。</summary>
        protected override void OnWindowDestroy()
        {
            base.OnWindowDestroy();
            OnToolDestroy();
            foreach (var menuItem in m_AllMenuItem)
            {
                menuItem.OnDestroy();
            }
        }

        /// <summary>子类重写: 自身内嵌模块的初始化。</summary>
        protected virtual void OnToolInitialize() { }

        /// <summary>子类重写: 自身内嵌模块的销毁。</summary>
        protected virtual void OnToolDestroy() { }
    }
}
#endif
