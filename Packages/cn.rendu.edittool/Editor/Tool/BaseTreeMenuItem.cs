using RD.Draw;
using RD.Draw.Editor;
using UnityEngine;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 工具树菜单项基类：定义「被选中 → 先初始化一次 → 再执行子类 Select」的模板流程。
    /// 子类只需实现 Select(以及可选的 Initialize)。
    /// </summary>
    [HideReferenceObjectPicker]
    public abstract class BaseTreeMenuItem : BaseToolModule
    {
        private bool m_InitEnd;

        /// <summary>空构造(供序列化使用)</summary>
        public BaseTreeMenuItem()
        {
        }

        /// <summary>构造: 绑定宿主窗口与菜单树</summary>
        protected BaseTreeMenuItem(MenuEditorWindow autoTool, MenuTree tree)
        {
            Window = autoTool;
            Tree     = tree;
        }

        /// <summary>菜单被选中: 保证初始化只执行一次, 然后交由子类 Select</summary>
        public override void SelectionMenu()
        {
            Init();
            Select();
        }

        /// <summary>首次选中时执行一次 Initialize</summary>
        private void Init()
        {
            if (m_InitEnd) return;
            Initialize();
            m_InitEnd = true;
        }

        /// <summary>子类实现的具体选中行为</summary>
        protected abstract void Select();
    }

    /// <summary>
    /// 绑定具体模块类型 T 的菜单项泛型封装。
    /// 构造时把自身注册进菜单树并记录模块名, Initialize 时创建并初始化 T 的实例。
    /// </summary>
    [HideReferenceObjectPicker]
    public class TreeMenuItem<T> : BaseTreeMenuItem where T : BaseToolModule, new()
    {
        /// <summary>该菜单项对应的模块实例</summary>
        [ShowInInspector]
        [HideLabel]
        [HideReferenceObjectPicker]
        public T Instance { get; internal set; }

        /// <summary>空构造(供序列化使用)</summary>
        public TreeMenuItem()
        {
        }

        /// <summary>以 Texture 图标注册菜单项</summary>
        public TreeMenuItem(MenuEditorWindow autoTool, MenuTree tree, string menuName, Texture icon) :
                base(autoTool, tree)
        {
            Tree.Add(menuName, this, icon);
            ModuleName = menuName;
        }

        /// <summary>以 EditorIcon 图标注册菜单项</summary>
        public TreeMenuItem(MenuEditorWindow autoTool, MenuTree tree, string menuName,
                            EditorIcon icon) : base(autoTool, tree)
        {
            Tree.Add(menuName, this, icon);
            ModuleName = menuName;
        }

        /// <summary>无图标注册菜单项</summary>
        public TreeMenuItem(MenuEditorWindow autoTool, MenuTree tree, string menuName) : base(autoTool,
                tree)
        {
            Tree.Add(menuName, this);
            ModuleName = menuName;
        }

        /// <summary>创建 T 实例并回填窗口 / 菜单树 / 模块名与自定义数据, 随后执行其 Initialize</summary>
        public override void Initialize()
        {
            Instance = new T
                       {
                           Window   = Window,
                           Tree       = Tree,
                           ModuleName = ModuleName,
                       };
            Instance.UserData = this.UserData;
            Instance.Initialize();
        }

        /// <summary>销毁时同步销毁内部模块实例</summary>
        public override void OnDestroy()
        {
            Instance?.OnDestroy();
        }

        /// <summary>选中时转发给内部模块实例</summary>
        protected override void Select()
        {
            Instance?.SelectionMenu();
        }
    }
}
