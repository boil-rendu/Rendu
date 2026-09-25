#if UNITY_EDITOR
using RD.Inspector;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 其他设置
    /// </summary>
    [HideReferenceObjectPicker]
    [HideLabel]
    public class OtherModule : BaseCreateModule
    {
        private EnumPrefs<UIBindEventTable.EUITaskEventType> m_UITaskEventTypePrefs;

        [ShowInInspector]
        [BoxGroup("EventTable 默认事件类型")]
        [EnumToggleButtons]
        [HideLabel]
        private UIBindEventTable.EUITaskEventType m_UITaskEventType;

        public override void Initialize()
        {
            m_UITaskEventTypePrefs =
                    new EnumPrefs<UIBindEventTable.EUITaskEventType>("UIAutoTool_OtherModule_TaskEventType", null,
                                                                     UIBindEventTable.EUITaskEventType.Async);
            m_UITaskEventType = m_UITaskEventTypePrefs.Value;
        }

        public override void OnDestroy()
        {
            m_UITaskEventTypePrefs.Value = m_UITaskEventType;
        }
    }
}
#endif
