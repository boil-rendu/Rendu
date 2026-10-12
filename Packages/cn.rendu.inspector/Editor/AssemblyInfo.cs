using System.Runtime.CompilerServices;

// 宿主测试程序集需要访问 Serialization 的 internal 规则函数
// (GetDataMembers / IsUnityPropertyBagType), 才能在不启动 IMGUI 的前提下
// 验证"哪些成员会进 JSON"。这不扩大公共 API: internal 在程序集之外依然不可见。
[assembly: InternalsVisibleTo("RD.Inspector.Tests")]
