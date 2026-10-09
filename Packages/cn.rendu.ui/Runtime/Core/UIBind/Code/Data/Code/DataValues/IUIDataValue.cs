using RD.Core;
namespace RD.Client
{
    /// <summary>
    /// 数据值只读取值契约
    /// 约定一个数据值能返回其内部承载的强类型值 <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">数据值承载的运行时类型</typeparam>
    public interface IUIDataValue<T>
    {
        /// <summary>获取当前值</summary>
        T GetValue();
    }
}
