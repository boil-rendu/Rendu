using System.IO;
using System.Linq;
using RD.Init.Editor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    public class PackageCreatePackageJsonCode : BaseTemplate
    {
        private readonly string m_EventName = "PackageJson";
        public override  string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override  bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override  bool ShowTips => m_ShowTips;

        public PackageCreatePackageJsonCode(PackageCreateInfo codeData) : base("")
        {
            Create(codeData);
        }

  
        private bool Create(PackageCreateInfo codeData)
        {
            var path     = $"{codeData.PackagePath}/package.json";
            var template = $"{PackageCreateHelper.PackageCreateTemplatePath}/package.txt";

            // 检查文件是否已存在
            bool fileExists = TemplateEngine.FileExists(path);

            // 如果文件已存在，进行增量更新
            if (fileExists)
            {
                return UpdateExistingPackageJson(path, codeData);
            }

            // 文件不存在，直接创建新文件
            CreateVo = new CreateVo(template, path);

            // 必需属性
            ValueDic["FormalName"]   = codeData.PackageInfo.Required.FormalName;
            ValueDic["Version"]   = codeData.PackageInfo.Required.Version;
            ValueDic["DisplayName"]   = codeData.PackageInfo.Required.DisplayName;
            ValueDic["Unity"] = codeData.PackageInfo.Required.Unity;

            // 可选属性
            ValueDic["Description"]   = codeData.PackageInfo.Optional.Description;
            ValueDic["AuthorName"] = codeData.PackageInfo.Optional.AuthorName;
            ValueDic["AuthorEmail"] = codeData.PackageInfo.Optional.AuthorEmail;
            ValueDic["AuthorUrl"] = codeData.PackageInfo.Optional.AuthorUrl;

            // Keywords 需要转为 JSON 数组格式
            var keywordsArray = ParseKeywords(codeData.PackageInfo.Optional.Keywords);
            ValueDic["Keywords"] = FormatKeywordsArray(keywordsArray);

            return CreateNewFile();
        }

        /// <summary>
        /// 写入已有的 package.json 文件
        /// 保留用户已有的配置，只更新指定的字段
        /// </summary>
        private bool UpdateExistingPackageJson(string path, PackageCreateInfo codeData)
        {
            try
            {
                var fullPath = EditorHelper.GetProjPath(path);
                var existingJson = File.ReadAllText(fullPath);

                // 解析已有 JSON
                var existingData = JsonUtility.FromJson<PackageJsonData>(existingJson);

                // 更新必需字段
                existingData.name = codeData.PackageInfo.Required.FormalName;
                existingData.version = codeData.PackageInfo.Required.Version;
                existingData.displayName = codeData.PackageInfo.Required.DisplayName;
                existingData.unity = codeData.PackageInfo.Required.Unity;

                // 更新可选属性（如果不为空）
                if (!string.IsNullOrEmpty(codeData.PackageInfo.Optional.Description))
                    existingData.description = codeData.PackageInfo.Optional.Description;

                // 更新 author 对象
                if (!string.IsNullOrEmpty(codeData.PackageInfo.Optional.AuthorName) ||
                    !string.IsNullOrEmpty(codeData.PackageInfo.Optional.AuthorEmail) ||
                    !string.IsNullOrEmpty(codeData.PackageInfo.Optional.AuthorUrl))
                {
                    if (existingData.author == null)
                        existingData.author = new PackageJsonData.AuthorInfo();

                    if (!string.IsNullOrEmpty(codeData.PackageInfo.Optional.AuthorName))
                        existingData.author.name = codeData.PackageInfo.Optional.AuthorName;
                    if (!string.IsNullOrEmpty(codeData.PackageInfo.Optional.AuthorEmail))
                        existingData.author.email = codeData.PackageInfo.Optional.AuthorEmail;
                    if (!string.IsNullOrEmpty(codeData.PackageInfo.Optional.AuthorUrl))
                        existingData.author.url = codeData.PackageInfo.Optional.AuthorUrl;
                }

                if (!string.IsNullOrEmpty(codeData.PackageInfo.Optional.Keywords))
                    existingData.keywords = ParseKeywords(codeData.PackageInfo.Optional.Keywords);

                // 保留已有字段（如果存在）
                if (existingData.repository == null)
                    existingData.repository = new { };
                if (existingData.relatedPackages == null)
                    existingData.relatedPackages = new { };
                if (existingData.dependencies == null)
                    existingData.dependencies = new { };
                if (existingData.publishConfig == null)
                    existingData.publishConfig = new { };

                // 写回文件
                var updatedJson = JsonUtility.ToJson(existingData, true);
                File.WriteAllText(fullPath, updatedJson);

                Debug.Log($"[PackageCreatePackageJsonCode] 已写入 package.json: {path}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PackageCreatePackageJsonCode] 写入 package.json 失败: {e.Message}");
                return false;
            }
        }

        private string[] ParseKeywords(string keywordsStr)
        {
            if (string.IsNullOrEmpty(keywordsStr))
                return new string[0];

            return keywordsStr.Split(new[] { ',', '，', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        private string FormatKeywordsArray(string[] keywords)
        {
            if (keywords == null || keywords.Length == 0)
                return "[]";

            var quotedKeywords = keywords.Select(k => $"\"{k}\"");
            return $"[{string.Join(", ", quotedKeywords)}]";
        }

        /// <summary>
        /// 用于解析 package.json 的数据结构
        /// 与 package.txt 模板完全一致
        /// </summary>
        [System.Serializable]
        private class PackageJsonData
        {
            public string name;
            public string displayName;
            public string version;
            public string unity;
            public string description;
            public string[] keywords;
            public AuthorInfo author;
            public object repository;
            public object relatedPackages;
            public object dependencies;
            public object publishConfig;

            [System.Serializable]
            public class AuthorInfo
            {
                public string name;
                public string email;
                public string url;
            }
        }
    }
}
