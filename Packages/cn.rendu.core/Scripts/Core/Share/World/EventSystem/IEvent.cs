using System;

namespace RD.Core
{
	public interface IEvent
	{
		Type Type { get; }
	}
	
	public abstract class AEvent<S, A>: IEvent where S: class, IScene where A: struct
	{
		public Type Type
		{
			get
			{
				return typeof (A);
			}
		}

		protected abstract Promise Run(S scene, A a);

		public async Promise Handle(S scene, A a)
		{
			try
			{
				await Run(scene, a);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}