using System.Runtime.CompilerServices;

// 包内测试程序集需要访问 InspectorEngine 的 internal 元数据(成员缓存与分组树),
// 才能在不启动 IMGUI 的前提下验证"画什么、按什么顺序、归到哪一组"。
// 这不扩大公共 API: internal 在程序集之外依然不可见。
[assembly: InternalsVisibleTo("RD.EditorKit.Tests.Editor")]
