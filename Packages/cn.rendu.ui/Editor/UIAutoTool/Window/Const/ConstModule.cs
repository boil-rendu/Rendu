using RD.Panel;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    ///  常量
    /// </summary>
    [UIAutoMenu("常量", int.MaxValue)]
    public class ConstModule : BaseToolModule
    {
        [Button("文档", 30)]
        [PropertyOrder(-99999)]
        public void OpenDocument()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/KjMgw5woPi4iOFkH3wrcUIZtnKd");
        }

        [Button("重置", 30)]
        [PropertyOrder(-9999)]
        public void Reset()
        {
            UnityTipsHelper.CallBackOk("确定重置常量数据!!!", () => { UIConstAsset = UIConstHelper.ResetAsset(); });
        }

        [ShowInInspector]
        [HideLabel]
        private UIConstAsset UIConstAsset;

        public override void Initialize()
        {
            UIConstAsset = UIConstHelper.Const;
        }

        public override void OnDestroy()
        {
            Save();
        }

        // [Button("保存", 50)]
        // [PropertyOrder(-999)]
        // [GUIColor(0.5f, 0.95f, 0.7f)]
        private void Save()
        {
            if (UIConstAsset == null)
            {
                return;
            }

            JsonSerializationUtility.Save(UIConstAsset, UIConstHelper.UIConstAssetPath);
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(UIConstHelper.UIConstAssetPath);
            if (textAsset != null)
            {
                EditorUtility.SetDirty(textAsset);
            }

            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }
    }
}