namespace RD
{
    /// <summary>
    /// 输出策略接口 - 定义不同输出类型的行为
    /// </summary>
    public interface IOutputStrategy
    {
        /// <summary>
        /// 获取需要导出的目标目录列表
        /// </summary>
        string[] GetOutputDirs(PathProvider pathProvider);

        /// <summary>
        /// 生成 CS 文件
        /// </summary>
        void Generate(string code, OpcodeRegistry opcodeRegistry, string protoFileName, PathProvider pathProvider, string protoName);
    }
}
