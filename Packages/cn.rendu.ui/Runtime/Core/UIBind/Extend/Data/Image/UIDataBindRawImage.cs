using System;
using RD;
using RD.Panel;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 原图数据绑定（String 资源名 → RawImage.texture 异步加载）
    /// 空串隐藏；同一资源复用不重载；切资源前释放上一个 Texture2D
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    [LabelText("RawImage 图片Raw")]
    [AddComponentMenu("UIBind/Data/图片Raw 【RawImage】 UIDataBindRawImage")]
    public sealed class UIDataBindRawImage : UIDataBindSelectBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("图片")]
        private RawImage m_RawImage;

        /// <summary>设置后按原始尺寸调整大小</summary>
        [SerializeField]
        [LabelText("自动调整图像大小")]
        private bool m_SetNativeSize = false;

        /// <summary>是否允许本组件修改 RawImage.enabled</summary>
        [SerializeField]
        [LabelText("可修改Enabled")]
        private bool m_ChangeEnabled = true;

        [NonSerialized]
        private string m_LastResName;

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

        /// <summary>刷新时补齐 RawImage 引用并检查 Enabled 配置冲突</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RawImage ??= GetComponent<RawImage>();
            if (!m_ChangeEnabled && !m_RawImage.enabled)
            {
                Logger.LogError($"{name} 当前禁止修改Enabled 且当前处于隐藏状态 可能会出现问题 请检查");
            }
        }

        /// <summary>按开关设置 RawImage.enabled（受 m_ChangeEnabled 限制）</summary>
        private void SetEnabled(bool set)
        {
            if (!m_ChangeEnabled) return;

            if (m_RawImage == null) return;

            m_RawImage.enabled = set;
        }

        /// <summary>数据变化：空串隐藏，否则异步加载并设置 Texture</summary>
        protected override void OnValueChanged()
        {
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            if (m_RawImage == null || gameObject == null) return;

            var dataValue = GetFirstValue<string>();

            if (string.IsNullOrEmpty(dataValue))
            {
                SetEnabled(false);
                return;
            }

            ChangeTexture2D(dataValue).NoContext();
        }

        /// <summary>异步加载 Texture2D 并赋值（协程锁防并发，加载后校验对象存活）</summary>
        private async Promise ChangeTexture2D(string resName)
        {
            using var _ = await EventSystem.Instance?.UIInvokeEntityAsyncSafety<UIInvokeEntity_CoroutineLock, Promise<Entity>>(UISingletonHelper.UIMgr, new UIInvokeEntity_CoroutineLock { Lock = GetHashCode() });

            if (m_LastResName == resName)
            {
                if (m_RawImage != null && m_RawImage.texture != null)
                {
                    SetEnabled(true);
                }
                else
                {
                    SetEnabled(false);
                }

                return;
            }

            var texture2d = await EventSystem.Instance?.UIInvokeEntityAsyncSafety<UIInvokeEntity_LoadTexture2D, Promise<Texture2D>>(UISingletonHelper.UIMgr, new UIInvokeEntity_LoadTexture2D { ResName = resName });

            if (texture2d == null)
            {
                m_LastResName = "";
                SetEnabled(false);
                return;
            }

            ReleaseLastTexture2D();

            if (this == null || gameObject == null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_Release { obj = texture2d });
                return;
            }

            if (m_RawImage == null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_Release { obj = texture2d });
                Logger.LogError($"{resName} 加载过程中 对象被摧毁了 m_Image == null");
                return;
            }

            m_LastTexture2D = texture2d;
            m_RawImage.texture = texture2d;
            if (m_SetNativeSize)
            {
                m_RawImage.SetNativeSize();
            }

            SetEnabled(true);
            m_LastResName = resName;
        }

        /// <summary>解绑时释放已加载的 Texture2D</summary>
        protected override void UnBindData()
        {
            base.UnBindData();
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            ReleaseLastTexture2D();
        }

        private Texture2D m_LastTexture2D;

        /// <summary>释放上一张 Texture2D</summary>
        private void ReleaseLastTexture2D()
        {
            if (m_LastTexture2D != null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_Release { obj = m_LastTexture2D });
                m_LastTexture2D = null;
            }
        }
    }
}