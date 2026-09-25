using System;

namespace RD.Core
{
	public struct UpdateEvent
	{
	}
	
	public interface IUpdate: IEvent<UpdateEvent>
	{
	}

	[EntitySystem]
	public abstract class UpdateSystem<T> : EventSystem<T, UpdateEvent> where T: Entity, IUpdate
	{
		protected override void Handle(Entity e, UpdateEvent t)
		{
			this.Update((T)e);
		}

		protected abstract void Update(T self);
	}
}
