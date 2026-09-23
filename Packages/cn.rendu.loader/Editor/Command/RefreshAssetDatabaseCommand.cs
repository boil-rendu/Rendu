using RD.Init.Editor;
using UnityEditor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 刷新资源数据库命令
    /// </summary>
    public class RefreshAssetDatabaseCommand : CommandBase
    {
        public override void Execute()
        {
            AssetDatabase.Refresh();
        }
    }
}
