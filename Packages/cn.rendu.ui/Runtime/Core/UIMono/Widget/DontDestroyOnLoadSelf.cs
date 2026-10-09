using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>跨场景不摧毁标记：Awake 时对自己调用 DontDestroyOnLoad</summary>
    [AddComponentMenu("UIFramework/Widget/跨场景不摧毁 【DontDestroyOnLoadSelf】")]
    public class DontDestroyOnLoadSelf : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
