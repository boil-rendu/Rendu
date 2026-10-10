using System.Collections;
using System.Collections.Generic;
using RD.EditorKit;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 一组 Component 显隐数据绑定（Bool → 批量 Behaviour.enabled）
    /// 支持瞬时 / 延时过渡两种模式
    /// </summary>
    [LabelText("任意集合的Component的显隐")]
    [AddComponentMenu("UIBind/Data/显隐 【ActiveComponents】 UIDataBindActiveComponents s")]
    public sealed class UIDataBindActiveComponents : UIDataBindBool
    {
        /// <summary>被控制的一组组件</summary>
        [SerializeField]
        [LabelText("控制的目标")]
        [Required("必须有此组件")]
        private List<Behaviour> m_Targets = new List<Behaviour>();

        /// <summary>过渡类型</summary>
        [SerializeField]
        [LabelText("过度类型")]
        private UITransitionModeEnum m_TransitionMode = UITransitionModeEnum.Instant;

        /// <summary>过渡时长</summary>
        [SerializeField]
        [LabelText("过度时间")]
        [ShowIf("m_TransitionMode", UITransitionModeEnum.Fade)]
        private float m_TransitionTime = 0.1f;

        /// <summary>数据变化：瞬时批量切换，或延时后切换</summary>
        protected override void OnValueChanged()
        {
            if (m_Targets == null) return;

            var result = GetResult();

            if (m_TransitionMode == UITransitionModeEnum.Instant)
            {
                SetEnabled(result);
            }
            else
            {
                m_WaitForSeconds ??= new WaitForSeconds(m_TransitionTime);
                m_Coroutine      =   StartCoroutine(WaitTime(result));
            }
        }

        private WaitForSeconds m_WaitForSeconds;
        private Coroutine      m_Coroutine;

        /// <summary>等待过渡时长后批量应用结果</summary>
        private IEnumerator WaitTime(bool result)
        {
            yield return m_WaitForSeconds;
            SetEnabled(result);
            m_Coroutine = null;
        }

        /// <summary>批量设置组件启用状态</summary>
        private void SetEnabled(bool set)
        {
            foreach (var value in m_Targets)
            {
                value.enabled = set;
            }
        }

        /// <summary>销毁时停掉未完成的过渡协程</summary>
        private new void OnDestroy()
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }
        }
    }
}
