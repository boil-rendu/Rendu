using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace RD.Core
{
    // 生命周期与父子挂载：IScene 注册、Parent/ComponentParent 挂载、Dispose、BeginInit
    public abstract partial class Entity
    {
        [BsonIgnoreIfDefault]
        [BsonDefaultValue(0L)]
        [BsonElement]
        [BsonId]
        public long Id { get; protected set; }

        [BsonIgnore]
        protected IScene iScene;

        /// <summary>
        /// 实体所属的场景/纤程上下文。设置 IScene 是实体"挂载到数据树"的充要条件：
        /// 一旦赋值，实体获得 InstanceId、被登记进场景系统，并将其子树（Components/Children）
        /// 一并级联注册到同一 IScene。
        /// <para>关键不变量：</para>
        /// <list type="bullet">
        ///   <item>不可设为 null，否则抛出（IScene 是实体的根上下文，丢失即脱离数据树）。</item>
        ///   <item>重复赋相同值直接返回，幂等。</item>
        ///   <item>已挂载到其它 IScene 时只允许替换为同一 IScene（避免跨树悬挂）。</item>
        ///   <item>反序列化重建的实体在此时恢复 Components/Children 的 parent 与 IScene 并触发 <see cref="EntitySystemSingleton.Deserialize"/>。</item>
        /// </list>
        /// </summary>
        [MemoryPackIgnore]
        [BsonIgnore]
        public IScene IScene
        {
            get
            {
                return this.iScene;
            }
            protected set
            {
                if (value == null)
                {
                    throw new Exception($"iScene cant set null: {this.GetType().FullName}");
                }

                if (this.iScene == value)
                {
                    return;
                }

                // 已属于其它 IScene：不允许跨树重挂，仅允许指向同一 IScene
                if (this.iScene != null)
                {
                    this.iScene = value;
                    return;
                }

                this.iScene = value;
                
                // 首次挂载才分配 InstanceId，保证同一实体在树上的实例标识稳定
                if (this.InstanceId == 0)
                {
                    this.InstanceId = IdGenerator.Instance.GenerateInstanceId();
                }

                this.IsRegister = true;

                // 反序列化出来的实体尚未建立父子关系，需要把挂在其下的
                // Components/Children 一并指向当前 IScene 并恢复 parent 引用
                if (this.components != null)
                {
                    foreach ((long _, Entity component) in this.components)
                    {
                        component.IsComponent = true;
                        component.parent = this;
                        component.IScene = this.iScene;
                    }
                }

                if (this.children != null)
                {
                    foreach ((long _, Entity child) in this.children)
                    {
                        child.IsComponent = false;
                        child.parent = this;
                        child.IScene = this.iScene;
                    }
                }
                    
                // 非新建实体（如反序列化）需执行反序列化系统恢复运行时状态
                if (!this.IsNew)
                {
                    EntitySystemSingleton.Instance.Deserialize(this);
                }
            }
        }

        // 该方法只能在AddComponent中调用，其他人不允许调用
        [BsonIgnore]
        private Entity ComponentParent
        {
            set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().FullName}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().FullName}");
                }

                // 严格限制parent必须要有iSence,也就是说parent必须在数据树上面
                if (value.IScene == null)
                {
                    throw new Exception($"cant set parent because parent iSence is null: {this.GetType().FullName} {value.GetType().FullName}");
                }

                if (this.parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this.parent == value)
                    {
                        Log.Error($"重复设置了Parent: {this.GetType().FullName} parent: {this.parent.GetType().FullName}");
                        return;
                    }

                    this.parent.RemoveComponentNoDispose(this);
                }

                this.parent = value;
                this.IsComponent = true;
                this.parent.AddToComponents(this);
                
                if (this is IScene scene)
                {
                    scene.Fiber = this.parent.iScene.Fiber;
                    this.IScene = scene;
                }
                else
                {
                    this.IScene = this.parent.iScene;
                }
            }
        }

        public T GetParent<T>() where T : Entity
        {
            return this.Parent as T;
        }

        [MemoryPackIgnore]
        [BsonIgnore]
        public bool IsDisposed => this.InstanceId == 0;

        [BsonIgnore]
        private Entity parent;

        // 可以改变parent，但是不能设置为null
        [MemoryPackIgnore]
        [BsonIgnore]
        public Entity Parent
        {
            get => this.parent;
            protected set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().FullName}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().FullName}");
                }

                // 严格限制parent必须要有iSence,也就是说parent必须在数据树上面
                if (value.IScene == null)
                {
                    throw new Exception($"cant set parent because parent iSence is null: {this.GetType().FullName} {value.GetType().FullName}");
                }

                if (this.parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this.parent == value)
                    {
                        Log.Error($"重复设置了Parent: {this.GetType().FullName} parent: {this.parent.GetType().FullName}");
                        return;
                    }

                    this.parent.RemoveChildNoDispose(this);
                }

                this.parent = value;
                this.IsComponent = false;
                this.parent.AddToChildren(this);

                if (this is IScene scene)
                {
                    scene.Fiber = this.parent.iScene.Fiber;
                    this.IScene = scene;
                }
                else
                {
                    this.IScene = this.parent.iScene;
                }

#if ENABLE_VIEW && UNITY_EDITOR
                this.ViewGO.GetComponent<ComponentView>().Component = this;
                this.ViewGO.transform.SetParent(this.Parent == null ?
                        UnityEngine.GameObject.Find("Global").transform : this.Parent.ViewGO.transform);
                foreach (Entity child in this.Children.Values)
                {
                    child.ViewGO.transform.SetParent(this.ViewGO.transform);
                }
                foreach (Entity comp in this.Components.Values)
                {
                    comp.ViewGO.transform.SetParent(this.ViewGO.transform);
                }
#endif
            }
        }

        /// <summary>
        /// 释放实体及其整棵子树（递归 Dispose 所有 Children 与 Components）。
        /// <para>注意事项：</para>
        /// <list type="bullet">
        ///   <item>已释放（InstanceId==0）直接返回，幂等。</item>
        ///   <item>先递归释放子树，再触发 <see cref="IDestroy"/>，最后从父节点摘除自身，避免悬挂引用。</item>
        ///   <item>释放后仅保留 <see cref="EntityStatus.IsFromPool"/> 标记，其余状态位清零，以便对象池安全复用。</item>
        ///   <item>从池中复用的实体再次 await/访问会产生灾难性后果，因此只有 IsFromPool 为 true 才回收到池。</item>
        /// </list>
        /// </summary>
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsRegister = false;
            this.InstanceId = 0;

            // 清理Children
            if (this.children != null)
            {
                foreach (Entity child in this.children.Values)
                {
                    child.Dispose();
                }

                this.children.Dispose();
                this.children = null;
            }

            // 清理Component
            if (this.components != null)
            {
                foreach (var kv in this.components)
                {
                    kv.Value.Dispose();
                }

                this.components.Dispose();
                this.components = null;
            }

            // 触发Destroy事件
            if (this is IDestroy)
            {
                EntitySystemSingleton.Instance.Destroy(this);
            }

            this.iScene = null;

            if (this.parent != null && !this.parent.IsDisposed)
            {
                if (this.IsComponent)
                {
                    this.parent.RemoveComponentNoDispose(this);
                }
                else
                {
                    this.parent.RemoveChildNoDispose(this);
                }
            }

            this.parent = null;

            base.Dispose();
            
            // 把status字段除了IsFromPool其它的status标记都还原
            bool isFromPool = this.IsFromPool;
            this.status = EntityStatus.None;
            this.IsFromPool = isFromPool;
            
            ObjectPool.Recycle(this);
        }

        public override void BeginInit()
        {
            // 如果没有挂到树上，不用执行SerializeSystem
            if (this.iScene == null)
            {
                return;
            }
            
            if (this is not ISerializeToEntity && !this.IsSerilizeWithParent)
            {
                return;
            }
            
            EntitySystemSingleton.Instance.Serialize(this);
            
            if (this.components != null && this.components.Count != 0)
            {
                foreach ((long _, Entity entity) in this.components)
                {
                    entity.BeginInit();
                }
            }

            if (this.children != null && this.children.Count != 0)
            {
                foreach ((long _, Entity entity) in this.children)
                {
                    entity.BeginInit();
                }
            }
        }
    }
}
