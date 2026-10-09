//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System.Collections.Generic;
using RD;
using RD.Core;

namespace RD.Client
{
    //管理所有继承IManager的管理器
    /// <summary>
    /// UI 管理器中心（单例 MonoBehaviour）
    /// 统一注册、初始化与逐帧驱动所有 IUIManager；初始化失败的进入失败队列，可重试
    /// </summary>
    public partial class UIMgrCenter : UIMonoSingleton<UIMgrCenter>
    {
        private MgrCore g_MgrCore = new MgrCore();

        //失败的初始化队列 //可根据需求对失败的进行二次处理
        /// <summary>初始化失败的管理器队列，可取出做二次处理</summary>
        private Queue<IUIManager> m_FailInited = new Queue<IUIManager>();

        //异步注册需要管理的 管理器 如果有异步初始化方法则会调用
        //返回初始化结果 如果失败是无法加入到管理中的
        //失败的可以重复调用直到成功
        //只有注册没有对应的移除  需要移除直接Dispose释放即可
        /// <summary>
        /// 异步注册一个管理器（会调用其异步初始化）
        /// 初始化失败则不会被纳入管理，而是进入失败队列，可反复调用直到成功
        /// 只提供注册、没有对应的反注册，需要移除时直接 Dispose 该管理器
        /// </summary>
        public async Promise<bool> Register(IUIManager manager)
        {
            var result = await g_MgrCore.Add(manager);
            if (!result)
            {
                m_FailInited.Enqueue(manager);
            }

            return result;
        }

        //获取失败的管理器
        //队列出队 可能没有
        /// <summary>取出一个初始化失败的管理器（出队，可能为空）</summary>
        public IUIManager GetFailInitedMgr()
        {
            return m_FailInited.Dequeue();
        }

        //获取当前失败数量
        /// <summary>当前初始化失败的管理器数量</summary>
        public int GetFailInitedCount()
        {
            return m_FailInited.Count;
        }

        //获取当前剩余的所有
        /// <summary>取出当前全部失败的管理器（会清空失败队列）</summary>
        public List<IUIManager> GetFailInitedMgrList()
        {
            var list  = new List<IUIManager>();
            var count = GetFailInitedCount();
            for (int i = 0; i < count; i++)
            {
                list.Add(GetFailInitedMgr());
            }

            return list;
        }

        private void Update()
        {
            g_MgrCore?.Update();
        }

        private void LateUpdate()
        {
            g_MgrCore?.LateUpdate();
        }

        private void FixedUpdate()
        {
            g_MgrCore?.FixedUpdate();
        }

        /// <summary>释放时一并释放 MgrCore</summary>
        protected override void OnDispose()
        {
            g_MgrCore?.Dispose();
            g_MgrCore = null;
        }
    }
}
