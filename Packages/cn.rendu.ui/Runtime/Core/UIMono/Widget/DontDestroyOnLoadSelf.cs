using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [AddComponentMenu("UIFramework/Widget/跨场景不摧毁 【DontDestroyOnLoadSelf】")]
    public class DontDestroyOnLoadSelf : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
