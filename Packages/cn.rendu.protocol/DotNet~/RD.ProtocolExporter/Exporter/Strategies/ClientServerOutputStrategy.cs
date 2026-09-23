namespace RD
{
    /// <summary>
    /// 共享输出策略 - 处理 _CS_ 或无标记的 Proto 文件
    /// 导出到 Client、Server 和 ClientServer 三个目录
    /// </summary>
    public class ClientServerOutputStrategy : IOutputStrategy
    {
        public string[] GetOutputDirs(PathProvider pathProvider)
        {
            return new[]
            {
                pathProvider.GetClientMessagePath(),
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
