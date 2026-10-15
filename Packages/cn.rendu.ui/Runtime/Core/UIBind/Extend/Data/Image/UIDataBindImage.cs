using System;
using RD;
using RD.Draw;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 图片数据绑定（String 资源名 → Image.sprite 异步加载）
    /// 空串隐藏；同一资源复用不重载；切资源前释放上一张 Sprite
    /// </summary>
    [RequireComponent(typeof(Image))]
    [LabelText("Image 图片")]
    [AddComponentMenu("UIBind/Data/图片 【Image】 UIDataBindImage")]
    public sealed class UIDataBindImage : UIDataBindSelectBase
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

        /// <summary>过滤出 String 类型的数据</summary>
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.String;
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

        /// <summary>数据变化：空串隐藏，否则异步加载并设置 Sprite</summary>
        protected override void OnValueChanged()
        {
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            if (m_Image == null || gameObject == null) return;

            var dataValue = GetFirstValue<string>();

            if (string.IsNullOrEmpty(dataValue))
            {
                SetEnabled(false);
                return;
            }

            ChangeSprite(dataValue).NoContext();
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
                Logger.LogError($"{resName} 加载过程中 对象被摧毁了 || m_Image == null");
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