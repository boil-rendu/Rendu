namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 行号文案(<c>"#12"</c>): 列表与表格每行每帧都要用一次。
    ///
    /// 行号只跟下标有关, 缓存成常量串即可; 超过上限(极少见)时才现场拼, 不为此无限扩表。
    /// </summary>
    internal static class IndexText
    {
        /// <summary>缓存到 #511; 表格默认每页 15 行、列表也分页, 正常远用不完。</summary>
        private const int CacheLimit = 512;

        private static readonly string[] s_Cache = Build();

        /// <summary>取 <paramref name="index"/> 的行号文案; 负数不缓存(直接拼)。</summary>
        public static string Of(int index)
        {
            return (uint)index < (uint)CacheLimit ? s_Cache[index] : "#" + index;
        }

        private static string[] Build()
        {
            var cache = new string[CacheLimit];
            for (var i = 0; i < cache.Length; i++)
            {
                cache[i] = "#" + i;
            }

            return cache;
        }
    }
}
