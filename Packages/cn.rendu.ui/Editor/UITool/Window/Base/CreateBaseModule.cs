#if UNITY_EDITOR
using System.IO;
using RD.Panel;
using RD.EditorWindows.Editor;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>
    /// Base 模块
    /// </summary>
    [HideReferenceObjectPicker]
    public class CreateBaseModule : BaseCreateModule
    {
        [LabelText("UI项目命名空间")]
        [ShowInInspector]
        [ReadOnly]
        private readonly string Namespace = UIConstHelper.Const.Namespace;

        [LabelText("UI项目资源路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private readonly string ProjectResPath = UIConstHelper.Const.ProjectResPath;

        [LabelText("组件·生成目录")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private readonly string UIGenerationPath = UIConstHelper.Const.ComponentGenPath;

        [LabelText("系统·生成目录")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private readonly string UICodeScriptsPath = UIConstHelper.Const.SystemGenPath;

        [HideLabel]
        [ShowInInspector]
        private CreateBaseEditorData UIBaseData = new CreateBaseEditorData();

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }

        private const string m_CommonPkg = "Common";

        [Button("初始化项目")]
        private void CreateProject()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            CopyUIPackage();
            EditorHelper.CreateExistsDirectory(ProjectResPath);
            CreateResModule.Create(m_CommonPkg); //默认初始化一个common模块
            CopyUIRoot();
            UITool.CloseWindowRefresh();
        }

        private void CopyUIRoot()
        {
            //如果statesync demo包已带UI的UIRoot 就不需要再生成这个root了
            if (File.Exists($"{Application.dataPath}/../Packages/cn.rendu.statesync/Assets/GameRes/UI/Common/Prefabs/UIRoot.prefab"))
            {
                return;
            }

            var loadRoot = (GameObject)AssetDatabase.LoadAssetAtPath($"{UIConstHelper.Const.RootPrefabPath}", typeof(Object));
            if (loadRoot == null)
            {
                Debug.LogError($"没有找到原始UIRoot {UIConstHelper.Const.RootPrefabPath}");
                return;
            }

            var newGameObj = Object.Instantiate(loadRoot);
            var commonPath = $"{ProjectResPath}/{m_CommonPkg}/{UIConstHelper.Const.Prefabs}/{UIConstHelper.Const.RootName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(newGameObj, commonPath);
            Object.DestroyImmediate(newGameObj);
        }

        private void CopyUIPackage()
        {
            var PackagesPath      = $"{Application.dataPath}/../Packages";
            var sourceFolder      = $"{PackagesPath}/cn.rendu.ui/.Template/cn.rendu.ui.sample";
            var targetPackagePath = $"{PackagesPath}/cn.rendu.ui.sample";
            CopyFolder.Copy(sourceFolder, targetPackagePath);
        }
    }
}
#endif