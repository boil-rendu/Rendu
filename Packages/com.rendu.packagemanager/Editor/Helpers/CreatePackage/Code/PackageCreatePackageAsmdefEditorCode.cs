using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    public class PackageCreatePackageAsmdefEditorCode : BaseTemplate
    {
        private readonly string m_EventName = "AsmdefEditor";
        public override  string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override  bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override  bool ShowTips => m_ShowTips;

        public PackageCreatePackageAsmdefEditorCode(PackageCreateInfo codeData) : base("")
        {
            Create(codeData);
        }

        public PackageCreatePackageAsmdefEditorCode(out bool result, PackageCreateInfo codeData) : base("")
        {
            result = Create(codeData);
        }

        private bool Create(PackageCreateInfo codeData)
        {
            var assemblyName =  codeData.PackageInfo.Required.DisplayName;
            var path     = $"{codeData.PackagePath}/Editor/{assemblyName}.Editor.asmdef";
            var template = $"{PackageCreateHelper.PackageCreateTemplatePath}/asmdefeditor.txt";

            // 检查文件是否已存在
            bool fileExists = TemplateEngine.FileExists(path);

            // 如果文件已存在，跳过更新（asmdef 文件的引用关系通常由用户维护）
            if (fileExists)
            {
                Debug.Log($"[PackageCreatePackageAsmdefEditorCode] asmdef 文件已存在，跳过更新: {path}");
                return true;
            }

            // 文件不存在，直接创建新文件
            CreateVo = new CreateVo(template, path);
            ValueDic["DisplayName"] = assemblyName;

            return CreateNewFile();
        }
    }
}
