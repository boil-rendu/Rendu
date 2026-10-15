#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using RD.Draw;
using UnityEditor;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    using Object = UnityEngine.Object;
    /// <summary>
    /// UIPanelSplitData 的编辑器扩展（partial，UNITY_EDITOR）
    /// 面板拆分工具：检查 Panel 下的 View 父节点、生成/关联 View 预制体、命名与重复校验
    /// </summary>
    public sealed partial class UIPanelSplitData
    {
        /// <summary>是否生成「通用界面枚举」（仅当存在 View 时可选）</summary>
        [SerializeField]
        [LabelText("生成通用界面枚举")]
        [ShowIf("ShowCreatePanelViewEnum")]
        internal bool CreatePanelViewEnum = true;

        /// <summary>当可自动检查且至少存在一个 View 时显示该选项</summary>
        internal bool ShowCreatePanelViewEnum()
        {
            if (!ShowAutoCheckBtn()) return false;
            return (AllCommonView.Count + AllCreateView.Count + AllPopupView.Count) >= 1;
        }

        /// <summary>是否显示「检查拆分数据」按钮：Panel 非空、名字以 PanelName 结尾、且非运行态</summary>
        private bool ShowAutoCheckBtn()
        {
            if (Panel == null)
            {
                return false;
            }

            if (!Panel.name.EndsWith(UIConstHelper.Const.PanelName))
            {
                return false;
            }

            if (UIOperationHelper.CheckUIOperationAll(Panel, false)) return false;
            return true;
        }

        /// <summary>是否显示「检查拆分数据」按钮（源文件态）：Panel 名字以 Source 结尾</summary>
        private bool ShowCheckBtn()
        {
            if (Panel == null)
            {
                return false;
            }

            if (!Panel.name.EndsWith(UIConstHelper.Const.Source))
            {
                return false;
            }

            return true;
        }

        /// <summary>「检查拆分数据」按钮（源文件态）：仅检查</summary>
        [GUIColor(0, 1, 1)]
        [Button("检查拆分数据", 30)]
        [ShowIf("ShowCheckBtn")]
        private void AutoCheckSourceBtn()
        {
            AutoCheck();
        }

        /// <summary>「检查拆分数据」按钮（Panel 态）：检查通过后额外执行 ResetChild</summary>
        [GUIColor(0f, 0.4f, 0.8f)]
        [Button("检查拆分数据", 30)]
        [ShowIf("ShowAutoCheckBtn")]
        private void AutoCheckPanelBtn()
        {
            if (AutoCheck())
            {
                ResetChild();
            }
        }

        /// <summary>
        /// 核心检查流程：
        /// 重新定位 View 父节点 → 校验 Panel 命名 → 收集子 View 父节点 → 校验命名/父子关系/重复
        /// </summary>
        internal bool AutoCheck()
        {
            if (Panel == null)
            {
                Debug.LogError($"没有找到 Panel");
                return false;
            }

            ReCheckViewParent();

            if (!CheckPanelName()) return false;

            FindChildParent();

            if (AllViewParent != null)
            {
                CheckViewParent(AllCommonView, AllViewParent);
                CheckViewParent(AllCreateView, AllViewParent);
            }
            else
            {
                AllCommonView.Clear();
                AllCreateView.Clear();
            }

            if (AllPopupViewParent != null)
            {
                CheckViewParent(AllPopupView, AllPopupViewParent);
            }
            else
            {
                AllPopupView.Clear();
            }

            if (AllViewParent != null)
            {
                CheckViewName(AllCommonView);
                CheckViewName(AllCreateView);
            }

            if (AllPopupViewParent != null)
            {
                CheckViewName(AllPopupView);
            }

            var hashList = new HashSet<RectTransform>();
            CheckRepetition(ref hashList, AllCommonView);
            CheckRepetition(ref hashList, AllCreateView);
            CheckRepetition(ref hashList, AllPopupView);

            return true;
        }

        /// <summary>重新定位 AllViewParent / AllPopupViewParent，并校验其确实是 Panel 的子物体</summary>
        private void ReCheckViewParent()
        {
            if (Panel == null) return;

            if (AllViewParent == null || AllViewParent?.name != UIConstHelper.Const.AllViewParentName)
            {
                AllViewParent = Panel.transform.FindChildByName(UIConstHelper.Const.AllViewParentName)?.GetComponent<RectTransform>();
            }

            if (AllViewParent != null)
            {
                if (!CheckViewParentIsPanel(AllViewParent))
                {
                    Debug.LogError($"{Panel.name}的{AllViewParent.name}不是Panel的子物体 请检查", Panel);
                    AllViewParent = null;
                }
            }

            if (AllPopupViewParent == null || AllPopupViewParent?.name != UIConstHelper.Const.AllPopupViewParentName)
            {
                AllPopupViewParent = Panel.transform.FindChildByName(UIConstHelper.Const.AllPopupViewParentName)?.GetComponent<RectTransform>();
            }

            if (AllPopupViewParent != null)
            {
                if (!CheckViewParentIsPanel(AllPopupViewParent))
                {
                    Debug.LogError($"{Panel.name}的{AllPopupViewParent.name}不是Panel的子物体 请检查", Panel);
                    AllPopupViewParent = null;
                }
            }
        }

        /// <summary>递归判断 parent 是否为 Panel 的后代（向上找到 Panel.transform 即为真）</summary>
        private bool CheckViewParentIsPanel(Transform parent)
        {
            if (parent == null || parent.parent == null) return false;
            if (parent.parent == Panel.transform) return true;
            return CheckViewParentIsPanel(parent.parent);
        }

        /// <summary>遍历两个 View 父节点下的子物体，把以 ViewParentName 结尾的收集进对应列表</summary>
        private void FindChildParent()
        {
            if (AllViewParent != null)
            {
                for (int i = 0; i < AllViewParent.childCount; i++)
                {
                    var child = AllViewParent.GetChild(i).GetComponent<RectTransform>();
                    if (child.name.EndsWith(UIConstHelper.Const.ViewParentName))
                    {
                        if (!AllCommonView.Contains(child) && !AllCreateView.Contains(child))
                        {
                            AllCreateView.Add(child);
                        }
                    }
                }
            }

            if (AllPopupViewParent != null)
            {
                for (int i = 0; i < AllPopupViewParent.childCount; i++)
                {
                    var child = AllPopupViewParent.GetChild(i).GetComponent<RectTransform>();
                    if (child.name.EndsWith(UIConstHelper.Const.ViewParentName))
                    {
                        if (!AllPopupView.Contains(child))
                        {
                            AllPopupView.Add(child);
                        }
                    }
                }
            }
        }

        /// <summary>重置各 View 父节点下的子物体（Common 需保留 View，Create/Popup 不保留）</summary>
        private void ResetChild()
        {
            if (AllViewParent != null)
            {
                CheckChild(AllCommonView, true);
                CheckChild(AllCreateView, false);
            }

            if (AllPopupViewParent != null)
            {
                CheckChild(AllPopupView, false);
            }
        }

        /// <summary>
        /// 校验 Panel 命名：首字母大写；禁止使用默认名；必须以 PanelName 或 PanelSourceName 结尾
        /// </summary>
        private bool CheckPanelName()
        {
            var qualifiedName = NameUtility.ToFirstUpper(Panel.name);
            if (Panel.name != qualifiedName)
            {
                Panel.name = qualifiedName;
            }

            var sourceName = Panel.name == UIConstHelper.Const.GetDefaultName(UIConstHelper.Const.PanelSourceName);
            var panelName  = Panel.name == $"{UIConstHelper.Const.ProjectName}{UIConstHelper.Const.PanelName}";
            if (sourceName || panelName)
            {
                Debug.LogError($"当前是默认名称 请手动修改名称 {(sourceName ? $"Xxx{UIConstHelper.Const.PanelSourceName}" : "")} {(panelName ? $"Xxx{UIConstHelper.Const.PanelName}" : "")}");
                return false;
            }

            var sourceEndsWith = Panel.name.EndsWith($"{UIConstHelper.Const.PanelSourceName}");
            var panelEndsWith  = Panel.name.EndsWith($"{UIConstHelper.Const.PanelName}");

            if (sourceEndsWith || panelEndsWith)
            {
                return true;
            }

            Debug.LogError($"{Panel.name} 命名必须以指定规则结尾: [{UIConstHelper.Const.PanelName} / {UIConstHelper.Const.PanelSourceName}] 请勿随意修改");
            return false;
        }

        /// <summary>
        /// 检查 View 父节点列表：
        /// 找到对应 View 预制（不存在则自动生成）、按需清理/重建子物体、必要时强制删除重新关联
        /// </summary>
        private void CheckChild(List<RectTransform> list, bool needView)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var current = list[i];
                if (current == null)
                {
                    list.RemoveAt(i);
                    continue;
                }

                var viewName = current.name.Replace(UIConstHelper.Const.ParentName, "");

                var viewPath = GetOnlyPrefabAssetsPath(viewName, false);

                if (string.IsNullOrEmpty(viewPath))
                {
                    viewPath = ViewSaveAsPrefabAsset(current);
                }

                if (string.IsNullOrEmpty(viewPath))
                {
                    list.RemoveAt(i);
                    Debug.LogError($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent 没有找到对应View预制 请手动生成",
                        current);
                    continue;
                }

                if (!needView)
                {
                    for (int j = 0; j < current.childCount; j++)
                    {
                        Object.DestroyImmediate(current.GetChild(0).gameObject);
                    }
                }
                else
                {
                    var reLink = false;

                    if (current.childCount == 1)
                    {
                        var child         = current.GetChild(0);
                        var checkInstance = PrefabUtility.IsPartOfPrefabInstance(child);
                        if (!checkInstance)
                        {
                            Debug.Log($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent 子物体必须是指定预制体 将强制删除重新关联",
                                current);
                            reLink = true;
                        }
                        else
                        {
                            var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child);
                            var prefabName      = Path.GetFileNameWithoutExtension(prefabAssetPath);
                            if (prefabName != viewName)
                            {
                                Debug.Log($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent 子物体必须是指定预制体 当前是预制但不是指定预制体 将强制删除重新关联",
                                    current);
                                reLink = true;
                            }
                        }
                    }
                    else
                    {
                        reLink = true;
                    }

                    if (reLink)
                    {
                        for (int j = 0; j < current.childCount; j++)
                        {
                            Object.DestroyImmediate(current.GetChild(0).gameObject);
                        }

                        var viewAsset = (GameObject)AssetDatabase.LoadAssetAtPath(viewPath, typeof(Object));
                        PrefabUtility.InstantiatePrefab(viewAsset, current);
                    }
                }
            }
        }

        /// <summary>按 ViewParent 名字生成对应的 View 预制并原地保存，返回保存路径（失败返回空串）</summary>
        private string ViewSaveAsPrefabAsset(RectTransform current)
        {
            var viewName = current.name.Replace(UIConstHelper.Const.ParentName, "");
            var view     = CreateUIView(viewName);
            var source   = Panel.GetComponent<UIBindCDETable>();
            var pkgName  = source?.PkgName;
            if (string.IsNullOrEmpty(pkgName))
            {
                Debug.LogError($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent: [{current.name}] Panel 不是CDE 且没有找到PakName",
                    current);
                return "";
            }

            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(source.gameObject);
            if (string.IsNullOrEmpty(path))
            {
                path = GetOnlyPrefabAssetsPath(source.gameObject.name);
            }

            var loadSource = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            if (loadSource == null)
            {
                Debug.LogError($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent: [{current.name}] 没有找到资源 {path}",
                    current);
                return "";
            }

            var savePath = Path.GetDirectoryName(path);
            var newPath = $"{savePath}/{viewName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(view, newPath);
            Object.DestroyImmediate(view);
            SelectAssetAtPath(newPath, false);
            return newPath.Replace("Assets/../", "");
        }

        /// <summary>创建一个空白 View 物体（挂 RectTransform/CanvasRenderer/CDE，重置铺满、设为 UI 层）</summary>
        private GameObject CreateUIView(string name)
        {
            var viewObject = new GameObject();
            viewObject.name = name;
            var viewRect = viewObject.GetOrAddComponent<RectTransform>();
            viewObject.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = viewObject.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType = EUICodeType.View;
            viewRect.ResetToFullScreen();
            viewObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            return viewObject;
        }

        //命名检查
        /// <summary>检查 View 父节点命名：首字母大写、非默认名、以 ViewParentName 结尾、子物体≤1，并同步子 CDE 命名</summary>
        private void CheckViewName(List<RectTransform> list)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var current = list[i];
                if (current == null)
                {
                    list.RemoveAt(i);
                    continue;
                }

                var qualifiedName = NameUtility.ToFirstUpper(current.name);
                if (current.name != qualifiedName)
                {
                    current.name = qualifiedName;
                }

                if (current.name == UIConstHelper.Const.GetDefaultName(UIConstHelper.Const.ViewParentName))
                {
                    list.RemoveAt(i);
                    Debug.LogError($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent 当前是默认名称 请手动修改名称 Xxx{UIConstHelper.Const.ViewParentName}",
                        current);
                    continue;
                }

                if (!current.name.EndsWith(UIConstHelper.Const.ViewParentName))
                {
                    list.RemoveAt(i);
                    Debug.LogError($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent: [{current.name}] 命名必须以 {UIConstHelper.Const.ViewParentName} 结尾 请勿随意修改",
                        current);
                    continue;
                }

                if (current.childCount >= 2)
                {
                    list.RemoveAt(i);
                    Debug.LogError($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent: [{current.name}] 只能有<=1个子物体 你应该通过其他方式实现层级结构",
                        current);
                    continue;
                }

                if (current.transform.childCount == 1)
                {
                    var firstChild = current.transform.GetChild(0);
                    var viewCde    = firstChild.GetOrAddComponent<UIBindCDETable>();
                    viewCde.gameObject.name = current.name.Replace(UIConstHelper.Const.ParentName, "");
                }
            }
        }

        //检查null / 父级
        /// <summary>检查 View 父节点列表：剔除 null / 无父级项；父级不匹配时按名字自动移动到对应的 View 列表</summary>
        private void CheckViewParent(List<RectTransform> list, RectTransform parent)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                    continue;
                }

                var current = list[i];

                var parentP = current.parent;
                if (parentP == null)
                {
                    list.RemoveAt(i);
                    continue;
                }

                if (parentP != parent)
                {
                    list.RemoveAt(i);

                    //因为只有2个父级 所以如果不是这个就会自动帮你移动到另外一个上面
                    //如果多了还是不要自动了
                    var currentParentName = parentP.name;
                    if (currentParentName == UIConstHelper.Const.AllViewParentName)
                    {
                        AllCreateView.Add(current);
                    }
                    else if (currentParentName == UIConstHelper.Const.AllPopupViewParentName)
                    {
                        AllPopupView.Add(current);
                    }
                }
            }
        }

        //检查重复
        /// <summary>检查列表内重复项（用 hashList 跨列表去重），发现重复则移除并报错</summary>
        private void CheckRepetition(ref HashSet<RectTransform> hashList, List<RectTransform> list)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var current = list[i];
                if (!hashList.Add(current))
                {
                    list.RemoveAt(i);
                    Debug.LogError($"{current.parent?.parent?.name}的{current.parent?.name}下的ViewParent: [{current.name}] 重复存在 已移除 请检查",
                        current);
                }
            }
        }

        /// <summary>刷新资源并选中指定路径的预制体（可选），同时触发其 CDE 的 AutoCheck</summary>
        internal static GameObject SelectAssetAtPath(string assetPath, bool select = true)
        {
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
            var selectPath = assetPath.Replace("Assets/../", "");
            var assetObj   = (GameObject)AssetDatabase.LoadAssetAtPath(selectPath, typeof(Object));
            var cde        = assetObj.GetComponent<UIBindCDETable>();
            if (cde == null)
            {
                Debug.LogError($"{assetObj.name} 没有找到CDE 这是必须的 请检查");
            }
            else
            {
                cde.AutoCheck();
            }
            if (select)
            {
                EditorGUIUtility.PingObject(assetObj);
                Selection.activeObject = assetObj;
            }

            return assetObj;
        }

        /// <summary>按名字查找唯一预制体路径；找到多个或 0 个时报错（tips 控制是否打印）</summary>
        internal static string GetOnlyPrefabAssetsPath(string assetName, bool tips = true)
        {
            var allResult = AssetDatabase.FindAssets($"{assetName} t:Prefab", null);
            if (allResult == null)
            {
                if (tips)
                    Debug.LogError($"未找到预制体 {assetName}");
                return null;
            }

            if (allResult.Length == 1)
            {
                return AssetDatabase.GUIDToAssetPath(allResult[0]);
            }

            var allPath = new List<string>();
            foreach (var guid in allResult)
            {
                var path     = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName == assetName)
                {
                    allPath.Add(path);
                }
            }

            if (allPath.Count <= 0)
            {
                if (tips)
                    Debug.LogError($"未找到预制体 {assetName}");
                return null;
            }

            if (allPath.Count >= 2)
            {
                for (int i = 0; i < allPath.Count; i++)
                {
                    Debug.LogError($"[{i + 1}] 找到多个预制体 {assetName} 请检查 命名不能有重复!!!\n[{i + 1}] {allPath[i]}", LoadAssetAtPath(allPath[i]));
                }
            }

            return allPath[0];

            GameObject LoadAssetAtPath(string path)
            {
                var selectPath = path.Replace("Assets/../", "");
                var assetObj   = (GameObject)AssetDatabase.LoadAssetAtPath(selectPath, typeof(Object));
                return assetObj;
            }
        }
    }
}
#endif
