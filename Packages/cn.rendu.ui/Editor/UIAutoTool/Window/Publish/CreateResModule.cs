#if UNITY_EDITOR

using RD.Inspector;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [HideReferenceObjectPicker]
    [HideLabel]
    public class CreateResModule : BaseCreateModule
    {
        [InfoBox("UI所在的分包中的包名[cn.rendu.{0}]，可以没有 没有时生成在Unity/Assets/下")]
        [LabelText("新增模块指定分包名")]
        public string PackageName;

        private StringPrefs m_LastPackageName = new("UICreateResModule_LastPackageName");

        [InfoBox("UI所对应的模块名 不是分包名称 全分包建议名称唯一")]
        [LabelText("新增模块名称")]
        public string Name;

        [GUIColor(0, 1, 0)]
        [Button("创建", 30)]
        private void Create()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            Create(Name, PackageName);
        }

        public static void Create(string createName, string packageName = "")
        {
            if (string.IsNullOrEmpty(createName))
            {
                UnityTipsHelper.ShowError("请设定 名称");
                return;
            }

            packageName = packageName.Replace(" ", "").Replace("cn.rendu.", "");

            createName = NameUtility.ToFirstUpper(createName);

            string basePath = "";
            if (string.IsNullOrEmpty(packageName))
            {
                basePath = $"{UIConstHelper.Const.ProjectResPath}/{createName}";
            }
            else
            {
                var packageFullName = $"{UIConstHelper.Const.PackagesFormat}{packageName}";
                var packagePath     = $"{Application.dataPath}/../Packages/{packageFullName}";
                if (!System.IO.Directory.Exists(packagePath))
                {
                    UnityTipsHelper.ShowError($"目标包不存在 请检查包名是否正确: {packageFullName}");
                    return;
                }

                basePath = $"{string.Format(UIConstHelper.Const.ProjectPackageResPath, packageName)}/{createName}";

                // 包内编辑器资源根（与 ProjectPackageResPath 对应；保留为后续包级编辑器资源扩展点）
                _ = $"{string.Format(UIConstHelper.Const.ProjectPackageEditorPath, packageName)}";
            }

            var prefabsPath       = $"{basePath}/{UIConstHelper.Const.Prefabs}";
            var spritesPath       = $"{basePath}/{UIConstHelper.Const.Sprites}";
            var spritesAtlas1Path = $"{basePath}/{UIConstHelper.Const.Sprites}/{UIConstHelper.Const.SpritesAtlas1}";
            var atlasIgnorePath   = $"{basePath}/{UIConstHelper.Const.Sprites}/{UIConstHelper.Const.AtlasIgnore}";
            var atlasPath         = $"{basePath}/{UIConstHelper.Const.Atlas}";
            var sourcePath        = $"{basePath}/{UIConstHelper.Const.Source}";

            EditorHelper.CreateExistsDirectory(prefabsPath);
            EditorHelper.CreateExistsDirectory(spritesPath);
            EditorHelper.CreateExistsDirectory(spritesAtlas1Path);
            EditorHelper.CreateExistsDirectory(atlasIgnorePath);
            EditorHelper.CreateExistsDirectory(atlasPath);
            EditorHelper.CreateExistsDirectory(sourcePath);

            MenuItemUIPanelSource.CreateUIPanelByPath(sourcePath, createName);

            AutoTool.CloseWindowRefresh();
        }

        public override void Initialize()
        {
            PackageName = m_LastPackageName;
        }

        public override void OnDestroy()
        {
            m_LastPackageName.Value = PackageName;
        }
    }
}
#endif