using UnityEditor;
using UnityEngine;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Proto 文件创建菜单
    /// </summary>
    public static class ProtoCreateMenu
    {
        /// <summary>
        /// 快速创建 Proto 文件
        /// </summary>
        [MenuItem("Assets/Create/Rendu/Protocol", false, 10)]
        static void GenerateProtocol()
        {
            ProjectWindowUtil.CreateAssetWithContent(
                "Temp.proto",
                "syntax = \"proto3\";\n\npackage RD;\n\n// *************************\n// ******* XXXX *********\n// *************************\n\n\n"
            );
        }
    }
}
