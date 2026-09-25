namespace RD.Core
{
    /// <summary>
    /// Invoke 统一键：把字符串 invokeType 稳定映射成 long id。
    /// <para>
    /// 字符串只在「声明侧 / 注册侧 / 事件绑定侧」出现一次，运行期分发统一走 long，
    /// 避免每次调用做字符串哈希。与 source generator 生成的 [Invoke("...")] 使用同一函数，
    /// 两侧结果天然一致。
    /// </para>
    /// </summary>
    public static class InvokeId
    {
        private const ulong Offset = 14695981039346656037UL;
        private const ulong Prime  = 1099511628211UL;

        /// <summary>
        /// FNV-1a 64bit。空串返回 0（与 [Invoke] 无参默认 Type=0 语义一致）。
        /// 非空串保证不会返回 0。
        /// </summary>
        public static long Of(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return 0;
            }

            unchecked
            {
                ulong hash = Offset;
                for (int i = 0; i < key.Length; i++)
                {
                    hash ^= key[i];
                    hash *= Prime;
                }

                long id = (long)hash;
                return id == 0 ? 1 : id;
            }
        }
    }
}
