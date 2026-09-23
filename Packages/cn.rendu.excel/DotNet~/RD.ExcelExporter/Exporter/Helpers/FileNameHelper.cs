using System.IO;

namespace RD
{
    /// <summary>
    /// 文件名辅助类
    /// </summary>
    public static class FileNameHelper
    {
        /// <summary>
        /// 从文件名提取 Proto 名称
        /// </summary>
        public static string ExtractProtoName(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            if (name.Contains('_'))
                name = name.Substring(0, name.LastIndexOf('_'));
            if (name.Contains('@'))
                name = name.Substring(0, name.IndexOf('@'));
            return name;
        }

        /// <summary>
        /// 从文件名提取配置类型
        /// </summary>
        public static ConfigType ExtractConfigType(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            if (name.Contains('@'))
            {
                string[] parts = name.Split('@');
                string cs = parts[1];
                if (string.IsNullOrEmpty(cs))
                    cs = "cs";

                return cs.Contains("c") && cs.Contains("s") ? ConfigType.cs :
                       cs.Contains("s") ? ConfigType.s : ConfigType.c;
            }
            return ConfigType.cs;
        }
    }
}
