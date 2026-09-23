
namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目初始化数据模型
    /// </summary>
    public class ProjectInitModel
    {
        public string PackageName { get; set; }
        public string SceneName { get; set; }
        public CodeMode CodeMode { get; set; }
        public string Address { get; set; }
        public bool IsInited { get; set; }
    }
}
