using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Proto视图模型 - 业务逻辑和数据管理
    /// </summary>
    public class ProtoViewModel
    {
        private readonly PathConfig _pathConfig = new PathConfig();

        // 配置
        private string _vsCodePath = "";
        private bool _sortEnabled = false;
        private bool _openFolderEnabled = false;
        private string _basePath;
        private string _tempPath;

        // 数据
        private List<ProtoItemModel> _protoItems = new List<ProtoItemModel>();

        public string VSCodePath
        {
            get => _vsCodePath;
            set => _vsCodePath = value;
        }

        /// <summary>
        /// 扫描并自动填充VSCode路径
        /// </summary>
        public string[] ScanVSCodePaths()
        {
            List<string> paths = new List<string>();

#if UNITY_EDITOR_WIN
            // Windows 常见路径
            string[] winPaths = new string[]
            {
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Microsoft VS Code\Code.exe",
                @"C:\Program Files\Microsoft VS Code\Code.exe",
                @"C:\Program Files (x86)\Microsoft VS Code\Code.exe"
            };
            paths.AddRange(winPaths);
#elif UNITY_EDITOR_OSX
            // macOS 常见路径
            string[] macPaths = new string[]
            {
                "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code",
                "/Applications/VSCode.app/Contents/Resources/app/bin/code",
                "/usr/local/bin/code"
            };
            paths.AddRange(macPaths);
#elif UNITY_EDITOR_LINUX
            // Linux 常见路径
            string[] linuxPaths = new string[]
            {
                "/usr/bin/code",
                "/usr/local/bin/code",
                "/snap/bin/code"
            };
            paths.AddRange(linuxPaths);
#endif

            // 过滤出实际存在的路径
            return paths.Where(File.Exists).ToArray();
        }

        public bool SortEnabled
        {
            get => _sortEnabled;
            set => _sortEnabled = value;
        }

        public bool OpenFolderEnabled
        {
            get => _openFolderEnabled;
            set => _openFolderEnabled = value;
        }

        public IReadOnlyList<ProtoItemModel> ProtoItems => _protoItems;

        public ProtoViewModel()
        {
            InitializePaths();
        }

        private void InitializePaths()
        {
#if UNITY_EDITOR_WIN
            _basePath = Application.dataPath.Replace("Assets", "").Replace("/", "\\");
#elif UNITY_EDITOR_OSX
            _basePath = Application.dataPath.Replace("Assets", "");
#endif
            _tempPath = Path.Combine(_basePath, "Temp");
        }

        /// <summary>
        /// 刷新Proto文件列表
        /// </summary>
        public void RefreshProtoList()
        {
            _protoItems.Clear();

            List<string> protoFiles = FindProtoFiles();
            foreach (string fullPath in protoFiles)
            {
                var model = ParseProtoFile(fullPath);
                if (model != null)
                {
                    CheckForDuplicates(model);
                    _protoItems.Add(model);
                }
            }

            if (_sortEnabled)
            {
                SortProtoItems();
            }
        }

        /// <summary>
        /// 查找所有Proto文件
        /// </summary>
        private List<string> FindProtoFiles()
        {
            List<string> protoFiles = new List<string>();
            string packagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages");

            if (Directory.Exists(packagesPath))
            {
                try
                {
                    string[] filePaths = Directory.GetFiles(packagesPath, "*.proto", SearchOption.AllDirectories);
                    protoFiles.AddRange(filePaths);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"搜索Proto文件失败: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Packages目录不存在: {packagesPath}");
            }

            return protoFiles;
        }

        /// <summary>
        /// 解析Proto文件
        /// </summary>
        private ProtoItemModel ParseProtoFile(string fullPath)
        {
            string separator = Path.DirectorySeparatorChar.ToString();
            string simpPath = Path.GetFileName(fullPath);
            string name = Path.GetFileNameWithoutExtension(fullPath);
            string[] parts = name.Split('_');
            string numStr = parts[^1];
            int num = -1;
            short errorCode = 0;

            // 检查格式错误
            if (IsStyleError(name))
            {
                Debug.LogError($"Proto文件名格式有误: {simpPath}");
                errorCode = 1;
            }
            else if (int.TryParse(numStr, out num))
            {
                errorCode = 0;
            }
            else
            {
                Debug.LogError($"Proto文件名数值解析失败: {simpPath}");
                errorCode = 3;
            }

            return new ProtoItemModel(name, num, simpPath, fullPath, errorCode);
        }

        /// <summary>
        /// 检查数值重复
        /// </summary>
        private void CheckForDuplicates(ProtoItemModel newItem)
        {
            if (newItem.Num < 0) return;

            foreach (var item in _protoItems)
            {
                if (item.Num == newItem.Num && item.Num >= 0)
                {
                    newItem.ErrorCode = 2;
                    item.ErrorCode = 2;
                    Debug.LogError($"Proto文件名数值重复: {newItem.Num}, {newItem.SimpPath}, {item.SimpPath}");
                    break;
                }
            }
        }

        /// <summary>
        /// 排序Proto文件
        /// </summary>
        private void SortProtoItems()
        {
            _protoItems.Sort((p1, p2) => p1.Num.CompareTo(p2.Num));
        }

        /// <summary>
        /// 检查Proto文件名格式
        /// </summary>
        private bool IsStyleError(string input)
        {
            Regex regexC = new Regex("_C_");
            Regex regexS = new Regex("_S_");
            Regex regexCS = new Regex("_CS_");
            bool isC = regexC.IsMatch(input);
            bool isS = regexS.IsMatch(input);
            bool isCS = regexCS.IsMatch(input);

            // 如果包含_CS_，则不需要_C_或_S_
            if (isCS)
            {
                return false;
            }
            // 否则必须有且只有一个_C_或_S_
            return !(isC ^ isS);
        }

        /// <summary>
        /// 合并Proto文件并打开VSCode
        /// </summary>
        public void CombineAndOpenInVSCode()
        {
            if (string.IsNullOrEmpty(_vsCodePath))
            {
                throw new Exception("VSCode路径未设置，请先设置VSCode路径");
            }

            string tempFolder = GetTempProtoFolder();

            // 清空临时文件夹
            ClearTempFolder(tempFolder);

            // 复制Proto文件到临时文件夹
            foreach (var item in _protoItems)
            {
                if (File.Exists(item.FullPath))
                {
                    string destPath = Path.Combine(tempFolder, item.SimpPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(item.FullPath, destPath, true);
                }
            }

            // 如果需要打开文件夹
            if (_openFolderEnabled)
            {
                EditorUtility.RevealInFinder(tempFolder);
            }

            // 在VSCode中打开
            OpenInVSCode(tempFolder);
        }

        /// <summary>
        /// 分离并保存Proto文件
        /// </summary>
        public void SeparateAndSave()
        {
            string tempFolder = GetTempProtoFolder();

            foreach (var item in _protoItems)
            {
                string tempFilePath = Path.Combine(tempFolder, item.SimpPath);
                if (!File.Exists(tempFilePath))
                {
                    Debug.LogError($"临时文件不存在: {tempFilePath}");
                    return;
                }

                File.Copy(tempFilePath, item.FullPath, true);
            }

            AssetDatabase.Refresh();

            // 删除临时文件
            ClearTempProtoFiles(tempFolder);
        }

        private string GetTempProtoFolder()
        {
#if UNITY_EDITOR_WIN
            return Path.Combine(_tempPath, "TmpProto");
#elif UNITY_EDITOR_OSX
            return Path.Combine(_tempPath, "TmpProto");
#else
            return Path.Combine(_tempPath, "TmpProto");
#endif
        }

        private void ClearTempFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"删除文件失败: {file.FullName}, 错误: {ex.Message}");
                    }
                }
            }
        }

        private void ClearTempProtoFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;

            foreach (var item in _protoItems)
            {
                string tempFilePath = Path.Combine(folderPath, item.SimpPath);
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        private void OpenInVSCode(string dirPath)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _vsCodePath,
                    Arguments = $"\"{dirPath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                System.Diagnostics.Process.Start(startInfo);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"打开VSCode失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行Protocol导出
        /// </summary>
        public void ExecuteProtocolExport()
        {
            ExecuteProtocolExportInternal(_pathConfig);
        }

        /// <summary>
        /// 静态方法：执行Protocol导出
        /// </summary>
        public static void ExecuteProtocolExportStatic()
        {
            var pathConfig = new PathConfig();
            ExecuteProtocolExportInternal(pathConfig);
        }

        /// <summary>
        /// 内部实现：执行Protocol导出
        /// </summary>
        private static void ExecuteProtocolExportInternal(PathConfig pathConfig)
        {
            var exporterDllPath = pathConfig.GetExporterDllPath();
            UnityEngine.Debug.Log($"[Protocol] 导出器路径: {exporterDllPath}");

            // 检查 Exporter 是否存在
            if (!File.Exists(exporterDllPath))
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "导出器未编译",
                    $"找不到导出器 DLL 文件：{exporterDllPath}\n\n请先编译 Exporter 项目：\n{System.IO.Path.Combine(pathConfig.PackagePath, PathConfig.DotNetDirName, "RD.ProtocolExporter")}",
                    "确定"
                );
                return;
            }

            Debug.Log("[Protocol] ========== 开始导出 Protocol ==========");
            var workingDir = pathConfig.GetWorkingDirectory();

            UnityEngine.Debug.Log($"[Protocol] 工作目录: {workingDir}");

            System.Diagnostics.Process process = ProcessHelper.DotNet(
                exporterDllPath,
                workingDir, true);

            string output = process.StandardOutput.ReadToEnd();

            UnityEngine.Debug.Log(output);
            UnityEngine.Debug.Log("[Protocol] ========== 导出 Protocol 完成 ==========");
        }
    }
}