using Unity.Mathematics;
using RD.Core;

namespace RD
{
	/// <summary>
	/// Unity.Mathematics 的 <c>float2/3/4</c> 模糊比较扩展：<see cref="Unity.Mathematics.math"/> 没有现成 Approximately，这里补一组按 epsilon 容差比较的重载。
	/// 区别于 <c>math.equals</c>（严格按 bit 相等，浮点运算后容易 false），<see cref="Approximately(Unity.Mathematics.float3, Unity.Mathematics.float3, float)"/> 容差更适合业务侧「位置/方向是否到达目标」判断。
	/// </summary>
	public static class Float_Extend
	{
		//float3.equals 的精度非常高,
		//这里提供一个精度更低的比较方法
		/// <summary>三轴分别按 <paramref name="epsilon"/> 容差比较；全 true 才返回 true。默认 0.001f。</summary>
		public static bool Approximately(this float3 a, float3 b, float epsilon = 0.001f)
		{
			bool xEqual = math.abs(a.x - b.x) < epsilon;
			bool yEqual = math.abs(a.y - b.y) < epsilon;
			bool zEqual = math.abs(a.z - b.z) < epsilon;
			return xEqual && yEqual && zEqual;
		}

		/// <summary>只比较 XZ 轴（Y 高度不参与），用于地面上的「是否到达」判断。</summary>
		public static bool ApproximatelyXZ(this float3 a, float3 b, float epsilon = 0.001f)
		{
			bool xEqual = math.abs(a.x - b.x) < epsilon;
			bool zEqual = math.abs(a.z - b.z) < epsilon;
			return xEqual && zEqual;
		}

		/// <summary>二维向量模糊比较。</summary>
		public static bool Approximately(this float2 a, float2 b, float epsilon = 0.001f)
		{
			bool xEqual = math.abs(a.x - b.x) < epsilon;
			bool yEqual = math.abs(a.y - b.y) < epsilon;
			return xEqual && yEqual;
		}

		/// <summary>四维向量（quaternion）模糊比较。</summary>
		public static bool Approximately(this float4 a, float4 b, float epsilon = 0.001f)
		{
			bool xEqual = math.abs(a.x - b.x) < epsilon;
			bool yEqual = math.abs(a.y - b.y) < epsilon;
			bool zEqual = math.abs(a.z - b.z) < epsilon;
			bool wEqual = math.abs(a.w - b.w) < epsilon;
			return xEqual && yEqual && zEqual && wEqual;
		}
	}
}
