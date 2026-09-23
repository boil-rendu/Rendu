using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// Promise 状态机与对象池约束测试。
    /// 这些不变量是框架异步正确性的核心，改动 Promise 时必须回归。
    /// </summary>
    [TestFixture]
    public class PromiseTests
    {
        [Test]
        public void CompletedTask_Is_Already_Completed()
        {
            Promise t = Promise.CompletedTask;
            Assert.IsTrue(t.IsCompleted);
        }

        [Test]
        public void Create_Is_Pending_Until_SetResult()
        {
            Promise t = Promise.Create(false);
            Assert.IsFalse(t.IsCompleted, "新建且未 SetResult 的 Promise 应处于 Pending");

            t.SetResult();
            Assert.IsTrue(t.IsCompleted);
        }

        [Test]
        public void SetResult_Then_GetResult_Does_Not_Throw()
        {
            Promise t = Promise.Create(false);
            t.SetResult();
            Assert.DoesNotThrow(() => t.GetResult());
        }

        [Test]
        public void SetResult_Twice_Throws()
        {
            Promise t = Promise.Create(false);
            t.SetResult();
            Assert.Throws<InvalidOperationException>(() => t.SetResult());
        }

        [Test]
        public void GetResult_Before_Complete_Throws()
        {
            Promise t = Promise.Create(false);
            Assert.Throws<NotSupportedException>(() => t.GetResult());
        }

        [Test]
        public void SetException_Throws_On_GetResult()
        {
            Promise t = Promise.Create(false);
            var ex = new InvalidOperationException("boom");
            t.SetException(ex);

            InvalidOperationException thrown = Assert.Throws<InvalidOperationException>(() => t.GetResult());
            Assert.AreSame(ex, thrown);
        }

        [Test]
        public void SetException_Twice_Throws()
        {
            Promise t = Promise.Create(false);
            t.SetException(new Exception("a"));
            Assert.Throws<InvalidOperationException>(() => t.SetException(new Exception("b")));
        }

        [Test]
        public async Task Await_Completes_When_SetResult_Is_Called()
        {
            Promise t = Promise.Create(false);
            Task runner = Task.Run(() =>
            {
                System.Threading.Thread.Sleep(10);
                t.SetResult();
            });

            await t; // 不应抛，且应完成
            Assert.IsTrue(t.IsCompleted);
            await runner;
        }

        [Test]
        public void Typed_Create_SetResult_Returns_Value()
        {
            Promise<int> t = Promise<int>.Create(false);
            t.SetResult(42);
            Assert.IsTrue(t.IsCompleted);
            Assert.AreEqual(42, t.GetResult());
        }

        [Test]
        public void Typed_SetResult_Twice_Throws()
        {
            Promise<string> t = Promise<string>.Create(false);
            t.SetResult("ok");
            Assert.Throws<InvalidOperationException>(() => t.SetResult("again"));
        }

        [Test]
        public void Typed_SetException_Throws_On_GetResult()
        {
            Promise<int> t = Promise<int>.Create(false);
            var ex = new ArgumentException("bad");
            t.SetException(ex);

            ArgumentException thrown = Assert.Throws<ArgumentException>(() => t.GetResult());
            Assert.AreSame(ex, thrown);
        }
    }
}
