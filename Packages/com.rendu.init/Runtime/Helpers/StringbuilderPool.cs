using System.Collections.Generic;
using System.Text;

namespace RD
{
    /// <summary>
    /// StringBuilder池
    /// </summary>
    public static class StringbuilderPool
    {
        private static readonly Stack<StringBuilder> _gPool = new Stack<StringBuilder>(5);

        public static StringBuilder Get()
        {
            lock (_gPool)
            {
                return _gPool.Count == 0
                        ? new StringBuilder()
                        : _gPool.Pop();
            }
        }

        public static void Put(StringBuilder sb)
        {
            sb.Clear();
            lock (_gPool)
            {
                _gPool.Push(sb);
            }
        }

        public static string PutAndToStr(StringBuilder sb)
        {
            var str = sb.ToString();
            Put(sb);
            return str;
        }
    }
}
