using RD.Core;
namespace RD
{
	/// <summary>空标记事件：场景切换开始。业务侧 <see cref="EventSystem"/> 监听此事件做切场景前清理。</summary>
	public struct SceneChangeStart
	{
	}

	/// <summary>空标记事件：场景切换完成。业务侧做切完场景后的初始化（加载资源/UI 等）。</summary>
	public struct SceneChangeFinish
	{
	}

	/// <summary>空标记事件：客户端 Scene 刚创建完。</summary>
	public struct AfterCreateClientScene
	{
	}

	/// <summary>空标记事件：当前 Scene 创建完（区别于 <see cref="AfterCreateClientScene"/>，后者是 ClientScene 任意一个，本标记是「主当前」）。</summary>
	public struct AfterCreateCurrentScene
	{
	}

	/// <summary>空标记事件：进入地图完成（地图资源加载 + 实体就位）。</summary>
	public struct EnterMapFinish
	{
	}
}
