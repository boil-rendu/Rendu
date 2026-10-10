using System;
using System.Linq;
using RD;
using RD.EditorKit;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 图片数据绑定（Int/String + 格式化 → Image.sprite 异步加载）
    /// 先把数据取值并套用 m_Format 拼成资源名，再加载 Sprite
    /// </summary>
    [RequireComponent(typeof(Image))]
    [LabelText("Image 图片Format")]
    [AddComponentMenu("UIBind/Data/图片Format 【ImageFormat】 UIDataBindImageFormat")]
    public sealed class UIDataBindImageFormat : UIDataBindSelectBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("图片")]
        private Image m_Image;

        /// <summary>设置后按原始尺寸调整大小</summary>
        [SerializeField]
        [LabelText("自动调整图像大小")]
        private bool m_SetNativeSize = false;

        /// <summary>是否允许本组件修改 Image.enabled</summary>
        [SerializeField]
        [LabelText("可修改Enabled")]
        private bool m_ChangeEnabled = true;

        [NonSerialized]
        private string m_LastSpriteName;

        /// <summary>资源名格式化模板（如 "icon_{0}"）</summary>
        [SerializeField]
        [LabelText("格式化字符串")]
        private string m_Format;

        /// <summary>过滤出 Int 与 String 类型的数据</summary>
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Int | 1 << (int)EUIBindDataType.String;
        }

        /// <summary>最多绑定 1 个数据</summary>
        protected override int SelectMax()
        {
            return 1;
        }

        /// <summary>刷新时补齐 Image 引用并检查 Enabled 配置冲突</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Image ??= GetComponent<Image>();
            if (!m_ChangeEnabled && !m_Image.enabled)
            {
                Logger.LogError($"{name} 当前禁止修改Enabled 且当前处于隐藏状态 可能会出现问题 请检查");
            }
        }

        /// <summary>按开关设置 Image.enabled（受 m_ChangeEnabled 限制）</summary>
        private void SetEnabled(bool set)
        {
            if (!m_ChangeEnabled) return;

            if (m_Image == null) return;

            m_Image.enabled = set;
        }

        /// <summary>数据变化：取值 → 格式化 → 异步加载 Sprite</summary>
        protected override void OnValueChanged()
        {
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            var data = DataSelectDic.Count >= 1 ? DataSelectDic.First().Value : null;

            if (data == null)
            {
                SetEnabled(false);
                return;
            }

            var dataValue = GetDataToString(data);

            if (string.IsNullOrEmpty(dataValue))
            {
                SetEnabled(false);
                return;
            }

            ChangeSprite(dataValue).NoContext();
        }

        /// <summary>把 Int/String 数据取值并按 m_Format 拼成资源名（负数视为空）</summary>
        private string GetDataToString(UIDataSelect dataSelect)
        {
            var dataValue = dataSelect?.Data?.DataValue;
            if (dataValue == null) return "";

            var dataString = "";

            try
            {
                switch (dataValue.UIBindDataType)
                {
                    case EUIBindDataType.Int:
                        var intValue = dataValue.GetValue<int>();
                        dataString = intValue < 0 ? "" : intValue.ToString();
                        break;
                    case EUIBindDataType.String:
                        dataString = dataValue.GetValue<string>();
                        break;
                    default:
                        Logger.LogError($"{name} 不支持此类型 {dataValue.UIBindDataType}", this);
                        break;
                }

                if (string.IsNullOrEmpty(dataString))
                {
                    return "";
                }

                if (!string.IsNullOrEmpty(m_Format))
                {
                    return string.Format(m_Format, dataString);
                }
            }
            catch (FormatException exp)
            {
                Logger.LogError($"{name} 字符串拼接Format 出错请检查是否有拼写错误  {m_Format} , {dataString}");
                Logger.LogError(exp.Message, this);
            }

            return "";
        }

        /// <summary>异步加载 Sprite 并赋值（协程锁防并发，加载后校验对象存活）</summary>
        private async Promise ChangeSprite(string resName)
        {
            using var _ = await EventSystem.Instance?.UIInvokeEntityAsyncSafety<UIInvokeEntity_CoroutineLock, Promise<Entity>>(UISingletonHelper.UIMgr, new UIInvokeEntity_CoroutineLock { Lock = this.GetHashCode() });

            if (m_LastSpriteName == resName)
            {
                if (m_Image != null && m_Image.sprite != null)
                {
                    SetEnabled(true);
                }
                else
                {
                    SetEnabled(false);
                }

                return;
            }

            var sprite = await EventSystem.Instance?.UIInvokeEntityAsyncSafety<UIInvokeEntity_LoadSprite, Promise<Sprite>>(UISingletonHelper.UIMgr, new UIInvokeEntity_LoadSprite { ResName = resName });

            if (sprite == null)
            {
                m_LastSpriteName = "";
                SetEnabled(false);
                return;
            }

            ReleaseLastSprite();

            if (this == null || gameObject == null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_ReleaseSprite { obj = sprite });
                return;
            }

            if (m_Image == null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_ReleaseSprite { obj = sprite });
                Logger.LogError($"{resName} 加载过程中 对象被摧毁了 m_Image == null");
                return;
            }

            m_LastSprite = sprite;
            m_Image.sprite = sprite;
            if (m_SetNativeSize)
            {
                m_Image.SetNativeSize();
            }

            SetEnabled(true);
            m_LastSpriteName = resName;
        }

        /// <summary>解绑时释放已加载的 Sprite</summary>
        protected override void UnBindData()
        {
            base.UnBindData();
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            ReleaseLastSprite();
        }

        private Sprite m_LastSprite;

        /// <summary>释放上一张 Sprite</summary>
        private void ReleaseLastSprite()
        {
            if (m_LastSprite != null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_ReleaseSprite { obj = m_LastSprite });
                m_LastSprite = null;
            }
        }
    }
}