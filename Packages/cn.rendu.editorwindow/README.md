# RD.EditorWindow

编辑器窗口宿主：把窗口"长什么样"从业务里摘出来 —— 左菜单树 + 右内容面板的骨架、滚动、分割条、选中持久化都在这里。绘制引擎与全部特性在 [`cn.rendu.panel`](../cn.rendu.panel/README.md)；绘制后端可在 IMGUI / UI Toolkit 之间切换（见下）。

## 包结构

```
cn.rendu.editorwindow/
├── Editor/
│   ├── RD.EditorWindow.Editor.asmdef   程序集：RD.EditorWindow.Editor（Editor 平台）
│   ├── Window/
│   │   ├── EditorWindowBase.cs         轻量编辑器窗口基类（自绘 + 滚动）
│   │   ├── MenuEditorWindow.cs         左菜单树 + 可拖拽分割条 + 右内容面板
│   │   └── WindowBackend.cs            绘制后端开关（IMGUI / UI Toolkit，含菜单项）
│   ├── Controls/
│   │   └── MenuTree.cs                 菜单树数据模型（MenuTree / MenuTreeItem / MenuTreeSelection）
│   └── Prefs/
│       └── EditorPrefsValue.cs         EditorPrefs 强类型泛型包装
└── Tests/Editor/                       EditMode 测试（RD.EditorWindow.Tests.Editor）
```

## 依赖

| 程序集 | 引用 | 说明 |
|---|---|---|
| `RD.EditorWindow.Editor` | `RD.Panel`、`RD.Panel.Editor` | 用 `Engine.DrawObject` 绘制窗口自身与右侧面板 |
| `RD.EditorWindow.Tests.Editor` | `RD.EditorWindow.Editor` | 仅测试用 |

## 命名空间为什么是 `RD.EditorWindows.Editor`（复数）

单数 `RD.EditorWindow` 会**遮蔽 `UnityEditor.EditorWindow` 类型**：C# 名称查找先扫外层命名空间 `RD` 的成员（找到命名空间 `RD.EditorWindow` 就停），轮不到 `using UnityEditor` —— 于是任何 `RD.*` 命名空间下裸写 `EditorWindow` 都会 CS0118。这与 `RD.Inspector.Editor` 里裸写 `Editor` 的坑同源（见 inspector 教程 0.3 节）。程序集名不参与查找，保持单数 `RD.EditorWindow.Editor`。

## EditorWindowBase

普通窗口；`OnGUI` 里自动用引擎绘制**窗口自身**的成员：

```csharp
using RD.Panel;
using RD.EditorWindows.Editor;
using UnityEditor;

public class MyWindow : EditorWindowBase
{
    [MenuItem("Tools/My Window")]
    private static void OpenWindow() => GetWindow<MyWindow>("My").Show();

    [Button("点我", 30)]
    private void DoSomething() { }

    protected override void Initialize() { }        // 首次显示前调用一次
    protected override void OnWindowDestroy() { }   // 窗口销毁时调用

    // 如需自定义 OnGUI：先调 EnsureWindowInitialized()，再调 DrawObjectPanel() 画自身成员
}
```

## MenuEditorWindow

左菜单树 + 右内容面板；右侧调 `DrawTargetObject(选中项.Value)`，所以**菜单项的 `Value` 可以是任意普通 C# 对象** —— 做工具面板时每个菜单项挂一个模块对象即可。

| 成员 | 说明 |
|---|---|
| `protected virtual MenuTree BuildMenuTree()` | 构建左侧菜单树，返回 `null` 表示无菜单（退化为普通窗口） |
| `protected MenuTree MenuTree { get; }` | 取菜单树（首次访问自动构建） |
| `protected void RebuildMenuTree()` | 强制重建（构建条件变化后调用） |
| `public virtual float MenuWidth { get; set; }` | 菜单栏宽度，最小 150 |
| `protected override void OnGUI()` | 已实现"左树 + 可拖拽分割条 + 右内容"，一般无需重写 |

```csharp
public class MyToolWindow : MenuEditorWindow
{
    [MenuItem("Tools/My Tool")]
    public static void OpenWindow() => GetWindow<MyToolWindow>("My Tool").Show();

    private EditorPrefsValue<string> m_LastSelectMenuPrefs = new("MyTool_LastSelectMenu", "全局设置");

    protected override MenuTree BuildMenuTree()
    {
        var tree = new MenuTree();
        tree.Selection.SelectionChanged += OnSelectionChanged;
        tree.Add("模块A", new ModuleA(), Icons.Folder);
        tree.Add("模块B", new ModuleB());
        return tree;
    }
}
```

## 绘制后端：IMGUI / UI Toolkit 切换

窗口后端收在包内，业务代码零改动 —— 所有继承 `EditorWindowBase` 的窗口一起跟着开关走：

- 菜单 **Tools / RD EditorWindow / UI Toolkit 绘制**（带勾选态）；
- 或任意编辑器代码里 `WindowBackend.UseUIToolkit = true`。

选择持久化在 EditorPrefs，重启后保持；切换后已打开的窗口在下一帧自己重建 / 拆掉那棵 UI Toolkit 树，不必关掉重开。与检视器侧的 `InspectorBackend` 是**各自独立**的开关。

| 后端 | 绘制方式 |
|---|---|
| IMGUI（默认） | `OnGUI` 逐帧绘制，与改造前完全一致 |
| UI Toolkit | 内容画进 `rootVisualElement` 的一棵 `VisualElement` 树（`UitkDriver`，按位置复用元素），重绘由绘制回调驱动 |

两种窗口在 UI Toolkit 模式下的分工：

- **普通窗口**（只用 `DrawObjectPanel()` / `DrawTargetObject()` 画引擎内容）：整窗一个滚动区，引擎内容画进去；
- **`MenuEditorWindow`**：左侧菜单树仍是 IMGUI 绘制（放进 `IMGUIContainer`，观感与交互跟 IMGUI 模式一致），分割条与右侧内容改由 UIElements 排布 —— 右侧字段控件是 UI Toolkit 控件。

写自定义窗口时注意两点：

- UI Toolkit 模式下**基类接管 `rootVisualElement`**（切回 IMGUI 时会 `Clear()`）。要加自己的东西，请重写 `BuildToolkitUI(VisualElement root)`，并用 `CreateToolkitContent(parent, draw, scroll)` 把引擎内容摆进自己的布局；
- 子类重写的 `OnGUI` 里那段 IMGUI 内容在 UI Toolkit 模式下不会被执行 —— 用 `UseToolkitBackend` 判断当前模式，必要时自行跳过。

## MenuTree（菜单树数据模型）

```csharp
var tree = new MenuTree();
tree.Selection.SelectionChanged += OnSelectionChanged;   // 选中项变化事件

tree.Add("全局设置", this, Icons.SettingsCog);          // 加一项（末段为名字，值为任意对象）
tree.Add("父级/子级", someObject, someIcon);             // 支持路径形式（自动补齐中间层）
tree.AddMenuItemAtPath("父级", item);                   // 已有 item 挂到指定路径
tree.AddAllAssetsAtPath("资源", "Assets/GameRes", typeof(Texture));          // 目录下资产批量入树
tree.AddAllAssetImporterAtPath("导入器", "Assets/GameRes", typeof(TextureImporter));

tree.SelectFirstMenuItem();     // 选第一项
tree.SelectMenuItem("a/b");     // 按路径选中
tree.FindMenuItem("a/b");       // 查找
tree.ForEach(item => { });      // 深度优先遍历（保持插入顺序）
tree.Clear();
tree.ConfigID = "xxx";          // 供使用方做持久化标识
```

`MenuTreeItem`：`Name`、`Value`、`Icon`、`Parent`、`ChildMenuItems`、`IsSelected`、`Toggled`、`HasChildren`、`AddChild`、`RemoveChild`、`AddIcon`、`Select()`。

建树的隐式约定（同名路径**就地替换**而非重复添加、重复选中不重复触发事件等）由 `MenuTreeTests` 固化为行为契约。

## EditorPrefsValue&lt;T&gt;

`EditorPrefs` 的强类型包装（string / int / float / bool / 枚举），窗口宽度、上次选中项之类的小状态一行落盘：

```csharp
private readonly EditorPrefsValue<float> m_MenuWidthPrefs = new("MyTool_MenuWidth", 300f);
```

## 测试

**不开 Unity 的快速回归**：`Tools~/Verify/` 是独立的 .NET 控制台工程（把本包与 cn.rendu.panel 的源码编进一个进程，自带极简 NUnit 运行器），只需 .NET 8：

```bash
cd "Packages/cn.rendu.editorwindow/Tools~/Verify" && dotnet run
```

当前规模：1 个夹具 14 例（`MenuTreeTests`），全部纯逻辑、无需 Unity 运行时。

## 与引擎的接口

`EditorWindowBase` 内部已经持有一份 `HostState`（折叠 / 滚动状态各窗口独立、不串台）并在两条绘制分支里把它传到 `Engine.DrawObject`，子类不需要直接管 session：

| 子类会调用的入口 | 说明 |
|---|---|
| `protected override void OnGUI()` | 默认实现：画窗口自身的成员；一般不需要重写 |
| `protected void DrawObjectPanel()` | 以本窗口的会话绘制窗口自身（重写 `OnGUI` 时手动调它） |
| `protected void DrawTargetObject(object target)` | 以本窗口的会话绘制任意对象（`MenuEditorWindow` 右侧用它画选中菜单项的值） |
| `protected override void BuildToolkitUI(VisualElement root)` | UI Toolkit 模式的外壳入口；默认整窗一个滚动区，自己有外壳（如菜单 + 分割条）的窗口重写它 |
| `protected VisualElement CreateToolkitContent(VisualElement parent, Action draw, bool scroll = true)` | 在自定义外壳里"开一块引擎内容区"并接上重绘链（`scroll=false` 不带滚动） |

要直接调 `Engine.DrawObject`（例如在 `EditorWindow` 体系外自己造一个宿主）走 `Engine.CreateHostState()` + 带 `HostState` 的重载 —— 那是 cn.rendu.panel 的事，详见 [cn.rendu.panel README §8](../cn.rendu.panel/README.md#8-辅助-api)。
