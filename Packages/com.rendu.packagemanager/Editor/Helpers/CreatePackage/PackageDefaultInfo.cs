namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 包默认配置
    /// </summary>
    [System.Serializable]
    public class PackageDefaultInfo
    {
        public string formalPrefix = "cn.rendu.";
        public string displayPrefix = "RD.";
        public PackageInfo defaultValues = new PackageInfo
        {
            Required = new PackageRequiredInfo(),
            Optional = new PackageOptionalInfo()
        };
    }
}
