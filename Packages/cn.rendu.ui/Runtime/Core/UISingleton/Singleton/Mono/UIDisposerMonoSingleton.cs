using System;
using RD;
using RD.Draw;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    //基类使用 RD.Draw 特性 方便做一些显示
    //没有的不需要的可以改为UnityEngine.MonoBehaviour
    public abstract class UIDisposerMonoSingleton : MonoBehaviour, IUIManagerAsyncInit
    {
        private bool m_Disposed;
        public bool Disposed => m_Disposed;

        //释放方法1: 对象释放
        public bool Dispose()
        {
            if (m_Disposed)
            {
                return false;
            }

            m_Disposed = true;
            OnDispose();
            gameObject.SafeDestroySelf();
            return true;
        }

        //如非必要 任何子类都不要重写unity的OnDestroy
        //重写请base
        //推荐使用OnDispose
        protected virtual void OnDestroy()
        {
            if (!m_Disposed)
            {
                if (GetDontDestroyOnLoad())
                {
                    if (UIOperationHelper.IsPlaying())
                    {
                        //进入到这里说明不是被dispose 调用后摧毁的
                        //而是直接被摧毁的 这种行为是不允许的
                        Debug.LogError($"{this.name} 请调用 Dispose/DisposeInst 来移除Mono单例 而非直接删除GameObject对象");
                    }
                }
            }
        }

        /// <summary>
        /// 是否切换场景时不销毁销毁
        /// </summary>
        protected virtual bool GetDontDestroyOnLoad()
        {
            return true;
        }

        /// <summary>
        /// 实例对象隐藏
        /// </summary>
        protected virtual bool GetHideAndDontSave()
        {
            return false;
        }

        /// <summary>
        /// 处理释放相关事情
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        //初始化
        protected virtual void OnInitSingleton()
        {
        }

        //每次使用前
        protected virtual void OnUseSingleton()
        {
        }

        private bool m_Enabled;

        public bool Enabled => m_Enabled;

        public void SetEnabled(bool value)
        {
            m_Enabled = value;
        }

        private bool m_InitedSucceed;

        public bool InitedSucceed => m_InitedSucceed;

        public async Promise<bool> ManagerAsyncInit()
        {
            if (m_InitedSucceed)
            {
                Debug.LogError($"{gameObject.name}已成功初始化过 请勿重复初始化");
                return true;
            }

            var result = await MgrAsyncInit();
            if (!result)
            {
                Debug.LogError($"{gameObject.name} 初始化失败");
            }
            else
            {
                //成功初始化才记录
                m_InitedSucceed = true;
            }

            return result;
        }

        protected virtual async Promise<bool> MgrAsyncInit()
        {
            await Promise.CompletedTask;
            return true;
        }
    }
}