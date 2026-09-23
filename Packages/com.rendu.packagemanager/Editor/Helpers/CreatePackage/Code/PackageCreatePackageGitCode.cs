using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    public class PackageCreatePackageGitCode : BaseTemplate
    {
        private readonly string m_EventName = "PackageGit";
        public override  string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override  bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override  bool ShowTips => m_ShowTips;

        public PackageCreatePackageGitCode(PackageCreateInfo codeData) : base("")
        {
            Create(codeData);
        }

        public PackageCreatePackageGitCode(out bool result, PackageCreateInfo codeData) : base("")
        {
            result = Create(codeData);
        }

        private bool Create(PackageCreateInfo codeData)
        {
            var path     = $"{codeData.PackagePath}/packagegit.json";
            var template = $"{PackageCreateHelper.PackageCreateTemplatePath}/packagegit.txt";

            // 检查文件是否已存在
            bool fileExists = TemplateEngine.FileExists(path);

            // 如果文件已存在，跳过更新（packagegit.json 通常由用户维护）
            if (fileExists)
            {
                Debug.Log($"[PackageCreatePackageGitCode] packagegit.json 已存在，跳过更新: {path}");
                return true;
            }

            // 文件不存在，直接创建新文件
            CreateVo = new CreateVo(template, path);

            ValueDic["PackageName"]       = codeData.PackageName;
            ValueDic["ScriptsReferences"] = GetScriptsReferences(codeData);

            return CreateNewFile();
        }

        private string GetScriptsReferences(PackageCreateInfo codeData)
        {
            var refModule = new List<string>();
            var sb        = StringbuilderPool.Get();

            var runtimeRefType = codeData.PackageInfo.Required.AsmRefType;
            foreach (var value in Enum.GetValues(typeof(AssemblyReferenceType)))
            {
                var enumValue = (AssemblyReferenceType)value;
                if ((int)enumValue <= 0) continue;
                var hasflag = runtimeRefType.HasFlag(enumValue);
                if (hasflag)
                {
                    refModule.Add(enumValue.ToString());
                }
            }

            for (int i = 0; i < refModule.Count; i++)
            {
                var line = i < refModule.Count - 1 ? ",\n" : "";
                sb.AppendFormat("        \"{0}\": [\"{1}\"]{2}", refModule[i], codeData.PackageInfo.Required.DisplayName, line);
            }

            return StringbuilderPool.PutAndToStr(sb);
        }
    }
}
