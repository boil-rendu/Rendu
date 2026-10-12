using System;
using System.Collections.Generic;
using RD.Panel;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 输入栏事件绑定（UGUI InputField，编辑结束时）
    /// 监听 onEndEdit，把「最终输入文本 string」作为事件参数派发
    /// </summary>
    [InfoBox("提示: 可用事件参数 <参数1:string(输入的值)>")]
    [LabelText("输入栏<string> 结束时")]
    [RequireComponent(typeof(InputField))]
    [AddComponentMenu("UIBind/Event/输入栏 【InputFieldEnd】 UIEventBindInputFieldEnd")]
    public class UIEventBindInputFieldEnd : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("输入栏")]
        private InputField m_InputField;

        /// <summary>非异步事件</summary>
        protected override bool IsTaskEvent => false;

        [NonSerialized]
        private readonly List<EUIEventParamType> m_FilterParamType = new()
        {
            EUIEventParamType.String
        };

        /// <summary>过滤出参数为 string 的事件</summary>
        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Awake()
        {
            m_InputField ??= GetComponent<InputField>();
        }

        /// <summary>启用时监听编辑结束</summary>
        private void OnEnable()
        {
            if (m_InputField == null) return;
            m_InputField.onEndEdit.AddListener(OnValueChanged);
        }

        /// <summary>禁用时移除监听</summary>
        private void OnDisable()
        {
            if (m_InputField == null) return;
            m_InputField.onEndEdit.RemoveListener(OnValueChanged);
        }

        /// <summary>编辑结束回调：派发最终输入文本</summary>
        private void OnValueChanged(string value)
        {
            try
            {
                m_UIEvent?.Invoke(value);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }
    }
}
