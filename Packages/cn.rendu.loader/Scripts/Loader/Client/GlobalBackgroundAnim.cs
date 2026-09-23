using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace RD.Client
{
    /// <summary>
    /// 全局背景动画：Lobby 之前的界面（启动 → 版本检查 → 登录）共用同一段背景视频。
    /// 独立于 LaunchScreen 实例（DontDestroyOnLoad），由 LaunchScreen 创建后贯穿整个流程，
    /// 热更层在进入 Lobby 后调用 Hide() 销毁。
    /// </summary>
    public class GlobalBackgroundAnim : MonoBehaviour
    {
        private const int RenderWidth = 1920;
        private const int RenderHeight = 1080;

        private static GlobalBackgroundAnim instance;

        private VideoPlayer videoPlayer;
        private RenderTexture renderTexture;

        public static GlobalBackgroundAnim Instance => instance;

        /// <summary>确保全局背景动画存在并播放（幂等）</summary>
        public static GlobalBackgroundAnim Ensure()
        {
            if (instance != null)
            {
                return instance;
            }
            return Create();
        }

        /// <summary>停止并销毁全局背景动画（幂等）</summary>
        public static void Hide()
        {
            if (instance == null)
            {
                return;
            }
            instance.Release();
            Destroy(instance.gameObject);
            instance = null;
        }

        private static GlobalBackgroundAnim Create()
        {
            VideoClip clip = Resources.Load<VideoClip>("LaunchBackground");
            if (clip == null)
            {
                Debug.LogWarning("[GlobalBackgroundAnim] 未找到原生启动背景视频（Packages/cn.rendu.loader/Resources/LaunchBackground.mp4），将使用纯色背景兜底。请确认已导入为 VideoClip");
            }

            GameObject go = new GameObject("GlobalBackgroundAnim");
            go.layer = LayerMask.NameToLayer("UI");
            DontDestroyOnLoad(go);

            // 独立 Canvas，置于所有 UI 层之下。
            // 注意：必须用 ScreenSpaceCamera（挂 UICamera）而非 ScreenSpaceOverlay，
            // 因为 Overlay 类型的 Canvas 永远渲染在 ScreenSpaceCamera 之上，会盖住登录/Lobby 等所有 UI。
            Canvas canvas = go.AddComponent<Canvas>();
            Camera uiCamera = GameObject.Find("UICamera")?.GetComponent<Camera>();
            if (uiCamera != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = uiCamera;
            }
            else
            {
                // 兜底：正常场景都有 UICamera；极少数没有时退化为 Overlay，仅作黑色兜底层
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            canvas.sortingOrder = -1000;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            RawImage rawImage = go.AddComponent<RawImage>();
            rawImage.raycastTarget = false;

            RectTransform rect = rawImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            GlobalBackgroundAnim anim = go.AddComponent<GlobalBackgroundAnim>();

            if (clip != null)
            {
                anim.renderTexture = CreateRenderTexture();
                rawImage.texture = anim.renderTexture;

                anim.videoPlayer = go.AddComponent<VideoPlayer>();
                anim.videoPlayer.clip = clip;
                anim.videoPlayer.isLooping = true;
                anim.videoPlayer.playOnAwake = false;
                anim.videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                anim.videoPlayer.targetTexture = anim.renderTexture;
                anim.videoPlayer.Play();
            }
            else
            {
                // 视频缺失：纯色黑底兜底，避免界面悬空
                rawImage.color = Color.black;
            }

            instance = anim;
            return anim;
        }

        /// <summary>按当前屏幕分辨率创建背景渲染纹理（不小于 1080p），保证与显示像素 1:1，避免被放大发虚</summary>
        private static RenderTexture CreateRenderTexture()
        {
            RenderTexture rt = new RenderTexture(Mathf.Max(RenderWidth, Screen.width),
                                                 Mathf.Max(RenderHeight, Screen.height),
                                                 0)
            {
                name = "LaunchBackgroundRT",
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
            };
            rt.Create();
            return rt;
        }

        /// <summary>屏幕分辨率变大（切全屏 / 换显示器 / 提高 Game 视图倍率）时重建渲染纹理，保持清晰</summary>
        private void Update()
        {
            if (renderTexture == null)
            {
                return;
            }

            if (Screen.width <= renderTexture.width && Screen.height <= renderTexture.height)
            {
                return;
            }

            RenderTexture newTexture = CreateRenderTexture();
            if (videoPlayer != null)
            {
                videoPlayer.targetTexture = newTexture;
            }

            RawImage image = GetComponent<RawImage>();
            if (image != null)
            {
                image.texture = newTexture;
            }

            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = newTexture;
        }

        private void Release()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.targetTexture = null;
                videoPlayer.clip = null;
            }
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
