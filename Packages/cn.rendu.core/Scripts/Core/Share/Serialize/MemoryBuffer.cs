using System;
using System.Buffers;
using System.IO;

namespace RD.Core
{
	/// <summary>
	/// 网络包专用 <see cref="MemoryStream"/>：在 <see cref="MemoryStream"/> 基础上实现 <see cref="IBufferWriter{T}"/> 接口（满足 MemoryPack 的 <c>writer</c> 要求），并记录 <c>origin</c> 偏移用于子包场景。
	/// 池化于 <see cref="AService.Fetch"/>：超过 1KB 的按需分配不入池。
	/// </summary>
	public class MemoryBuffer: MemoryStream, IBufferWriter<byte>
	{
		private int origin;

		/// <summary>默认构造：空缓冲，由 <see cref="GetSpan(int)"/>/<see cref="GetMemory(int)"/> 按需扩展。</summary>
		public MemoryBuffer()
		{
		}

		/// <summary>按 <paramref name="capacity"/> 预分配底层字节数组。</summary>
		public MemoryBuffer(int capacity): base(capacity)
		{
		}

		/// <summary>用现有 <paramref name="buffer"/> 作底层存储（外部拥有所有权）。</summary>
		public MemoryBuffer(byte[] buffer): base(buffer)
		{
		}

		/// <summary>用 <c>buffer[index..index+length]</c> 作底层存储：<paramref name="index"/> 记为 <see cref="origin"/>，<see cref="WrittenMemory"/>/<see cref="WrittenSpan"/> 都从 <c>origin</c> 开始算，避免子包边界错乱。</summary>
		public MemoryBuffer(byte[] buffer, int index, int length): base(buffer, index, length)
		{
			this.origin = index;
		}

		/// <summary>已写入数据 <c>[origin, origin+Position)</c> 的不可变视图：用于 <c>MemoryPack.Serialize(writer, span)</c> 形式提交给序列化器。</summary>
		public ReadOnlyMemory<byte> WrittenMemory => this.GetBuffer().AsMemory(this.origin, (int)this.Position);

		/// <summary>已写入数据 <c>[origin, origin+Position)</c> 的 <see cref="ReadOnlySpan{T}"/> 视图。</summary>
		public ReadOnlySpan<byte> WrittenSpan => this.GetBuffer().AsSpan(this.origin, (int)this.Position);

		/// <summary><see cref="IBufferWriter{T}.Advance(int)"/>：把 <see cref="MemoryStream.Position"/> 推进 <paramref name="count"/> 字节（必要时 <c>SetLength</c> 扩展流）。</summary>
		public void Advance(int count)
		{
			long newLength = this.Position + count;
			if (newLength > this.Length)
			{
				this.SetLength(newLength);
			}
			this.Position = newLength;
		}

		/// <summary><see cref="IBufferWriter{T}.GetMemory(int)"/>：返回从当前 <see cref="MemoryStream.Position"/> 起的 <see cref="Memory{T}"/> 视图（必要时 <c>SetLength</c> 扩展），由 <c>Advance</c> 通知写完。</summary>
		public Memory<byte> GetMemory(int sizeHint = 0)
		{
			if (this.Length - this.Position < sizeHint)
			{
				this.SetLength(this.Position + sizeHint);
			}
			var memory = this.GetBuffer().AsMemory((int)this.Position + this.origin, (int)(this.Length - this.Position));
			return memory;
		}

		/// <summary><see cref="IBufferWriter{T}.GetSpan(int)"/>：返回从当前 <see cref="MemoryStream.Position"/> 起的 <see cref="Span{T}"/> 视图（必要时 <c>SetLength</c> 扩展）。</summary>
		public Span<byte> GetSpan(int sizeHint = 0)
		{
			if (this.Length - this.Position < sizeHint)
			{
				this.SetLength(this.Position + sizeHint);
			}
			var span = this.GetBuffer().AsSpan((int)this.Position + this.origin, (int)(this.Length - this.Position));
			return span;
		}
	}
}
