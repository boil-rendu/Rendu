using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace RD.Core
{
	/// <summary>网络工具：枚举本机 IP、DNS 解析（优先 IPv4）、<c>host:port</c> 字符串转 <see cref="IPEndPoint"/>、Windows 平台关闭 UDP <c>WSAECONNRESET</c>（避免「对端关闭后下一发包抛异常」）。</summary>
	public static class NetworkHelper
	{
		/// <summary>枚举本机所有以太网网卡的 IPv4/IPv6 地址（仅 <c>Ethernet</c>，过滤掉回环/虚拟网卡）。</summary>
		public static string[] GetAddressIPs()
		{
			List<string> list = new List<string>();
			foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
				{
					continue;
				}
				foreach (UnicastIPAddressInformation add in networkInterface.GetIPProperties().UnicastAddresses)
				{
					list.Add(add.Address.ToString());
				}
			}
			return list.ToArray();
		}

		/// <summary>DNS 解析 <paramref name="hostName"/>：优先返回 IPv4 地址，无 IPv4 时退回解析结果的第一条。</summary>
		public static IPAddress GetHostAddress(string hostName)
		{
			IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
			IPAddress returnIpAddress = null;
			foreach (IPAddress ipAddress in ipAddresses)
			{
				returnIpAddress = ipAddress;
				if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					return ipAddress;
				}
			}
			return returnIpAddress;
		}

		/// <summary>用 (host, port) 字符串构造 <see cref="IPEndPoint"/>：host 必须是 IP 字面量，不做 DNS 解析。</summary>
		public static IPEndPoint ToIPEndPoint(string host, int port)
		{
			return new IPEndPoint(IPAddress.Parse(host), port);
		}

		/// <summary>从 <c>"host:port"</c> 字符串解析 <see cref="IPEndPoint"/>：用最后一个 <c>:</c> 分割 host/port。</summary>
		public static IPEndPoint ToIPEndPoint(string address)
		{
			int index = address.LastIndexOf(':');
			string host = address.Substring(0, index);
			string p = address.Substring(index + 1);
			int port = int.Parse(p);
			return ToIPEndPoint(host, port);
		}

		/// <summary>Windows 平台调 <c>SIO_UDP_CONNRESET</c> 关掉「对端关闭后下一发包抛 <c>WSAECONNRESET</c>」行为（macOS/Linux 静默返回）。</summary>
		public static void SetSioUdpConnReset(Socket socket)
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}

			const uint IOC_IN = 0x80000000;
			const uint IOC_VENDOR = 0x18000000;
			const int SIO_UDP_CONNRESET = unchecked((int)(IOC_IN | IOC_VENDOR | 12));

			socket.IOControl(SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
		}
	}
}
