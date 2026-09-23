using System;
using UnityEngine;

namespace RD
{
	public static class GameObjectHelper
	{
		public static T Get<T>(this GameObject gameObject, string key) where T : class
		{
			try
			{
				return gameObject.GetComponent<ReferenceRegistry>().Get<T>(key);
			}
			catch (Exception e)
			{
				throw new Exception($"获取{gameObject.name}的ReferenceRegistry key失败, key: {key}", e);
			}
		}
	}
}