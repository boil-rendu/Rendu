using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace RD.Core
{
    /// <summary>
    /// 需要显式释放资源的对象基类：<see cref="Object"/> + <see cref="IDisposable"/> + <see cref="ISupportInitialize"/>。
    /// 派生：<see cref="ASingleton"/>（单例生命周期）、<see cref="MessageObject"/>（网络消息池化）。
    /// </summary>
    public abstract class DisposeObject: Object, IDisposable, ISupportInitialize
    {
        /// <summary>释放资源：默认空实现，派生类重写释放 socket/线程/订阅等。</summary>
        public virtual void Dispose()
        {
        }

        /// <summary><see cref="ISupportInitialize.BeginInit"/>：Bson 反序列化开始钩子，默认空实现。</summary>
        public virtual void BeginInit()
        {
        }

        /// <summary><see cref="ISupportInitialize.EndInit"/>：Bson 反序列化结束钩子，默认空实现。</summary>
        public virtual void EndInit()
        {
        }
    }

    /// <summary>
    /// 池化对象标记接口：实现类在反序列化/回收时会从 <see cref="ObjectPool"/> 取出而非新建，<see cref="IsFromPool"/> 标识当前实例是否池化。
    /// 派生：<see cref="MessageObject"/>（网络消息高频池化）。
    /// </summary>
    public interface IPool: IDisposable
    {
        /// <summary>当前实例是否来自 <see cref="ObjectPool"/>（池化实例需要在 <see cref="IDisposable.Dispose"/> 时回池而非真正释放）。</summary>
        bool IsFromPool
        {
            get;
            set;
        }
    }
}
