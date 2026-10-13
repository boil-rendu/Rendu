using RD.Panel;
using UnityEngine;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 所有工具模块的基类：持有宿主窗口、工具树与模块名, 由子类实现具体的检查 / 创建逻辑。
    /// </summary>
    [HideReferenceObjectPicker]
    public abstract class BaseToolModule : BaseCreateModule
    {
        /// <summary>宿主编辑器窗口</summary>
        [HideInInspector]
        public MenuEditorWindow Window { get; set; }

        /// <summary>宿主窗口的菜单树</summary>
        [HideInInspector]
        public MenuTree Tree { get; set; }

        /// <summary>模块名</summary>
        [HideInInspector]
        public string ModuleName { get; set; }

        /// <summary>模块自定义数据</summary>
        [HideInInspector]
        public object UserData { get; set; }

        /// <summary>菜单被选中时的回调(子类按需重写)</summary>
        public virtual void SelectionMenu()
        {
        }
    }
}
