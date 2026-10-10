using System;
using System.Linq;
using RD.EditorKit;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 常量配置
    /// 部类 可以根据需求自行扩展
    /// </summary>
    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
    public partial class UIConstAsset
    {
        /// <summary>
        /// 由 ProjectName + 后缀 拼出默认完整物体名
        /// 例: ProjectName="UI", suffix="ViewParent" => "UIViewParent"
        /// </summary>
        public string GetDefaultName(string suffix) => ProjectName + suffix;

        #region 项目配置

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("根目录名称")]
        public string ProjectName = "UI"; //不要修改

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("命名空间")]
        public string Namespace = "RD.Client"; //所有生成文件的命名空间

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("编辑器资源")]
        public string ProjectEditorPath = "Assets/Editor/UI"; //编辑器才会用到的资源

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("指定包编辑器资源")]
        public string ProjectPackageEditorPath = "Assets/../Packages/cn.rendu.{0}/Assets/Editor/UI"; //指定包的编辑器才会用到的资源

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("资源")]
        public string ProjectResPath = "Assets/GameRes/UI"; //玩家的预设/图片等资源存放的地方

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("指定包资源")]
        public string ProjectPackageResPath = "Assets/../Packages/cn.rendu.{0}/Assets/GameRes/UI"; //指定包的资源存放的地方

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("生成包名")]
        public string CreatePackageName = "sample"; //对应指定代码生成到指定的包中

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("生成默认包")]
        public string CreatePackagePath = "Assets/../Packages/cn.rendu.ui.sample";

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("组件·生成目录（自动覆盖）")]
        public string ComponentGenPath = "Assets/../Packages/cn.rendu.{0}/Scripts/ModelView/Client/UIGen"; //自动生成的代码会覆盖不可修改

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("组件·自定义目录")]
        public string ComponentPath = "Assets/../Packages/cn.rendu.{0}/Scripts/ModelView/Client/UIComponent"; //玩家可编写的核心代码部分 ET系统

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("系统·生成目录（自动覆盖）")]
        public string SystemGenPath = "Assets/../Packages/cn.rendu.{0}/Scripts/HotfixView/Client/UIGen"; //自动生成的代码会覆盖不可修改

        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("系统·自定义目录")]
        public string SystemPath = "Assets/../Packages/cn.rendu.{0}/Scripts/HotfixView/Client/UISystem"; //玩家可编写的核心代码部分 ET系统

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("框架所处位置")]
        public string FrameworkPath = "Assets/../Packages/cn.rendu.ui";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("代码模板")]
        public string TemplatePath => $"{FrameworkPath}/Editor/AutoTool/Template";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        [LabelText("Root预制")]
        public string RootPrefabPath => $"{FrameworkPath}/Editor/UIRootPrefab/UIRoot.prefab";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string Packages = "Packages";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string PackagesFormat = "cn.rendu.";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string RootName = "UIRoot";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string LayerRootName = "UILayerRoot";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string RootPkgName = "Common";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string PanelName = "Panel";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string CommonName = "Common";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string ViewName = "View";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string ParentName = "Parent";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string Prefabs = "Prefabs";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string PrefabsCN = "预制";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string Sprites = "Sprites";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string SpritesCN = "精灵";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string Atlas = "Atlas";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string AtlasCN = "图集";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string Source = "Source";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string SourceCN = "源文件";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string AtlasIgnore = "AtlasIgnore"; //图集忽略文件夹名称

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string SpritesAtlas1 = "Atlas1"; //图集1 不需要华丽的取名 每个包内的自定义图集就按顺序就好 自动创建的默认名称

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string AllViewParentName = "AllViewParent";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string AllPopupViewParentName = "AllPopupViewParent";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string PanelSourceName = "PanelSource";

        [ReadOnly]
        [BoxGroup("项目配置", CenterLabel = true)]
        public string ViewParentName = "ViewParent";

        #endregion

        #region 基础设置

        [BoxGroup("基础设置", CenterLabel = true)]
        [LabelText("源文件拆分后是否保留")]
        public bool SourceSplitReserve = false;

        [BoxGroup("基础设置", CenterLabel = true)]
        [LabelText("使用老的CDEInspector显示模式")]
        public bool DisplayOldCDEInspector = false;

        #endregion

        #region Root

        [BoxGroup("Root", CenterLabel = true)]
        [LabelText("低品质")]
        public bool IsLowQuality = false; //低品质 将会影响动画等逻辑 也可以根据这个参数自定义一些区别

        [BoxGroup("Root", CenterLabel = true)]
        [LabelText("UI宽度")]
        public int DesignScreenWidth = 1920;

        [BoxGroup("Root", CenterLabel = true)]
        [LabelText("UI高度")]
        public int DesignScreenHeight = 1080;

        [BoxGroup("Root", CenterLabel = true)]
        [LabelText("UI宽度(float)")]
        public float DesignScreenWidth_F = 1920f;

        [BoxGroup("Root", CenterLabel = true)]
        [LabelText("UI高度(float)")]
        public float DesignScreenHeight_F = 1080f;

        [BoxGroup("Root", CenterLabel = true)]
        [LabelText("根节点偏移")]
        public int RootPosOffset = 1000;

        [BoxGroup("Root", CenterLabel = true)]
        [LabelText("每个层级的距离")]
        public int LayerDistance = 0;

        #endregion

        #region 安全区调试编辑器下有效

        //这个需要自行实现黑底背景 这里只是提供参数
        [BoxGroup("安全区", CenterLabel = true)]
        [LabelText("在刘海屏机子时，是否打开黑边")]
        public bool OpenBlackBorder = false;

        [BoxGroup("安全区", CenterLabel = true)]
        [LabelText("启用2倍安全 则左右2边都会裁剪")]
        public bool DoubleSafe = false;

        [BoxGroup("安全区", CenterLabel = true)]
        [LabelText("安全区X")]
        public int SafeAreaX = 0;

        [BoxGroup("安全区", CenterLabel = true)]
        [LabelText("安全区Y")]
        public int SafeAreaY = 0;

        #endregion

        #region 动画

        [BoxGroup("动画", CenterLabel = true)]
        [LabelText("Dotween默认动画缩放")]
        public Vector3 DotweenAnimScale = new(0.8f, 0.8f, 0.8f);

        [BoxGroup("动画", CenterLabel = true)]
        [LabelText("全局禁用默认的Dotween动画")]
        public bool BanDotweenAnim = false;

        #endregion

        #region 编辑器工具

        #if UNITY_EDITOR
        public const string UIGraphicSelectorEnableKey = "UIGraphicSelector_Enable";

        [BoxGroup("编辑器工具", CenterLabel = true)]
        [LabelText("图层快选工具")]
        [OnValueChanged("UIGraphicSelectorEnableChanged")]
        [ShowInInspector]
        private bool m_UIGraphicSelectorEnable;

        public bool UIGraphicSelectorEnable => UnityEditor.EditorPrefs.GetBool(UIGraphicSelectorEnableKey, true);

        private void UIGraphicSelectorEnableInit()
        {
            m_UIGraphicSelectorEnable = UnityEditor.EditorPrefs.GetBool(UIGraphicSelectorEnableKey, true);
        }

        private void UIGraphicSelectorEnableChanged()
        {
            try
            {
                var editorAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "RD.UI.Editor");

                if (editorAssembly != null)
                {
                    var graphicSelectorType = editorAssembly.GetType("RD.UI.Editor.GraphicSelector");
                    if (graphicSelectorType != null)
                    {
                        var enableProperty = graphicSelectorType.GetProperty("Enable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        if (enableProperty != null && enableProperty.CanWrite)
                        {
                            enableProperty.SetValue(null, m_UIGraphicSelectorEnable);
                        }
                        else
                        {
                            Debug.LogError($"没有找到 RD.UI.Editor.GraphicSelector  Enable 属性");
                        }
                    }
                    else
                    {
                        Debug.LogError($"没有找到 RD.UI.Editor.GraphicSelector");
                    }
                }
                else
                {
                    Debug.LogError($"没有找到程序集 RD.UI.Editor");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法通过反射更新 GraphicSelector.Enable： {ex}");
            }
        }
        #endif

        #endregion

        #region AI

        [BoxGroup("AI", CenterLabel = true)]
        [LabelText("AI客户端名称")]
        public string DefaultOpenAIName = "droid";

        #endregion
    }
}