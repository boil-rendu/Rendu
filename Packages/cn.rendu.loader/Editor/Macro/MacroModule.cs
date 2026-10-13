#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using RD.Panel;
using RD.EditorWindows.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>
    /// 编译宏管理模块: 按平台切换、增删单个宏、用 [EnumMacro] 标注的 Flags 枚举宏预设。
    /// 旧版在 cn.rendu.ui (Rendu/UI 自动化工具 → 宏设置); 现按"编译符号属编译域"归到 loader,
    /// 在 LoaderTool 里以"宏定义"出现。底层读写 PlayerSettings.ScriptingDefineSymbols,
    /// 与 ui 旧版写同一份(loader 旧的 DefineWindow 只管 Standalone 一组, 已废弃被本模块替代)。
    /// </summary>
    [UIAutoMenu("宏定义", 100, Scope = "loader")]
    public class MacroModule : BaseToolModule
    {
        [Button("文档", 30)]
        [PropertyOrder(-99999)]
        public void OpenDocument()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/F1cDwqKSliIiBIkWMwmcfsj6nUe");
        }

        [GUIColor(0.4f, 0.8f, 1)]
        [BoxGroup("目标平台切换", centerLabel: true)]
        [OnValueChanged("OnBuildTargetGroupChange")]
        [ShowInInspector]
        [HideLabel]
        public static BuildTargetGroup BuildTargetGroup = BuildTargetGroup.Standalone;

        private void OnBuildTargetGroupChange()
        {
            SelfInitialize();
        }

        [BoxGroup("当前平台所有宏", centerLabel: true)]
        [HideLabel]
        public MacroCurrentData MacroStaticData;

        [BoxGroup("自定义宏数据", centerLabel: true)]
        [HideLabel]
        [ShowInInspector]
        [ListDrawerSettings(IsReadOnly = true)]
        [HideReferenceObjectPicker]
        private List<MacroDataBase> AllMacroData;

        [Button("更新自定义宏", 50), GUIColor(0.53f, 0.95f, 0.72f)]
        public void Refresh()
        {
            var allRemove = new List<string>();
            var allSelect = new List<string>();
            foreach (var macroData in AllMacroData)
            {
                allRemove.AddRange(macroData.GetAll());
                allSelect.AddRange(macroData.GetSelect());
            }

            MacroHelper.ChangeSymbols(allRemove, allSelect, BuildTargetGroup);

            // 写完编译符号后刷一下资源, 让下一次编译带上新宏; 不关任何窗口(loader 包的 Macro 不该去关 ui 包的 AutoTool)
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }

        public override void Initialize()
        {
            BuildTargetGroup = GetCurrentBuildPlatform();
            SelfInitialize();
        }

        private void SelfInitialize()
        {
            MacroStaticData = new MacroCurrentData(SelfInitialize);

            AllMacroData = new List<MacroDataBase>();

            // 扫描本程序集中带 [EnumMacro] 的枚举, 装成 MacroDataBase<T> 实例
            // (按包归位后, 框架与 UI / 资源宏都在 loader 自己的程序集里, 不必跨包反射)
            var allMacroEnum = new List<Type>();
            foreach (var type in typeof(MacroModule).Assembly.GetTypes())
            {
                if (type.IsDefined(typeof(EnumMacroAttribute), false))
                {
                    allMacroEnum.Add(type);
                }
            }

            Type macroDataBaseType = typeof(MacroDataBase<>);

            foreach (var macroEnum in allMacroEnum)
            {
                Type specificType = macroDataBaseType.MakeGenericType(macroEnum);
                var  instance     = (MacroDataBase)Activator.CreateInstance(specificType);
                instance.Initialize();
                AllMacroData.Add(instance);
            }
        }

        private static BuildTargetGroup GetCurrentBuildPlatform()
        {
            var buildTargetName = EditorUserBuildSettings.activeBuildTarget.ToString();
            buildTargetName = buildTargetName.ToLower();
            if (buildTargetName.IndexOf("standalone", StringComparison.Ordinal) >= 0)
            {
                return BuildTargetGroup.Standalone;
            }
            else if (buildTargetName.IndexOf("android", StringComparison.Ordinal) >= 0)
            {
                return BuildTargetGroup.Android;
            }
            else if (buildTargetName.IndexOf("iphone", StringComparison.Ordinal) >= 0)
            {
                return BuildTargetGroup.iOS;
            }
            else if (buildTargetName.IndexOf("ios", StringComparison.Ordinal) >= 0)
            {
                return BuildTargetGroup.iOS;
            }

            return BuildTargetGroup.Standalone;
        }

        public override void OnDestroy()
        {
        }
    }
}
#endif
