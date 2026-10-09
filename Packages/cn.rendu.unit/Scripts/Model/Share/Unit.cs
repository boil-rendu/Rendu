using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using RD.Core;
using Unity.Mathematics;

namespace RD
{
	/// <summary>
	/// Unit 实体：游戏世界中的基础角色/对象（玩家/怪物/NPC/物件通用）。继承 <see cref="Entity"/> + <see cref="IAwake{T}"/>，
	/// 持有 <see cref="ConfigId"/>（指向配置表 <c>UnitConfig</c>）、<see cref="Position"/>/<see cref="Rotation"/>（Unity.Mathematics 类型），
	/// setter 触发 <see cref="ChangePosition"/>/<see cref="ChangeRotation"/> 事件。
	/// </summary>
	[ChildOf(typeof(UnitComponent))]
	[DebuggerDisplay("ViewName,nq")]
	public partial class Unit: Entity, IAwake<int>
	{
		/// <summary>配置表 Id：指向 <c>UnitConfig</c>，启动期从表里读出 model/prefab 路径等。</summary>
		public int ConfigId { get; set; } //配置表id

		[BsonElement]
		private float3 position; //坐标

		/// <summary>Unit 世界坐标：setter 写入 <c>position</c> 字段并 publish <see cref="ChangePosition"/> 事件，携带旧坐标。</summary>
		[BsonIgnore]
		public float3 Position
		{
			get => this.position;
			set
			{
				float3 oldPos = this.position;
				this.position = value;
				EventSystem.Instance.Publish(this.Scene(), new ChangePosition() { Unit = this, OldPos = oldPos });
			}
		}

		/// <summary>Unit 前向向量：setter 通过 <c>LookRotation</c> 反算 <see cref="Rotation"/>。</summary>
		[BsonIgnore]
		public float3 Forward
		{
			get => math.mul(this.Rotation, math.forward());
			set => this.Rotation = quaternion.LookRotation(value, math.up());
		}

		[BsonElement]
		private quaternion rotation;

		/// <summary>Unit 旋转：setter 写入 <c>rotation</c> 字段并 publish <see cref="ChangeRotation"/> 事件。</summary>
		[BsonIgnore]
		public quaternion Rotation
		{
			get => this.rotation;
			set
			{
				this.rotation = value;
				EventSystem.Instance.Publish(this.Scene(), new ChangeRotation() { Unit = this });
			}
		}

		/// <inheritdoc />
		protected override string ViewName
		{
			get
			{
				return $"{this.GetType().FullName} ({this.Id})";
			}
		}
	}
}
