using System;
using UnityEngine;
using RD.Init.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目文件视图模型
    /// </summary>
    public class ProjectFilesViewModel : ViewModelBase
    {
        private ProjectFilesModel _model;
        private RegenerateProjectFilesCommand _regenerateCommand;
        private bool _isRegenerating;
        private int _deletedFilesCount;
        private string _lastGeneratedTime;
        private bool _isSlnLinked;

        public ProjectFilesViewModel(ProjectFilesModel model)
        {
            _model = model;
            _isRegenerating = false;
            _deletedFilesCount = 0;
            _lastGeneratedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _isSlnLinked = false;

            _regenerateCommand = new RegenerateProjectFilesCommand();
        }

        public bool IsRegenerating
        {
            get => _isRegenerating;
            set => SetProperty(ref _isRegenerating, value);
        }

        public int DeletedFilesCount
        {
            get => _deletedFilesCount;
            set => SetProperty(ref _deletedFilesCount, value);
        }

        public string LastGeneratedTime
        {
            get => _lastGeneratedTime;
            set => SetProperty(ref _lastGeneratedTime, value);
        }

        public bool IsSlnLinked
        {
            get => _isSlnLinked;
            private set => SetProperty(ref _isSlnLinked, value);
        }

        public ProjectFilesModel Model => _model;

        /// <summary>
        /// 重新生成项目文件
        /// </summary>
        public void RegenerateProjectFiles()
        {
            if (_model.IsRegenerating)
            {
                Debug.LogWarning("[ProjectFiles] 项目文件生成已在运行中");
                return;
            }

            IsRegenerating = true;

            try
            {
                Debug.Log("[ProjectFiles] ========== 开始重新生成项目文件 ==========");

                // 执行重新生成命令
                _regenerateCommand.Execute();

                // 获取删除的文件数量
                int deletedCount = ProjectFilesCommand.ReGenerateProjectFiles();
                DeletedFilesCount = deletedCount;

                // 更新生成时间
                LastGeneratedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Debug.Log($"[ProjectFiles] 项目文件生成完成，删除了 {deletedCount} 个文件");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 项目文件生成失败: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                IsRegenerating = false;
            }
        }

        /// <summary>
        /// 重新生成项目文件（旧方法别名）
        /// </summary>
        public void ReGenerateProjectFiles()
        {
            RegenerateProjectFiles();
        }

        /// <summary>
        /// 链接Sln文件
        /// </summary>
        public void LinkSln()
        {
            try
            {
                Debug.Log("[ProjectFiles] ========== 链接 Sln 文件 ==========");
                new LinkSlnCommand().Execute();
                IsSlnLinked = true;
                Debug.Log("[ProjectFiles] Sln 文件链接成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProjectFiles] Sln 文件链接失败: {ex.Message}");
                IsSlnLinked = false;
            }
        }

        /// <summary>
        /// 刷新资源数据库
        /// </summary>
        public void RefreshAssetDatabase()
        {
            try
            {
                Debug.Log("[ProjectFiles] ========== 刷新资源数据库 ==========");
                new RefreshAssetDatabaseCommand().Execute();
                Debug.Log("[ProjectFiles] 资源数据库刷新完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 资源数据库刷新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成程序集引用
        /// </summary>
        public void GenerateAssemblyReferences()
        {
            try
            {
                Debug.Log("[ProjectFiles] ========== 生成程序集引用 ==========");
                // 使用默认的 Client 模式生成程序集引用
                new GenerateAssemblyReferenceCommand(CodeMode.Client).Execute();
                Debug.Log("[ProjectFiles] 程序集引用生成完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 程序集引用生成失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新脚本引用
        /// </summary>
        public void UpdateScriptsReferences()
        {
            try
            {
                Debug.Log("[ProjectFiles] ========== 更新脚本引用 ==========");
                new UpdateScriptsReferencesCommand().Execute();
                Debug.Log("[ProjectFiles] 脚本引用更新完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 脚本引用更新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 运行初始化脚本
        /// </summary>
        public void RunInitScripts()
        {
            try
            {
                Debug.Log("[ProjectFiles] ========== 运行初始化脚本 ==========");
                new RunInitScriptsCommand().Execute();
                Debug.Log("[ProjectFiles] 初始化脚本运行完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 初始化脚本运行失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除单个忽略的文件
        /// </summary>
        public void DeleteIgnoredFile(string file)
        {
            ProjectFilesCommand.DeleteIgnoredCsprojFile(file);
            DeletedFilesCount++;
        }

        /// <summary>
        /// 获取忽略的文件列表
        /// </summary>
        public string[] GetIgnoredFiles()
        {
            return ProjectFilesCommand.GetIgnoredCsprojFiles();
        }
    }
}
