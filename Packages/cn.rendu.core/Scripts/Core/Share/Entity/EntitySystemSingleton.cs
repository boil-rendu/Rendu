using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// 实体 System 注册中心（单例）：在 <see cref="Awake"/> 阶段扫描所有标有 <see cref="EntitySystemAttribute"/> 的 System 类，
    /// 建立 <c>实体类型 → System 集合</c> 的索引（<see cref="TypeSystems"/>），供后续 <c>Awake/Destroy/Update/Publish/Serialize</c> 等派发使用。
    /// 由源生成器生成的 <c>[CodeProcess]</c> 处理类确保单例在 <c>World</c> 初始化时即被创建。
    /// </summary>
    [CodeProcess]
    public class EntitySystemSingleton: Singleton<EntitySystemSingleton>, ISingletonAwake
    {
        /// <summary>类型 → System 集合的注册表（按 <see cref="ISystemType.Type"/> 索引）。</summary>
        public TypeSystems TypeSystems { get; private set; }

        /// <summary>初始化 <see cref="TypeSystems"/> 并扫描 <see cref="EntitySystemAttribute"/> 注册所有 System；若 System 同时是 <see cref="IEventSystem"/>，额外加入 <c>ClassType</c> 事件队列。</summary>
        public void Awake()
        {
            using var _ = Profiler.Once("EntitySystemSingleton.Awake");
            this.TypeSystems = new TypeSystems();

            foreach (Type type in CodeTypes.Instance.GetTypes(typeof (EntitySystemAttribute)))
            {
                SystemObject obj = (SystemObject)Activator.CreateInstance(type);

                if (obj is ISystemType iSystemType)
                {
                    TypeSystems.OneTypeSystems oneTypeSystems = this.TypeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Map.Add(iSystemType.SystemType(), obj);

                    if (iSystemType is IEventSystem)
                    {
                        oneTypeSystems.ClassType.Add(iSystemType.SystemType());
                    }
                }
            }
        }

        /// <summary>派发序列化钩子：调用实体上注册的所有 <c>ISerializeSystem</c>（若实体实现 <see cref="ISerialize"/>）。</summary>
        public void Serialize(Entity component)
        {
            if (component is not ISerialize)
            {
                return;
            }

            List<SystemObject> iSerializeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (ISerializeSystem));
            if (iSerializeSystems == null)
            {
                return;
            }

            foreach (ISerializeSystem serializeSystem in iSerializeSystems)
            {
                if (serializeSystem == null)
                {
                    continue;
                }

                try
                {
                    serializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发反序列化钩子：调用实体上注册的所有 <c>IDeserializeSystem</c>（若实体实现 <see cref="IDeserialize"/>）。</summary>
        public void Deserialize(Entity component)
        {
            if (component is not IDeserialize)
            {
                return;
            }

            List<SystemObject> iDeserializeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IDeserializeSystem));
            if (iDeserializeSystems == null)
            {
                return;
            }

            foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems)
            {
                if (deserializeSystem == null)
                {
                    continue;
                }

                try
                {
                    deserializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发 <c>GetComponent</c> 钩子：调用实体上注册的所有 <see cref="IGetComponentSystem"/>。</summary>
        // GetComponentSystem
        public void RunGetComponentSystems(Entity entity, Type type)
        {
            List<SystemObject> iGetSystem = this.TypeSystems.GetSystems(entity.GetType(), typeof (IGetComponentSystem));
            if (iGetSystem == null)
            {
                return;
            }

            foreach (IGetComponentSystem getSystem in iGetSystem)
            {
                if (getSystem == null)
                {
                    continue;
                }

                try
                {
                    getSystem.Run(entity, type);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发无参 Awake 钩子：调用实体上注册的所有无参 <see cref="IAwakeSystem"/>。</summary>
        public void Awake(Entity component)
        {
            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发 1 参 Awake 钩子（要求实体实现 <c>IAwake&lt;P1&gt;</c>）。</summary>
        public void Awake<P1>(Entity component, P1 p1)
        {
            if (component is not IAwake<P1>)
            {
                return;
            }

            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发 2 参 Awake 钩子（要求实体实现 <c>IAwake&lt;P1, P2&gt;</c>）。</summary>
        public void Awake<P1, P2>(Entity component, P1 p1, P2 p2)
        {
            if (component is not IAwake<P1, P2>)
            {
                return;
            }

            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发 3 参 Awake 钩子（要求实体实现 <c>IAwake&lt;P1, P2, P3&gt;</c>）。</summary>
        public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
        {
            if (component is not IAwake<P1, P2, P3>)
            {
                return;
            }

            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发 4 参 Awake 钩子（要求实体实现 <c>IAwake&lt;P1, P2, P3, P4&gt;</c>）。</summary>
        public void Awake<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (component is not IAwake<P1, P2, P3, P4>)
            {
                return;
            }

            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3, P4>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3, P4> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发 5 参 Awake 钩子（要求实体实现 <c>IAwake&lt;P1, P2, P3, P4, P5&gt;</c>）。</summary>
        public void Awake<P1, P2, P3, P4, P5>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (component is not IAwake<P1, P2, P3, P4, P5>)
            {
                return;
            }

            List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3, P4, P5>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3, P4, P5> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3, p4, p5);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>派发 Destroy 钩子：调用实体上注册的所有 <see cref="IDestroySystem"/>（若实体实现 <see cref="IDestroy"/>）。</summary>
        public void Destroy(Entity component)
        {
            if (component is not IDestroy)
            {
                return;
            }

            List<SystemObject> iDestroySystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IDestroySystem));
            if (iDestroySystems == null)
            {
                return;
            }

            foreach (IDestroySystem iDestroySystem in iDestroySystems)
            {
                if (iDestroySystem == null)
                {
                    continue;
                }

                try
                {
                    iDestroySystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
