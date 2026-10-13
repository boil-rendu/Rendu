using RD.Init.Editor;
using RD.EditTool.Editor;
using UnityEditor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 生成程序集引用命令
    /// </summary>
    public class GenerateAssemblyReferenceCommand : CommandBase
    {
        private readonly CodeMode _codeMode;

        public GenerateAssemblyReferenceCommand(CodeMode codeMode)
        {
            _codeMode = codeMode;
        }

        public override void Execute()
        {
            UnityEngine.Debug.Log("[AssemblyRef] ========== 开始生成 ==========");
            CodeModeChangeHelper.ChangeToCodeMode(_codeMode.ToString());
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[AssemblyRef] ========== 生成完成 ==========");
        }
    }
}
