using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 包创建器 - 静态工具类
    /// </summary>
    public static class PackageCreateHelper
    {
        private const string Pattern = "[^a-z]";

        public static string PackageCreateTemplatePath =
            "Packages/com.rendu.packagemanager/Editor/Helper/CreatePackage/Template";

        public static string GetPackagePath(ref string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                UnityTipsHelper.ShowError($"包名不能为空");
                return "";
            }

            packageName = packageName.ToLower();
            packageName = System.Text.RegularExpressions.Regex.Replace(packageName, Pattern, "");
            var packagepath = $"Packages/cn.rendu.{packageName}";

            return packagepath;
        }

        private static bool CreateDirectory(string path, bool force = true)
        {
            if (Directory.Exists(path))
            {
                // 如果目录已存在，直接返回成功（不删除，实现增量更新）
                Debug.Log($"[PackageCreateHelper] 目录已存在，跳过创建: {path}");
                return true;
            }

            Directory.CreateDirectory(path);
            Debug.Log($"[PackageCreateHelper] 创建目录: {path}");
            return true;
        }


        static void CreateAsmdef(PackageCreateInfo data)
        {
            var createTypeValues = Enum.GetValues(typeof(EAssemblyDefineType));
            foreach (var value in createTypeValues)
            {
                var createType = (EAssemblyDefineType)value;
                if ((int)createType <= 0)
                {
                    continue;
                }

                var hasReslut = data.PackageInfo.Required.AsmDefType.HasFlag(createType);
                if (hasReslut)
                {
                    string packagePath = data.PackagePath;

                    switch (createType)
                    {
                        case EAssemblyDefineType.Runtime:
                            data.FolderPath =$"{packagePath}/Runtime";
                            new PackageCreatePackageAsmdefCode(data, data.PackageInfo.Required.DisplayName);
                            break;
                        case EAssemblyDefineType.Editor:
                            new PackageCreatePackageAsmdefEditorCode(data);
                            break;
                        case EAssemblyDefineType.Hotfix:
                        case EAssemblyDefineType.HotfixView:
                        case EAssemblyDefineType.Model:
                        case EAssemblyDefineType.ModelView:
                            data.FolderPath = $"{packagePath}/Runtime/{createType.ToString()}";
                            CreateDirectory(EditorHelper.GetProjPath(data.FolderPath));
                            new PackageCreatePackageAsmdefCode(data, $"RD.{createType.ToString()}");
                            break;
                        default:
                            UnityTipsHelper.ShowError($"未实现的功能 {createType}");
                            break;
                    }
                }
            }
        }

        public static void CreatePackage(PackageCreateInfo data, bool force = false)
        {
            Debug.Log($"[PackageCreateHelper] CreatePackage called - CreateType: {data.PackageInfo.Required.AsmDefType}, CreateFolderType: {data.PackageInfo.Required.eCodeModeType}, RuntimeRefType: {data.PackageInfo.Required.AsmRefType}");

            CreateAsmdef(data);

            // 创建 Scripts 文件夹
            CreateScriptsFolders(data);

            new PackageCreatePackageJsonCode(data);
            new PackageCreatePackageGitCode(data);
            
            // 根据选项决定是否生成 Ignore Asmdef
            if (data.PackageInfo.Required.GenerateIgnoreAsmdef)
            {
                new PackageCreatePackageIgnoreAsmdefCode(data);
            }

            new PackageCreateAssemblyReferenceCode(data);
        }

        private static void CreateScriptsFolders(PackageCreateInfo data)
        {
            var packagePath = data.PackagePath;
            var refType = data.PackageInfo.Required.AsmRefType;
            var codeModeType = data.PackageInfo.Required.eCodeModeType;

            Debug.Log($"[CreateScriptsFolders] Starting folder creation - packagePath: {packagePath}, refType: {refType}, codeModeType: {codeModeType}");

            // 定义文件夹类型及其对应的子文件夹
            var folderTypes = new[]
            {
                new { RefType = EAssemblyReferenceType.Model, FolderName = "Model", SubFolders = new[] { ECodeModeType.Client, ECodeModeType.Server, ECodeModeType.Share } },
                new { RefType = EAssemblyReferenceType.Hotfix, FolderName = "Hotfix", SubFolders = new[] { ECodeModeType.Client, ECodeModeType.Server, ECodeModeType.Share } },
                
                new { RefType = EAssemblyReferenceType.ModelView, FolderName = "ModelView", SubFolders = new[] { ECodeModeType.Client } },
                new { RefType = EAssemblyReferenceType.HotfixView, FolderName = "HotfixView", SubFolders = new[] { ECodeModeType.Client } }
            };

            foreach (var folderType in folderTypes)
            {
                if ((refType & folderType.RefType) != 0)
                {
                    string relativePath = $"{packagePath}/Scripts/{folderType.FolderName}";
                    string basePath = EditorHelper.GetProjPath(relativePath);
                    Debug.Log($"[CreateScriptsFolders] Creating {folderType.FolderName} folder - relativePath: {relativePath}, absolutePath: {basePath}");
                    CreateDirectory(basePath);

                    foreach (var subFolder in folderType.SubFolders)
                    {
                        if ((codeModeType & subFolder) != 0)
                        {
                            CreateDirectory($"{basePath}/{subFolder.ToString()}");
                        }
                    }
                }
            }
        }
    }
}