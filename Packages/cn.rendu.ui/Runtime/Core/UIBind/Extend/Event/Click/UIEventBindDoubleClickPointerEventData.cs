using System;
using System.Collections.Generic;
using RD.Inspector;
using UnityEngine;
using UnityEngine.EventSystems;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 双击事件绑定（带 PointerEventData 参数版）
    /// 覆写 OnUIEvent，把 PointerEventData 以 object 形式作为事件参数派发
    /// </summary>
    [InfoBox("提示: 可用事件参数 1个 , Object(PointerEventData)")]
    [LabelText("双击<PointerEventData>")]
    [AddComponentMenu("UIBind/Event/双击 【DoubleClick PointerEventData】 UIEventBindDoubleClickPointerEventData")]
    public class UIEventBindDoubleClickPointerEventData : UIEventBindDoubleClick
    {
        [NonSerialized]
        private readonly List<EUIEventParamType> m_FilterParamType = new() { EUIEventParamType.Object };

        /// <summary>过滤出参数为 Object 的事件</summary>
        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        /// <summary>判定为双击后的派发点：把 PointerEventData 作为 object 派发</summary>
        protected override void OnUIEvent(PointerEventData eventData)
        {
            //额外添加 如果想要这个点击事件 使用此监听
            //响应方法那边参数是obj 自己在转一次 没有扩展这个参数 因为没必要
            m_UIEvent?.Invoke(eventData as object);
        }
    }
}
