using System.Collections;
using RD.EditorKit;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 单个 Component 显隐数据绑定（Bool → Behaviour.enabled）
    /// 支持瞬时 / 延时过渡两种模式
    /// </summary>
    [LabelText("任意Component的显隐")]
    [AddComponentMenu("UIBind/Data/显隐 【ActiveComponent】 UIDataBindActiveComponent l")]
    public sealed class UIDataBindActiveComponent : UIDataBindBool
    {
        /// <summary>被控制的组件</summary>
        [SerializeField]
        [LabelText("控制的目标")]
        [Required("必须有此组件")]
        private Behaviour m_Target;

        /// <summary>过渡类型</summary>
        [SerializeField]
        [LabelText("过度类型")]
        private UITransitionModeEnum m_TransitionMode = UITransitionModeEnum.Instant;

        /// <summary>过渡时长</summary>
        [SerializeField]
        [LabelText("过度时间")]
        [ShowIf("m_TransitionMode", UITransitionModeEnum.Fade)]
        private float m_TransitionTime = 0.1f;

        /// <summary>数据变化：瞬时切换，或延时后切换</summary>
        protected override void OnValueChanged()
        {
            if (m_Target == null)
                return;

            var result = GetResult();

            if (m_TransitionMode == UITransitionModeEnum.Instant)
            {
                m_Target.enabled = result;
            }
            else
            {
                m_WaitForSeconds ??= new WaitForSeconds(m_TransitionTime);
                m_Coroutine      =   StartCoroutine(WaitTime(result));
            }
        }

        private WaitForSeconds m_WaitForSeconds;
        private Coroutine      m_Coroutine;

        /// <summary>等待过渡时长后应用结果</summary>
        private IEnumerator WaitTime(bool result)
        {
            yield return m_WaitForSeconds;
            m_Target.enabled = result;
            m_Coroutine      = null;
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
