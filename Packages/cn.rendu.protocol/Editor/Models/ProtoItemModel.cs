using UnityEngine;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Proto文件数据模型
    /// </summary>
    public class ProtoItemModel
    {
        public string Name { get; set; }
        public int Num { get; set; }
        public string SimpPath { get; set; }
        public string FullPath { get; set; }
        public short ErrorCode { get; set; }

        /// <summary>
        /// 错误代码: 0-无错误, 1-格式错误, 2-数值重复, 3-数值解析失败
        /// </summary>
        public bool HasError => ErrorCode != 0;
        public bool IsFormatError => ErrorCode == 1;
        public bool IsDuplicateError => ErrorCode == 2;
        public bool IsParseError => ErrorCode == 3;

        public ProtoItemModel(string name, int num, string simpPath, string fullPath, short errorCode)
        {
            Name = name;
            Num = num;
            SimpPath = simpPath;
            FullPath = fullPath;
            ErrorCode = errorCode;
        }
    }
}
