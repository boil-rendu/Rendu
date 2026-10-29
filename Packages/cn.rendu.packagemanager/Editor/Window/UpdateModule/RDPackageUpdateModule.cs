using System;
using System.IO;
using RD.Draw;
using RD.EditTool.Editor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    [ToolMenu("更新")]
    public class RDPackageUpdateModule : BaseToolModule
    {
        [Button("文档", 30, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        [PropertyOrder(-9999)]
        public void OpenDocument()
        {
            RDPackageDocumentModule.RDPackageUpdate();
        }
        
        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }
    }
}
