namespace RD
{
	/// <summary>
	/// Entity 自定义 Bson serializer 注册入口（partial）：由 SourceGenerator 在 partial 部分自动写入各 Entity 类型的 <c>RegisterSerializer</c> 调用，
	/// <see cref="Init"/> 调 <c>Register()</c> 一次性把全部 Entity 类型注册到 Bson 序列化器。
	/// 业务代码不应直接调 <see cref="Init"/>，由 <see cref="Entry.StartAsync"/> 启动期统一调用。
	/// </summary>
	public static partial class EntitySerializeRegister
	{
		static partial void Register();

		/// <summary>统一入口：触发 SourceGenerator 生成的 partial <c>Register()</c>。</summary>
		public static void Init()
		{
			Register();
		}
	}
}
