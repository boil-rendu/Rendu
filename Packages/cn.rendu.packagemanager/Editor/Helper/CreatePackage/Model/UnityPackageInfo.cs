using System;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// Unity package.json 数据模型
    /// </summary>
    [Serializable]
    public class UnityPackageInfo
    {
        public string name;
        public string version;
        public string displayName;
        public string description;
        public string unity;
        public AuthorInfo author;
        public string[] keywords;
    }

    [Serializable]
    public class AuthorInfo
    {
        public string name;
        public string email;
        public string url;
    }
}
