using System;

namespace RD.Core
{
    public interface IDynamicEvent<A> : IEvent<A> where A : struct
    {
    }

    public interface IDynamicEventSystem<A> : ISystemType where A : struct
    {
        Promise Run(Entity o, A message);
    }

    [EntitySystem]
    public abstract class DynamicEventSystem<T, A> : EventSystem<T, A>, IDynamicEventSystem<A>
            where T : Entity, IDynamicEvent<A> where A : struct
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IDynamicEventSystem<A>);
        }

        public new async Promise Run(Entity o, A message)
        {
            await DynamicEvent((T)o, message);
        }

        protected override void Handle(Entity e, A t)
        {
            throw new NotImplementedException();
        }

        protected abstract Promise DynamicEvent(T self, A message);
    }
}
