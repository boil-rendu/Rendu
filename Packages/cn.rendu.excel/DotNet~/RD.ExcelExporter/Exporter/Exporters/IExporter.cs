using OfficeOpenXml;

namespace RD
{
    /// <summary>
    /// 数据导出器接口
    /// </summary>
    public interface IDataExporter
    {
        /// <summary>
        /// 导出数据
        /// </summary>
        void Export(ExcelPackage package, ExcelFileInfo excelFile, Table table, string excelRootDir);
    }
}
