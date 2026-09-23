using System;
using RD.Inspector;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    [Serializable]
    [LabelText("改变颜色")]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UIBind/Data/颜色 【Color】 UIDataBindColor")]
    public class UIDataBindColor : UIDataBindSelectBase
    {
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Color;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("图")]
        private Graphic m_Graphic;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Graphic ??= GetComponent<Graphic>();
        }

        protected override void OnValueChanged()
        {
            var dataValue = GetFirstValue<Color>();

            if (m_Graphic != null)
            {
                m_Graphic.color = dataValue;
            }
        }
    }
}
