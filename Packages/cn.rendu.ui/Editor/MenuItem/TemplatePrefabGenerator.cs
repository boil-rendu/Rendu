using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using RD.Client;

namespace RD.UI.Editor
{
    /// <summary>
    /// 一键生成 CommonMenuItem 依赖的右键菜单模板 Prefab，风格对齐 LoginPanel。
    /// 菜单：Tools/UI/生成模板 Prefab（重复执行会覆盖重建）
    ///
    /// LoginPanel 风格规范：
    /// - 拦截层用 UIBlock（Graphic + ICanvasRaycastFilter），不用 Image
    /// - 图形底用 Image.Sliced + 内置圆角 UISprite
    /// - 主按钮金色 rgb(0.91,0.77,0.46) 300x80，文字 sz=34 深棕 rgb(0.12,0.1,0.08)
    /// - 正文 sz=36 白 0.95，内置字体，legacy UGUI Text
    /// </summary>
    internal static class TemplatePrefabGenerator
    {
        private const string OutputDir = "Packages/cn.rendu.ui/Editor/TemplatePrefabs";

        /// <summary>LoginBtn 金色</summary>
        private static readonly Color Gold     = new Color(0.91f, 0.77f, 0.46f, 1f);

        /// <summary>LoginBtn 文字深棕</summary>
        private static readonly Color DarkBrown = new Color(0.12f, 0.10f, 0.08f, 1f);

        /// <summary>输入框底 深藏青半透明</summary>
        private static readonly Color DeepNavy  = new Color(0.07f, 0.10f, 0.16f, 0.78f);

        /// <summary>正文白 0.95</summary>
        private static readonly Color TextWhite = new Color(1f, 1f, 1f, 0.95f);

        [MenuItem("Tools/UI/生成模板 Prefab")]
        private static void GenerateAll()
        {
            Debug.Log($"[TemplatePrefabGenerator] 开始生成模板 → {OutputDir}");

            var ok     = 0;
            var failed = 0;
            CreateSafe("UIBlockBG",        CreateBlockBG,        ref ok, ref failed);
            CreateSafe("UIText_NoRaycast", CreateTextNoRaycast,  ref ok, ref failed);
            CreateSafe("UIText (TMP)",     CreateTextTMP,        ref ok, ref failed);
            CreateSafe("UIImage_NoRaycast",CreateImageNoRaycast, ref ok, ref failed);
            CreateSafe("UIButton",         CreateButton,         ref ok, ref failed);
            CreateSafe("UIButton_NoText",  CreateButtonNoText,   ref ok, ref failed);
            CreateSafe("UIInputField",     CreateInputField,     ref ok, ref failed);
            CreateSafe("UIToggle",         CreateToggle,         ref ok, ref failed);
            CreateSafe("UISlider",         CreateSlider,         ref ok, ref failed);
            CreateSafe("UIScrollView",     CreateScrollView,     ref ok, ref failed);
            CreateSafe("UIEmpty",          CreateEmpty,          ref ok, ref failed);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (failed == 0)
                Debug.Log($"[TemplatePrefabGenerator] 模板生成完毕：{ok}/{ok} 成功 → {OutputDir}");
            else
                Debug.LogError($"[TemplatePrefabGenerator] 模板生成结束：{ok} 成功 / {failed} 失败 → {OutputDir}");
        }

        private static void CreateSafe(string name, System.Action creator, ref int ok, ref int failed)
        {
            try
            {
                creator();
                ok++;
                Debug.Log($"[TemplatePrefabGenerator] ✓ {name}");
            }
            catch (System.Exception e)
            {
                failed++;
                Debug.LogError($"[TemplatePrefabGenerator] ✗ {name} 失败：{e}");
            }
        }

        private static void Save(GameObject root, string name)
        {
            root.name = name;
            PrefabUtility.SaveAsPrefabAsset(root, $"{OutputDir}/{name}.prefab");
            Object.DestroyImmediate(root);
        }

        /// <summary>创建自带 RectTransform 的 UI 对象（new GameObject() 只有 Transform，取 RectTransform 会 null）</summary>
        private static GameObject NewUIObject(string name = "GameObject")
        {
            return new GameObject(name, typeof(RectTransform));
        }

        /// <summary>取 RectTransform（对象未挂 Graphic 时也安全）</summary>
        private static RectTransform GetRect(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            if (rect == null)
                rect = go.AddComponent<RectTransform>();
            return rect;
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin            = Vector2.zero;
            rect.anchorMax            = Vector2.one;
            rect.offsetMin            = Vector2.zero;
            rect.offsetMax            = Vector2.zero;
        }

        private static void SetCenter(RectTransform rect, Vector2 size)
        {
            rect.anchorMin            = Vector2.one * 0.5f;
            rect.anchorMax            = Vector2.one * 0.5f;
            rect.pivot                = Vector2.one * 0.5f;
            rect.anchoredPosition     = Vector2.zero;
            rect.sizeDelta            = size;
        }

        /// <summary>Image 设为登录风格：Sliced + 内置圆角 UISprite</summary>
        private static Image SetupRoundedImage(GameObject go, Color color)
        {
            var image      = go.AddComponent<Image>();
            image.sprite   = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (image.sprite == null)
                Debug.LogWarning("[TemplatePrefabGenerator] 未取到内置 UISprite，圆角样式将缺失");
            image.type     = Image.Type.Sliced;
            image.color    = color;
            return image;
        }

        private static Text SetupText(GameObject go, string content, int size, Color color, TextAnchor align)
        {
            var text         = go.AddComponent<Text>();
            text.text        = content;
            text.fontSize    = size;
            text.color       = color;
            text.alignment   = align;
            return text;
        }

        /// <summary>UIBlockBG：全屏点击拦截层（与 LoginPanel 一致，用 UIBlock 而非 Image）</summary>
        private static void CreateBlockBG()
        {
            var go = NewUIObject();
            go.AddComponent<UIBlock>();
            SetStretch(GetRect(go));
            Save(go, "UIBlockBG");
        }

        /// <summary>UIText_NoRaycast：标准文本（sz=36 白 0.95，与登录输入文字一致，不拦截点击）</summary>
        private static void CreateTextNoRaycast()
        {
            var go    = NewUIObject();
            var text  = SetupText(go, "Text", 36, TextWhite, TextAnchor.MiddleLeft);
            text.raycastTarget = false;
            SetCenter(text.rectTransform, new Vector2(600, 100));
            Save(go, "UIText_NoRaycast");
        }

        /// <summary>UIText (TMP)：TextMeshPro 文本（风格参数同 UIText_NoRaycast）</summary>
        private static void CreateTextTMP()
        {
            var go    = NewUIObject();
            var text  = go.AddComponent<TMPro.TextMeshProUGUI>();
            text.text             = "Text";
            text.fontSize         = 36;
            text.color            = TextWhite;
            text.alignment        = TMPro.TextAlignmentOptions.MidlineLeft;
            text.raycastTarget    = false;
            SetCenter(text.rectTransform, new Vector2(600, 100));
            Save(go, "UIText (TMP)");
        }

        /// <summary>UIImage_NoRaycast：圆角图片底（Sliced UISprite，尺寸同登录输入框 600x100，不拦截点击）</summary>
        private static void CreateImageNoRaycast()
        {
            var go    = NewUIObject();
            var image = SetupRoundedImage(go, Color.white);
            image.raycastTarget = false;
            SetCenter(image.rectTransform, new Vector2(600, 100));
            Save(go, "UIImage_NoRaycast");
        }

        /// <summary>UIButton：登录主按钮风格（金色 300x80 + 深棕 sz=34 文字 + UIClickEffect + UITaskEventBindClick）</summary>
        private static void CreateButton()
        {
            var go    = NewUIObject();
            var image = SetupRoundedImage(go, Gold);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            go.AddComponent<UIClickEffect>();
            go.AddComponent<UITaskEventBindClick>();
            SetCenter(image.rectTransform, new Vector2(300, 80));

            var textGo = NewUIObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = SetupText(textGo, "Button", 34, DarkBrown, TextAnchor.MiddleCenter);
            text.raycastTarget = true; // 与 LoginBtn 的 Text 一致
            SetStretch(text.rectTransform);
            Save(go, "UIButton");
        }

        /// <summary>UIButton_NoText：登录主按钮风格，无子文本</summary>
        private static void CreateButtonNoText()
        {
            var go    = NewUIObject();
            var image = SetupRoundedImage(go, Gold);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            go.AddComponent<UIClickEffect>();
            go.AddComponent<UITaskEventBindClick>();
            SetCenter(image.rectTransform, new Vector2(300, 80));
            Save(go, "UIButton_NoText");
        }

        /// <summary>UIInputField：登录账号/密码输入框同款（深藏青圆角底 600x100 + InputField + 占位符）。
        /// 注意：InputField 继承 Selectable，不能再叠加 Button（LoginPanel 实际结构无 Button）。</summary>
        private static void CreateInputField()
        {
            var go    = NewUIObject();
            var image = SetupRoundedImage(go, DeepNavy);
            SetCenter(image.rectTransform, new Vector2(600, 100));

            // 先建文本子物体，再挂 InputField 并回填引用
            var placeholderGo = NewUIObject("Placeholder");
            placeholderGo.transform.SetParent(go.transform, false);
            var placeholder = SetupText(placeholderGo, "请输入内容", 36,
                new Color(1f, 1f, 1f, 0.35f), TextAnchor.MiddleLeft);
            placeholder.raycastTarget    = false;
            placeholder.supportRichText  = false;
            SetStretch(placeholder.rectTransform);
            placeholder.rectTransform.offsetMin = new Vector2(40, -13);
            placeholder.rectTransform.offsetMax = new Vector2(-40, -13);

            var textGo = NewUIObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = SetupText(textGo, "", 36, TextWhite, TextAnchor.MiddleLeft);
            text.raycastTarget    = false;
            text.supportRichText  = false;
            SetStretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(40, -13);
            text.rectTransform.offsetMax = new Vector2(-40, -13);

            var input          = go.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder   = placeholder;
            input.targetGraphic = image;
            Save(go, "UIInputField");
        }

        /// <summary>UIToggle：登录风格开关（圆角底 + 勾选框 + 标签）</summary>
        private static void CreateToggle()
        {
            var go = NewUIObject();
            var image = SetupRoundedImage(go, DeepNavy);
            SetCenter(image.rectTransform, new Vector2(600, 100));

            var checkGo = NewUIObject("Checkmark");
            checkGo.transform.SetParent(go.transform, false);
            var checkImage = SetupRoundedImage(checkGo, Gold);
            checkImage.raycastTarget = false;
            var checkRect = checkImage.rectTransform;
            checkRect.anchorMin        = new Vector2(0f, 0.5f);
            checkRect.anchorMax        = new Vector2(0f, 0.5f);
            checkRect.pivot            = new Vector2(0.5f, 0.5f);
            checkRect.anchoredPosition = new Vector2(50, 0);
            checkRect.sizeDelta        = new Vector2(60, 60);

            var labelGo = NewUIObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var label = SetupText(labelGo, "Toggle", 36, TextWhite, TextAnchor.MiddleLeft);
            label.raycastTarget = false;
            SetStretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(100, -13);
            label.rectTransform.offsetMax = new Vector2(-40, -13);
            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = image;
            toggle.graphic       = checkImage;
            Save(go, "UIToggle");
        }

        /// <summary>UISlider：登录风格滑条（轨道深藏青 + 金色填充 + 深藏青把手）</summary>
        private static void CreateSlider()
        {
            var go = NewUIObject();
            SetCenter(GetRect(go), new Vector2(600, 100));

            var bgGo = NewUIObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bg = SetupRoundedImage(bgGo, DeepNavy);
            bg.raycastTarget = false;
            var bgRect = bg.rectTransform;
            bgRect.anchorMin        = new Vector2(0f, 0.5f);
            bgRect.anchorMax        = new Vector2(0f, 0.5f);
            bgRect.pivot            = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta        = new Vector2(600, 20);

            var fillGo = NewUIObject("Fill Area");
            fillGo.transform.SetParent(go.transform, false);
            var fillArea = GetRect(fillGo);
            fillArea.anchorMin     = new Vector2(0f, 0.5f);
            fillArea.anchorMax     = new Vector2(1f, 0.5f);
            fillArea.pivot         = new Vector2(0.5f, 0.5f);
            fillArea.anchoredPosition = Vector2.zero;
            fillArea.sizeDelta     = new Vector2(-20, 20);

            var fillChildGo = NewUIObject("Fill");
            fillChildGo.transform.SetParent(fillGo.transform, false);
            var fill = SetupRoundedImage(fillChildGo, Gold);
            fill.raycastTarget = false;
            var fillRect = fill.rectTransform;
            fillRect.anchorMin        = Vector2.zero;
            fillRect.anchorMax        = new Vector2(0f, 1f);
            fillRect.pivot            = new Vector2(0.5f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta        = new Vector2(20, 20);

            var handleGo = NewUIObject("Handle Slide Area");
            handleGo.transform.SetParent(go.transform, false);
            var handleArea = GetRect(handleGo);
            handleArea.anchorMin        = Vector2.zero;
            handleArea.anchorMax        = Vector2.one;
            handleArea.anchoredPosition = Vector2.zero;
            handleArea.sizeDelta        = new Vector2(20, 0);

            var handleChildGo = NewUIObject("Handle");
            handleChildGo.transform.SetParent(handleGo.transform, false);
            var handle = SetupRoundedImage(handleChildGo, Color.white);
            var handleRect = handle.rectTransform;
            handleRect.sizeDelta = new Vector2(40, 40);

            var slider = go.AddComponent<Slider>();
            slider.fillRect       = fillRect;
            slider.handleRect     = handleRect;
            slider.targetGraphic  = handle;
            slider.direction      = Slider.Direction.LeftToRight;
            Save(go, "UISlider");
        }

        /// <summary>UIScrollView：登录风格滚动视图（深藏青底 + 白色滚动条）</summary>
        private static void CreateScrollView()
        {
            var go = NewUIObject();
            SetStretch(GetRect(go));
            GetRect(go).sizeDelta = new Vector2(600, 800);

            var viewportGo = NewUIObject("Viewport");
            viewportGo.transform.SetParent(go.transform, false);
            viewportGo.AddComponent<RectMask2D>();
            var viewport = SetupRoundedImage(viewportGo, new Color(1f, 1f, 1f, 0f));
            viewport.raycastTarget = false;
            SetStretch(viewport.rectTransform);

            var contentGo = NewUIObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var content = GetRect(contentGo);
            content.anchorMin     = new Vector2(0f, 1f);
            content.anchorMax     = new Vector2(1f, 1f);
            content.pivot         = new Vector2(0.5f, 1f);
            content.sizeDelta     = new Vector2(0, 800);

            var scrollbarGo = NewUIObject("Scrollbar Vertical");
            scrollbarGo.transform.SetParent(go.transform, false);
            var sbBg = SetupRoundedImage(scrollbarGo, DeepNavy);
            var sbRect = sbBg.rectTransform;
            sbRect.anchorMin        = new Vector2(1f, 0f);
            sbRect.anchorMax        = new Vector2(1f, 1f);
            sbRect.pivot            = new Vector2(1f, 0.5f);
            sbRect.anchoredPosition = Vector2.zero;
            sbRect.sizeDelta        = new Vector2(20, 0);

            var slidingGo = NewUIObject("Sliding Area");
            slidingGo.transform.SetParent(scrollbarGo.transform, false);
            SetStretch(GetRect(slidingGo));

            var handleGo = NewUIObject("Handle");
            handleGo.transform.SetParent(slidingGo.transform, false);
            var handle = SetupRoundedImage(handleGo, Color.white);
            SetStretch(handle.rectTransform);

            var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.targetGraphic = handle;
            scrollbar.handleRect    = handle.rectTransform;
            scrollbar.direction     = Scrollbar.Direction.BottomToTop;

            var scroll = go.AddComponent<ScrollRect>();
            scroll.content             = content;
            scroll.viewport            = viewport.rectTransform;
            scroll.verticalScrollbar   = scrollbar;
            scroll.horizontal          = false;
            scroll.vertical            = true;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            Save(go, "UIScrollView");
        }

        /// <summary>UIEmpty：空 RectTransform 容器</summary>
        private static void CreateEmpty()
        {
            var go = NewUIObject();
            SetStretch(GetRect(go));
            Save(go, "UIEmpty");
        }
    }
}
