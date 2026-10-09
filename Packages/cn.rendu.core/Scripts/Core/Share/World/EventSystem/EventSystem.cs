using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// Scene 级事件 + Invoke 总线：<c>[CodeProcess]</c> 阶段扫描 <see cref="CodeTypes"/> 反射出的
    /// <c>[Event]</c> handler（→ <c>allEvents</c>）和 <c>[Invoke]</c> handler（→ <c>allInvokers</c>），
    /// 提供 <see cref="PublishAsync{S,T}"/>/<see cref="Publish{S,T}"/> 多播派发 + <c>Invoke 调单</c> 两种语义。
    /// Invoke 跟 Publish 的区别见 <see cref="EventSystem_Invoke"/> 注释头。
    /// </summary>
    [CodeProcess]
    public partial class EventSystem: Singleton<EventSystem>, ISingletonAwake
    {
        private class EventInfo
        {
            public IEvent IEvent { get; }

            public int SceneType {get; }

            public EventInfo(IEvent iEvent, int sceneType)
            {
                this.IEvent = iEvent;
                this.SceneType = sceneType;
            }
        }

        private readonly Dictionary<Type, List<EventInfo>> allEvents = new();

        private readonly Dictionary<Type, Dictionary<long, object>> allInvokers = new();

        /// <summary>启动时反射 <see cref="CodeTypes"/>：把所有 <c>[Event]</c> handler 装入 <c>allEvents</c>，<c>[Invoke]</c> handler 装入 <c>allInvokers</c>（按 <c>IInvoke.Type</c> → <c>InvokeId</c> → handler 实例）。</summary>
        public void Awake()
        {
            using var _ = Profiler.Once("EventSystem.Awake");
            CodeTypes codeTypes = CodeTypes.Instance;
            foreach (Type type in codeTypes.GetTypes(typeof (EventAttribute)))
            {
                IEvent obj = Activator.CreateInstance(type) as IEvent;
                if (obj == null)
                {
                    throw new Exception($"type not is AEvent: {type.Name}");
                }

                object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);
                foreach (object attr in attrs)
                {
                    EventAttribute eventAttribute = attr as EventAttribute;

                    Type eventType = obj.Type;

                    EventInfo eventInfo = new(obj, eventAttribute.SceneType);

                    if (!this.allEvents.ContainsKey(eventType))
                    {
                        this.allEvents.Add(eventType, new List<EventInfo>());
                    }
                    this.allEvents[eventType].Add(eventInfo);
                }
            }

            foreach (Type type in codeTypes.GetTypes(typeof (InvokeAttribute)))
            {
                object obj = Activator.CreateInstance(type);
                IInvoke iInvoke = obj as IInvoke;
                if (iInvoke == null)
                {
                    throw new Exception($"type not is callback: {type.Name}");
                }

                object[] attrs = type.GetCustomAttributes(typeof(InvokeAttribute), false);
                foreach (object attr in attrs)
                {
                    if (!this.allInvokers.TryGetValue(iInvoke.Type, out var dict))
                    {
                        dict = new Dictionary<long, object>();
                        this.allInvokers.Add(iInvoke.Type, dict);
                    }

                    InvokeAttribute invokeAttribute = attr as InvokeAttribute;

                    // Invoke 统一键：字符串 invokeType（如 Invoke 源生成器产出的 [Invoke("LoginPanelComponent.OnEventLoginInvoke")]）
                    // 在这里稳定映射成 long；无字符串时回退到 long Type。
                    long invokeId = string.IsNullOrEmpty(invokeAttribute.InvokeType)
                            ? invokeAttribute.Type
                            : InvokeId.Of(invokeAttribute.InvokeType);

                    try
                    {
                        dict.Add(invokeId, obj);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"action type duplicate: {iInvoke.Type.Name} {invokeId}", e);
                    }
                }
            }
        }

        /// <summary>异步多播派发事件：取出 <c>allEvents[typeof(T)]</c> 中匹配当前 <c>SceneType</c> 的全部 handler，<see cref="PromiseHelper.WaitAll"/> 等待全部完成；任一抛异常由 <see cref="Log.Error"/> 记录。</summary>
        public async Promise PublishAsync<S, T>(S scene, T a) where S: class, IScene where T : struct
        {
            using var _ = Profiler.Section("EventSystem.PublishAsync");
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof(T), out iEvents))
            {
                return;
            }

            using ListComponent<Promise> list = ListComponent<Promise>.Create();

            foreach (EventInfo eventInfo in iEvents)
            {
                if (!SceneTypeSingleton.IsSame(scene.SceneType, eventInfo.SceneType))
                {
                    continue;
                }

                if (!(eventInfo.IEvent is AEvent<S, T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().FullName}");
                    continue;
                }

                list.Add(aEvent.Handle(scene, a));
            }

            try
            {
                await PromiseHelper.WaitAll(list);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>同步多播派发事件：取出匹配 <c>SceneType</c> 的全部 handler，<c>Handle().NoContext()</c> 触发（不等待结果）；<c>SceneType</c> 不匹配的跳过。</summary>
        public void Publish<S, T>(S scene, T a) where S: class, IScene where T : struct
        {
            using var _ = Profiler.Section("EventSystem.Publish");
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof (T), out iEvents))
            {
                return;
            }

            int sceneType = scene.SceneType;
            foreach (EventInfo eventInfo in iEvents)
            {
                if (!SceneTypeSingleton.IsSame(sceneType, eventInfo.SceneType))
                {
                    continue;
                }


                if (!(eventInfo.IEvent is AEvent<S, T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().FullName}");
                    continue;
                }

                aEvent.Handle(scene, a).NoContext();
            }
        }

    }
}
