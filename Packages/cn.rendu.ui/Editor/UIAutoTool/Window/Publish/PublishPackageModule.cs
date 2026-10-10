#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RD.EditorKit;
using RD.EditorKit.Editor;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>发布面板上方切换的展示类型</summary>
    public enum EUIPublishPackageData
    {
        /// <summary>组件（UIBindCDETable）</summary>
        [LabelText("组件")]
        CDETable,

        /// <summary>精灵（贴图导入器）</summary>
        [LabelText("精灵")]
        Sprites,

        /// <summary>图集</summary>
        [LabelText("图集")]
        Atlas,
    }

    /// <summary>由 PublishModule 扫描出的单个模块数据，作为菜单项的 UserData 传给 PublishPackageModule</summary>
    public class UIPublishPackageModuleData
    {
        /// <summary>所属发布模块</summary>
        public PublishModule PublishModule;
        /// <summary>在工具树上的菜单路径</summary>
        public string PublishPath;
        /// <summary>资源根路径</summary>
        public string ResPath;
        /// <summary>模块名</summary>
        public string PkgName;
    }

    /// <summary>
    /// 单个模块的发布面板：展示组件 / 精灵 / 图集三类资源，
    /// 支持发布当前模块（顺带生成代码与图集）、重建目录结构、创建并强制更新图集
    /// </summary>
    public partial class PublishPackageModule : BaseToolModule
    {
        private PublishModule m_UIPublishModule;

        private AtlasModule m_UIAtlasModule;

        /// <summary>面板底部留白</summary>
        [OnInspectorGUI]
        [PropertyOrder(999)]
        private void BaseSpace1()
        {
            GUILayout.Space(20);
        }

        /// <summary>模块名（只读）</summary>
        [PropertyOrder(1000)]
        [BoxGroup("基础信息", centerLabel: true)]
        [LabelText("模块名")]
        [ReadOnly]
        public string PkgName;

        /// <summary>模块路径（只读）</summary>
        [PropertyOrder(1000)]
        [BoxGroup("基础信息", centerLabel: true)]
        [FolderPath]
        [LabelText("模块路径")]
        [ReadOnly]
        public string PkgPath;

        private UIPublishPackageModuleData Data;

        /// <summary>当前展示的资源类型</summary>
        [PropertyOrder(1000)]
        [BoxGroup("基础信息", centerLabel: true)]
        [EnumToggleButtons]
        [HideLabel]
        public EUIPublishPackageData m_UIPublishPackageData = EUIPublishPackageData.CDETable;

        /// <summary>当前模块下所有组件（仅「组件」类型下显示）</summary>
        [PropertyOrder(1000)]
        [BoxGroup("基础信息", centerLabel: true)]
        [LabelText("当前模块所有组件")]
        [ReadOnly]
        [ShowInInspector]
        [OnStateUpdate("@$property.State.Expanded = true")]
        [ShowIf("m_UIPublishPackageData", EUIPublishPackageData.CDETable)]
        private List<UIBindCDETable> m_AllCDETable = new List<UIBindCDETable>();

        /// <summary>当前模块下所有精灵（仅「精灵」类型下显示）</summary>
        [PropertyOrder(1000)]
        [BoxGroup("基础信息", centerLabel: true)]
        [LabelText("当前模块所有精灵")]
        [ReadOnly]
        [ShowInInspector]
        [OnStateUpdate("@$property.State.Expanded = true")]
        [ShowIf("m_UIPublishPackageData", EUIPublishPackageData.Sprites)]
        private List<TextureImporter> m_AllTextureImporter = new List<TextureImporter>();

        //根据精灵文件夹创建对应的图集数量
        /// <summary>由精灵目录名归纳出的所有图集名（仅「图集」类型下显示）</summary>
        [LabelText("所有图集名称")]
        [PropertyOrder(1000)]
        [BoxGroup("基础信息", centerLabel: true)]
        [ReadOnly]
        [HideInInspector]
        [OnStateUpdate("@$property.State.Expanded = true")]
        [ShowIf("m_UIPublishPackageData", EUIPublishPackageData.Atlas)]
        public HashSet<string> m_AtlasName = new HashSet<string>();

        /// <summary>当前模块下所有图集资源（仅「图集」类型下显示）</summary>
        [PropertyOrder(1000)]
        [BoxGroup("基础信息", centerLabel: true)]
        [LabelText("当前模块所有图集")]
        [ReadOnly]
        [ShowInInspector]
        [OnStateUpdate("@$property.State.Expanded = true")]
        [ShowIf("m_UIPublishPackageData", EUIPublishPackageData.Atlas)]
        private List<SpriteAtlas> m_AllSpriteAtlas = new List<SpriteAtlas>();

        /// <summary>按钮入口：发布当前模块（带提示）</summary>
        [GUIColor(0.4f, 0.8f, 1)]
        [PropertyOrder(-999)]
        [Button("发布当前模块", 50)]
        private void PublishCurrent()
        {
            PublishCurrent(true);
        }

        /// <summary>
        /// 发布当前模块：逐个组件生成代码（有无包名分别走 Common / Packages），
        /// 再创建图集（不重置，需要重置请手动），showTips 控制结束后是否弹窗刷新
        /// </summary>
        public void PublishCurrent(bool showTips)
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            foreach (var current in m_AllCDETable)
            {
                if (string.IsNullOrEmpty(current.PackagesName))
                {
                    CreateModule.CreateCommon(current, false, false);
                }
                else
                {
                    CreateModule.CreatePackages(current, false, false, current.PackagesName);
                }
            }

            //创建图集 不重置 需要重置需要手动
            CreateOrResetAtlas();

            if (showTips)
            {
                UnityTipsHelper.CallBackOk($"UI当前模块 {PkgName} 发布完毕", AutoTool.CloseWindowRefresh);
            }
        }

        /// <summary>按模块名重建资源目录结构（Prefabs/Sprites/图集/Source 等）</summary>
        [Button("创建or重置 文件结构", 30)]
        [PropertyOrder(-998)]
        public void ResetDirectory()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            CreateResModule.Create(PkgName);
        }

        #region 初始化

        /// <summary>空构造（供序列化使用）</summary>
        public PublishPackageModule()
        {
        }

        /// <summary>构造：绑定发布模块与模块信息后立即刷新资源列表</summary>
        public PublishPackageModule(PublishModule publishModule, string resPath, string pkgName)
        {
            SetData(publishModule, resPath, pkgName);
            Refresh();
        }

        /// <summary>缓存发布模块与图集模块引用，记录模块名与模块路径</summary>
        private void SetData(PublishModule publishModule, string resPath, string pkgName)
        {
            m_UIPublishModule = publishModule;
            m_UIAtlasModule = ((AutoTool)publishModule.Window).AtlasModule;
            PkgName = pkgName;
            PkgPath = $"{resPath}/{pkgName}";
        }

        /// <summary>重新扫描组件 / 精灵 / 图集三类资源</summary>
        private void Refresh()
        {
            FindUIBindCDETableResources();
            FindUITextureResources();
            FindUISpriteAtlasResources();
        }

        /// <summary>模块加载：从 UserData 取出模块数据，刷新资源并把资源挂到工具树对应节点下</summary>
        public override void Initialize()
        {
            if (this.UserData is UIPublishPackageModuleData data)
            {
                Data = data;
                SetData(data.PublishModule, data.ResPath, data.PkgName);
                Refresh();
                AddAllAssetsAtPath();
                PartialInitialize();
            }
            else
            {
                Debug.LogError($"数据错误");
            }
        }

        partial void PartialInitialize();

        /// <summary>把图集 / 预制体 / 源文件 / 精灵四类资源挂到「发布」菜单对应子节点下</summary>
        private void AddAllAssetsAtPath()
        {
            var publishPath = Data.PublishPath;
            var resPath = Data.ResPath;
            var pkgName = Data.PkgName;

            //1 图集
            Tree.AddAllAssetsAtPath($"{publishPath}/{UIConstHelper.Const.AtlasCN}",
                $"{resPath}/{pkgName}/{UIConstHelper.Const.Atlas}", typeof(SpriteAtlas), true, false);

            //2 预制体
            Tree.AddAllAssetsAtPath($"{publishPath}/{UIConstHelper.Const.PrefabsCN}",
                $"{resPath}/{pkgName}/{UIConstHelper.Const.Prefabs}", typeof(UIBindCDETable), true, false);

            //3 源文件
            Tree.AddAllAssetsAtPath($"{publishPath}/{UIConstHelper.Const.SourceCN}",
                $"{resPath}/{pkgName}/{UIConstHelper.Const.Source}", typeof(UIBindCDETable), true, false);

            //4 精灵
            Tree.AddAllAssetImporterAtPath($"{publishPath}/{UIConstHelper.Const.SpritesCN}",
                $"{resPath}/{pkgName}/{UIConstHelper.Const.Sprites}", typeof(TextureImporter), true, false);
        }

        /// <summary>收集 Prefabs 目录下的组件；拆分式（IsSplitData）面板不纳入，只发布完整面板</summary>
        private void FindUIBindCDETableResources()
        {
            var strings = AssetDatabase.GetAllAssetPaths()
                                       .Where(x => x.StartsWith($"{PkgPath}/{UIConstHelper.Const.Prefabs}", StringComparison.InvariantCultureIgnoreCase));

            foreach (var path in strings)
            {
                var cdeTable = AssetDatabase.LoadAssetAtPath<UIBindCDETable>(path);
                if (cdeTable == null) continue;
                if (!cdeTable.IsSplitData)
                {
                    m_AllCDETable.Add(cdeTable);
                }
            }
        }

        /// <summary>
        /// 收集 Sprites 目录下的贴图并归纳图集名
        /// 直接放在 Sprites 根目录（不在 XX 子目录里）的贴图会报错并跳过
        /// </summary>
        private void FindUITextureResources()
        {
            var strings = AssetDatabase.GetAllAssetPaths().Where(x => x.StartsWith($"{PkgPath}/{UIConstHelper.Const.Sprites}", StringComparison.InvariantCultureIgnoreCase));

            m_AtlasName.Clear();

            foreach (var path in strings)
            {
                if (AssetImporter.GetAtPath(path) is TextureImporter texture)
                {
                    var atlasName = GetSpritesAtlasName(path);
                    if (string.IsNullOrEmpty(atlasName))
                    {
                        RD.Client.Logger.LogError(texture, $"此文件位置错误 {path}  必须在 {UIConstHelper.Const.Sprites}/XX 图集文件下 不可以直接在根目录");
                        continue;
                    }

                    m_AtlasName.Add(atlasName);
                    m_AllTextureImporter.Add(texture);
                }
            }
        }

        /// <summary>
        /// 沿目录向上递归，取 Sprites 下第一级子目录名作为图集名；
        /// 已到 Sprites 层则返回空串（表示位置非法）
        /// </summary>
        private string GetSpritesAtlasName(string path, string currentName = "")
        {
            if (!path.Replace("\\", "/").Contains($"{PkgPath}/{UIConstHelper.Const.Sprites}"))
            {
                return null;
            }

            var parentInfo = System.IO.Directory.GetParent(path);
            if (parentInfo == null)
            {
                return currentName;
            }

            if (parentInfo.Name == UIConstHelper.Const.Sprites)
            {
                return currentName;
            }

            return GetSpritesAtlasName(parentInfo.FullName, parentInfo.Name);
        }

        /// <summary>收集 Atlas 目录下的全部 SpriteAtlas 资源</summary>
        private void FindUISpriteAtlasResources()
        {
            var strings = AssetDatabase.GetAllAssetPaths().Where(x =>
                    x.StartsWith($"{PkgPath}/{UIConstHelper.Const.Atlas}",
                        StringComparison.InvariantCultureIgnoreCase));

            foreach (var path in strings)
            {
                var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                if (spriteAtlas != null)
                {
                    m_AllSpriteAtlas.Add(spriteAtlas);
                }
            }
        }

        #endregion

        #region 图集

        /// <summary>按钮入口：创建并强制重置全部图集，完成后刷新窗口</summary>
        [Button("创建and强制更新 图集", 30)]
        [PropertyOrder(-997)]
        private void CurrentAtlas()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            CreateOrResetAtlas(true);
            AutoTool.CloseWindowRefresh();
        }

        /// <summary>逐个图集名创建，reset 为 true 时顺带重置打包内容</summary>
        public void CreateOrResetAtlas(bool reset = false)
        {
            foreach (var atlasName in m_AtlasName)
            {
                CreateAtlas(atlasName);
                if (reset)
                {
                    ResetAtlas(atlasName);
                }
            }
        }

        /// <summary>重置指定图集（忽略 AtlasIgnore 目录与不存在的图集文件）</summary>
        public void ResetAtlas(string atlasName)
        {
            if (atlasName == UIConstHelper.Const.AtlasIgnore) return;

            var atlasFillName = $"{PkgPath}/{UIConstHelper.Const.Atlas}/Atlas_{PkgName}_{atlasName}.spriteatlasv2";

            if (!File.Exists(atlasFillName)) return;

            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFillName);
            var spriteAtlasAsset = SpriteAtlasAsset.Load(atlasFillName);

            ResetAtlas(spriteAtlas, spriteAtlasAsset, atlasName, true);
        }

        /// <summary>
        /// 创建指定图集：已存在则跳过；否则新建 SpriteAtlas 资源、写盘，
        /// 并按图集模块的规则重置导入设置
        /// </summary>
        public void CreateAtlas(string atlasName)
        {
            if (atlasName == UIConstHelper.Const.AtlasIgnore) return;

            var atlasFillName = $"{PkgPath}/{UIConstHelper.Const.Atlas}/Atlas_{PkgName}_{atlasName}.spriteatlasv2";

            if (File.Exists(atlasFillName)) return;

            var spriteAtlas = new SpriteAtlas();
            var spriteAtlasAsset = new SpriteAtlasAsset();
            ResetAtlas(spriteAtlas, spriteAtlasAsset, atlasName);

            if (!Directory.Exists(Path.GetDirectoryName(atlasFillName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(atlasFillName));
            }

            SpriteAtlasAsset.Save(spriteAtlasAsset, atlasFillName);
            AssetDatabase.Refresh();

            SpriteAtlasImporter sai = (SpriteAtlasImporter)AssetImporter.GetAtPath(atlasFillName);
            if (sai != null)
            {
                m_UIAtlasModule.ResetTargetBuildSetting(sai);
                AssetDatabase.WriteImportSettingsIfDirty(atlasFillName);
            }
        }

        /// <summary>重置图集内容：reset 为 true 时先清空已有打包项，再把对应精灵目录整个加进去</summary>
        private void ResetAtlas(SpriteAtlas spriteAtlas, SpriteAtlasAsset spriteAtlasAsset, string atlasName, bool reset = false)
        {
            if (spriteAtlas == null || spriteAtlasAsset == null) return;

            if (reset && spriteAtlas != null)
            {
                var packables = spriteAtlas.GetPackables();
                if (packables != null)
                {
                    spriteAtlasAsset.Remove(packables);
                    spriteAtlas.Remove(packables);
                }
            }

            var itemPath = $"{PkgPath}/{UIConstHelper.Const.Sprites}/{atlasName}";
            spriteAtlasAsset.Add(new[] { AssetDatabase.LoadMainAssetAtPath(itemPath) });
            spriteAtlas.Add(new[] { AssetDatabase.LoadMainAssetAtPath(itemPath) });
        }

        #endregion
    }
}
#endif