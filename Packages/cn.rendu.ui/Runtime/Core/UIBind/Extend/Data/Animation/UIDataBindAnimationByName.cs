using System.Collections.Generic;
using RD.Inspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using RD.Core;
#endif

namespace RD.Client
{
    /// <summary>
    /// 动画数据绑定（String → 按名称播放动画片段）
    /// 名称不存在会报错；编辑器下会维护「当前所有可用动画」列表
    /// </summary>
    [RequireComponent(typeof(Animation))]
    [DetailedInfoBox("注意动画组件的 自动播放不要勾选",
                     @"根据名称播放动画 不存在则会提示
使用方法: 
1 挂上 Animation 组件
2 Ctrl + 6 动态创建一个动画片段
3 吧对应的动画片段拖到组件中
4 修改了组件后记得保存 触发一次刷新")]
    [LabelText("动画")]
    [AddComponentMenu("UIBind/Data/动画 【AnimationByName】 UIDataBindAnimationByName")]
    public sealed class UIDataBindAnimationByName : UIDataBindSelectBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("动画")]
        private Animation m_Animation;

        /// <summary>当前所有可用动画片段名（编辑器下刷新）</summary>
        [SerializeField]
        [ReadOnly]
        [LabelText("当前所有可用动画")]
        private List<string> m_AllClipName = new List<string>();

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

        /// <summary>刷新时补齐 Animation 引用并采集所有片段名（仅编辑器）</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Animation ??= GetComponent<Animation>();
            if (m_Animation != null)
            {
                m_Animation.playAutomatically = false;
                #if UNITY_EDITOR
                m_AllClipName.Clear();

                var clips = AnimationUtility.GetAnimationClips(m_Animation.gameObject);

                foreach (var clip in clips)
                {
                    m_AllClipName.Add(clip.name);
                }
                #endif
            }
            else
            {
                #if UNITY_EDITOR
                m_AllClipName.Clear();
                #endif
            }
        }

        /// <summary>数据变化：名称合法则播放对应片段</summary>
        protected override void OnValueChanged()
        {
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            if (m_Animation == null) return;

            var dataValue = GetFirstValue<string>();
            if (string.IsNullOrEmpty(dataValue)) return;

            if (!m_AllClipName.Contains(dataValue))
            {
                Logger.LogErrorContext(this, $"{name} 播放失败 请检查动画名称是否存在 {dataValue}");
                return;
            }

            m_Animation.Play(dataValue);
        }
    }
}
