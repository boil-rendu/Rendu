using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIWindowComponent 的接口探测部分：判断 OwnerUIEntity 是否实现了各补间/关闭相关接口
    /// 四个属性都带「已探测」标记，接口判定结果只算一次并缓存
    /// </summary>
    public partial class UIWindowComponent
    {
        private bool m_CloseTweenEnd;

        private bool m_CheckCloseTweenEnd;

        /// <summary>OwnerUIEntity 是否实现 IUICloseTweenEnd（关闭补间结束回调）</summary>
        public bool CloseTweenEnd
        {
            get
            {
                if (m_CheckCloseTweenEnd)
                {
                    return m_CloseTweenEnd;
                }

                m_CloseTweenEnd      = OwnerUIEntity is IUICloseTweenEnd;
                m_CheckCloseTweenEnd = true;
                return m_CloseTweenEnd;
            }
        }

        private bool m_OpenTweenEnd;

        private bool m_CheckOpenTweenEnd;

        /// <summary>OwnerUIEntity 是否实现 IUIOpenTweenEnd（打开补间结束回调）</summary>
        public bool OpenTweenEnd
        {
            get
            {
                if (m_CheckOpenTweenEnd)
                {
                    return m_OpenTweenEnd;
                }

                m_OpenTweenEnd      = OwnerUIEntity is IUIOpenTweenEnd;
                m_CheckOpenTweenEnd = true;
                return m_OpenTweenEnd;
            }
        }

        private bool m_OpenTween;

        private bool m_CheckOpenTween;

        /// <summary>OwnerUIEntity 是否实现 IUIOpenTween（需要自定义打开补间）</summary>
        public bool OpenTween
        {
            get
            {
                if (m_CheckOpenTween)
                {
                    return m_OpenTween;
                }

                m_OpenTween      = OwnerUIEntity is IUIOpenTween;
                m_CheckOpenTween = true;
                return m_OpenTween;
            }
        }

        private bool m_CloseTween;

        private bool m_CheckCloseTween;

        /// <summary>OwnerUIEntity 是否实现 IUICloseTween（需要自定义关闭补间）</summary>
        public bool CloseTween
        {
            get
            {
                if (m_CheckCloseTween)
                {
                    return m_CloseTween;
                }

                m_CloseTween      = OwnerUIEntity is IUICloseTween;
                m_CheckCloseTween = true;
                return m_CloseTween;
            }
        }
    }
}