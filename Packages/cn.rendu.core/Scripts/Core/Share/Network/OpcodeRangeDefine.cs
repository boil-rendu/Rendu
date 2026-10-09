namespace RD.Core
{
    /// <summary>Opcode 区间划分常量：外网/内网消息各占一段，避免内/外网 Opcode 冲突。</summary>
    public static class OpcodeRangeDefine
    {
        /// <summary>外网消息 Opcode 下界（含）。</summary>
        public const ushort OuterMinOpcode = 10001;
        /// <summary>外网消息 Opcode 上界（含）。</summary>
        public const ushort OuterMaxOpcode = 20000;

        // 20001-30000 内网pb
        /// <summary>内网消息 Opcode 下界（含）。</summary>
        public const ushort InnerMinOpcode = 20001;
        /// <summary>内网消息 Opcode 上界（含）。</summary>
        public const ushort InnerMaxOpcode = 40000;

        /// <summary>Opcode 硬上限：超过此值不合法。</summary>
        public const ushort MaxOpcode = 60000;
    }
}
