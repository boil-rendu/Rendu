using System;
using System.Collections.Generic;

namespace RD
{
    /// <summary>
    /// Opcode 注册器 - 管理所有 Opcode 信息
    /// </summary>
    public class OpcodeRegistry
    {
        private readonly List<OpcodeInfo> opcodes = new();
        private readonly HashSet<int> usedOpcodes = new();

        public void Add(string name, int opcode)
        {
            opcodes.Add(new OpcodeInfo { Name = name, Opcode = opcode });

            if (usedOpcodes.Contains(opcode))
                Console.WriteLine($"[OpcodeRegistry] 警告: 重复的 opcode {opcode}");
            else
                usedOpcodes.Add(opcode);
        }

        public void Clear()
        {
            opcodes.Clear();
            usedOpcodes.Clear();
        }

        public List<OpcodeInfo> GetAllOpcodes()
        {
            return opcodes;
        }
    }

    /// <summary>
    /// Opcode 信息
    /// </summary>
    public class OpcodeInfo
    {
        public string Name { get; set; }
        public int Opcode { get; set; }
    }
}
