using System;
using System.Text;

namespace RD
{
    /// <summary>
    /// 字段解析器接口 - 策略模式
    /// </summary>
    public interface IFieldParser
    {
        void Parse(CodeBuilder builder, string line, StringBuilder disposeBuilder);
    }

    /// <summary>
    /// Map 字段解析器
    /// </summary>
    public class MapFieldParser : IFieldParser
    {
        public void Parse(CodeBuilder builder, string line, StringBuilder disposeBuilder)
        {
            int start = line.IndexOf('<') + 1;
            int end = line.IndexOf('>');
            string types = line.Substring(start, end - start);
            string[] typeParts = types.Split(',');
            string keyType = TypeConverter.Convert(typeParts[0].Trim());
            string valueType = TypeConverter.Convert(typeParts[1].Trim());
            string tail = line[(end + 1)..];
            string[] parts = tail.Trim().Replace(";", "").Split(' ');
            string fieldName = parts[0];
            int order = int.Parse(parts[2]);

            builder.AppendLine("\t\t[MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]");
            builder.AppendLine($"\t\t[MemoryPackOrder({order - 1})]");
            builder.AppendLine($"\t\tpublic Dictionary<{keyType}, {valueType}> {fieldName} {{ get; set; }} = new();");

            disposeBuilder.Append($"this.{fieldName}.Clear();\n\t\t\t");
        }
    }

    /// <summary>
    /// Repeated 字段解析器
    /// </summary>
    public class RepeatedFieldParser : IFieldParser
    {
        public void Parse(CodeBuilder builder, string line, StringBuilder disposeBuilder)
        {
            try
            {
                int index = line.IndexOf(';');
                line = line.Remove(index);
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string type = TypeConverter.Convert(parts[1]);
                string fieldName = parts[2];
                int order = int.Parse(parts[4]);

                builder.AppendLine($"\t\t[MemoryPackOrder({order - 1})]");
                builder.AppendLine($"\t\tpublic List<{type}> {fieldName} {{ get; set; }} = new();");
                builder.AppendLine();

                disposeBuilder.Append($"this.{fieldName}.Clear();\n\t\t\t");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[RepeatedFieldParser] 解析失败: {line}");
                Console.WriteLine(e.ToString());
            }
        }
    }

    /// <summary>
    /// 简单字段解析器
    /// </summary>
    public class SimpleFieldParser : IFieldParser
    {
        public void Parse(CodeBuilder builder, string line, StringBuilder disposeBuilder)
        {
            try
            {
                int index = line.IndexOf(';');
                line = line.Remove(index);
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string type = parts[0];
                string fieldName = parts[1];
                int order = int.Parse(parts[3]);
                string typeCs = TypeConverter.Convert(type);

                builder.AppendLine($"\t\t[MemoryPackOrder({order - 1})]");
                builder.AppendLine($"\t\tpublic {typeCs} {fieldName} {{ get; set; }}");
                builder.AppendLine();

                if (typeCs != "bytes")
                {
                    disposeBuilder.Append($"this.{fieldName} = default;\n\t\t\t");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SimpleFieldParser] 解析失败: {line}");
                Console.WriteLine(e.ToString());
            }
        }
    }

    /// <summary>
    /// 字段解析器工厂 - 工厂模式
    /// </summary>
    public static class FieldParserFactory
    {
        public static IFieldParser CreateParser(string line)
        {
            if (line.StartsWith("map<"))
                return new MapFieldParser();
            if (line.StartsWith("repeated"))
                return new RepeatedFieldParser();
            return new SimpleFieldParser();
        }
    }

    /// <summary>
    /// 类型转换器
    /// </summary>
    public static class TypeConverter
    {
        public static string Convert(string type)
        {
            return type switch
            {
                "int16" => "short",
                "int32" => "int",
                "bytes" => "byte[]",
                "uint32" => "uint",
                "long" => "long",
                "int64" => "long",
                "uint64" => "ulong",
                "uint16" => "ushort",
                _ => type
            };
        }
    }
}
