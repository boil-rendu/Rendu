using System;
using System.Collections.Generic;
using RD.EditorKit;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [InfoBox("提示: 可用事件参数 <参数1:Unity.GameObj(被控制的对象),bool(激活状态)>")]
    [LabelText("显隐<Unity.GameObj,bool>")]
    [AddComponentMenu("UIBind/Event/显隐 【Active】 UIEventBindActive")]
    /// <summary>
    /// 显隐事件绑定（同步）：组件 OnEnable/OnDisable 时，派发 (自身 gameObject, true/false)
    /// 用于让外部监听某个 UI 元素被激活/隐藏
    /// </summary>
    public class UIEventBindActive : UIEventBind
    {
        /// <summary>非异步事件</summary>
        protected override bool IsTaskEvent => false;

        [NonSerialized]
        private readonly List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType>
                                                                     {
                                                                         EUIEventParamType.UnityGameObject,
                                                                         EUIEventParamType.Bool,
                                                                     };

        /// <summary>过滤出参数为「Unity.GameObj,bool」的事件</summary>
        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        /// <summary>启用时派发（自身 gameObject, true）</summary>
        private void OnEnable()
        {
            try
            {
                m_UIEvent?.Invoke(gameObject, true);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        /// <summary>禁用时派发（自身 gameObject, false）</summary>
        private void OnDisable()
        {
            try
            {
                m_UIEvent?.Invoke(gameObject, false);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }
    }
}
