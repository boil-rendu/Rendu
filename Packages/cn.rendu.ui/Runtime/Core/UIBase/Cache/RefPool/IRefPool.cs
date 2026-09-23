using RD.Core;
namespace RD.Client
{
    /// <summary>
    /// 引用接口
    /// </summary>
    public interface IRefPool
    {
        /// <summary>
        /// 被回收时 重置所有数据
        /// </summary>
        void Recycle();
    }
}
