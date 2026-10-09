using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>Component 右键菜单：把当前组件添加到所属 UI 的组件表</summary>
    public class ComponentTableContextMenu
    {
        [MenuItem("CONTEXT/Component/添加到UI组件表", false, -int.MaxValue)]
        private static void AddUIComponentTable(MenuCommand command)
        {
            if (!UIOperationHelper.CheckUIOperation()) return;
            if (command.context is not Component menuComponent) return;
            var cdeTable = Selection.activeGameObject.GetComponentInParent<UIBindCDETable>();
            if (cdeTable == null) return;
            cdeTable.ComponentTable ??= cdeTable.gameObject.AddComponent<UIBindComponentTable>();
            cdeTable.ComponentTable.EditorAddComponent(menuComponent);
        }

        /// <summary>菜单可用性：选中对象必须处于某个 UIBindCDETable 之下</summary>
        [MenuItem("CONTEXT/Component/添加到UI组件表", true)]
        private static bool AddUIComponentTableValidate(MenuCommand command)
        {
            if (Selection.activeGameObject == null) return false;
            var cdeTable = Selection.activeGameObject.GetComponentInParent<UIBindCDETable>();
            if (cdeTable == null) return false;
            return true;
        }
    }
}
