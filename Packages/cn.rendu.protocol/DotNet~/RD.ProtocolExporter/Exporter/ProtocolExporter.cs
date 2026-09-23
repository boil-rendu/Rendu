using System;

namespace RD
{
    public static class ProtocolExporter
    {
        public static void Export()
        {
            InnerProtocolExporter.Proto2CS();
        }
    }
}