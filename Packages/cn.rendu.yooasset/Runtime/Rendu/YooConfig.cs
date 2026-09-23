using UnityEngine;
using YooAsset;

namespace RD.YooAsset
{
    [CreateAssetMenu(menuName = "Rendu/YooConfig", fileName = "YooConfig", order = 0)]
    public class YooConfig: ScriptableObject
    {
        public EPlayMode EPlayMode;

        [Header("Editor Simulate Mode（测试流程）")]
        [Tooltip("编辑器模拟模式：每次启动清空虚拟下载缓存，强制重新走一遍「检查 → 下载核心」流程")]
        public bool EditorForceRedownload = true;

        [Tooltip("编辑器模拟模式：虚拟下载速度（字节/秒），调小可放慢便于观察下载进度")]
        public int EditorSimulateDownloadSpeed = 1024 * 1000;

        [Header("Host Play Mode")]
        public string Url;

        [Header("Host Play Mode（测试流程）")]
        [Tooltip("联机模式：每次启动清空本地沙盒缓存（已下载资源 + 缓存清单），模拟首次安装，强制重新走「检查 → 下载」完整流程。仅测试用，发布前务必取消勾选")]
        public bool HostForceClearSandbox = false;
    }
}
