namespace RD
{
    /// <summary>
    /// 服务端输出策略 - 处理 _S_ 标记的 Proto 文件
    /// 导出: Server/, ClientServer/
    ///
    /// _S_ 是 Inner 消息（服务端进程间通信，如 Gate <-> Realm <-> Map），客户端不需要。
    /// 由于 Server/ = _C_ ∪ _S_ ∪ _CS_ 与 ClientServer/ 恒等，本策略不会导致 Client/
    /// 缺少任何类型。
    /// </summary>
    public class ServerOutputStrategy : IOutputStrategy
    {
        public string[] GetOutputDirs(PathProvider pathProvider)
        {
            return new[]
            {
                pathProvider.GetServerMessagePath(),
                pathProvider.GetClientServerMessagePath()
            };
        }

        public void Generate(string code, OpcodeRegistry opcodeRegistry, string protoFileName, PathProvider pathProvider, string protoName)
        {
            var codeGenerator = new CodeGenerator(pathProvider, opcodeRegistry);

            // 添加 opcode 常量类
            string codeWithConstants = codeGenerator.AppendOpcodeConstants(code, protoName);

            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(protoFileName);

            // 导出到多个目录
            foreach (string outputDir in GetOutputDirs(pathProvider))
            {
                codeGenerator.GenerateToPath(codeWithConstants, outputDir, fileNameWithoutExtension);
            }
        }
    }
}
