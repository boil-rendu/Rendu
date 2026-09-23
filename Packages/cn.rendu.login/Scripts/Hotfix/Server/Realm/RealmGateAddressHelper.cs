using System.Collections.Generic;
using RD.Core;


namespace RD.Server
{
	public static partial class RealmGateAddressHelper
	{
		public static StartSceneConfig GetGate(int zone, string account)
		{
			ulong hash = (ulong)account.GetLongHashCode();
			
			List<StartSceneConfig> zoneGates = StartSceneConfigCategory.Instance.GetBySceneType(zone, SceneType.Gate);
			
			return zoneGates[(int)(hash % (ulong)zoneGates.Count)];
		}
	}
}
