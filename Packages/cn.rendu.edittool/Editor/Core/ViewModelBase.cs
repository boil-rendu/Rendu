using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// ViewModel 基类 - 实现 INotifyPropertyChanged
    /// 提供属性变更通知和统一的属性设置方法
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并通知变更
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名（自动获取）</param>
        /// <returns>是否发生了变更</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并通知变更（带回调）
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="onChanged">变更回调</param>
        /// <param name="propertyName">属性名（自动获取）</param>
        /// <returns>是否发生了变更</returns>
        protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            onChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 批量设置属性（优化性能）
        /// </summary>
        /// <param name="actions">属性设置操作集合</param>
        protected void SetPropertyBatch(params Action[] actions)
        {
            foreach (var action in actions)
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            PropertyChanged = null;
        }
    }
}
