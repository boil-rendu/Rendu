using RD.Inspector.Editor;
using UnityEditor;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 业务侧组件的 Inspector 检视器登记表。
    ///
    /// 被登记的类型分散在 ui / uireddot / yooassets / ui3ddisplay / loader 等包里,
    /// 它们的 Runtime 代码通过 UIAssemblyReference.asmref 统一归属 RD.UI(或 RD.Loader),
    /// Editor 代码则归属 RD.UI.Editor —— 本文件正位于其中。
    ///
    /// 放在业务侧(而不是通用包 cn.rendu.inspector 里)的原因: 检视器登记属于业务知识,
    /// 通用包不应反向依赖业务类型; RD.UI.Editor 已引用 RD.Inspector.Editor,
    /// 依赖链 RD.UI.Editor -> RD.Inspector.Editor -> RD.UI 单向, 不会形成循环。
    ///
    /// 带 true 的注册会一次覆盖整个继承族, 例如 UIDataBind 的所有绑定子类、
    /// UIEventBind 的所有事件子类、UIDisposerMonoSingleton 的所有单例子类都无需再单独登记。
    /// </summary>
    // ---- 绑定基类: 一次覆盖整个继承族 ----
    [CustomEditor(typeof(UIDataBind), true)]
    public class UIDataBindInspectorEditor : InspectorEditor { }

    [CustomEditor(typeof(UIEventBind), true)]
    public class UIEventBindInspectorEditor : InspectorEditor { }

    [CustomEditor(typeof(UIDisposerMonoSingleton), true)]
    public class UIDisposerMonoSingletonInspectorEditor : InspectorEditor { }

    // ---- 绑定表组件 ----
    [CustomEditor(typeof(UIBindCDETable), true)]
    public class UIBindCDETableInspectorEditor : InspectorEditor { }

    [CustomEditor(typeof(UIBindComponentTable), true)]
    public class UIBindComponentTableInspectorEditor : InspectorEditor { }

    [CustomEditor(typeof(UIBindDataTable), true)]
    public class UIBindDataTableInspectorEditor : InspectorEditor { }

    [CustomEditor(typeof(UIBindEventTable), true)]
    public class UIBindEventTableInspectorEditor : InspectorEditor { }

    // ---- 独立组件 ----
    [CustomEditor(typeof(UIClickEventPenetration), true)]
    public class UIClickEventPenetrationInspectorEditor : InspectorEditor { }

    // [CustomEditor(typeof(UIGameObjectEntityRef), true)]
    // public class UIGameObjectEntityRefInspectorEditor : InspectorEditor { }
    //
    // [CustomEditor(typeof(RedDotBind), true)]
    // public class RedDotBindInspectorEditor : InspectorEditor { }
    //
    // [CustomEditor(typeof(UI3DDisplay), true)]
    // public class UI3DDisplayInspectorEditor : InspectorEditor { }
    //
    // // ---- ScriptableObject 资产 ----
    // [CustomEditor(typeof(RedDotConfigAsset), true)]
    // public class RedDotConfigAssetInspectorEditor : InspectorEditor { }
    //
    // [CustomEditor(typeof(RedDotKeyAsset), true)]
    // public class RedDotKeyAssetInspectorEditor : InspectorEditor { }

    // TODO(RD): UIAtlasData 所属的图集模块尚未迁入本项目, 迁入后请恢复此检视器登记
    // [CustomEditor(typeof(UIAtlasData), true)]
    // public class UIAtlasDataInspectorEditor : InspectorEditor { }
}
