using System.Net;
using NUnit.Framework;
using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// 测试用空实现传输层。所有方法均为空操作，避免依赖真实 UDP socket。
    /// </summary>
    public class FakeTransport : IKcpTransport
    {
        public void Send(byte[] bytes, int index, int length, EndPoint endPoint, ChannelType channelType)
        {
        }

        public int Recv(byte[] buffer, ref EndPoint endPoint) => 0;

        public int Available() => 0;

        public void Update()
        {
        }

        public void OnError(long id, int error)
        {
        }

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// KChannelTable 连接表测试（不含副作用：Dispose / Transport.OnError）。
    /// 通过 connect 路径构造 KChannel（不触发 unsafe 的 Kcp 构造），再手动设置
    /// RemoteConn 以验证等待 accept 表的关联与移除。
    /// </summary>
    [TestFixture]
    public class KChannelTableTests
    {
        private KService service;

        [SetUp]
        public void SetUp()
        {
            World.Instance.AddSingleton<TimeInfo>();
            World.Instance.AddSingleton<ObjectPool>();
            service = new KService(new FakeTransport(), ServiceType.Inner);
        }

        [TearDown]
        public void TearDown()
        {
            ObjectPool.Instance?.Dispose();
        }

        private KChannel NewConnectChannel(uint localConn)
        {
            var ep = new IPEndPoint(IPAddress.Loopback, 1234);
            return new KChannel(localConn, ep, service);
        }

        [Test]
        public void AddActive_Then_Get_Returns_Same()
        {
            var table = new KChannelTable();
            KChannel ch = NewConnectChannel(100);
            table.AddActive(ch);
            Assert.AreEqual(1, table.ActiveCount);
            Assert.AreSame(ch, table.Get(100));
            Assert.IsTrue(table.ContainsLocalConn(100));
        }

        [Test]
        public void Remove_Active_Returns_Channel_And_Clears()
        {
            var table = new KChannelTable();
            KChannel ch = NewConnectChannel(100);
            table.AddActive(ch);
            KChannel removed = table.Remove(100);
            Assert.AreSame(ch, removed);
            Assert.AreEqual(0, table.ActiveCount);
            Assert.IsNull(table.Get(100));
            Assert.IsFalse(table.ContainsLocalConn(100));
        }

        [Test]
        public void Remove_Missing_Returns_Null()
        {
            var table = new KChannelTable();
            Assert.IsNull(table.Remove(999));
        }

        [Test]
        public void WaitAccept_Association_Removed_With_Active()
        {
            var table = new KChannelTable();
            KChannel ch = NewConnectChannel(100);
            ch.RemoteConn = 200;
            table.AddActive(ch);
            table.AddWaitAccept(ch);
            Assert.AreEqual(1, table.ActiveCount);
            Assert.AreEqual(1, table.WaitAcceptCount);
            Assert.IsTrue(table.TryGetWaitAccept(200, out KChannel w));
            Assert.AreSame(ch, w);

            KChannel removed = table.Remove(100);
            Assert.AreSame(ch, removed);
            Assert.AreEqual(0, table.ActiveCount);
            Assert.AreEqual(0, table.WaitAcceptCount);
            Assert.IsFalse(table.TryGetWaitAccept(200, out _));
        }

        [Test]
        public void WaitAccept_Only_Removed_When_LocalConn_Matches()
        {
            var table = new KChannelTable();
            KChannel a = NewConnectChannel(100);
            a.RemoteConn = 200;
            KChannel b = NewConnectChannel(300);
            b.RemoteConn = 201;
            table.AddActive(a);
            table.AddActive(b);
            table.AddWaitAccept(a);
            table.AddWaitAccept(b);

            table.Remove(100); // 仅移除 a(localConn=100, remoteConn=200)

            Assert.AreEqual(1, table.ActiveCount);
            Assert.AreEqual(1, table.WaitAcceptCount);
            Assert.IsFalse(table.TryGetWaitAccept(200, out _));
            Assert.IsTrue(table.TryGetWaitAccept(201, out KChannel w));
            Assert.AreSame(b, w);
        }
    }
}
