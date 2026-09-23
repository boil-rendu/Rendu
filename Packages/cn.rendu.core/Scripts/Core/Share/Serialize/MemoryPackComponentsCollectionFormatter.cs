using MemoryPack.Internal;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;

// 引擎侧基类的 scoped 以 ScopedRef 属性编码，无法在 langversion 9.0 下写出（CS8652），
// 缺少该修饰符会让现代 Roslyn 报 CS9074 警告，属预期噪声，与 csproj 的 NoWarn 保持一致在此抑制。
#pragma warning disable CS9074

#nullable enable
namespace RD.Core
{
    /// <summary>
    /// 替换默认的SortedDictionaryFormatter
    /// </summary>
    [Preserve]
    public sealed class MemoryPackComponentsCollectionFormatter : MemoryPackFormatter<ComponentsCollection>
    {
        // 基类签名两侧不同：dotnet 侧（MemoryPack 1.21.4）为 scoped ref T?，Unity 侧为 ref T?；
        // 团结引擎 langversion 9.0 无法解析 scoped（CS8652），因此该修饰符按平台条件编译。
        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer,
#if !UNITY
                scoped
#endif
                ref ComponentsCollection? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }
            
            // writer.WriteCollectionHeader(value.Count);
            var formatter = writer.GetFormatter<Entity>();
            ref byte spanReference = ref writer.GetSpanReference(4);
            writer.Advance(4);
            int count = 0;
            foreach (var kv in value)
            {
                Entity entity = kv.Value;
                if (entity is ISerializeToEntity || entity.IsSerilizeWithParent)
                {
                    ++count;
                    formatter.Serialize(ref writer, ref entity!);
                }
            }
            Unsafe.WriteUnaligned(ref spanReference, count);
        }

        // 同 Serialize：scoped 仅 dotnet 侧基类签名需要。
        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader,
#if !UNITY
                scoped
#endif
                ref ComponentsCollection? value)
        {
            if (!reader.TryReadCollectionHeader(out int length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                // 这里甚至可以用对象池
                value = ComponentsCollection.Create(true);
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<Entity>();
            for (int i = 0; i < length; i++)
            {
                Entity entity = null!;
                formatter.Deserialize(ref reader, ref entity!);
                entity.IsSerilizeWithParent = true;
                long key = entity.GetLongHashCode();
                value.Add(key, entity);
            }
        }
    }
}