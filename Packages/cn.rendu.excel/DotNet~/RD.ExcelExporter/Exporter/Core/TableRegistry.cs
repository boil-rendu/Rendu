using System;
using System.Collections.Generic;

namespace RD
{
    /// <summary>
    /// 表注册器 - 管理所有配置表的定义
    /// </summary>
    public class TableRegistry
    {
        private readonly Dictionary<string, Table> tables = new Dictionary<string, Table>();

        public Table GetOrCreate(string tableName)
        {
            if (!tables.TryGetValue(tableName, out var table))
            {
                table = new Table { Name = tableName };
                tables[tableName] = table;
            }
            return table;
        }

        public IEnumerable<Table> GetAllTables()
        {
            return tables.Values;
        }

        public void Clear()
        {
            tables.Clear();
        }
    }
}
