using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace RD
{
    /// <summary>
    /// 表头信息
    /// </summary>
    public class HeadInfo
    {
        [BsonElement]
        public string FieldCS;
        public string FieldDesc;
        public string FieldName;
        public string FieldType;
        public int FieldIndex;

        public HeadInfo(string cs, string desc, string name, string type, int index)
        {
            FieldCS = cs;
            FieldDesc = desc;
            FieldName = name;
            FieldType = type;
            FieldIndex = index;
        }
    }

    /// <summary>
    /// 表信息
    /// </summary>
    public class Table
    {
        public string Name;
        public bool C;
        public bool S;
        public int Index;
        public Dictionary<string, HeadInfo> HeadInfos = new();
    }
}
