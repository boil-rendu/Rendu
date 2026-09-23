using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Settings;
using UnityEditor;

namespace RD
{
    public static class HybridCLREditor
    {
        [MenuItem("HybridCLR/Rendu/CopyAotDlls")]
        public static void CopyAotDll()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string fromDir = Path.Combine(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());
            const string toDir = "Packages/cn.rendu.loader/Bundles/AotDlls";
            if (Directory.Exists(toDir))
            {
                Directory.Delete(toDir, true);
            }
            Directory.CreateDirectory(toDir);
            
            foreach (string aotDll in HybridCLRSettings.Instance.patchAOTAssemblies)
            {
                File.Copy(Path.Combine(fromDir, aotDll), Path.Combine(toDir, $"{aotDll}.bytes"), true);
            }
            UnityEngine.Debug.Log($"CopyAotDll Finish!");
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem("HybridCLR/Rendu/Init")]
        public static void Init()
        {
            const string fromFile = "Packages/cn.rendu.hybridclr/HybridCLR/AssemblyReferenceToLoader.asmref";
            
            const string toDir = "Assets/HybridCLR";
            if (Directory.Exists(toDir))
            {
                Directory.Delete(toDir, true);
            }
            Directory.CreateDirectory(toDir);
            
            File.Copy(fromFile, Path.Combine(toDir, "AssemblyReferenceToLoader.asmref"));
        }
    }
}