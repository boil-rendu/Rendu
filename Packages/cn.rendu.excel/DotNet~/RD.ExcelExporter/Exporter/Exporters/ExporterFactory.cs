using OfficeOpenXml;

namespace RD
{
    /// <summary>
    /// 导出器工厂
    /// </summary>
    public class ExporterFactory
    {
        /// <summary>
        /// 创建 JSON 导出器
        /// </summary>
        public static IDataExporter CreateJsonExporter(PathProvider pathProvider)
        {
            return new JsonExporter(pathProvider);
        }

        /// <summary>
        /// 创建 Protobuf 导出器
        /// </summary>
        public static IDataExporter CreateProtobufExporter(PathProvider pathProvider, AssemblyProvider assemblyProvider)
        {
            return new ProtobufExporter(pathProvider, assemblyProvider);
        }
    }
}
