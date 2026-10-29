using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    public class PackageCreatePackageAsmdefCode : BaseTemplate
    {
        private readonly string m_EventName = "Asmdef";
        public override string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public PackageCreatePackageAsmdefCode(PackageCreateInfo codeData, string assemblyName) : base("")
        {
            Create(codeData, assemblyName);
        }

        private bool Create(PackageCreateInfo codeData, string assemblyName)
        {
            var path = $"{codeData.FolderPath}/{assemblyName}.asmdef";
            var template = $"{PackageCreateHelper.PackageCreateTemplatePath}/asmdef.txt";

            // 检查文件是否已存在
            bool fileExists = TemplateEngine.FileExists(path);
            // 如果文件已存在，跳过更新（asmdef 文件的引用关系通常由用户维护）
            if (fileExists)
            {
                Debug.Log($"[PackageCreatePackageAsmdefCode] asmdef 文件已存在，跳过更新: {path}");
                return true;
            }

            // 文件不存在，直接创建新文件
            CreateVo = new CreateVo(template, path);
            ValueDic["DisplayName"] = assemblyName;

            return CreateNewFile();
        }
    }
}