namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目文件数据模型
    /// </summary>
    public class ProjectFilesModel
    {
        public bool IsRegenerating { get; set; }
        public int DeletedFilesCount { get; set; }
        public string LastGeneratedTime { get; set; }
    }
}
