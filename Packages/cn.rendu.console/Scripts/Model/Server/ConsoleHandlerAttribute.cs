using RD.Core;

namespace RD
{
    public class ConsoleHandlerAttribute: BaseAttribute
    {
        public string Mode { get; }

        public ConsoleHandlerAttribute(string mode)
        {
            this.Mode = mode;
        }
    }
}