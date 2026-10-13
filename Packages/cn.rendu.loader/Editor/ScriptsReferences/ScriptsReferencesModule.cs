#if UNITY_EDITOR

using RD.Panel;
using RD.EditorWindows.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 脚本引用管理模块: 收集 cn.rendu.* 包的 packagegit.json 里的 ScriptsReferences,
    /// 写回四个 RD.* 程序集定义的 references 字段。
    /// 替代原 ScriptsReferencesWindow + ScriptsReferencesViewModel + ScriptsReferencesModel + ScriptsReferencesWindow.uxml。
    /// </summary>
    [UIAutoMenu("脚本引用", 700, Scope = "loader")]
    public class ScriptsReferencesModule : BaseToolModule
    {
        [LabelText("说明")]
        [DisplayAsString(false, 0, TextAlign.Left, false)]
        [ShowInInspector]
        public string InfoText { get; } =
            "脚本引用配置用于管理项目中各个程序集之间的引用关系。\n" +
            "通过正确的引用配置，可以确保代码的模块化和清晰的依赖关系。\n\n" +
            "操作流程:\n" +
            "1.「生成」会读 cn.rendu.* 包的 packagegit.json 收集 ScriptsReferences, 写回四个 RD.* 程序集定义\n" +
            "2.「更新」做一次更轻量的 Save + Refresh, 不重算引用表\n" +
            "3.「查看配置」在 Console 打印当前四个程序集定义里实际写的 references";

        [LabelText("目标程序集")]
        [ReadOnly]
        [ShowInInspector]
        public string TargetAssemblies { get; } = "Model / Hotfix / ModelView / HotfixView";

        [Button("生成脚本引用", 10)]
        [GUIColor(0.53f, 0.95f, 0.72f)]
        public void Generate()
        {
            if (!EditorUtility.DisplayDialog("确认操作", "确定要生成脚本引用吗？", "确定", "取消")) return;
            try
            {
                ScriptsReferencesCommand.UpdateScriptsReferences();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ScriptsReferences] 生成失败: {ex.Message}");
            }
        }

        [Button("更新脚本引用 (轻量)", 20)]
        public void UpdateReferences()
        {
            try
            {
                new UpdateScriptsReferencesCommand().Execute();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ScriptsReferences] 更新失败: {ex.Message}");
            }
        }

        [Button("查看当前配置 (打印到 Console)", 30)]
        public void ViewConfig()
        {
            try
            {
                var packageGits = ScriptsReferencesCommand.LoadPackageGits();
                Debug.Log($"[ScriptsReferences] 找到 {packageGits.Count} 个声明了 ScriptsReferences 的包:");
                foreach (var pg in packageGits)
                {
                    Debug.Log($"  - {pg.Name}:");
                    foreach (var kv in pg.ScriptsReferences)
                    {
                        Debug.Log($"      {kv.Key} -> [{string.Join(", ", kv.Value)}]");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ScriptsReferences] 查看配置失败: {ex.Message}");
            }
        }

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }
    }
}

#endif
