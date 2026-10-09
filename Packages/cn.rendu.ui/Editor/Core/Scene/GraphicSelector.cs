using System.Collections.Generic;
using System.Linq;
using RD;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// Scene 视图里的 UI 图层快选工具
    /// 在每个 Graphic 右上角画一个小方块：左键选中、Shift 追加选中、Ctrl 打开代码编辑器；
    /// 鼠标悬停时显示对象名（多个悬停则列成带序号的清单）
    /// 开关状态持久化到 EditorPrefs
    /// </summary>
    public class GraphicSelector : UnityEditor.Editor
    {
        /// <summary>进编辑器时先按 EditorPrefs 复位开关（不打印日志），并订阅 Scene 视图回调</summary>
        [InitializeOnLoadMethod]
        private static void Init()
        {
            UIGraphicSelectorEnableReset(false);
            SceneView.duringSceneGui += DuringSceneGUI;
        }

        /// <summary>进入运行模式时强制复位开关并打印状态</summary>
        [InitializeOnEnterPlayMode]
        private static void Start()
        {
            UIGraphicSelectorEnableReset();
        }

        private static readonly Vector3[] FourCorners = new Vector3[4];
        private static readonly Vector3 LabelOffset = new(-5, 10, 0);
        private static readonly List<(GameObject gameObject, Vector3 position)> HoveredObjects = new List<(GameObject, Vector3)>();
        private static GUIStyle _labelStyle;

        /// <summary>悬停标签样式（懒创建：半透明黑底 + 白字）</summary>
        private static GUIStyle LabelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle()
                    {
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        normal = { textColor = Color.white },
                        padding = new RectOffset(6, 6, 2, 2),
                        border = new RectOffset(2, 2, 2, 2)
                    };

                    var backgroundTexture = new Texture2D(1, 1);
                    backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.8f));
                    backgroundTexture.Apply();

                    _labelStyle.normal.background = backgroundTexture;
                }

                return _labelStyle;
            }
        }

        private const float Size = 5;
        private static readonly Vector3 Offset = new(Size, -Size, 0);
        private static bool m_Enable;

        /// <summary>工具开关（写入 EditorPrefs 持久化并打印状态）</summary>
        public static bool Enable
        {
            get => m_Enable;
            set
            {
                m_Enable = value;
                EditorPrefs.SetBool(UIConstAsset.UIGraphicSelectorEnableKey, value);
                UIGraphicSelectorEnableLog();
            }
        }

        private static void UIGraphicSelectorEnableLog()
        {
            Debug.Log($"<color=yellow>UI图层快选工具:</color> {(m_Enable ? "<color=green>启用</color>" : "<color=red>禁用</color>")}");
        }

        /// <summary>从 EditorPrefs 读取开关（默认启用）</summary>
        private static void UIGraphicSelectorEnableReset(bool log = true)
        {
            m_Enable = EditorPrefs.GetBool(UIConstAsset.UIGraphicSelectorEnableKey, true);
            if (log)
            {
                UIGraphicSelectorEnableLog();
            }
        }

        /// <summary>
        /// Scene 视图绘制：遍历所有可见 Graphic，在其右上角画手柄方块并收集悬停对象
        /// 会先裁掉鼠标不在相机视口内的情况；Prefab 编辑模式下只处理当前 Prefab 内的 Graphic；
        /// 跳过无 Canvas 或 CanvasGroup.alpha == 0 的对象
        /// </summary>
        private static void DuringSceneGUI(SceneView sceneView)
        {
            if (!m_Enable)
            {
                return;
            }

            HoveredObjects.Clear();

            var mousePosition = Event.current.mousePosition;
            var cam = sceneView.camera ?? Camera.current;
            if (cam == null)
            {
                return;
            }

            var screenPos = HandleUtility.GUIPointToScreenPixelCoordinate(mousePosition);
            
            // 检查屏幕坐标是否在相机视口范围内
            var pixelRect = cam.pixelRect;
            if (screenPos.x < pixelRect.xMin || screenPos.x > pixelRect.xMax ||
                screenPos.y < pixelRect.yMin || screenPos.y > pixelRect.yMax)
            {
                return;
            }

            // 使用ViewportPointToRay避免屏幕坐标超出视锥体的问题
            var viewportPoint = cam.ScreenToViewportPoint(screenPos);
            if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1)
            {
                return;
            }
            
            var ray = cam.ViewportPointToRay(viewportPoint);

            var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var allGraphics = currentPrefabStage != null ? currentPrefabStage.prefabContentsRoot.GetComponentsInChildren<Graphic>() : FindObjectsByType<Graphic>(FindObjectsSortMode.None);

            foreach (var g in allGraphics)
            {
                if (g.canvas == null)
                {
                    continue;
                }

                var canvasGroup = g.gameObject.GetComponentInParent<CanvasGroup>();
                if (canvasGroup && canvasGroup.alpha == 0)
                {
                    continue;
                }

                var scale = g.canvas.rootCanvas.transform.localScale.x;

                if (g.gameObject.activeInHierarchy)
                {
                    Handles.color = Color.green;
                    g.rectTransform.GetWorldCorners(FourCorners);

                    var buttonPosition = FourCorners[1] + Offset * scale;

                    if (Handles.Button(buttonPosition, Quaternion.identity, Size * scale, 0, Handles.RectangleHandleCap))
                    {
                        var current = Event.current;
                        if (current.control)
                        {
                            Unity.CodeEditor.CodeEditor.Editor.CurrentCodeEditor.OpenProject();
                        }
                        else if (current.shift)
                        {
                            Selection.objects = Selection.objects.Append(g.gameObject).ToArray();
                        }
                        else
                        {
                            Selection.activeObject = g.gameObject;
                        }
                    }

                    if (ray.origin.x < buttonPosition.x + Size * scale / 2
                     && ray.origin.x > buttonPosition.x - Size * scale / 2
                     && ray.origin.y < buttonPosition.y + Size * scale / 2
                     && ray.origin.y > buttonPosition.y - Size * scale / 2)
                    {
                        HoveredObjects.Add((g.gameObject, ray.origin + LabelOffset * scale));
                    }
                }
            }

            DisplayHoveredObjectLabels();
        }

        /// <summary>绘制悬停标签：单个直接显示名字，多个列成「[序号] 名字」的多行清单</summary>
        private static void DisplayHoveredObjectLabels()
        {
            if (HoveredObjects.Count == 0) return;

            if (HoveredObjects.Count == 1)
            {
                var (gameObject, position) = HoveredObjects[0];
                Handles.Label(position, gameObject.name, LabelStyle);
                return;
            }

            var labelBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < HoveredObjects.Count; i++)
            {
                var (gameObject, _) = HoveredObjects[i];
                labelBuilder.AppendLine($"[{i + 1}] {gameObject.name}");
            }

            var combinedLabel = labelBuilder.ToString().TrimEnd('\r', '\n');
            Handles.Label(HoveredObjects[0].position, combinedLabel, LabelStyle);
        }
    }
}