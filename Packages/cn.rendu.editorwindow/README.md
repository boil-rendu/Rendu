# RD.EditorWindow

编辑器窗口宿主（IMGUI）：把窗口"长什么样"从业务里摘出来 —— 左菜单树 + 右内容面板的骨架、滚动、分割条、选中持久化都在这里。绘制引擎与全部特性在 [`cn.rendu.panel`](../cn.rendu.panel/README.md)。

## 包结构

```
cn.rendu.editorwindow/
├── Editor/
│   ├── RD.EditorWindow.Editor.asmdef   程序集：RD.EditorWindow.Editor（Editor 平台）
│   ├── Window/
│   │   ├── EditorWindowBase.cs         轻量编辑器窗口基类（自绘 + 滚动）
│   │   └── MenuEditorWindow.cs         左菜单树 + 可拖拽分割条 + 右内容面板
│   ├── Controls/
│   │   └── MenuTree.cs                 菜单树数据模型（MenuTree / MenuTreeItem / MenuTreeSelection）
│   └── Prefs/
│       └── EditorPrefsValue.cs         EditorPrefs 强类型泛型包装
└── Tests/Editor/                       EditMode 测试（RD.EditorWindow.Tests.Editor）
```

## 依赖

| 程序集 | 引用 | 说明 |
|---|---|---|
| `RD.EditorWindow.Editor` | `RD.Inspector`、`RD.Inspector.Editor` | 用 `Engine.DrawObject` 绘制窗口自身与右侧面板 |
| `RD.EditorWindow.Tests.Editor` | `RD.EditorWindow.Editor` | 仅测试用 |

## 命名空间为什么是 `RD.EditorWindows.Editor`（复数）

单数 `RD.EditorWindow` 会**遮蔽 `UnityEditor.EditorWindow` 类型**：C# 名称查找先扫外层命名空间 `RD` 的成员（找到命名空间 `RD.EditorWindow` 就停），轮不到 `using UnityEditor` —— 于是任何 `RD.*` 命名空间下裸写 `EditorWindow` 都会 CS0118。这与 `RD.Inspector.Editor` 里裸写 `Editor` 的坑同源（见 inspector 教程 0.3 节）。程序集名不参与查找，保持单数 `RD.EditorWindow.Editor`。

## EditorWindowBase

普通窗口；`OnGUI` 里自动用引擎绘制**窗口自身**的成员：

```csharp
using RD.Inspector;
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

窗口跨程序集持有独立会话的方式（各窗口折叠 / 滚动状态互不串台）：

```csharp
var state = Engine.CreateHostState();     // 窗口各持一份, 生命周期与窗口一致
Engine.DrawObject(target, this, null, null, state);
```

`Engine` 侧说明见 [cn.rendu.panel README §8](../cn.rendu.panel/README.md#8-辅助-api)。
