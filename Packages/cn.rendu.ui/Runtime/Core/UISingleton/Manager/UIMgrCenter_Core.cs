#define UIMACRO_SINGLETON_LOG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using RD;
using Debug = UnityEngine.Debug;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIMgrCenter 的核心实现部分</summary>
    public partial class UIMgrCenter
    {
        //内部类 核心管理
        /// <summary>
        /// 管理器的实际持有者：负责注册、分类缓存（Update/LateUpdate/FixedUpdate）与逐帧驱动
        /// 已 Dispose 的管理器会在 Update 结束时统一清理
        /// </summary>
        private class MgrCore
        {
            private List<IUIManager>            m_MgrList            = new List<IUIManager>();
            private List<IUIManagerUpdate>      m_MgrUpdateList      = new List<IUIManagerUpdate>();
            private List<IUIManagerLateUpdate>  m_MgrLateUpdateList  = new List<IUIManagerLateUpdate>();
            private List<IUIManagerFixedUpdate> m_MgrFixedUpdateList = new List<IUIManagerFixedUpdate>();
            private HashSet<IUIManager>         m_CacheInitMgr       = new HashSet<IUIManager>();

            /// <summary>
            /// 注册管理器并完成初始化：
            /// 重复注册 / 正在异步初始化中 都直接返回 false；实现 IUIManagerAsyncInit 的会 await 其初始化，
            /// 失败则不纳入管理（由调用方决定是否重试）；成功后按实现的 Update 接口分类缓存
            /// </summary>
            public async Promise<bool> Add(IUIManager manager)
            {
                if (m_MgrList.Contains(manager))
                {
                    Debug.LogError($"已存在Mgr {manager.GetType().Name} 请勿重复注册");
                    return false;
                }

                if (m_CacheInitMgr.Contains(manager))
                {
                    Debug.LogError($"{manager.GetType().Name} 请等待异步初始化中的管理器  请检查是否调用错误");
                    return false;
                }

                m_CacheInitMgr.Add(manager);

                if (manager is IUIManagerAsyncInit initialize)
                {
                    #if UIMACRO_SINGLETON_LOG
                    var sw = new Stopwatch();
                    sw.Start();
                    #endif

                    var result = await initialize.ManagerAsyncInit();

                    #if UIMACRO_SINGLETON_LOG
                    sw.Stop();
                    Debug.Log($"<color=green>MgrCenter: 管理器[<color=Brown>{manager.GetType().Name}</color>]初始化耗时 {sw.ElapsedMilliseconds} 毫秒</color>");
                    #endif

                    if (!result)
                    {
                        #if UIMACRO_SINGLETON_LOG
                        Debug.Log($"<color=red>MgrCenter: 管理器[<color=Brown>{manager.GetType().Name}</color>]初始化失败</color>");
                        #endif
                        return false; //初始化失败的管理器 不添加
                    }
                }

                m_CacheInitMgr.Remove(manager);

                #if UIMACRO_SINGLETON_LOG
                Debug.Log($"<color=navy>MgrCenter: 管理器[<color=Brown>{manager.GetType().Name}</color>]启动完毕</color>");
                #endif

                m_MgrList.Add(manager);

                //UIDisposerMonoSingleton 类型的单例 因为继承UnityMono 其实是可以使用自带的Update
                //也可以使用UI提供的独立Update 2种有区分 2选一即可

                if (manager is IUIManagerUpdate update)
                {
                    m_MgrUpdateList.Add(update);
                }

                if (manager is IUIManagerLateUpdate lateUpdate)
                {
                    m_MgrLateUpdateList.Add(lateUpdate);
                }

                if (manager is IUIManagerFixedUpdate fixedUpdate)
                {
                    m_MgrFixedUpdateList.Add(fixedUpdate);
                }

                return true;
            }

            /// <summary>驱动所有 Update 管理器（跳过已 Dispose / 未启用），单管理器异常只记日志不中断</summary>
            public void Update()
            {
                for (int i = 0; i < m_MgrUpdateList.Count; i++)
                {
                    IUIManagerUpdate manager = m_MgrUpdateList[i];

                    if (manager.Disposed) continue;
                    if (!manager.Enabled) continue;

                    try
                    {
                        manager.ManagerUpdate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"管理器={manager.GetType().Name}, err={e.Message}{e.StackTrace}");
                    }
                }

                CheckDisposed();
            }

            /// <summary>驱动所有 LateUpdate 管理器（同样的跳过与异常保护策略）</summary>
            public void LateUpdate()
            {
                for (int i = 0; i < m_MgrLateUpdateList.Count; i++)
                {
                    IUIManagerLateUpdate manager = m_MgrLateUpdateList[i];

                    if (manager.Disposed) continue;
                    if (!manager.Enabled) continue;

                    try
                    {
                        manager.ManagerLateUpdate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"管理器={manager.GetType().Name}, err={e.Message}{e.StackTrace}");
                    }
                }
            }

            /// <summary>驱动所有 FixedUpdate 管理器（同样的跳过与异常保护策略）</summary>
            public void FixedUpdate()
            {
                for (int i = 0; i < m_MgrFixedUpdateList.Count; i++)
                {
                    IUIManagerFixedUpdate manager = m_MgrFixedUpdateList[i];

                    if (manager.Disposed) continue;
                    if (!manager.Enabled) continue;

                    try
                    {
                        manager.ManagerFixedUpdate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"管理器={manager.GetType().Name}, err={e.Message}{e.StackTrace}");
                    }
                }
            }

            /// <summary>倒序扫描并摘除已 Dispose 的管理器（倒序以配合 Remove 时的下标安全）</summary>
            private void CheckDisposed()
            {
                for (int i = m_MgrList.Count - 1; i >= 0; i--)
                {
                    IUIManager manager = m_MgrList[i];
                    if (manager.Disposed)
                    {
                        Remove(manager);
                    }
                }
            }

            /// <summary>从全部缓存列表中摘除指定管理器</summary>
            private void Remove(IUIManager manager)
            {
                if (manager == null)
                {
                    return;
                }

                #if UIMACRO_SINGLETON_LOG
                Debug.Log($"<color=navy>MgrCenter: 管理器[<color=Brown>{manager.GetType().Name}</color>]移除</color>");
                #endif

                m_MgrList.Remove(manager);

                if (manager is IUIManagerUpdate update)
                {
                    m_MgrUpdateList.Remove(update);
                }

                if (manager is IUIManagerLateUpdate lateUpdate)
                {
                    m_MgrLateUpdateList.Remove(lateUpdate);
                }

                if (manager is IUIManagerFixedUpdate fixedUpdate)
                {
                    m_MgrFixedUpdateList.Remove(fixedUpdate);
                }
            }

            /// <summary>关闭：倒序 Dispose 所有管理器（与注册顺序相反）</summary>
            public void Dispose()
            {
                #if UIMACRO_SINGLETON_LOG
                Debug.Log("<color=navy>MgrCenter: 关闭MgrCenter</color>");
                #endif

                //倒过来dispose
                for (int i = m_MgrList.Count - 1; i >= 0; i--)
                {
                    IUIManager manager = m_MgrList[i];
                    manager.Dispose();
                }
            }
        }
    }
}
