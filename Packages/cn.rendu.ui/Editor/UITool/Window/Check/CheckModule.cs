using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RD.Panel;
using RD.Panel.Editor;
using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// UITool 的「检查」模块：用开关在「脚本检查」与「预制体检查」两套面板间切换
    /// 勾选状态通过 EnumPrefs 落地，Initialize 读取、OnDestroy 回写
    /// </summary>
    [UIAutoMenu("检查", int.MaxValue, Scope = "ui")]
    public class CheckModule : BaseToolModule
    {
        /// <summary>打开在线检查说明文档</summary>
        [Button("文档", 30)]
        [PropertyOrder(-99999)]
        public void OpenDocument()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/GOBCwA1rdi7nBhkaBh8cLCjRnah");
        }

        /// <summary>当前检查类型（脚本 / 预制体）</summary>
        [BoxGroup("检查类型", centerLabel: true)]
        [HideLabel]
        [EnumToggleButtons]
        public ECheckViewType CheckViewType = ECheckViewType.Script;

        /// <summary>脚本检查面板（仅「脚本」类型下显示）</summary>
        [HideLabel]
        [ShowIf("CheckViewType", ECheckViewType.Script)]
        public CheckScriptModule CheckScript = new();

        /// <summary>预制体检查面板（仅「预制」类型下显示）</summary>
        [HideLabel]
        [ShowIf("CheckViewType", ECheckViewType.Prefab)]
        public CheckPrefabModule CheckPrefab = new();

        /// <summary>检查类型的选择记忆（EditorPrefs 持久化）</summary>
        [HideLabel]
        private EditorPrefsValue<ECheckViewType> UICheckViewPrefs = new("UITool_EUICheckViewType");

        /// <summary>模块加载时恢复上次选择的检查类型</summary>
        public override void Initialize()
        {
            CheckViewType = UICheckViewPrefs.Value;
        }

        /// <summary>模块销毁时保存当前检查类型</summary>
        public override void OnDestroy()
        {
            UICheckViewPrefs.Value = CheckViewType;
        }
    }
}