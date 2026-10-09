using System;
using System.Net;
using System.Net.Sockets;

namespace RD.Core
{
    /// <summary><see cref="EndPoint"/> 扩展工具：避免 <c>IPEndPoint.Clone</c> 时的隐式分配（KCP 高频调用路径）。</summary>
    public static class EndPointHelper
    {
        /// <summary>返回 <paramref name="endPoint"/> 的一份拷贝（同样的 Address + Port，分配新 <see cref="IPEndPoint"/>）。</summary>
        public static IPEndPoint Clone(this EndPoint endPoint)
        {
            IPEndPoint ip = (IPEndPoint)endPoint;
            ip = new IPEndPoint(ip.Address, ip.Port);
            return ip;
        }
    }
}
