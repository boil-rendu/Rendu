#if UNITY_EDITOR
using RD.Inspector;
using RD.Inspector.Editor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [HideReferenceObjectPicker]
    public abstract class BaseTreeMenuItem : BaseToolModule
    {
        private bool m_InitEnd;

        public BaseTreeMenuItem()
        {
        }

        protected BaseTreeMenuItem(InspectorMenuEditorWindow autoTool, InspectorMenuTree tree)
        {
            Window = autoTool;
            Tree     = tree;
        }

        public override void SelectionMenu()
        {
            Init();
            Select();
        }

        private void Init()
        {
            if (m_InitEnd) return;
            Initialize();
            m_InitEnd = true;
        }

        protected abstract void Select();
    }

    [HideReferenceObjectPicker]
    public class TreeMenuItem<T> : BaseTreeMenuItem where T : BaseToolModule, new()
    {
        [ShowInInspector]
        [HideLabel]
        [HideReferenceObjectPicker]
        public T Instance { get; internal set; }

        public TreeMenuItem()
        {
        }

        public TreeMenuItem(InspectorMenuEditorWindow autoTool, InspectorMenuTree tree, string menuName, Texture icon) :
                base(autoTool, tree)
        {
            Tree.Add(menuName, this, icon);
            ModuleName = menuName;
        }

        public TreeMenuItem(InspectorMenuEditorWindow autoTool, InspectorMenuTree tree, string menuName,
                            InspectorIcon icon) : base(autoTool, tree)
        {
            Tree.Add(menuName, this, icon);
            ModuleName = menuName;
        }

        public TreeMenuItem(InspectorMenuEditorWindow autoTool, InspectorMenuTree tree, string menuName) : base(autoTool,
                tree)
        {
            Tree.Add(menuName, this);
            ModuleName = menuName;
        }

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

        public override void OnDestroy()
        {
            Instance?.OnDestroy();
        }

        protected override void Select()
        {
            Instance?.SelectionMenu();
        }
    }
}
#endif
