using System;
using System.Collections.Generic;
using System.Linq;
using RD.Draw;
using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    [ToolMenu("文档", -int.MaxValue)]
    public class RDPackageDocumentModule : BaseToolModule
    {
        [Button("RDPackageManager 包管理", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageManager()
        {
            OpenLocalDocSection("Packages/cn.rendu.packagemanager/Docs/ArchitectureDesign.md", "总览：三层分工");
        }

        [Button("一键生成包", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageCreate()
        {
            OpenLocalDocSection("Packages/cn.rendu.packagemanager/Docs/Tutorial.md", "4.1 包创建窗口");
        }

        [Button("库", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageHub()
        {
            OpenLocalDocSection("Packages/cn.rendu.packagemanager/Docs/Tutorial.md", "3.1 库（Hub）");
        }

        [Button("版本管理", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageVersion()
        {
            OpenLocalDocSection("Packages/cn.rendu.packagemanager/Docs/Tutorial.md", "3.2 版本管理");
        }

        [Button("更新", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void RDPackageUpdate()
        {
            OpenLocalDocSection("Packages/cn.rendu.packagemanager/Docs/Tutorial.md", "4.2 更新检查窗口");
        }

        /// <summary>
        /// 打开本包本地文档，并尽量跳转到对应小节标题（动态搜索标题行号，抗文档增删行）。
        /// </summary>
        internal static void OpenLocalDocSection(string relativePath, string heading)
        {
            var doc = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);
            if (doc == null)
            {
                Debug.LogWarning($"[RDPackageManager] 未找到本地文档: {relativePath}");
                return;
            }
            var lines = doc.text.Split('\n');
            var line  = -1;
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(heading))
                {
                    line = i + 1; // OpenAsset 行号从 1 开始
                    break;
                }
            }
            if (line > 0)
                AssetDatabase.OpenAsset(doc, line);
            else
                AssetDatabase.OpenAsset(doc);
        }

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }
    }
}
