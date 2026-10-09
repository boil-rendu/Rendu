using System.Collections.Generic;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    //UI最高层级之上的 屏蔽所有操作的模块
    //屏蔽层显示时所有UI操作会被屏蔽 默认隐藏
    //可通过API 快捷屏蔽操作
    //适用于 统一动画播放时 某些操作需要等待时 都可以调节
    /// <summary>
    /// UIMgrComponent 的操作屏蔽部分
    /// 屏蔽层位于 UI 最高层级之上，显示时挡住下方所有 UI 的输入，默认隐藏
    /// </summary>
    public partial class UIMgrComponent
    {
        //当前层级屏蔽操作状态 true = 显示 = 无法操作 不要与可操作搞混
        /// <summary>屏蔽层是否处于显示状态（true = 显示 = 不可操作，注意语义与「可操作」相反）</summary>
        public bool LayerBlockActiveSelf => m_LayerBlock.activeSelf;

        //当前UI是否可以操作 true = 可以操作
        /// <summary>当前 UI 是否可操作</summary>
        public bool CanLayerBlockOption => !LayerBlockActiveSelf;

        /// <summary>内部屏蔽对象；显示时其下所有 UI 不可操作</summary>
        public GameObject m_LayerBlock; //内部屏蔽对象 显示时之下的所有UI将不可操作

        //永久屏蔽引用计数 一定要成对使用且保证
        //否则将会出现永久屏蔽的情况只能通过RecoverLayerOptionCountDown 强制恢复
        /// <summary>永久屏蔽的引用计数集合；必须成对增删，否则会永久卡住屏蔽，只能靠 RecoverLayerOptionCountDown 强制恢复</summary>
        public HashSet<long> m_AllForeverBlockCode = new();
    }
}
