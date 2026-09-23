using UnityEngine.Serialization;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 必需属性
    /// </summary>
    [System.Serializable]
    public class PackageRequiredInfo
    {
        public string FormalName;
        public string DisplayName;
        public string Version;
        public string Unity;
        public AssemblyDefineType AsmDefType;
        public CodeModeType  CodeModeType;
        public AssemblyReferenceType AsmRefType;
        public bool GenerateIgnoreAsmdef;
    }

    /// <summary>
    /// 可选属性
    /// </summary>
    [System.Serializable]
    public class PackageOptionalInfo
    {
        public string Description;
        public string AuthorName;
        public string AuthorEmail;
        public string AuthorUrl;
        public string Keywords;
    }

    /// <summary>
    /// 包信息数据模型
    /// </summary>
    [System.Serializable]
    public class PackageInfo
    {
        public PackageRequiredInfo Required = new PackageRequiredInfo();
        public PackageOptionalInfo Optional = new PackageOptionalInfo();
    }
}
