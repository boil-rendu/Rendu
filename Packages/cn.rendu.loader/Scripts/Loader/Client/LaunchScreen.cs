using UnityEngine;
using UnityEngine.UI;

namespace RD.Client
{
    /// <summary>
    /// 加载器层启动界面（不依赖热更 dll）。
    /// 负责在核心数据（代码 dll）下载 / 加载阶段展示进度与状态，
    /// 背景动画由全局背景动画（GlobalBackgroundAnim）提供，贯穿到登录界面与进入游戏。
    /// 热更层 UI（UIResCheck）就绪后由 CodeLoader 或热更层调用 Hide 隐藏。
    /// 界面结构完全来自 Packages/cn.rendu.loader/Resources/LaunchScreen.prefab（已随项目提供，可在场景中直接编辑），
    /// 生命周期为 DontDestroyOnLoad。prefab 缺失时 Show 返回 null，不创建任何界面。
    /// </summary>
    public class LaunchScreen : MonoBehaviour
    {
        private static LaunchScreen instance;

        private Text stateText;
        private Text percentText;
        private Image progressFill;
        private Text serverInfoText;

        public static LaunchScreen Instance => instance;

        /// <summary>显示启动界面（懒创建，幂等）。界面完全来自 prefab，缺失时返回 null 并报错。</summary>
        public static LaunchScreen Show()
        {
            if (instance == null)
            {
                instance = CreateFromPrefab();
            }

            if (instance == null)
            {
                Debug.LogError("[LaunchScreen] 未找到 Packages/cn.rendu.loader/Resources/LaunchScreen.prefab，启动界面无法显示。请确认该资源存在于 loader 包 Resources 目录");
                return null;
            }

            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>隐藏并销毁启动界面（幂等）</summary>
        public static void Hide()
        {
            if (instance == null) return;

            Destroy(instance.gameObject);
            instance = null;
        }

        /// <summary>更新状态文字</summary>
        public void SetState(string state)
        {
            if (stateText != null)
            {
                stateText.text = state ?? string.Empty;
            }
        }

        /// <summary>设置服务器/渠道信息（显示在界面左下角小字）</summary>
        public void SetServerInfo(string info)
        {
            if (serverInfoText != null)
            {
                serverInfoText.text = info ?? string.Empty;
            }
        }

        /// <summary>更新进度条与状态文字，progress 取值 0~1</summary>
        public void SetProgress(float progress, string state = null)
        {
            SetProgress(progress, 0L, 0L, state);
        }

        /// <summary>
        /// 更新进度条、百分比与字节进度。
        /// progress 取值 0~1；currentBytes/totalBytes 为 0 时仅显示百分比。
        /// </summary>
        public void SetProgress(float progress, long currentBytes, long totalBytes, string state = null)
        {
            progress = Mathf.Clamp01(progress);

            if (progressFill != null)
            {
                progressFill.fillAmount = progress;
                progressFill.SetAllDirty();
            }

            if (percentText != null)
            {
                percentText.text = $"{progress * 100f:0}%";
            }

            if (stateText != null)
            {
                string prefix = !string.IsNullOrEmpty(state) ? state : "正在下载";
                if (totalBytes > 0)
                {
                    string current = FormatBytes(currentBytes);
                    string total = FormatBytes(totalBytes);
                    string remaining = FormatBytes(System.Math.Max(0L, totalBytes - currentBytes));
                    stateText.text = $"{prefix}  {current} / {total}（剩余 {remaining}）";
                }
                else if (!string.IsNullOrEmpty(state))
                {
                    stateText.text = state;
                }
            }
        }

        // ==================== 构建 ====================

        /// <summary>从 Packages/cn.rendu.loader/Resources/LaunchScreen.prefab 实例化启动界面（prefab 缺失返回 null）</summary>
        private static LaunchScreen CreateFromPrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("LaunchScreen");
            if (prefab == null)
            {
                return null;
            }

            GameObject go = Instantiate(prefab);
            go.name = "LaunchScreen";
            DontDestroyOnLoad(go);

            // prefab 已内置 Canvas/CanvasScaler/GraphicRaycaster，脚本由运行时挂载
            LaunchScreen screen = go.AddComponent<LaunchScreen>();
            screen.BindReferences(go.transform);
            screen.SetupGlobalBackground();
            return screen;
        }

        /// <summary>按节点名绑定运行时引用（节点名约定：ProgressBar/Fill、Percent、State、ServerInfo）</summary>
        private void BindReferences(Transform root)
        {
            Transform fillT = root.Find("ProgressBar/Fill");
            progressFill = fillT != null ? fillT.GetComponent<Image>() : null;

            Transform pctT = root.Find("Percent");
            percentText = pctT != null ? pctT.GetComponent<Text>() : null;

            Transform stateT = root.Find("State");
            stateText = stateT != null ? stateT.GetComponent<Text>() : null;

            Transform infoT = root.Find("ServerInfo");
            serverInfoText = infoT != null ? infoT.GetComponent<Text>() : null;
        }

        /// <summary>创建全局背景动画并加载原生视频播放（启动阶段不依赖热更资源）</summary>
        private void SetupGlobalBackground()
        {
            // 全局背景动画独立于本界面存活（DontDestroyOnLoad），贯穿 启动 → 版本检查 → 登录，
            // 进入 Lobby 后由热更层调用 GlobalBackgroundAnim.Hide() 销毁
            GlobalBackgroundAnim.Ensure();
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024L) return $"{bytes} B";
            if (bytes < 1024L * 1024L) return $"{bytes / 1024f:F1} KB";
            if (bytes < 1024L * 1024L * 1024L) return $"{bytes / (1024f * 1024f):F2} MB";
            return $"{bytes / (1024f * 1024f * 1024f):F2} GB";
        }
    }
}
