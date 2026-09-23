using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RD
{
    /// <summary>
    /// Proto 文件解析器 - 使用策略模式解析不同的字段类型
    /// </summary>
    public partial class ProtoParser
    {
        private static readonly char[] splitChars = [' ', '\t'];
        private readonly OpcodeRegistry opcodeRegistry;

        public ProtoParser(OpcodeRegistry opcodeRegistry)
        {
            this.opcodeRegistry = opcodeRegistry;
        }

        public ParseResult Parse(string protoContent, string protoName, int startOpcode)
        {
            var result = new ParseResult { CodeBuilder = new CodeBuilder() };

            BuildClassHeader(result.CodeBuilder);
            ParseContent(result.CodeBuilder, protoContent, protoName, startOpcode);

            // 关闭命名空间
            result.CodeBuilder.AppendLine("}");

            return result;
        }

        private void BuildClassHeader(CodeBuilder builder)
        {
            builder.AppendLine("using MemoryPack;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using RD.Core;");
            builder.AppendLine();
            builder.AppendLine("namespace RD");
            builder.AppendLine("{");
        }

        private void ParseContent(CodeBuilder builder, string protoContent, string protoName, int startOpcode)
        {
            bool isMsgStart = false;
            string msgName = "";
            string responseType = "";
            var disposeBuilder = new StringBuilder();
            Regex responseTypeRegex = ResponseTypeRegex();

            foreach (string line in protoContent.Split('\n'))
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                if (responseTypeRegex.IsMatch(trimmedLine))
                {
                    responseType = responseTypeRegex.Replace(trimmedLine, string.Empty);
                    responseType = responseType.Trim().Split(' ')[0].TrimEnd('\r', '\n');
                    continue;
                }

                if (ParseMessageStart(builder, trimmedLine, ref isMsgStart, ref msgName, ref responseType, protoName, ref startOpcode))
                    continue;

                if (isMsgStart)
                    ProcessMessageBody(builder, disposeBuilder, trimmedLine, ref isMsgStart, ref responseType, msgName);
            }
        }

        private bool ParseMessageStart(CodeBuilder builder, string line, ref bool isMsgStart, ref string msgName,
            ref string responseType, string protoName, ref int startOpcode)
        {
            if (!isMsgStart && line.StartsWith("//"))
            {
                BuildComment(builder, line);
                return true;
            }

            if (line.StartsWith("message"))
            {
                isMsgStart = true;
                msgName = ExtractMessageName(line);
                string parentClass = ExtractParentClass(line);

                opcodeRegistry.Add(msgName, ++startOpcode);
                BuildMessageAttributes(builder, protoName, msgName, responseType);
                BuildMessageDeclaration(builder, msgName, parentClass);

                return true;
            }

            return false;
        }

        private string ExtractMessageName(string line)
        {
            return line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
        }

        private string ExtractParentClass(string line)
        {
            string[] parts = line.Split(["//"], StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 2 ? parts[1].Trim() : "";
        }

        private void BuildMessageAttributes(CodeBuilder builder, string protoName, string msgName, string responseType)
        {
            builder.AppendLine("\t[MemoryPackable]");
            builder.AppendLine($"\t[Message({protoName}.{msgName})]");

            if (!string.IsNullOrEmpty(responseType))
            {
                builder.AppendLine($"\t[ResponseType(nameof({responseType}))]");
            }
        }

        private void BuildMessageDeclaration(CodeBuilder builder, string msgName, string parentClass)
        {
            builder.Append($"\tpublic partial class {msgName} : MessageObject");

            if (parentClass is "IActorMessage" or "IActorRequest" or "IActorResponse")
                builder.AppendLine($", {parentClass}");
            else if (!string.IsNullOrEmpty(parentClass))
                builder.AppendLine($", {parentClass}");
            else
                builder.AppendLine();
        }

        private void ProcessMessageBody(CodeBuilder builder, StringBuilder disposeBuilder, string line,
            ref bool isMsgStart, ref string responseType, string msgName)
        {
            if (line.StartsWith('{'))
            {
                disposeBuilder.Clear();
                builder.AppendLine("\t{");
                BuildCreateMethod(builder, msgName);
                return;
            }

            if (line.StartsWith('}'))
            {
                isMsgStart = false;
                responseType = "";
                BuildDisposeMethod(builder, disposeBuilder);
                builder.AppendLine("\t}");
                return;
            }

            if (line.StartsWith("//"))
            {
                builder.AppendLine("\t\t/// <summary>");
                builder.AppendLine($"\t\t/// {line.TrimStart('/', ' ')}");
                builder.AppendLine("\t\t/// </summary>");
                return;
            }

            string memberStr = line;
            if (line.Contains("//"))
            {
                string[] lineSplit = line.Split("//");
                memberStr = lineSplit[0].Trim();
                builder.AppendLine("\t\t/// <summary>");
                builder.AppendLine($"\t\t/// {lineSplit[1].Trim()}");
                builder.AppendLine("\t\t/// </summary>");
            }

            IFieldParser parser = FieldParserFactory.CreateParser(memberStr);
            parser.Parse(builder, memberStr, disposeBuilder);
        }

        private void BuildCreateMethod(CodeBuilder builder, string msgName)
        {
            builder.AppendLine($"\t\tpublic static {msgName} Create(bool isFromPool = false)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\treturn ObjectPool.Fetch<{msgName}>(isFromPool);");
            builder.AppendLine("\t\t}");
            builder.AppendLine();
        }

        private void BuildDisposeMethod(CodeBuilder builder, StringBuilder disposeBuilder)
        {
            builder.AppendLine($"\t\tpublic override void Dispose()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tif (!this.IsFromPool)");
            builder.AppendLine("\t\t\t{");
            builder.AppendLine("\t\t\t\treturn;");
            builder.AppendLine("\t\t\t}");
            builder.AppendLine();

            string disposeContent = disposeBuilder.ToString().TrimEnd('\t', '\n', '\r', ' ');
            if (!string.IsNullOrEmpty(disposeContent))
            {
                builder.AppendLine($"\t\t\t{disposeContent}");
            }

            builder.AppendLine("\t\t\tObjectPool.Recycle(this);");
            builder.AppendLine("\t\t}");
        }

        private void BuildComment(CodeBuilder builder, string line)
        {
            if (line.StartsWith("///"))
            {
                builder.AppendLine("\t/// <summary>");
                builder.AppendLine($"\t/// {line.TrimStart('/', ' ')}");
                builder.AppendLine("\t/// </summary>");
            }
            else
            {
                builder.AppendLine($"\t// {line.TrimStart('/', ' ')}");
            }
        }

        [GeneratedRegex(@"//\s*ResponseType")]
        private static partial Regex ResponseTypeRegex();
    }

    /// <summary>
    /// 解析结果
    /// </summary>
    public class ParseResult
    {
        public CodeBuilder CodeBuilder { get; set; }
    }
}
