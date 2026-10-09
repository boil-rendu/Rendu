using System;
using RD.Inspector;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 数据值抽象基类（非泛型）
    /// UIBind 中所有「数据」的统一基类，仅携带类型信息与无参「值改变」事件；
    /// 承载具体类型值的泛型实现见 <see cref="UIDataValueBase{T}"/>
    /// </summary>
    [Serializable]
    [HideLabel]
    public abstract class UIDataValue
    {
        /// <summary>该数据值对应的绑定数据类型</summary>
        [ShowInInspector]
        [PropertyOrder(-99)]
        public abstract EUIBindDataType UIBindDataType { get; }

        /// <summary>该数据值承载的 CLR 运行时类型</summary>
        public abstract Type UIDataValueType { get; }

        /// <summary>
        /// 从另一个Value设置数据
        /// </summary>
        internal abstract bool SetValueFrom(UIDataValue dataValue);

        //值改变消息 无参
        private Action m_OnValueChangAction;

        /// <summary>注册无参的「值改变」回调（先移除再添加，避免重复订阅）</summary>
        internal void AddValueChangeAction(Action action)
        {
            m_OnValueChangAction -= action;
            m_OnValueChangAction += action;
        }

        /// <summary>移除无参的「值改变」回调</summary>
        internal void RemoveValueChangeAction(Action action)
        {
            m_OnValueChangAction -= action;
        }

        /// <summary>触发无参「值改变」回调（异常时记录并向上抛）</summary>
        internal void InvokeValueChangAction()
        {
            try
            {
                m_OnValueChangAction?.Invoke();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }
    }
}
