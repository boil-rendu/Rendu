#if TextMeshPro
using System;
using System.Collections.Generic;
using RD.Draw;
using TMPro;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 输入栏事件绑定（TextMeshPro TMP_InputField，值变化时）
    /// 监听 onValueChanged，把「当前输入文本 string」作为事件参数派发
    /// </summary>
    [InfoBox("提示: 可用事件参数 <参数1:string(输入的值)>")]
    [LabelText("输入栏<string> 变化时")]
    [RequireComponent(typeof(TMP_InputField))]
    [AddComponentMenu("UIBind/Event/输入栏 【InputField TMP】 UIEventBindInputFieldTMP")]
    public class UIEventBindInputFieldTMP : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("输入栏")]
        private TMP_InputField m_InputField;

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
            m_InputField ??= GetComponent<TMP_InputField>();
        }

        /// <summary>启用时监听输入值变化</summary>
        private void OnEnable()
        {
            if (m_InputField == null) return;
            m_InputField.onValueChanged.AddListener(OnValueChanged);
        }

        /// <summary>禁用时移除监听</summary>
        private void OnDisable()
        {
            if (m_InputField == null) return;
            m_InputField.onValueChanged.RemoveListener(OnValueChanged);
        }

        /// <summary>值变化回调：派发当前输入文本</summary>
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
#endif
