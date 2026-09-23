using RD.Core;
namespace RD.Client
{
    /// <summary>
    /// 时间提供器
    /// </summary>
    public interface ITimeProvider
    {
        float Time { get; }
    }
}
