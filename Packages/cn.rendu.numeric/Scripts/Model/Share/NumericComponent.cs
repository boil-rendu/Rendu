using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 数值组件扩展系统：数值 = <c>int numericType → long value</c> 字典，
	/// 业务侧用 <see cref="GetAsInt"/> / <see cref="GetAsLong"/> / <see cref="GetAsFloat"/>（自动除 10000）读写，
	/// <see cref="Set(int, int)"/> 等会触发 <see cref="NumericChange"/> 事件。
	/// <para>数值设计：每个最终值由 5 个基础值合成（公式见 <see cref="Update"/>）。</para>
	/// </summary>
	[FriendOf(typeof (NumericComponent))]
	public static class NumericComponentSystem
	{
		/// <summary>取 <paramref name="numericType"/> 的浮点值（内部 long 实际值除以 10000，保留两位小数精度）。</summary>
		public static float GetAsFloat(this NumericComponent self, int numericType)
		{
			return (float)self.GetByKey(numericType) / 10000;
		}

		/// <summary>取整数值（直接强转 <see cref="long"/>→<see cref="int"/>）。</summary>
		public static int GetAsInt(this NumericComponent self, int numericType)
		{
			return (int)self.GetByKey(numericType);
		}

		/// <summary>取 long 原始值。</summary>
		public static long GetAsLong(this NumericComponent self, int numericType)
		{
			return self.GetByKey(numericType);
		}

		/// <summary>写 float 值（内部乘 10000 存为 long 保留精度）。</summary>
		public static void Set(this NumericComponent self, int nt, float value)
		{
			self[nt] = (long)(value * 10000);
		}

		/// <summary>写 int 值。</summary>
		public static void Set(this NumericComponent self, int nt, int value)
		{
			self[nt] = value;
		}

		/// <summary>写 long 值。</summary>
		public static void Set(this NumericComponent self, int nt, long value)
		{
			self[nt] = value;
		}

		/// <summary>写值但不 publish <see cref="NumericChange"/> 事件（用于高频内部状态更新）。</summary>
		public static void SetNoEvent(this NumericComponent self, int numericType, long value)
		{
			self.Insert(numericType, value, false);
		}

		/// <summary>
		/// 内部插入：与旧值相等直接返回；<paramref name="numericType"/> &gt;= <see cref="NumericType.Max"/>（即衍生最终值）走 <see cref="Update"/> 重算，
		/// 否则 publish <see cref="NumericChange"/> 事件（<paramref name="isPublicEvent"/> 控制是否 publish）。
		/// </summary>
		public static void Insert(this NumericComponent self, int numericType, long value, bool isPublicEvent = true)
		{
			long oldValue = self.GetByKey(numericType);
			if (oldValue == value)
			{
				return;
			}

			self.NumericDic[numericType] = value;

			if (numericType >= NumericType.Max)
			{
				self.Update(numericType, isPublicEvent);
				return;
			}

			if (isPublicEvent)
			{
				EventSystem.Instance.Publish(self.Scene(),
					new NumericChange() { Unit = self.GetParent<Unit>(), New = value, Old = oldValue, NumericType = numericType });
			}
		}

		/// <summary>取 <paramref name="key"/> 对应的原始 long 值（找不到返回 0）。</summary>
		public static long GetByKey(this NumericComponent self, int key)
		{
			long value = 0;
			self.NumericDic.TryGetValue(key, out value);
			return value;
		}

		/// <summary>
		/// 5 段公式重算：<c>final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100</c>。
		/// 每个最终值由 base/add/pct/finalAdd/finalPct 五个基础值合成（支持 buff 叠加）。
		/// </summary>
		public static void Update(this NumericComponent self, int numericType, bool isPublicEvent)
		{
			int final = (int)numericType / 10;
			int bas = final * 10 + 1;
			int add = final * 10 + 2;
			int pct = final * 10 + 3;
			int finalAdd = final * 10 + 4;
			int finalPct = final * 10 + 5;

			// 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
			// final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
			long result = (long)(((self.GetByKey(bas) + self.GetByKey(add)) * (100 + self.GetAsFloat(pct)) / 100f + self.GetByKey(finalAdd)) *
				(100 + self.GetAsFloat(finalPct)) / 100f);
			self.Insert(final, result, isPublicEvent);
		}
	}

	/// <summary>数值变化事件：每次 <see cref="NumericComponentSystem.Insert"/> 写值（且与旧值不等）时 publish。</summary>
	public struct NumericChange
	{
		/// <summary>数值所属 Unit。</summary>
		public Unit Unit;
		/// <summary>变化的具体 numericType。</summary>
		public int NumericType;
		/// <summary>旧值。</summary>
		public long Old;
		/// <summary>新值。</summary>
		public long New;
	}

	/// <summary>
	/// 数值容器组件：挂在 <see cref="Unit"/> 上，以 <c>int numericType → long value</c> 字典存所有数值。
	/// 业务侧用 <see cref="NumericComponentSystem"/> 的扩展方法访问。
	/// </summary>
	[ComponentOf(typeof (Unit))]
	public class NumericComponent: Entity, IAwake, ITransfer
	{
		/// <summary>numericType → 原始 long 值字典。<c>[BsonDictionaryOptions(ArrayOfArrays)]</c> 让 Bson 序列化为 [[k,v]...] 数组（兼容老格式）。</summary>
		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
		public Dictionary<int, long> NumericDic = new Dictionary<int, long>();

		/// <summary>索引器：get 调 <see cref="NumericComponentSystem.GetByKey"/>，set 调 <see cref="NumericComponentSystem.Insert"/>（默认 publish 事件）。</summary>
		public long this[int numericType]
		{
			get
			{
				return this.GetByKey(numericType);
			}
			set
			{
				this.Insert(numericType, value);
			}
		}
	}
}
