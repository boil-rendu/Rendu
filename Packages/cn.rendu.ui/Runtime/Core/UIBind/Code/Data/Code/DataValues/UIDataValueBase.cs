using System;
using RD.Panel;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 数据基类
    /// 泛型 扩展请参考其他数据 注意满足需求
    /// 具体请查看文档
    /// @李胜扬
    /// </summary>
    /// <typeparam name="T">承载的运行时类型</typeparam>
    [Serializable]
    public abstract class UIDataValueBase<T> : UIDataValue, IUIDataValue<T>
    {
        [SerializeField]
        [LabelText("值")]
        [HideReferenceObjectPicker]
        private T m_Value;

        //基类中的事件 双参数 1 新值 2 老值
        private Action<T, T> m_OnValueChangeAction;

        /// <summary>注册带 (新值, 旧值) 的「值改变」回调（先移除再添加，避免重复订阅）</summary>
        public void AddValueChangeAction(Action<T, T> action)
        {
            m_OnValueChangeAction -= action;
            m_OnValueChangeAction += action;
        }

        /// <summary>移除带 (新值, 旧值) 的「值改变」回调</summary>
        public void RemoveValueChangeAction(Action<T, T> action)
        {
            m_OnValueChangeAction -= action;
        }

        /// <summary>触发带 (新值, 旧值) 的「值改变」回调（异常时记录并向上抛）</summary>
        public void InvokeValueChangAction(T newValue, T oldValue)
        {
            try
            {
                m_OnValueChangeAction?.Invoke(newValue, oldValue);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        /// <summary>获取当前值</summary>
        public T GetValue()
        {
            return m_Value;
        }

        /// <summary>以强制刷新方式从另一个数据值拷贝（基类统一入口）</summary>
        internal sealed override bool SetValueFrom(UIDataValue dataValue)
        {
            return SetValueFrom(dataValue, true); //必须使用强制刷新
        }

        /// <summary>
        /// 从另一个数据值拷贝
        /// </summary>
        /// <param name="dataValue">源数据值，类型必须与 <typeparamref name="T"/> 一致</param>
        /// <param name="force">true 时跳过相等判断强制赋值</param>
        /// <param name="notify">是否派发「值改变」事件</param>
        public bool SetValueFrom(UIDataValue dataValue, bool force, bool notify = true)
        {
            if (dataValue == null)
            {
                Logger.LogError($"{typeof(T)} 失败，Value为空");
                return false;
            }

            if (!(dataValue is UIDataValueBase<T>))
            {
                Logger.LogError($"失败，类型不一致 当前类型 {typeof(T)} 传入类型 {dataValue.UIDataValueType}");
                return false;
            }

            return SetValue(dataValue.GetValue<T>(), force, notify);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value">新值</param>
        /// <param name="force">true 时跳过相等判断强制赋值</param>
        /// <param name="notify">是否派发「值改变」事件</param>
        /// <returns>值是否发生变化（false 表示被相等判断拦下）</returns>
        public bool SetValue(T value, bool force = false, bool notify = true)
        {
            if (!force && EqualsValue(value)) return false;

            var oldValue = m_Value;
            SetValueFrom(value);
            if (notify)
            {
                InvokeValueChangAction();
                InvokeValueChangAction(value, oldValue);
            }
            return true;
        }

        /// <summary>写入值（子类可覆写以处理容器类型，如 List 的原地更新）</summary>
        protected virtual void SetValueFrom(T value)
        {
            m_Value = value;
        }

        /// <summary>判断给定值是否与当前值相等（子类实现各自比较规则/精度）</summary>
        protected abstract bool EqualsValue(T value);
    }
}
