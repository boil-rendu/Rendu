#if UNITY_EDITOR
using System;
using System.Reflection;
using RD.Draw;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    public sealed partial class UIBindCDETable
    {
        #if ENABLE_VIEW && UNITY_EDITOR

        [NonSerialized]
        private GameObject m_UIChildViewGO;

        [PropertyOrder(int.MinValue)]
        [ReadOnly]
        [ShowInInspector]
        [HideInEditorMode]
        [LabelText("ViewGO")]
        public GameObject UIChildViewGO
        {
            get
            {
                if (m_UIChildViewGO == null)
                {
                    m_UIChildViewGO = Entity?.ViewGO;
                }

                return m_UIChildViewGO;
            }
        }

        #endif

        [PropertyOrder(-1000)]
        [GUIColor(0, 1, 0)]
        [ButtonGroup]
        [Button("查看Component代码", 20)]
        private void OpenComponentScript()
        {
            OpenScriptByReflection($"{this.ResName}Component");
        }

        [PropertyOrder(-1000)]
        [GUIColor(1, 0.6f, 0.4f)]
        [ButtonGroup]
        [Button("查看System代码", 20)]
        private void OpenSystemScript()
        {
            OpenScriptByReflection($"{this.ResName}ComponentSystem");
        }

        /// <summary>
        /// 通过反射调用 ScriptHelper.OpenScript
        /// 解决 Runtime 程序集不能引用 Editor 程序集的问题
        /// </summary>
        private static MethodInfo s_OpenScriptMethod;

        private static void OpenScriptByReflection(string scriptName, string searchKey = "")
        {
            // 缓存类型和方法，避免每次都遍历程序集
            if (s_OpenScriptMethod == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var s_ScriptHelperType = assembly.GetType("RD.Core.Editor.ScriptHelper");
                    if (s_ScriptHelperType != null)
                    {
                        s_OpenScriptMethod = s_ScriptHelperType.GetMethod("OpenScript", new[] { typeof(string), typeof(string) });
                        break;
                    }
                }
            }

            if (s_OpenScriptMethod != null)
            {
                s_OpenScriptMethod.Invoke(null, new object[] { scriptName, searchKey ?? "" });
            }
            else
            {
                Debug.LogError("无法找到 ScriptHelper.OpenScript 方法");
            }
        }

        #region 界面参数

        [LabelText("组件类型")]
        [OnValueChanged("OnValueChangedEUICodeType")]
        [ReadOnly]
        public EUICodeType UICodeType = EUICodeType.Common;

        [BoxGroup("配置", true, true)]
        [LabelText("窗口选项")]
        [GUIColor(0, 1, 1)]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        public EWindowOption WindowOption = EWindowOption.None;

        [BoxGroup("配置", true, true)]
        [OnValueChanged("OnValueChangedEPanelLayer")]
        [GUIColor(0, 1, 1)]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        public EPanelLayer PanelLayer = EPanelLayer.Panel;

        [BoxGroup("配置", true, true)]
        [GUIColor(0, 1, 1)]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        public EPanelOption PanelOption = EPanelOption.None;

        [BoxGroup("配置", true, true)]
        [GUIColor(0, 1, 1)]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        public EPanelStackOption PanelStackOption = EPanelStackOption.VisibleTween;

        [BoxGroup("配置", true, true)]
        [GUIColor(0, 1, 1)]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        public EViewWindowType ViewWindowType = EViewWindowType.View;

        [BoxGroup("配置", true, true)]
        [GUIColor(0, 1, 1)]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        public EViewStackOption ViewStackOption = EViewStackOption.VisibleTween;

        [BoxGroup("配置", true, true)]
        [GUIColor(0, 1, 1)]
        [LabelText("缓存时间")]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        [MinValue(0.01)]
        public float CachePanelTime = 10;

        private bool ShowCachePanelTime => PanelOption.HasFlag(EPanelOption.TimeCache) && !PanelOption.HasFlag(EPanelOption.ForeverCache);

        [LabelText("同层级时 优先级高的在前面")] //相同时后开的在前
        [BoxGroup("配置", true, true)]
        [GUIColor(0, 1, 1)]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        public int Priority = 0;

        private void OnValueChangedEUICodeType()
        {
            if (name.EndsWith(UIConstHelper.Const.PanelName) || name.EndsWith(UIConstHelper.Const.PanelSourceName))
            {
                if (UICodeType != EUICodeType.Panel)
                {
                    Debug.LogWarning($"{name} 结尾{UIConstHelper.Const.PanelName} 必须设定为{UIConstHelper.Const.PanelName}类型");
                }

                UICodeType = EUICodeType.Panel;
            }
            else if (name.EndsWith(UIConstHelper.Const.ViewName))
            {
                if (UICodeType != EUICodeType.View)
                {
                    Debug.LogWarning($"{name} 结尾{UIConstHelper.Const.ViewName} 必须设定为{UIConstHelper.Const.ViewName}类型");
                }

                UICodeType = EUICodeType.View;
            }
            else
            {
                if (UICodeType != EUICodeType.Common)
                {
                    Debug.LogWarning($"{name} 想设定为其他类型 请按照规则设定 请勿强行修改");
                }

                UICodeType = EUICodeType.Common;
            }
        }

        private void OnValueChangedEPanelLayer()
        {
            if (PanelLayer >= EPanelLayer.Cache)
            {
                Debug.LogError($" {name} 层级类型 选择错误 请重新选择");
                PanelLayer = EPanelLayer.Panel;
            }
        }

        #endregion

        private bool ShowAutoCheckBtn()
        {
            if (!UIOperationHelper.CheckUIOperation(false)) return false;
            return true;
        }

        [GUIColor(1, 1, 0)]
        [Button("自动检查所有", 30)]
        [PropertyOrder(-100)]
        [ShowIf("ShowAutoCheckBtn")]
        private void AutoCheckBtn()
        {
            AutoCheck();
        }

        [GUIColor(1, 1, 1)]
        [Button("重置子预制", 20)]
        [PropertyOrder(-100)]
        [ShowIf("ShowAutoCheckBtn")]
        private void RevertPrefabInstance()
        {
            UnityTipsHelper.CallBack("将会重置所有子CDE 还原到预制初始状态 \n(防止嵌套预制修改)",
                () =>
                {
                    InvokeTargetMethod(CreateModuleType, "RefreshChildCdeTable", this);
                    foreach (var cdeTable in AllChildCdeTable)
                    {
                        try
                        {
                            PrefabUtility.RevertPrefabInstance(cdeTable.gameObject, InteractionMode.AutomatedAction);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"这个CDETable 不是预制体 请检查 {cdeTable.name}\n{e.Message}", cdeTable.gameObject);
                        }
                    }
                });
        }

        [GUIColor(0, 1, 1)]
        [Button("保存选中", 50)]
        [PropertyOrder(-100)]
        [ShowIf("ShowSaveSelectSelf")]
        private void SaveSelectSelf()
        {
            var stage = PrefabStageUtility.GetPrefabStage(gameObject);

            if (stage == null)
            {
                Debug.LogError($"未知错误 没有找到预制 {gameObject.name}");
                return;
            }

            var methodInfo = stage.GetType().GetMethod("Save", BindingFlags.Instance | BindingFlags.NonPublic);

            if (methodInfo != null)
            {
                bool result = (bool)methodInfo.Invoke(stage, null);
                if (!result)
                {
                    Debug.LogError("自动保存失败 注意请手动保存");
                    return;
                }
            }
            else
            {
                Debug.LogError("Save方法不存在 自动保存失败 注意请手动保存");
            }

            var assetPath = stage.assetPath;
            StageUtility.GoToMainStage();
            var assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            EditorGUIUtility.PingObject(assetObj);
            Selection.activeObject = assetObj;
        }

        private bool ShowSaveSelectSelf()
        {
            var parentName = gameObject.transform.parent?.name ?? "";
            if (!parentName.Contains("Canvas"))
            {
                return false;
            }

            return !UIOperationHelper.CheckUIIsPackages(this, false);
        }

        internal bool AutoCheck()
        {
            if (!UIOperationHelper.CheckUIOperation(this)) return false;

            var (result1, result2) = InvokeTargetMethod<bool>(CreateModuleType, "InitVoName", this);

            if (!result1 || !result2) return false;

            ShowPackagesCreateBtn();
            OnValueChangedEUICodeType();
            OnValueChangedEPanelLayer();
            if (UICodeType == EUICodeType.Panel)
            {
                PanelSplitData.Panel = gameObject;
                if (!PanelSplitData.AutoCheck()) return false;
            }

            InvokeTargetMethod(CreateModuleType, "RefreshChildCdeTable", this);
            if (ComponentTable != null)
                ComponentTable.AutoCheck();
            if (DataTable != null)
                DataTable.AutoCheck();
            if (EventTable != null)
                EventTable.AutoCheck();

            EditorUtility.SetDirty(this);
            return true;
        }

        private bool ShowCreateBtn()
        {
            if (IsSplitData) return false;
            if (gameObject.transform.parent != null) return false;
            if (UIOperationHelper.CheckUIIsPackages(this, false)) return false;
            return UIOperationHelper.CheckUIOperationAll(this, false);
        }

        private bool ShowPackagesCreateBtn()
        {
            if (IsSplitData) return false;
            if (gameObject.transform.parent != null) return false;
            var result = UIOperationHelper.CheckUIIsPackages(this, false);
            if (result && string.IsNullOrEmpty(m_PackagesName))
            {
                m_PackagesName = UIOperationHelper.GetRDPackagesName(this, false);
            }

            return result;
        }

        [GUIColor(0.7f, 0.4f, 0.8f)]
        [Button("生成", 50)]
        [ShowIf("ShowCreateBtn")]
        internal void CreateUICode()
        {
            if (!UIOperationHelper.CheckUIOperation(this)) return;

            if (!InvokeTargetMethod(CreateModuleType, "CreateCommon", this, false, false)) return;

            AssetDatabase.Refresh();
        }

        [LabelText("指定生成包名")]
        [ShowIf("ShowPackagesCreateBtn")]
        [SerializeField]
        [ShowInInspector]
        private string m_PackagesName;

        public string PackagesName => m_PackagesName;

        [Button("重置", 25)]
        [GUIColor(1f, 1, 0f)]
        private void ResrdPackagesName()
        {
            m_PackagesName = UIOperationHelper.GetRDPackagesName(this, false);
        }

        [GUIColor(0.7f, 0.4f, 0.8f)]
        [Button("Packages生成", 50)]
        [ShowIf("ShowPackagesCreateBtn")]
        internal void CreatePackagesUICode()
        {
            if (!UIOperationHelper.CheckUIIsPackages(this)) return;

            if (!InvokeTargetMethod(CreateModuleType, "CreatePackages", this, false, false, m_PackagesName)) return;

            AssetDatabase.Refresh();
        }

        private bool ShowPanelSourceSplit()
        {
            if (!UIOperationHelper.CheckUIOperationAll(this, false)) return false;
            return IsSplitData;
        }

        [GUIColor(0f, 0.4f, 0.8f)]
        [Button("源数据拆分", 50)]
        [ShowIf("ShowPanelSourceSplit")]
        internal void PanelSourceSplit()
        {
            if (!UIOperationHelper.CheckUIOperation(this)) return;

            if (IsSplitData)
            {
                if (AutoCheck())
                {
                    InvokeTargetMethod(SourceSplitType, "Do", this);
                }
            }
            else
            {
                UnityTipsHelper.ShowError($"{name} 当前数据不是源数据 无法进行拆分 请检查数据");
            }
        }

        private void OnValidate()
        {
            ComponentTable ??= GetComponent<UIBindComponentTable>();
            DataTable ??= GetComponent<UIBindDataTable>();
            EventTable ??= GetComponent<UIBindEventTable>();
        }

        #region CDE显示

        private enum EUICDEInspectorType
        {
            [LabelText("[C]组件")]
            Component,

            [LabelText("[D]数据")]
            Data,

            [LabelText("[E]事件")]
            Event,
        }

        [TitleGroup("UI CDE")]
        [ShowInInspector]
        [PropertyOrder(int.MaxValue - 100)]
        [HideLabel]
        [NonSerialized]
        [EnumToggleButtons]
        [ShowIf(nameof(ShowIfCDEInspector))]
        [OnValueChanged(nameof(OnValueChangedCDEInspector))]
        private EUICDEInspectorType _CDEInspectorType = EUICDEInspectorType.Data;

        [TitleGroup("UI CDE")]
        [ShowInInspector]
        [PropertyOrder(int.MaxValue - 99)]
        [InlineEditor]
        [HideLabel]
        [NonSerialized]
        [HideReferenceObjectPicker]
        [ShowIf(nameof(ShowIfCDEInspector))]
        private Component _InspectorComponent;

        private bool ShowIfCDEInspector()
        {
            var useNewCDE = !UIConstHelper.Const.DisplayOldCDEInspector;

            // 三个表组件的显隐跟开关走: 新版 CDE 检视器下隐藏(只通过 [C]/[D]/[E] 页签访问),
            // 否则同一个表会被页签内联一份、检视器下面再显示一份(两份内容重复); 切回旧版则恢复显示。
            // 挂在这里是因为引擎没有初始化钩子, 挂在 ShowIf 里才能"首次打开检视器"就生效
            UICDEHideInInspector();

            return useNewCDE;
        }

        private void OnValueChangedCDEInspector()
        {
            switch (_CDEInspectorType)
            {
                case EUICDEInspectorType.Component:
                    _InspectorComponent = ComponentTable;
                    break;
                case EUICDEInspectorType.Data:
                    _InspectorComponent = DataTable;
                    break;
                case EUICDEInspectorType.Event:
                    _InspectorComponent = EventTable;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UICDEHideInInspector()
        {
            var flags   = UIConstHelper.Const.DisplayOldCDEInspector ? HideFlags.None : HideFlags.HideInInspector;
            var changed = SetHideFlags(ComponentTable, flags)
                        | SetHideFlags(DataTable, flags)
                        | SetHideFlags(EventTable, flags);

            // 改 hideFlags 不会让检视器重建组件列表(不重新选中对象就看不到效果), 主动重建一次。
            // 延后到本次绘制之后: 当场重建会在绘制过程中销毁自己的 Editor
            if (changed)
            {
                EditorApplication.delayCall += () =>
                {
                    var tracker = ActiveEditorTracker.sharedTracker;
                    tracker?.ForceRebuild();
                };
            }
        }

        /// <summary>设置隐藏标志, 返回是否真的改动了(没变就不写, 避免每帧把对象弄脏)。</summary>
        private static bool SetHideFlags(Component table, HideFlags flags)
        {
            if (table == null || table.hideFlags == flags) return false;

            table.hideFlags = flags;
            return true;
        }

        [Button("添加组件表", 25)]
        [GUIColor(0, 1, 1)]
        [ShowIf(nameof(ShowAddComponentTable))]
        [TitleGroup("UI CDE")]
        [PropertyOrder(int.MaxValue)]
        public void AddComponentTable()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;
            ComponentTable = gameObject.GetOrAddComponent<UIBindComponentTable>();
            ComponentTable.hideFlags = UIConstHelper.Const.DisplayOldCDEInspector ? HideFlags.None : HideFlags.HideInInspector;
            OnValueChangedCDEInspector();
        }

        private bool ShowAddComponentTable()
        {
            if (UIConstHelper.Const.DisplayOldCDEInspector) return ComponentTable == null;
            if (_CDEInspectorType != EUICDEInspectorType.Component) return false;
            return _InspectorComponent == null;
        }

        [Button("添加数据表", 25)]
        [GUIColor(1, 0, 1)]
        [ShowIf(nameof(ShowAddDataTable))]
        [TitleGroup("UI CDE")]
        [PropertyOrder(int.MaxValue)]
        public void AddDataTable()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;
            DataTable = gameObject.GetOrAddComponent<UIBindDataTable>();
            DataTable.hideFlags = UIConstHelper.Const.DisplayOldCDEInspector ? HideFlags.None : HideFlags.HideInInspector;

            OnValueChangedCDEInspector();
        }

        private bool ShowAddDataTable()
        {
            if (UIConstHelper.Const.DisplayOldCDEInspector) return DataTable == null;
            if (_CDEInspectorType != EUICDEInspectorType.Data) return false;
            return _InspectorComponent == null;
        }

        [Button("添加事件表", 25)]
        [GUIColor(0, 1, 0)]
        [ShowIf(nameof(ShowAddEventTable))]
        [TitleGroup("UI CDE")]
        [PropertyOrder(int.MaxValue)]
        public void AddEventTable()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;
            EventTable = gameObject.GetOrAddComponent<UIBindEventTable>();
            EventTable.hideFlags = UIConstHelper.Const.DisplayOldCDEInspector ? HideFlags.None : HideFlags.HideInInspector;

            OnValueChangedCDEInspector();
        }

        private bool ShowAddEventTable()
        {
            if (UIConstHelper.Const.DisplayOldCDEInspector) return EventTable == null;
            if (_CDEInspectorType != EUICDEInspectorType.Event) return false;
            return _InspectorComponent == null;
        }

        #endregion

        #region 反射

        private Type m_CreateModuleType;

        private Type CreateModuleType => m_CreateModuleType ??= GetTargetType("RD.UI.Editor", "RD.UI.Editor.CreateModule");

        private Type m_SourceSplitType;

        private Type SourceSplitType => m_SourceSplitType ??= GetTargetType("RD.UI.Editor", "RD.UI.Editor.PanelSourceSplit");

        private Type GetTargetType(string assemblyName, string className)
        {
            var assembly = AssemblyHelper.GetAssembly(assemblyName);
            if (assembly == null)
            {
                Debug.LogError($"没有这个程序集 {assemblyName}");
                return null;
            }

            var targetType = assembly.GetType(className);

            if (targetType == null)
            {
                Debug.LogError($"没有这个类 {className}");
            }

            return targetType;
        }

        private bool InvokeTargetMethod(Type targetType, string methodName, params object[] parameters)
        {
            if (targetType == null)
            {
                Debug.LogError($"targetType == null");
                return false;
            }

            MethodInfo method = targetType?.GetMethod(methodName);
            if (method == null)
            {
                Debug.LogError($"没有这个方法 {methodName}");
                return false;
            }

            try
            {
                method.Invoke(null, parameters);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{e}");
            }

            return false;
        }

        private (bool, T) InvokeTargetMethod<T>(Type targetType, string methodName, params object[] parameters)
        {
            if (targetType == null)
            {
                Debug.LogError($"targetType == null");
                return (false, default);
            }

            MethodInfo method = targetType.GetMethod(methodName);
            if (method == null)
            {
                Debug.LogError($"没有这个方法 {methodName}");
                return (false, default);
            }

            try
            {
                object result = method.Invoke(null, parameters);
                return (true, (T)result);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e}");
            }

            return (false, default);
        }

        #endregion
    }
}
#endif