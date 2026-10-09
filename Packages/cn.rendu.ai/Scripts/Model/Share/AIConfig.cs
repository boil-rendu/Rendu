using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RD
{
	/// <summary>
	/// AI 配置表 category partial 扩展：启动期（<see cref="Bson.Serialization.IBsonIdProvider.EndInit"/>）把扁平表项按 <see cref="AIConfig.AIConfigId"/> 分组为 <c>List&lt;AIConfig&gt;</c> 树，
	/// 并按 <c>AIConfigId</c> 升序排序（<see cref="AIComponent"/> 沿该顺序执行）。
	/// </summary>
	public partial class AIConfigCategory
	{
		/// <summary>按 <see cref="AIConfig.AIConfigId"/> 分组的 AI 子树字典：运行时索引，<c>[BsonIgnore]</c> 不进 Bson。</summary>
		[BsonIgnore]
		public Dictionary<int, List<AIConfig>> AIConfigs = new ();

		/// <summary>取 <paramref name="aiConfigId"/> 对应的整棵子树（已排序）。</summary>
		public List<AIConfig> GetAI(int aiConfigId)
		{
			return this.AIConfigs[aiConfigId];
		}

		/// <inheritdoc />
		public override void EndInit()
		{
			foreach (var kv in this.GetAll())
			{
				List<AIConfig> aiNodeConfig;
				if (!this.AIConfigs.TryGetValue(kv.Value.AIConfigId, out aiNodeConfig))
				{
					aiNodeConfig = new List<AIConfig>();
					this.AIConfigs.Add(kv.Value.AIConfigId, aiNodeConfig);
				}

				aiNodeConfig.Add(kv.Value);
			}

			foreach (var kv in this.AIConfigs)
			{
				kv.Value.Sort((x, y)=>x.AIConfigId - y.AIConfigId);
			}
		}
	}
}
