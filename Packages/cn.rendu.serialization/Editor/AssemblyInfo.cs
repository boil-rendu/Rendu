using System.Runtime.CompilerServices;

// 序列化内核(Serialization)的成员规则是 internal 实现细节, 但行为契约由测试钉住:
// 开放给本包的测试程序集, 不扩大公共 API —— internal 在程序集之外依然不可见。
[assembly: InternalsVisibleTo("RD.Serialization.Tests")]
