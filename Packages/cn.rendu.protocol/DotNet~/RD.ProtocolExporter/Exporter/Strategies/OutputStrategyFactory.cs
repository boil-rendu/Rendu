using System.Collections.Generic;

namespace RD
{
    /// <summary>
    /// 输出策略工厂 - 根据文件名返回对应的策略
    /// </summary>
    public static class OutputStrategyFactory
    {
        private static readonly Dictionary<string, IOutputStrategy> _strategies = new Dictionary<string, IOutputStrategy>
        {
            { "_C_", new ClientOutputStrategy() },
            { "_S_", new ServerOutputStrategy() },
            { "_CS_", new ClientServerOutputStrategy() }
        };

        /// <summary>
        /// 根据文件名获取输出策略
        ///
        /// 判定顺序不可调整：_CS_ 中 "_C" 后紧跟 "S" 而非 "_"，故 Contains("_C_") 为 false；
        /// 同理 Contains("_S_") 亦为 false。三个标记互不误匹配，顺序无隐含依赖，
        /// 但改动前请用含 _C_ / _S_ / _CS_ 的文件名实测确认。
        /// </summary>
        public static IOutputStrategy GetStrategy(string fileNameWithoutExtension)
        {
            if (fileNameWithoutExtension.Contains("_C_"))
                return _strategies["_C_"];
            else if (fileNameWithoutExtension.Contains("_S_"))
                return _strategies["_S_"];
            else if (fileNameWithoutExtension.Contains("_CS_"))
                return _strategies["_CS_"];
            else
                return _strategies["_CS_"]; // 默认为共享
        }
    }
}
