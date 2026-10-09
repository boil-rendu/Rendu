using System;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    //生命周期
    /// <summary>
    /// UIBindCDETable 的生命周期转发（partial）
    /// 把 Unity 的 OnEnable/Start/OnDisable/OnDestroy 转成可挂接的 Action（由业务系统订阅）
    /// </summary>
    public sealed partial class UIBindCDETable
    {
        /// <summary>OnEnable 回调</summary>
        internal Action UIBaseOnEnable;

        private void OnEnable()
        {
            try
            {
                UIBaseOnEnable?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        /// <summary>Start 回调</summary>
        internal Action UIBaseStart;

        private void Start()
        {
            try
            {
                UIBaseStart?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        /// <summary>OnDisable 回调</summary>
        internal Action UIBaseOnDisable;

        private void OnDisable()
        {
            try
            {
                UIBaseOnDisable?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        /// <summary>OnDestroy 回调（异常只记录，不再向上抛）</summary>
        internal Action UIBaseOnDestroy;

        private void OnDestroy()
        {
            try
            {
                UIBaseOnDestroy?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
