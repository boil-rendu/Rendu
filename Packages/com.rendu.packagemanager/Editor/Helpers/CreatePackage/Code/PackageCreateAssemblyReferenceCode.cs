using System;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 自动生成 AssemblyReference.asmref 文件
    /// 根据 PackageRuntimeRefType 和 PackageCreateFolderType 在对应目录生成引用文件
    /// </summary>
    public class PackageCreateAssemblyReferenceCode : BaseTemplate
    {
        private readonly string m_EventName = "AssemblyReference";
        public override string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public PackageCreateAssemblyReferenceCode(PackageCreateInfo codeData) : base("")
        {
            Create(codeData);
        }

        public PackageCreateAssemblyReferenceCode(out bool result, PackageCreateInfo codeData) : base("")
        {
            result = Create(codeData);
        }

        private bool Create(PackageCreateInfo codeData)
        {
            var runtimeRefType = codeData.PackageInfo.Required.AsmRefType;
            var folderType = codeData.PackageInfo.Required.CodeModeType;

            if (runtimeRefType == AssemblyReferenceType.None)
            {
                Debug.Log($"[PackageCreateAssemblyReferenceCode] RuntimeRefType is None, skip creating asmref files");
                return true;
            }

            Debug.Log($"[PackageCreateAssemblyReferenceCode] Creating asmref files - RuntimeRefType: {runtimeRefType}, CreateFolderType: {folderType}");

            var template = $"{PackageCreateHelper.PackageCreateTemplatePath}/asmref.txt";
            bool allSuccess = true;

            // Hotfix 引用
            if ((runtimeRefType & AssemblyReferenceType.Hotfix) != 0)
            {
                if ((folderType & CodeModeType.Client) != 0)
                {
                    allSuccess &= CreateAsmrefFile(codeData.PackagePath, "Hotfix/Client", "Hotfix", template);
                }
                if ((folderType & CodeModeType.Share) != 0)
                {
                    allSuccess &= CreateAsmrefFile(codeData.PackagePath, "Hotfix/Share", "Hotfix", template);
                }
            }

            // HotfixView 引用
            if ((runtimeRefType & AssemblyReferenceType.HotfixView) != 0)
            {
                allSuccess &= CreateAsmrefFile(codeData.PackagePath, "HotfixView/Client", "HotfixView", template);
            }

            // Model 引用
            if ((runtimeRefType & AssemblyReferenceType.Model) != 0)
            {
                if ((folderType & CodeModeType.Client) != 0)
                {
                    allSuccess &= CreateAsmrefFile(codeData.PackagePath, "Model/Client", "Model", template);
                }
                if ((folderType & CodeModeType.Share) != 0)
                {
                    allSuccess &= CreateAsmrefFile(codeData.PackagePath, "Model/Share", "Model", template);
                }
            }

            // ModelView 引用
            if ((runtimeRefType & AssemblyReferenceType.ModelView) != 0)
            {
                allSuccess &= CreateAsmrefFile(codeData.PackagePath, "ModelView/Client", "ModelView", template);
            }

            return allSuccess;
        }

        /// <summary>
        /// 创建单个 AssemblyReference.asmref 文件
        /// </summary>
        private bool CreateAsmrefFile(string packagePath, string subPath, string runtimeRefTypeName, string template)
        {
            var path = $"{packagePath}/Scripts/{subPath}/AssemblyReference.asmref";

            // 检查文件是否已存在
            bool fileExists = TemplateEngine.FileExists(path);
            if (fileExists)
            {
                Debug.Log($"[PackageCreateAssemblyReferenceCode] asmref 文件已存在，跳过创建: {path}");
                return true;
            }

            // 创建新文件
            CreateVo = new CreateVo(template, path);
            ValueDic["PackageRuntimeRefType"] = runtimeRefTypeName;

            return CreateNewFile();
        }
    }
}
