using System.Runtime.CompilerServices;

// 预留: 本包当前没有需要开放 internal 的测试 —— 检视器宿主是纯 GUI 胶水,
// 序列化内核的测试已随内核迁至 cn.rendu.serialization(RD.Serialization.Tests)。
// 将来若在本包加测试并需要访问 internal, 在这里补一行即可(不扩大公共 API)。
[assembly: InternalsVisibleTo("RD.Inspector.Tests")]
