using System;
using RD.Client;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 一次资源加载的句柄，以「包名 + 资源名」为键，内部维护引用计数
    /// 从 RefPool 池化取用（实现 IRefPool），计数归零时自动释放底层 Handle 并归还池
    /// </summary>
    [IgnoreCircularDependency]
    [EnableClass]
    public class LoadHandle : IRefPool
    {
        internal string PkgName { get; private set; }
        internal string ResName { get; private set; }
        internal int Handle { get; private set; }
        internal int RefCount { get; private set; }
        public UnityObject Object { get; private set; }
        private long m_NameCode;
        public long NameCode => m_NameCode;

        /// <summary>设置包名/资源名，并分配一个唯一 NameCode</summary>
        internal void SetGroupHandle(string pkgName, string resName)
        {
            PkgName = pkgName;
            ResName = resName;
            m_NameCode = IdGenerator.Instance.GenerateId();
        }

        /// <summary>归还池前清空全部状态</summary>
        public void Recycle()
        {
            PkgName = string.Empty;
            ResName = string.Empty;
            Handle = 0;
            RefCount = 0;
            m_NameCode = 0;
            Object = null;
        }

        /// <summary>设置实际加载到的对象与底层 Handle</summary>
        public void ResetHandle(UnityObject obj, int handle)
        {
            Object = obj;
            Handle = handle;
        }

        /// <summary>引用计数 +1</summary>
        public void AddRefCount()
        {
            RefCount++;
        }

        /// <summary>引用计数 -1；减到 0 及以下时真正释放资源</summary>
        public void RemoveRefCount()
        {
            RefCount--;
            if (RefCount <= 0)
            {
                Release();
            }
        }

        /// <summary>调用释放回调并把该 Handle 从 LoadHelper 中摘除</summary>
        private void Release()
        {
            if (Handle != 0)
            {
                UILoadDI.ReleaseAction?.Invoke(Handle);
            }

            LoadHelper.PutLoad(PkgName, ResName);
        }
    }
}