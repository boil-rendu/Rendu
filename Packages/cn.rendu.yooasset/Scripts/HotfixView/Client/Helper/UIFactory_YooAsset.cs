using UnityEngine;
using RD.Core;

namespace RD.Client
{
    public static partial class UIFactory
    {
        public static async Promise<GameObject> InstantiateGameObjectAsync(Scene scene, string resName)
        {
            return await InstantiateGameObjectAsync(scene, "", resName);
        }
    }
}