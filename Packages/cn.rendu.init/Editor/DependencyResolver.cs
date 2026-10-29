using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace RD.Init.Editor
{
    [InitializeOnLoad]
    public class DependencyResolver
    {
        internal static void MoveToPackage(string package, string version)
        {
            string packageName = default;
            string moveFileName = default;
            #if UNITY_6000_0_OR_NEWER
            packageName = package;
            moveFileName = "MoveToPackages_6";
            #else 
            packageName =$"{package}@{version}";
            moveFileName = "MoveToPackages";
            #endif
            string dir = Path.Combine("Library/PackageCache", packageName);
            if (!Directory.Exists(dir))
            {
                return;
            }
        
            UnityEngine.Debug.Log($"move package: {packageName}");
            Process process = ProcessHelper.PowerShell($"-NoExit -ExecutionPolicy Bypass -File ./Packages/cn.rendu.init/Editor/Tools/{moveFileName}.ps1 {package} {version}", waitExit: true);
            process.WaitForExit();
        }
        
        static DependencyResolver()
        {
            Events.registeredPackages += OnPackagesRegistered;
        }

        // Invoked when the package manager completes registering new packages
        static void OnPackagesRegistered(PackageRegistrationEventArgs packageRegistrationInfo)
        {
            if (packageRegistrationInfo.added.Count == 0 && packageRegistrationInfo.changedFrom.Count == 0)
            {
                return;
            }
            
            UnityEngine.Debug.Log($"Packages Registered: {string.Join(" ", packageRegistrationInfo.added.Select(x=>x.name))}");
            
            // loop through all of the added packages and get their git
            // dependencies and add it to the list that contains all the
            // dependencies that need to be installed
            foreach (var package in packageRegistrationInfo.added)
            {
                if (!package.name.StartsWith("cn.rendu."))
                {
                    continue;
                }
                MoveToPackage(package.name, package.version);
            }
            
            foreach (var package in packageRegistrationInfo.changedFrom)
            {
                if (!package.name.StartsWith("cn.rendu."))
                {
                    continue;
                }
                MoveToPackage(package.name, package.version);
            }
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem("RD/Init/RepairDependencies",false, 12)]
        static void RepairDependencies()
        {
            foreach (var directory in Directory.GetDirectories("Library/PackageCache", "cn.rendu.*"))
            {
                string baseName = Path.GetFileName(directory);
                if (!baseName.StartsWith("cn.rendu."))
                {
                    continue;
                }
                
                string[] ss = baseName.Split("@");
                string packageName = ss[0];
                #if UNITY_6000_0_OR_NEWER
                string version = "";
                #else 
                string version = ss[1];
                #endif

                MoveToPackage(packageName, version);
            }
            
            AssetDatabase.Refresh();
            
            UnityEngine.Debug.Log($"repair package finish");
        }
    }
}
