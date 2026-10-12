using System;
using System.Collections.Generic;
using RD;
using RD.Panel;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [InfoBox("提示: 可用事件参数 <参数1:Unity.GameObj(被控制的对象),bool(激活状态)>")]
    [LabelText("显隐<Unity.GameObj,bool>")]
    [AddComponentMenu("UIBind/TaskEvent/显隐 【Active】 UITaskEventBindActive")]
    /// <summary>
    /// 显隐事件绑定（异步 Task）：组件 OnEnable/OnDisable 时，异步派发 (自身 gameObject, true/false)
    /// 与 UIEventBindActive 的区别是走 InvokeAsync，用 NoContext 不等待回调完成
    /// </summary>
    public class UITaskEventBindActive : UIEventBind
    {
        /// <summary>异步事件</summary>
        protected override bool IsTaskEvent => true;

        [NonSerialized]
        private readonly List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType>
                                                                     {
                                                                         EUIEventParamType.UnityGameObject, EUIEventParamType.Bool,
                                                                     };

        /// <summary>过滤出参数为「Unity.GameObj,bool」的事件</summary>
        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        /// <summary>启用时异步派发（自身 gameObject, true）</summary>
        private void OnEnable()
        {
            try
            {
                OnTaskEvent(true).NoContext();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        /// <summary>禁用时异步派发（自身 gameObject, false）</summary>
        private void OnDisable()
        {
            try
            {
                OnTaskEvent(false).NoContext();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        /// <summary>异步派发：NoContext 发起后不等待完成</summary>
        private async Promise OnTaskEvent(bool value)
        {
            await m_UIEvent?.InvokeAsync(gameObject, value);
        }
    }
}
