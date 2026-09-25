using System;

namespace RD.Core
{
	public struct LateUpdateEvent
	{
	}
	
	public interface ILateUpdate: IEvent<LateUpdateEvent>
	{
	}

	[EntitySystem]
	public abstract class LateUpdateSystem<T> : EventSystem<T, LateUpdateEvent> where T: Entity, ILateUpdate
	{
		protected override void Handle(Entity e, LateUpdateEvent t)
		{
			this.LateUpdate((T)e);
		}

		protected abstract void LateUpdate(T self);
	}
}
