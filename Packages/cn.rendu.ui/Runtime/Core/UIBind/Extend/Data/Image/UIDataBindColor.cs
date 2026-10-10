using System;
using RD.EditorKit;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 颜色数据绑定（Color → Graphic.color）
    /// 适用于 Image / RawImage / Text 等所有 Graphic
    /// </summary>
    [Serializable]
    [LabelText("改变颜色")]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UIBind/Data/颜色 【Color】 UIDataBindColor")]
    public class UIDataBindColor : UIDataBindSelectBase
    {
        /// <summary>过滤出 Color 类型的数据</summary>
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Color;
        }

        /// <summary>最多绑定 1 个数据</summary>
        protected override int SelectMax()
        {
            return 1;
        }

        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("图")]
        private Graphic m_Graphic;

        /// <summary>刷新时补齐 Graphic 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Graphic ??= GetComponent<Graphic>();
        }

        /// <summary>数据变化：把颜色应用到 Graphic</summary>
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
