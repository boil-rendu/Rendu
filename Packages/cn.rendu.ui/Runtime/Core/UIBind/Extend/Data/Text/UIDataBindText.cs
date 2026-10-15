using RD.Draw;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 文本数据绑定（UGUI Text）
    /// 具体取值/格式化由基类 UIDataBindTextBase 完成，这里只负责拿组件与读写文本
    /// </summary>
    [RequireComponent(typeof(Text))]
    [LabelText("Text 文本")]
    [AddComponentMenu("UIBind/Data/文本 【Text】 UIDataBindText")]
    public sealed class UIDataBindText : UIDataBindTextBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("文本")]
        private Text m_Text;

        /// <summary>初始化：补齐 Text 引用并检查 Enabled 配置冲突</summary>
        protected override void OnInit()
        {
            m_Text ??= GetComponent<Text>();
            if (m_Text == null)
            {
                Logger.LogError($"{name} 错误没有 Text 组件");
                return;
            }

            if (!m_ChangeEnabled && !m_Text.enabled)
            {
                Logger.LogError($"{name} 当前文本禁止修改Enabled 且当前处于隐藏状态 可能会出现问题 请检查");
            }
        }

        /// <summary>设置文本组件启用状态</summary>
        protected override void SetEnabled(bool value)
        {
            if (m_Text == null) return;
            m_Text.enabled = value;
        }

        /// <summary>设置文本内容</summary>
        protected override void SetText(string value)
        {
            if (m_Text == null) return;
            m_Text.text = value;
        }

        /// <summary>文本组件是否存在</summary>
        protected override bool ExistText()
        {
            return m_Text != null;
        }
    }
}