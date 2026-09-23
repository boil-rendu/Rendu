using System.IO;
using UnityEditor;
using UnityEngine;


namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 包视图模型 - 数据绑定和业务逻辑
    /// </summary>
    public class PackageViewModel
    {
        private static readonly PackageDefaultInfo DefaultInfo;

        static PackageViewModel()
        {
            string configPath = $"Packages/com.rendu.packagemanager/Editor/Helpers/CreatePackage/PackageDefaultInfo.json";
            if (File.Exists(configPath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(configPath);
                    DefaultInfo = JsonUtility.FromJson<PackageDefaultInfo>(jsonContent);
                    Debug.Log($"[PackageViewModel] Loaded default config from {configPath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PackageViewModel] Failed to load default config: {e.Message}. Using default values.");
                    DefaultInfo = new PackageDefaultInfo();
                }
            }
            else
            {
                Debug.LogWarning($"[PackageViewModel] Default config not found at {configPath}. Using default values.");
                DefaultInfo = new PackageDefaultInfo();
            }
        }
        
        public string FormalPrefix { get; private set; }
        public string DisplayPrefix { get; private set; }

        // 统一数据源 - 使用 JSON 配置中的默认值
        public PackageInfo Info;

        public PackageViewModel()
        {
            // 在构造函数中初始化字段
            FormalPrefix = DefaultInfo?.formalPrefix ?? "cn.rendu.";
            DisplayPrefix = DefaultInfo?.displayPrefix ?? "RD.";

            // 深拷贝默认值，避免修改 _defaultInfo
            if (DefaultInfo?.defaultValues != null)
            {
                Info = DeepCopyPackageInfo(DefaultInfo.defaultValues);
            }
            else
            {
                Info = new PackageInfo
                {
                    Required = new PackageRequiredInfo(),
                    Optional = new PackageOptionalInfo()
                };
            }
        }

        /// <summary>
        /// 深拷贝 PackageInfo 对象，避免修改默认配置
        /// </summary>
        private PackageInfo DeepCopyPackageInfo(PackageInfo source)
        {
            return new PackageInfo
            {
                Required = new PackageRequiredInfo
                {
                    FormalName = source.Required?.FormalName ?? "",
                    DisplayName = source.Required?.DisplayName ?? "",
                    Version = source.Required?.Version ?? "",
                    Unity = source.Required?.Unity ?? "",
                    AsmDefType = source.Required?.AsmDefType ?? AssemblyDefineType.None,
                    AsmRefType = source.Required?.AsmRefType ?? AssemblyReferenceType.None,
                    CodeModeType = source.Required?.CodeModeType ?? CodeModeType.None,
                    GenerateIgnoreAsmdef = source.Required?.GenerateIgnoreAsmdef ?? false
                },
                Optional = new PackageOptionalInfo
                {
                    Description = source.Optional?.Description ?? "",
                    AuthorName = source.Optional?.AuthorName ?? "",
                    AuthorEmail = source.Optional?.AuthorEmail ?? "",
                    AuthorUrl = source.Optional?.AuthorUrl ?? "",
                    Keywords = source.Optional?.Keywords ?? ""
                }
            };
        }

        // 便捷属性（访问 Info 的字段）
        public string SimpleName
        {
            get => _simpleName;
            set
            {
                _simpleName = value;
            }
        }
        private string _simpleName = "";

        public string FormalName
        {
            get => Info.Required.FormalName;
            set => Info.Required.FormalName = value;
        }
        public string DisplayName
        {
            get => Info.Required.DisplayName;
            set => Info.Required.DisplayName = value;
        }
        public string Description
        {
            get => Info.Optional.Description;
            set => Info.Optional.Description = value;
        }
        public string Version
        {
            get => Info.Required.Version;
            set => Info.Required.Version = value;
        }
        public string Unity
        {
            get => Info.Required.Unity;
            set => Info.Required.Unity = value;
        }
        public string AuthorName
        {
            get => Info.Optional.AuthorName;
            set => Info.Optional.AuthorName = value;
        }
        public string AuthorEmail
        {
            get => Info.Optional.AuthorEmail;
            set => Info.Optional.AuthorEmail = value;
        }
        public string AuthorUrl
        {
            get => Info.Optional.AuthorUrl;
            set => Info.Optional.AuthorUrl = value;
        }
        public string Keywords
        {
            get => Info.Optional.Keywords;
            set => Info.Optional.Keywords = value;
        }

        // 包创建配置
        public AssemblyDefineType AsmDefType
        {
            get => Info.Required.AsmDefType;
            set => Info.Required.AsmDefType = value;
        }
        public AssemblyReferenceType AsmRefType
        {
            get => Info.Required.AsmRefType;
            set => Info.Required.AsmRefType = value;
        }
        public CodeModeType CodeMode
        {
            get => Info.Required.CodeModeType;
            set => Info.Required.CodeModeType = value;
        }

        public bool GenerateIgnoreAsmdef
        {
            get => Info.Required.GenerateIgnoreAsmdef;
            set => Info.Required.GenerateIgnoreAsmdef = value;
        }

        public void UpdateFromSimpleName()
        {
            Info.Required.FormalName = $"{FormalPrefix}{_simpleName.ToLower()}";
            Info.Required.DisplayName = $"{DisplayPrefix}{_simpleName}";
        }

        /// <summary>
        /// 检查当前选择的 是否需要指定 CodeMode
        /// </summary>
        private bool NeedCodeModeType()
        {
            var asmDefType = Info.Required.AsmDefType;

            // Hotfix、HotfixView、Model、ModelView 需要 CodeMode
            bool needsAsmDefCodeMode = (asmDefType & AssemblyDefineType.Hotfix) != 0 ||
                                   (asmDefType & AssemblyDefineType.HotfixView) != 0 ||
                                   (asmDefType & AssemblyDefineType.Model) != 0 ||
                                   (asmDefType & AssemblyDefineType.ModelView) != 0;

            var asmRefType = Info.Required.AsmRefType;
            // Hotfix、HotfixView、Model、ModelView 需要 CodeMode
            bool needsAsmRefCodeMode = (asmRefType & AssemblyReferenceType.Hotfix) != 0 ||
                                       (asmRefType & AssemblyReferenceType.HotfixView) != 0 ||
                                       (asmRefType & AssemblyReferenceType.Model) != 0 ||
                                       (asmRefType & AssemblyReferenceType.ModelView) != 0;
            
            return needsAsmRefCodeMode || needsAsmDefCodeMode;
        }

        /// <summary>
        /// 格式化 Unity 版本(去掉 't' 后缀,只保留主版本号)
        /// </summary>
        private string FormatUnityVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return version;

            // 去掉 't' 开头的后缀
            int tIndex = version.IndexOf('t');
            string formatted = tIndex > 0 ? version.Substring(0, tIndex) : version;

            // 只保留主版本号 (如 "2022.3")
            string[] parts = formatted.Split('.');
            if (parts.Length >= 2)
            {
                return $"{parts[0]}.{parts[1]}";
            }
            return formatted;
        }

        /// <summary>
        /// 重置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            Debug.Log($"[ResetToDefaults] Resetting to defaults for: {_simpleName}");

            // 从默认配置重新初始化（使用深拷贝）
            if (DefaultInfo?.defaultValues != null)
            {
                Info = DeepCopyPackageInfo(DefaultInfo.defaultValues);
            }
            else
            {
                Info = new PackageInfo
                {
                    Required = new PackageRequiredInfo(),
                    Optional = new PackageOptionalInfo()
                };
            }

            // 重置 Form 相关字段
            if (!string.IsNullOrEmpty(_simpleName))
            {
                UpdateFromSimpleName();
            }
        }

        public bool LoadPackageInfoFromSimpleName()
        {
            // 加载包信息（内部会处理 simpleName 名称到 formalName 的转换）
            return LoadPackageInfo();
        }
        
        public bool LoadPackageInfoFromFormalName()
        {
            // 加载包信息（内部会处理 formalName 到 simpleName 的转换）
            return LoadPackageInfo();
        }

        public bool LoadPackageInfo()
        {
            if (string.IsNullOrEmpty(Info.Required.FormalName))
            {
                Debug.LogWarning("[LoadPackageInfo] FormalName is empty");
                return false;
            }

            string packagePath = $"Packages/{Info.Required.FormalName}";
            string packageJsonPath = $"{packagePath}/package.json";
            string packageGitJsonPath = $"{packagePath}/packagegit.json";

            Debug.Log($"[LoadPackageInfo] Checking package at: {packagePath}");

            // 快速检查：先检查目录是否存在（更快）
            if (!Directory.Exists(packagePath))
            {
                Debug.LogWarning($"[LoadPackageInfo] Package directory does not exist: {packagePath}");
                return false;
            }

            // 检查关键文件是否存在
            if (!File.Exists(packageJsonPath))
            {
                Debug.LogWarning($"[LoadPackageInfo] package.json does not exist: {packageJsonPath}");
                return false;
            }

            try
            {
                // 加载 package.json
                string jsonContent = File.ReadAllText(packageJsonPath);
                var packageInfo = JsonUtility.FromJson<UnityPackageInfo>(jsonContent);

                Debug.Log($"[LoadPackageInfo] Loaded package - Name: {packageInfo.name}, DisplayName: {packageInfo.displayName}");

                // 从 package.json 的 name 字段读取 formal 名称
                Info.Required.FormalName = packageInfo.name;
                Info.Required.DisplayName = packageInfo.displayName;
                Info.Required.Version = packageInfo.version;
                Info.Optional.Description = packageInfo.description;

                // 格式化 Unity 版本(去掉 't' 后缀,只保留主版本号)
                Info.Required.Unity = FormatUnityVersion(packageInfo.unity);

                // 从 formal 名称反推 Input (例如: cn.rendu.core -> Core)
                if (Info.Required.FormalName.StartsWith(FormalPrefix))
                {
                    _simpleName = Info.Required.FormalName.Substring(FormalPrefix.Length);
                    // 首字母大写
                    if (!string.IsNullOrEmpty(_simpleName))
                    {
                        _simpleName = char.ToUpper(_simpleName[0]) + _simpleName.Substring(1);
                    }
                }

                if (packageInfo.author != null)
                {
                    Info.Optional.AuthorName = packageInfo.author.name;
                    Info.Optional.AuthorEmail = packageInfo.author.email;
                    Info.Optional.AuthorUrl = packageInfo.author.url;
                }

                // 优化关键词处理：只在不为空时才拼接
                if (packageInfo.keywords != null && packageInfo.keywords.Length > 0)
                {
                    Info.Optional.Keywords = string.Join(", ", packageInfo.keywords);
                }
                else
                {
                    Info.Optional.Keywords = "";
                }

                // 加载 RuntimeRefType 从 packagegit.json
                if (File.Exists(packageGitJsonPath))
                {
                    LoadRuntimeRefType(packageGitJsonPath);
                }

                // 从目录结构推断 CreateType 
                LoadCreateTypesFromDirectories(packagePath);
                // 从目录结构推断 CreateFolderType
                LoadCreateFolderTypesFromDirectories(packagePath);
                
                Debug.Log($"[LoadPackageInfo] Successfully loaded package info - CreateType: {Info.Required.AsmDefType}, RuntimeRefType: {Info.Required.AsmRefType}, CreateFolderType: {Info.Required.CodeModeType}");

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LoadPackageInfo] Failed to load package info: {e.Message}");
                return false;
            }
        }

        private void LoadRuntimeRefType(string packageGitJsonPath)
        {
            try
            {
                string gitJsonContent = File.ReadAllText(packageGitJsonPath);
                Info.Required.AsmRefType = AssemblyReferenceType.None;

                Debug.Log($"[LoadRuntimeRefType] Loading from: {packageGitJsonPath}");

                // JsonUtility 不支持 Dictionary,手动解析
                if (gitJsonContent.Contains("\"Hotfix\""))
                {
                    Info.Required.AsmRefType |= AssemblyReferenceType.Hotfix;
                    Debug.Log("[LoadRuntimeRefType] Found Hotfix reference");
                }
                if (gitJsonContent.Contains("\"HotfixView\""))
                {
                    Info.Required.AsmRefType |= AssemblyReferenceType.HotfixView;
                    Debug.Log("[LoadRuntimeRefType] Found HotfixView reference");
                }
                if (gitJsonContent.Contains("\"Model\""))
                {
                    Info.Required.AsmRefType |= AssemblyReferenceType.Model;
                    Debug.Log("[LoadRuntimeRefType] Found Model reference");
                }
                if (gitJsonContent.Contains("\"ModelView\""))
                {
                    Info.Required.AsmRefType |= AssemblyReferenceType.ModelView;
                    Debug.Log("[LoadRuntimeRefType] Found ModelView reference");
                }

                Debug.Log($"[LoadRuntimeRefType] Loaded RuntimeRefType: {Info.Required.AsmRefType}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LoadRuntimeRefType] Failed to load RuntimeRefType: {e.Message}");
            }
        }

        private void LoadCreateTypesFromDirectories(string packagePath)
        {
            Info.Required.AsmDefType = AssemblyDefineType.None;
            // 优化：使用 Path.Combine 构建路径，减少字符串拼接
            // 检查 CreateType (通过 .asmdef 文件和 Scripts 子目录)
            if (Directory.Exists(Path.Combine(packagePath, "Runtime")))
                Info.Required.AsmDefType |= AssemblyDefineType.Runtime;
            if (Directory.Exists(Path.Combine(packagePath, "Editor")))
                Info.Required.AsmDefType |= AssemblyDefineType.Editor;

            string path = Path.Combine(packagePath, "Runtime");
            // 检查 Runtime 子目录来确定 Hotfix/Model 相关的 CreateType
            if (Directory.Exists(Path.Combine(path, "Hotfix")))
                Info.Required.AsmDefType |= AssemblyDefineType.Hotfix;
            if (Directory.Exists(Path.Combine(path, "HotfixView")))
                Info.Required.AsmDefType |= AssemblyDefineType.HotfixView;
            if (Directory.Exists(Path.Combine(path, "Model")))
                Info.Required.AsmDefType |= AssemblyDefineType.Model;
            if (Directory.Exists(Path.Combine(path, "ModelView")))
                Info.Required.AsmDefType |= AssemblyDefineType.ModelView;

            Debug.Log($"[LoadCreateTypesFromDirectories] Loaded CreateType: {Info.Required.AsmDefType}, CreateFolderType: {Info.Required.CodeModeType}");
        }
        private void LoadCreateFolderTypesFromDirectories(string packagePath)
        {
            Info.Required.CodeModeType = CodeModeType.None;
            string scriptsPath = $"{packagePath}/Scripts";
            
            // 检查 CreateFolderType (通过 Hotfix 和 Model 目录下的子目录)
            string hotfixPath = Path.Combine(scriptsPath, "Hotfix");
            if (Directory.Exists(hotfixPath))
            {
                if (Directory.Exists(Path.Combine(hotfixPath, "Client")))
                    Info.Required.CodeModeType |= CodeModeType.Client;
                if (Directory.Exists(Path.Combine(hotfixPath, "Server")))
                    Info.Required.CodeModeType |= CodeModeType.Server;
                if (Directory.Exists(Path.Combine(hotfixPath, "Share")))
                    Info.Required.CodeModeType |= CodeModeType.Share;
            }

            // 同样检查 Model 目录下的子目录
            string modelPath = Path.Combine(scriptsPath, "Model");
            if (Directory.Exists(modelPath))
            {
                if (Directory.Exists(Path.Combine(modelPath, "Client")))
                    Info.Required.CodeModeType |= CodeModeType.Client;
                if (Directory.Exists(Path.Combine(modelPath, "Server")))
                    Info.Required.CodeModeType |= CodeModeType.Server;
                if (Directory.Exists(Path.Combine(modelPath, "Share")))
                    Info.Required.CodeModeType |= CodeModeType.Share;
            }
            
            
            // 检查 CreateFolderType (通过 Hotfix 和 Model 目录下的子目录)
            string hotfixViewPath = Path.Combine(scriptsPath, "HotfixView");
            if (Directory.Exists(hotfixPath))
            {
                if (Directory.Exists(Path.Combine(hotfixViewPath, "Client")))
                    Info.Required.CodeModeType |= CodeModeType.Client;
            }

            // 同样检查 Model 目录下的子目录
            string modelViewPath = Path.Combine(scriptsPath, "ModelView");
            if (Directory.Exists(modelPath))
            {
                if (Directory.Exists(Path.Combine(modelPath, "Client")))
                    Info.Required.CodeModeType |= CodeModeType.Client;
            }

            Debug.Log($"[LoadCreateTypesFromDirectories] Loaded CreateType: {Info.Required.AsmDefType}, CreateFolderType: {Info.Required.CodeModeType}");
        }

        public ValidationResult ValidateCreatePackage()
        {
            // 必需属性检查
            if (string.IsNullOrEmpty(_simpleName))
            {
                return ValidationResult.Failure("Package Name is required.");
            }

            if (string.IsNullOrEmpty(Info.Required.Version))
            {
                return ValidationResult.Failure("Version is required.");
            }

            if (Info.Required.AsmDefType == AssemblyDefineType.None && Info.Required.AsmRefType == AssemblyReferenceType.None)
            {
                return ValidationResult.Failure("Please select at least one AsmDef or AsmRef.");
            }
            
            if (NeedCodeModeType() && Info.Required.CodeModeType == CodeModeType.None)
            {
                return ValidationResult.Failure("Please select at least one CodeMode Type for the selected AsmDef or AsmRef.");
            }

            // 强制属性检查
            if (string.IsNullOrEmpty(Info.Required.DisplayName))
            {
                return ValidationResult.Failure("Display Name is required.");
            }

            if (string.IsNullOrEmpty(Info.Required.Unity))
            {
                return ValidationResult.Failure("Unity Version is required.");
            }

            return ValidationResult.Success();
        }
    }
}
