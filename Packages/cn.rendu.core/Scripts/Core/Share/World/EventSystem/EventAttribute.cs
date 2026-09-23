using System;

namespace RD.Core
{
	public class EventAttribute: BaseAttribute
	{
		public int SceneType { get; }

		public EventAttribute(int sceneType)
		{
			this.SceneType = sceneType;
		}
	}
}