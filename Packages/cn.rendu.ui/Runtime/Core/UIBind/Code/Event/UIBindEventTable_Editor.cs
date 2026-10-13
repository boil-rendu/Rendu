#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using RD.Panel;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIBindEventTable 的编辑器扩展（UNITY_EDITOR）
    /// 事件表的自动检查、添加新事件（同步/异步）、移除回调与查询接口
    /// </summary>
    public sealed partial class UIBindEventTable
    {
        /// <summary>自动检查：规整所有事件命名（补前缀、首字母大写），最后刷新事件表</summary>
        [GUIColor(1, 1, 0)]
        [Button("自动检查", 30)]
        [PropertyOrder(-999)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        public void AutoCheck()
        {
            if (!UIOperationHelper.CheckUIOperation(this)) return;

            var dicKey = m_EventDic.Keys.ToList();
            foreach (var oldName in dicKey)
            {
                if (string.IsNullOrEmpty(oldName))
                {
                    continue;
                }

                var newName = oldName;

                if (!oldName.CheckFirstName(NameUtility.EventName))
                {
                    newName = $"{NameUtility.FirstName}{NameUtility.EventName}{oldName}";
                }

                newName = newName.ChangeToBigName(NameUtility.EventName);

                if (oldName != newName)
                {
                    var uiData = m_EventDic[oldName];
                    m_EventDic.Remove(oldName);
                    m_EventDic.Add(newName, uiData);
                }
            }

            OnValidate();
        }

        /// <summary>事件类型（同步/异步）</summary>
        [HideLabel]
        public enum EUITaskEventType
        {
            /// <summary>同步事件</summary>
            [LabelText("同步事件")]
            Sync,

            /// <summary>异步事件</summary>
            [LabelText("异步事件")]
            Async,
        }

        [NonSerialized]
        private bool m_FirstGetEventType;

        [NonSerialized]
        private EUITaskEventType m_UITaskEventTypeTemp;

        /// <summary>待添加事件类型（首次读取时从偏好设置恢复）</summary>
        [ShowInInspector]
        [BoxGroup("添加新事件")]
        [EnumToggleButtons]
        [HideLabel]
        [PropertyOrder(-100)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private EUITaskEventType m_UITaskEventType
        {
            get
            {
                if (!m_FirstGetEventType)
                {
                    m_UITaskEventTypeTemp = new EnumPrefs<EUITaskEventType>("UITool_OtherModule_TaskEventType", null, EUITaskEventType.Async)
                            .Value;
                    m_FirstGetEventType = true;
                }

                return m_UITaskEventTypeTemp;
            }
            set
            {
                m_UITaskEventTypeTemp = value;
            }
        }

        /// <summary>待添加的事件名</summary>
        [ShowInInspector]
        [BoxGroup("添加新事件")]
        [HideReferenceObjectPicker]
        [HideLabel]
        [PropertyOrder(-99)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private string m_AddUIEventName = "";

        /// <summary>待添加事件的参数类型列表</summary>
        [BoxGroup("添加新事件")]
        [PropertyOrder(-98)]
        [SerializeField]
        [ShowInInspector]
        [LabelText("需要添加的事件参数")]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        public List<EUIEventParamType> AllEventParamType = new List<EUIEventParamType>();

        /// <summary>「添加」按钮：添加新事件后清空输入</summary>
        [GUIColor(0, 1, 0)]
        [BoxGroup("添加新事件")]
        [Button("添加")]
        [PropertyOrder(-98)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private void AddNewUIEvent()
        {
            var uiEventBase = EditorAddEvent(m_UITaskEventType, m_AddUIEventName, AllEventParamType);
            if (uiEventBase == null)
            {
                return;
            }

            m_AddUIEventName = "";
            AllEventParamType = new List<EUIEventParamType>();
        }

        /// <summary>
        /// 编辑器添加事件：校验名称非空/不重名，按 同步/异步 走对应 Helper 动态创建并加入字典
        /// </summary>
        public UIEventBase EditorAddEvent(EUITaskEventType eventType, string eventMame, List<EUIEventParamType> eventParamType = null)
        {
            if (string.IsNullOrEmpty(eventMame))
            {
                UnityTipsHelper.ShowError($"必须填写名称才可以添加");
                return null;
            }

            if (m_EventDic.ContainsKey(eventMame))
            {
                UnityTipsHelper.ShowError($"已存在同名数据  请修改 {eventMame}");
                return null;
            }

            if (eventParamType == null)
            {
                eventParamType = new();
            }

            UIEventBase uiEventBase;

            switch (eventType)
            {
                case EUITaskEventType.Sync:
                    uiEventBase = UIEventBaseHelper.CreatorUIEventBase(eventMame, eventParamType);
                    break;
                case EUITaskEventType.Async:
                    uiEventBase = UITaskEventBaseHelper.CreatorUITaskEventBase(eventMame, eventParamType);
                    break;
                default:
                    uiEventBase = UIEventBaseHelper.CreatorUIEventBase(eventMame, eventParamType);
                    Logger.LogError($"是否实现了一个新的事件类型 未实现请检查 {eventType}");
                    break;
            }

            if (uiEventBase == null)
            {
                UnityTipsHelper.ShowError($"创建失败 {eventMame}");
                return null;
            }

            m_EventDic.Add(eventMame, uiEventBase);

            AutoCheck();

            return uiEventBase;
        }

        /// <summary>移除回调：先解绑再从字典移除</summary>
        private void RemoveCallBack(UIEventBase uiEvent)
        {
            uiEvent.OnRemoveVariableCallBack();
            m_EventDic.Remove(uiEvent.EventName);
        }

        /// <summary>移除事件入口：不存在则报错；已有绑定时弹确认框，否则直接移除</summary>
        private void OnRemoveUIEvent(UIEventBase uiEvent)
        {
            if (!m_EventDic.ContainsValue(uiEvent))
            {
                UnityTipsHelper.ShowError($"不存在无法移除 {uiEvent.EventName}  请检查是否有配置错误");
                return;
            }

            //如果已经有绑定了 需要提醒是否移除
            if (uiEvent.GetBindCount() >= 1)
            {
                var callBackTips = $"{uiEvent.EventName} 已绑定 {uiEvent.GetBindCount()}个目标\n移除会强制清楚所有绑定 请确认是否需要移除!!!";
                UnityTipsHelper.CallBack(callBackTips, () => { RemoveCallBack(uiEvent); });
                return;
            }

            RemoveCallBack(uiEvent);
        }

        /// <summary>获取所有事件名</summary>
        public List<string> GetAllEventName()
        {
            var allName = new List<string>();
            foreach (var name in m_EventDic.Keys)
            {
                allName.Add(name);
            }

            return allName;
        }

        /// <summary>按（同步/异步 + 参数列表一致）过滤出事件名</summary>
        public List<string> GetFilterParamTypeEventName(List<EUIEventParamType> list, bool taskEvent)
        {
            var allName = new List<string>();
            foreach (var eventValue in m_EventDic.Values)
            {
                if (eventValue.IsTaskEvent == taskEvent && eventValue.AllEventParamType.ParamEquals(list))
                {
                    allName.Add(eventValue.EventName);
                }
            }

            return allName;
        }

        /// <summary>序列化校验：重新挂接事件的改名/移除回调并刷新事件表</summary>
        private void OnValidate()
        {
            if (m_EventDic == null)
            {
                return;
            }

            if (UIOperationHelper.IsPlaying())
            {
                return;
            }

            foreach (var data in m_EventDic)
            {
                var uiEventName = data.Key;
                var uiEvent = data.Value;
                uiEvent.ClearBinds();
                uiEvent.ChangeName(uiEventName);
                uiEvent.OnRemoveAction = OnRemoveUIEvent;
            }

            InitEventTable();
        }
    }
}

#endif
