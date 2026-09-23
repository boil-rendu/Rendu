namespace RD.Init.Editor
{
    /// <summary>
    /// 命令基类 - 实现 ICommand 接口
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// 是否可以执行命令
        /// </summary>
        public virtual bool CanExecute() => true;

        /// <summary>
        /// 执行前的验证
        /// </summary>
        protected virtual bool Validate()
        {
            return true;
        }

        /// <summary>
        /// 执行命令（带验证）
        /// </summary>
        public void ExecuteWithValidation()
        {
            if (!Validate())
            {
                UnityEngine.Debug.LogWarning($"[{GetType().Name}] 命令验证失败，无法执行");
                return;
            }

            if (!CanExecute())
            {
                UnityEngine.Debug.LogWarning($"[{GetType().Name}] 命令当前不可执行");
                return;
            }

            Execute();
        }
    }
}
