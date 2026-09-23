#if UNITY_EDITOR
using System.IO;
using RD.Inspector;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>
    /// UIBase 模块
    /// </summary>
    [HideReferenceObjectPicker]
    public class CreateUIBaseModule : BaseCreateModule
    {
        [LabelText("UI项目命名空间")]
        [ShowInInspector]
        [ReadOnly]
        private readonly string UINamespace = UIConstHelper.Const.UINamespace;

        [LabelText("UI项目资源路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private readonly string UIProjectResPath = UIConstHelper.Const.UIProjectResPath;

        [LabelText("UI项目脚本路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private readonly string UIGenerationPath = UIConstHelper.Const.UIETComponentGenPath;

        [LabelText("UI项目自定义脚本路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private readonly string UICodeScriptsPath = UIConstHelper.Const.UIETSystemGenPath;

        [HideLabel]
        [ShowInInspector]
        private CreateUIBaseEditorData UIBaseData = new CreateUIBaseEditorData();

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
            EditorHelper.CreateExistsDirectory(UIProjectResPath);
            UICreateResModule.Create(m_CommonPkg); //默认初始化一个common模块
            CopyUIRoot();
            UIAutoTool.CloseWindowRefresh();
        }

        private void CopyUIRoot()
        {
            //如果statesync demo包已带UI的UIRoot 就不需要再生成这个root了
            if (File.Exists($"{Application.dataPath}/../Packages/cn.rendu.statesync/Assets/GameRes/UI/Common/Prefabs/UIRoot.prefab"))
            {
                return;
            }

            var loadRoot = (GameObject)AssetDatabase.LoadAssetAtPath($"{UIConstHelper.Const.UIRootPrefabPath}", typeof(Object));
            if (loadRoot == null)
            {
                Debug.LogError($"没有找到原始UIRoot {UIConstHelper.Const.UIRootPrefabPath}");
                return;
            }

            var newGameObj = Object.Instantiate(loadRoot);
            var commonPath = $"{UIProjectResPath}/{m_CommonPkg}/{UIConstHelper.Const.UIPrefabs}/{UIConstHelper.Const.UIRootName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(newGameObj, commonPath);
            Object.DestroyImmediate(newGameObj);
        }

        private void CopyUIPackage()
        {
            var PackagesPath      = $"{Application.dataPath}/../Packages";
            var sourceFolder      = $"{PackagesPath}/cn.rendu.ui/.Template/cn.rendu.uiproject";
            var targetPackagePath = $"{PackagesPath}/cn.rendu.uiproject";
            CopyFolder.Copy(sourceFolder, targetPackagePath);
        }
    }
}
#endif