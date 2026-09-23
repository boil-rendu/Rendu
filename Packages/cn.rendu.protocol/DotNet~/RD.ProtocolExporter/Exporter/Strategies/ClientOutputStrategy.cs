namespace RD
{
    /// <summary>
    /// 客户端输出策略 - 处理 _C_ 标记的 Proto 文件
    /// 导出: Client/, Server/, ClientServer/
    ///
    /// _C_ 是 Outer 消息（客户端与服务端之间的通信），并非"客户端专用"：
    /// 其中包含客户端发往服务端的请求（如 C2R_Login），服务端必须持有该类型才能反序列化。
    /// 因此 Server/ 在此处是必需输出，不可移除。
    /// </summary>
    public class ClientOutputStrategy : IOutputStrategy
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
