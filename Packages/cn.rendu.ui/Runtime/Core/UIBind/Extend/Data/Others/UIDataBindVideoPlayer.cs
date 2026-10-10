using System;
using RD;
using RD.EditorKit;
using UnityEngine;
using UnityEngine.Video;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 视频播放器数据绑定（String 资源名 → VideoPlayer 播放）
    /// 空串则隐藏；同一资源复用不重载；切资源前释放上一个 VideoClip
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    [LabelText("VideoPlayer 视频播放器")]
    [AddComponentMenu("UIBind/Data/视频播放器 【VideoPlayer】 UIDataBindVideoPlayer")]
    public sealed class UIDataBindVideoPlayer : UIDataBindSelectBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("视频播放器")]
        private VideoPlayer m_VideoPlayer;

        /// <summary>是否允许本组件修改 VideoPlayer.enabled</summary>
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

        /// <summary>刷新时补齐 VideoPlayer 引用并检查 Enabled 配置冲突</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_VideoPlayer ??= GetComponent<VideoPlayer>();
            if (!m_ChangeEnabled && !m_VideoPlayer.enabled)
            {
                Logger.LogError($"{name} 当前禁止修改Enabled 且当前处于隐藏状态 可能会出现问题 请检查");
            }
        }

        /// <summary>按开关设置 VideoPlayer.enabled（受 m_ChangeEnabled 限制）</summary>
        private void SetEnabled(bool set)
        {
            if (!m_ChangeEnabled) return;

            if (m_VideoPlayer == null) return;

            m_VideoPlayer.enabled = set;
        }

        /// <summary>数据变化：空串隐藏，否则异步加载并播放</summary>
        protected override void OnValueChanged()
        {
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            if (this.m_VideoPlayer == null || gameObject == null) return;

            var dataValue = GetFirstValue<string>();

            if (string.IsNullOrEmpty(dataValue))
            {
                SetEnabled(false);
                return;
            }

            this.ChangeAudio(dataValue).NoContext();
        }

        /// <summary>异步加载 VideoClip 并赋值（协程锁防并发，加载后校验对象存活）</summary>
        private async Promise ChangeAudio(string resName)
        {
            using var _ = await EventSystem.Instance?.UIInvokeEntityAsyncSafety<UIInvokeEntity_CoroutineLock, Promise<Entity>>(UISingletonHelper.UIMgr, new UIInvokeEntity_CoroutineLock { Lock = GetHashCode() });

            if (m_LastResName == resName)
            {
                if (this.m_VideoPlayer != null && this.m_VideoPlayer.clip != null)
                {
                    SetEnabled(true);
                }
                else
                {
                    SetEnabled(false);
                }

                return;
            }

            var loadResult = await EventSystem.Instance?.UIInvokeEntityAsyncSafety<UIInvokeEntity_Load, Promise<UnityObject>>(UISingletonHelper.UIMgr, new UIInvokeEntity_Load
            {
                LoadType = typeof(VideoClip),
                ResName  = resName
            });

            if (loadResult == null)
            {
                m_LastResName = "";
                SetEnabled(false);
                return;
            }

            this.ReleaseLastAudioClip();

            if (this == null || gameObject == null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_Release { obj = loadResult });
                return;
            }

            if (m_VideoPlayer == null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_Release { obj = loadResult });
                Logger.LogError($"{resName} 加载过程中 对象被摧毁了 m_VideoPlayer == null");
                return;
            }

            var videoClip = loadResult as VideoClip;
            this.m_LastVideoClip    = videoClip;
            this.m_VideoPlayer.clip = videoClip;

            SetEnabled(true);
            m_LastResName = resName;
        }

        /// <summary>解绑时释放已加载的视频资源</summary>
        protected override void UnBindData()
        {
            base.UnBindData();
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            this.ReleaseLastAudioClip();
        }

        private VideoClip m_LastVideoClip;

        /// <summary>释放上一个 VideoClip</summary>
        private void ReleaseLastAudioClip()
        {
            if (this.m_LastVideoClip != null)
            {
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_Release { obj = this.m_LastVideoClip });
                this.m_LastVideoClip = null;
            }
        }
    }
}