namespace RD.Loader.Editor
{
    /// <summary>
    /// 程序集编译服务接口
    /// </summary>
    public interface IAssemblyCompilerService
    {
        /// <summary>
        /// 执行完整编译流程
        /// </summary>
        void Compile();

        /// <summary>
        /// 编译 DLL
        /// </summary>
        /// <returns>编译是否成功</returns>
        bool CompileDlls();

        /// <summary>
        /// 复制热更新 DLLs
        /// </summary>
        void CopyHotUpdateDlls();
    }
}
