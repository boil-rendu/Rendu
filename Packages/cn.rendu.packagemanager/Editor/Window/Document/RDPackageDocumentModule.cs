using System;
using System.Collections.Generic;
using System.Linq;
using RD.Draw;
using RD.EditTool.Editor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    [ToolMenu("文档", -int.MaxValue)]
    public class RDPackageDocumentModule : BaseToolModule
    {
        [Button("RDPackageManager 包管理", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageManager()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/DzqwwwBJvixRvtkCI4dcatGcnAd");
        }

        [Button("一键生成包", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageCreate()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/O6Ukw7k3SiBHmek8MMwc5HdInev");
        }

        [Button("库", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageHub()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/DABBw0ZrJiSYcLknKZBczGHAnqg");
        }

        [Button("版本管理", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageVersion()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/SLlHwBMAMiAjVnkqiWAcKU5InIc");
        }

        [Button("更新", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageUpdate()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/LsSNwacK5iqBvDkTFAgcEh15nfc");
        }

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }
    }
}
