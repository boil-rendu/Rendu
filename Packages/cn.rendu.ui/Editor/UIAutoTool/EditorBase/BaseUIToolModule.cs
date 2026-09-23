#if UNITY_EDITOR
using RD.Inspector;
using RD.Inspector.Editor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [HideReferenceObjectPicker]
    public abstract class BaseUIToolModule : BaseCreateModule
    {
        [HideInInspector]
        public InspectorMenuEditorWindow AutoTool { get; set; }

        [HideInInspector]
        public InspectorMenuTree Tree { get; set; }

        [HideInInspector]
        public string ModuleName { get; set; }

        [HideInInspector]
        public object UserData { get; set; }

        public virtual void SelectionMenu()
        {
        }
    }
}
#endif
