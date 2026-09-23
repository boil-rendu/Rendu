using System.Text;

namespace RD
{
    /// <summary>
    /// 代码构建器 - 封装 StringBuilder
    /// </summary>
    public class CodeBuilder
    {
        private readonly StringBuilder _builder = new();

        public void Append(string value)
        {
            _builder.Append(value);
        }

        public void AppendLine(string value = "")
        {
            _builder.AppendLine(value);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        public void FormatCode()
        {
            string code = _builder.ToString();
            _builder.Clear();
            code = code.Replace("\t", "    ");
            code = code.ReplaceLineEndings("\r\n");
            _builder.Append(code);
        }
    }
}
