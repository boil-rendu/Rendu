using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;

namespace RD.Core
{
    /// <summary>
    /// 场景根实体，实现 <see cref="IScene"/>，是数据树的根节点（游戏内所有实体都直接或间接挂在其下）。
    /// 持有所属 <see cref="Fiber"/> 引用，并通过 IScene = this 把自己登记为场景树根。
    /// </summary>
    [EnableMethod]
    [ChildOf]
    public class Scene: Entity, IScene
    {
        /// <summary>所属纤程（Fiber）。场景在纤程内创建，其下所有实体的 System 由该 Fiber 驱动执行。</summary>
        [BsonIgnore]
        public Fiber Fiber { get; set; }
        
        /// <summary>场景名称（仅用于调试/日志）。</summary>
        public string Name { get; }
        
        /// <summary>场景类型（对应 SceneType 枚举，决定该场景承载的逻辑，如游戏服/地图服等）。</summary>
        public int SceneType
        {
            get;
            set;
        }

        public Scene()
        {
        }

        /// <summary>
        /// 创建并初始化一个场景：设置 Id/Name/InstanceId/SceneType，绑定 Fiber，
        /// 并将自身登记为数据树根（IScene = this、IsRegister = true），同时标记 IsNew。
        /// </summary>
        public Scene(Fiber fiber, long id, long instanceId, int sceneType, string name)
        {
            this.Id = id;
            this.Name = name;
            this.InstanceId = instanceId;
            this.SceneType = sceneType;
            this.IsNew = true;
            this.Fiber = fiber;
            this.IScene = this;
            this.IsRegister = true;
            Log.Info($"scene create: {this.Name} {this.SceneType} {this.Id} {this.InstanceId}");
        }

        /// <summary>释放场景：先释放其下所有实体，再记录销毁日志。</summary>
        public override void Dispose()
        {
            base.Dispose();
            
            Log.Info($"scene dispose: {this.SceneType} {this.Id} {this.InstanceId}");
        }
        
        /// <summary>视图名：用于日志/调试标识，格式为"类型名 (SceneType)"。</summary>
        protected override string ViewName
        {
            get
            {
                return $"{this.GetType().Name} ({this.SceneType})";
            }
        }
    }
}