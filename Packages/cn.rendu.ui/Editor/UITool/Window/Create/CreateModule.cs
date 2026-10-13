#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 代码创建入口：校验预制体合法性 → 采集绑定信息 → 按 UI 类型派发对应的模板生成器
    /// 同时提供模块名解析、公共组件收集等辅助能力
    /// </summary>
    public static class CreateModule
    {
        /// <summary>当前创建所使用的包名（供各模板生成器拼接输出路径）</summary>
        public static string CreatePackageName;

        /// <summary>设置包名后创建（包名会转小写）</summary>
        private static void CreateTargetPackages(UIBindCDETable cdeTable, bool refresh, bool tips, string createPackageName)
        {
            if (string.IsNullOrEmpty(createPackageName))
            {
                UnityTipsHelper.ShowErrorContext(cdeTable, $"必须指定目标创建模块名");
                return;
            }

            CreatePackageName = createPackageName.ToLower();
            Create(cdeTable, refresh, tips);
        }

        /// <summary>创建分包代码：未指定包名时从预制体路径推断，推断不到则报错返回</summary>
        public static void CreatePackages(UIBindCDETable cdeTable, bool refresh, bool tips, string createPackageName = "")
        {
            if (string.IsNullOrEmpty(createPackageName))
            {
                createPackageName = UIOperationHelper.GetRDPackagesName(cdeTable);
                if (string.IsNullOrEmpty(createPackageName))
                {
                    UnityTipsHelper.ShowErrorContext(cdeTable, $"没有找到模块名 请在预制体上使用 且 必须在指定的文件夹下才可使用 {UIConstHelper.Const.ProjectResPath}");
                    return;
                }
            }

            CreateTargetPackages(cdeTable, refresh, tips, createPackageName);
        }

        /// <summary>创建公共（Common）代码：固定使用 CreatePackageName 常量作为包名</summary>
        public static void CreateCommon(UIBindCDETable cdeTable, bool refresh, bool tips)
        {
            CreatePackageName = UIConstHelper.Const.CreatePackageName.ToLower();
            Create(cdeTable, refresh, tips);
        }

        /// <summary>
        /// 核心创建流程：
        /// 1. 必须操作的是 Project 面板下的预制体原件（预制体实例、非预制体一律拒绝）
        /// 2. 先做 AutoCheck 自检，再采集 CreateBaseData / CreateSystemData
        /// 3. 依次生成：手写 Component、ComponentGen、按类型生成 System（Panel/View/Common），
        ///    任一 System 生成失败即中断
        /// </summary>
        private static void Create(UIBindCDETable cdeTable, bool refresh, bool tips)
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            /*
            //留在这里看的 方便以后查API
            //当这个是个资源的时候 存在磁盘中
            var is0 = UnityEditor.EditorUtility.IsPersistent(cdeTable);
            //返回></para>如果对象是Prefab的一部分，则为True/返回>Functionl
            var is1= UnityEditor.PrefabUtility.IsPartOfAnyPrefab(cdeTable);
            //</para> . 0如果对象是不能编辑的Prefab的一部分，则为True
            var is2= UnityEditor.PrefabUtility.IsPartOfImmutablePrefab(cdeTable);
            //>如果给定对象是Model Prefab Asset或Model Prefab实例的一部分，则返回true
            var is3= UnityEditor.PrefabUtility.IsPartOfModelPrefab(cdeTable);
            //<摘要></para>如果给定对象是预制资源的一部分，则返回true
            var is4= UnityEditor.PrefabUtility.IsPartOfPrefabAsset(cdeTable);
            //摘要></para>如果给定对象是Prefab实例的一部分，则返回true
            var is5= UnityEditor.PrefabUtility.IsPartOfPrefabInstance(cdeTable);
            //a>如果给定对象是常规预制实例或预制资源的一部分，则返回true。</para>
            var is6= UnityEditor.PrefabUtility.IsPartOfRegularPrefab(cdeTable);
            //>如果给定对象是Prefab Variant资产或Prefab Variant实例的一部分，则返回true。
            var is7= UnityEditor.PrefabUtility.IsPartOfVariantPrefab(cdeTable);
            //>如果给定对象是Prefab实例的一部分，而不是资产的一部分，则返回true。</pa
            var is8= UnityEditor.PrefabUtility.IsPartOfNonAssetPrefabInstance(cdeTable);
            //>这个对象是Prefab的一部分，不能应用吗?
            var is9= UnityEditor.PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(cdeTable);
            */

            var checkInstance = PrefabUtility.IsPartOfPrefabInstance(cdeTable);
            if (checkInstance)
            {
                UnityTipsHelper.ShowErrorContext(cdeTable, $"不能对实体进行操作  必须进入预制体编辑!!!");
                return;
            }

            var checkAsset = PrefabUtility.IsPartOfPrefabAsset(cdeTable);
            if (!checkAsset)
            {
                UnityTipsHelper.ShowErrorContext(cdeTable,
                    $"1: 必须是预制体 2: 不能在Hierarchy面板中使用 必须在Project面板下的预制体原件才能使用使用 ");
                return;
            }

            if (!cdeTable.AutoCheck())
            {
                return;
            }

            //component信息
            var createBaseData = new CreateBaseData
            {
                AutoRefresh   = false,
                ShowTips      = tips,
                Namespace     = UIConstHelper.Const.Namespace,
                PkgName       = cdeTable.PkgName,
                ResName       = cdeTable.ResName,
                CodeType      = cdeTable.UICodeType,
                PanelLayer    = cdeTable.PanelLayer,
                Variables     = CreateVariables.Get(cdeTable),
                UIFriend      = CreateBind.GetFriend(cdeTable),
                UIBase        = CreateBind.GetBase(cdeTable),
                UIBind        = CreateBind.GetBind(cdeTable),
                UIUnBind      = CreateBind.GetUnBind(cdeTable),
                VirtualMethod = CreateMethod.Get(cdeTable),
                PanelViewEnum = CreatePanelViewEnum.Get(cdeTable),
            };

            //自定义的component
            new CreateComponentCode(out var resultComponent, UITool.Author, createBaseData);

            //生成的system
            new CreateSystemGenCode(out var resultSystemGen, UITool.Author, createBaseData);

            //system信息
            var createSystemData = new CreateSystemData
            {
                AutoRefresh = false,
                ShowTips    = tips,
                Namespace   = UIConstHelper.Const.Namespace,
                PkgName     = cdeTable.PkgName,
                ResName     = cdeTable.ResName,
                OverrideDic = CreateMethod.GetSystemEventOverrideDic(cdeTable),
            };

            //生成的component
            //自定义的system
            //目前看上去3个都一样 是特意设定的 以后可独立扩展
            if (cdeTable.UICodeType == EUICodeType.Panel)
            {
                new CreatePanelSystemCode(out var resultSystem, UITool.Author, createSystemData);
                if (!resultSystem) return;
                new CreateComponentGenCode(out var resultComponentBase, UITool.Author, createBaseData);
                if (!resultComponentBase) return;
            }
            else if (cdeTable.UICodeType == EUICodeType.View)
            {
                new CreateViewSystemCode(out var resultSystem, UITool.Author, createSystemData);
                if (!resultSystem) return;
                new CreateComponentGenCode(out var resultComponentBase, UITool.Author, createBaseData);
                if (!resultComponentBase) return;
            }
            else if (cdeTable.UICodeType == EUICodeType.Common)
            {
                new CreateCommonSystemCode(out var resultSystem, UITool.Author, createSystemData);
                if (!resultSystem) return;
                new CreateCommonComponentGenCode(out var resultComponentBase, UITool.Author, createBaseData);
                if (!resultComponentBase) return;
            }
            else
            {
                Debug.LogError($"是新增了 新类型嘛????? {cdeTable.UICodeType}");
            }

            if (refresh)
                AssetDatabase.Refresh();
        }

        /// <summary>生成 UIEvent 的 region 骨架；事件表为空时返回空串</summary>
        public static string GetRegionEvent(UIBindCDETable cdeTable)
        {
            if (cdeTable.EventTable == null || cdeTable.EventTable.EventDic == null)
                return "";
            return cdeTable.EventTable.EventDic.Count <= 0
                    ? ""
                    : "        #region UIEvent开始\r\n\r\n        #endregion UIEvent结束";
        }

        /// <summary>
        /// 由预制体路径解析出模块名与资源名并回填到 cdeTable
        /// 资源名非法时会重命名资源文件，解析不到预制体或模块名则返回 false
        /// </summary>
        public static bool InitVoName(UIBindCDETable cdeTable)
        {
            //var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(cdeTable);
            var path = MenuItemHelper.GetOnlyPrefabAssetsPath(cdeTable.gameObject.name);
            if (string.IsNullOrEmpty(path))
            {
                UnityTipsHelper.ShowErrorContext(cdeTable, $"没有找到预制 {cdeTable.gameObject.name}");
                return false;
            }

            var pkgName = GrdPkgName(path);
            if (string.IsNullOrEmpty(pkgName))
            {
                UnityTipsHelper.ShowErrorContext(cdeTable,
                    $"没有找到模块名 请在预制体上使用 且 必须在指定的文件夹下才可使用 {UIConstHelper.Const.ProjectResPath}");
                return false;
            }

            cdeTable.PkgName = Regex.Replace(pkgName, NameUtility.NameRegex, "");
            cdeTable.ResName = Regex.Replace(cdeTable.name, NameUtility.NameRegex, "");
            if (cdeTable.ResName != cdeTable.name)
            {
                cdeTable.name = cdeTable.ResName;
                AssetDatabase.RenameAsset(path, cdeTable.ResName);
            }

            return true;
        }

        //这个是模块名
        //不是ET的包名
        /// <summary>
        /// 从资源路径里取模块名：路径必须位于 ProjectResPath 下且要有下一级目录
        /// （注意取的是模块名，不是 ET 的包名）
        /// </summary>
        public static string GrdPkgName(string path)
        {
            var resPath = $"{UIConstHelper.Const.ProjectResPath}/";
            if (!path.Replace("\\", "/").Contains(resPath))
            {
                return null;
            }

            var relativePath = path.Substring(path.IndexOf(resPath, StringComparison.Ordinal) + resPath.Length);

            var splitPath = relativePath.Split('/');

            if (splitPath.Length <= 1)
            {
                return null;
            }

            return splitPath[0];
        }

        //收集所有公共组件
        /// <summary>重新收集该 CDE 下的全部公共组件：先递归收集子节点，再剔除自身 View</summary>
        public static void RefreshChildCdeTable(UIBindCDETable cdeTable)
        {
            cdeTable.AllChildCdeTable.Clear();
            AddCdeTable(ref cdeTable.AllChildCdeTable, cdeTable.transform);
            CheckAddCdeTable(ref cdeTable.AllChildCdeTable, cdeTable);
        }

        //如果自己是panel 则还需要额外检查 是不是把自己的view给收集进去了
        /// <summary>
        /// 倒序剔除误收集进来的自身 View（仅拆分式 Panel 需要）
        /// 判定依据：父节点名同时包含 ParentName 与子节点名
        /// </summary>
        public static void CheckAddCdeTable(ref List<UIBindCDETable> addCdeTable, UIBindCDETable cdeTable)
        {
            if (cdeTable.UICodeType != EUICodeType.Panel && !cdeTable.IsSplitData)
                return;

            for (var i = addCdeTable.Count - 1; i >= 0; i--)
            {
                var targetTable = addCdeTable[i];
                var parent      = (RectTransform)targetTable.gameObject.transform.parent;
                var parentName  = parent.name;

                //这里使用的是强判断 如果使用|| 可以弱判断根据需求  如果遵守View规则是没有问题的
                if (parentName.Contains(UIConstHelper.Const.ParentName) && parentName.Contains(targetTable.gameObject.name))
                {
                    addCdeTable.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 倒序递归收集子节点上带 UIBindCDETable 的组件：
        /// 模块名/资源名为空或嵌套了 Panel 的会被跳过并报错
        /// </summary>
        public static void AddCdeTable(ref List<UIBindCDETable> cdeTable, Transform transform)
        {
            var childCount = transform.childCount;
            if (childCount <= 0) return;

            for (var i = childCount - 1; i >= 0; i--)
            {
                var child    = transform.GetChild(i);
                var childCde = child.GetComponent<UIBindCDETable>();

                if (childCde == null)
                {
                    AddCdeTable(ref cdeTable, child);
                    continue;
                }

                if (string.IsNullOrEmpty(childCde.PkgName) || string.IsNullOrEmpty(childCde.ResName)) continue;
                if (childCde.UICodeType == EUICodeType.Panel)
                {
                    Debug.LogError($"{transform.name} 公共组件嵌套了 其他面板 这是不允许的 {childCde.ResName} 已跳过忽略");
                    continue;
                }

                var newName = Regex.Replace(childCde.name, NameUtility.NameRegex, "");
                if (childCde.name != newName)
                {
                    childCde.name = newName;
                }

                cdeTable.Add(childCde);
            }
        }
    }
}
#endif