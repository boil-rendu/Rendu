# RD.EditTool

编辑器窗口宿主：把窗口"长什么样"从业务里摘出来 —— 左菜单树 + 右内容面板的骨架、滚动、分割条、选中持久化都在这里。绘制引擎与全部特性在 [`cn.rendu.draw`](../cn.rendu.draw/README.md)；绘制后端由 cn.rendu.draw 的 `Driver` 提供（UIElements 保留模式，见 §绘制后端）。

## 包结构

```
cn.rendu.edittool/
├── Editor/
│   ├── RD.EditTool.Editor.asmdef   程序集：RD.EditTool.Editor（Editor 平台）
│   ├── Window/                     窗口宿主层
│   │   ├── EditorWindowBase.cs         轻量编辑器窗口基类（继承 DrawWindow）
│   │   ├── MenuEditorWindow.cs         左菜单树 + 可拖拽分割条 + 右内容面板
│   │   └── WindowTypes.cs              停靠窗口类型注册表（RegisterWindowType / GetAllWindowTypes）
│   ├── Menu/                       菜单树数据模型（纯数据，绘制在 MenuEditorWindow）
│   │   └── MenuTree.cs                 MenuTree / MenuTreeItem / MenuTreeSelection
│   ├── Tool/                       工具模块框架（GenericToolWindow 及其模块体系）
│   │   ├── GenericToolWindow.cs        通用工具窗口基类（反射扫描 [ToolMenu] 模块自动建树 + 生命周期调度）
│   │   ├── ToolMenuAttribute.cs        [ToolMenu] 模块登记特性（MenuName / Order / Scope）+ 扫描结果数据 ToolMenuData
│   │   ├── ToolModuleBase.cs           工具模块最底层基类（Initialize / OnDestroy 契约）
│   │   ├── BaseToolModule.cs           工具模块基类（持有宿主窗口 / 菜单树 / 模块名 / 自定义数据）
│   │   └── BaseTreeMenuItem.cs         菜单项模板（首次选中初始化一次 → Select）+ TreeMenuItem<T> 泛型封装
│   ├── Mvvm/                       通用 MVVM 基础设施（命令 / ViewModel，与窗口宿主无关，可复用）
│   │   ├── ICommand.cs                 ICommand / ICommand<T> 命令契约
│   │   ├── CommandBase.cs              命令基类（CanExecute / Validate / ExecuteWithValidation）
│   │   └── ViewModelBase.cs            INotifyPropertyChanged + IDisposable 的 ViewModel 基类
│   └── Prefs/
│       └── EditorPrefsValue.cs         EditorPrefs 强类型泛型包装
└── Tests/Editor/                       EditMode 测试（RD.EditTool.Tests.Editor；MenuTreeTests 14 例）
```

文件夹按职责命名、不放与内容不符的名字：`Menu/` 是纯数据模型（不是 UI 控件，故不叫 Controls），`Mvvm/` 直说内容（不是 Core 这种抽屉名），窗口类型注册表归 `Window/`（不单设 Defines）。

## 依赖

| 程序集 | 引用 | 说明 |
|---|---|---|
| `RD.EditTool.Editor` | `RD.Draw`、`RD.Draw.Editor` | 用 `Engine.DrawObject` 绘制窗口自身与右侧面板 |
| `RD.EditTool.Tests.Editor` | `RD.EditTool.Editor` | 仅测试用 |

工具模块要存**编辑器数据资产**时，按需给 `RD.EditTool.Editor` 加一条 `RD.Serialization.Editor` 引用即可用 `SerializedScriptableObject`（包 **cn.rendu.serialization**）—— 数据资产层不依赖任何宿主，与检视器宿主无依赖关系。

## 命名空间为什么是 `RD.EditTool.Editor`（复数）

单数 `RD.EditTool` 会**遮蔽 `UnityEditor.EditorWindow` 类型**：C# 名称查找先扫外层命名空间 `RD` 的成员（找到命名空间 `RD.EditTool` 就停），轮不到 `using UnityEditor` —— 于是任何 `RD.*` 命名空间下裸写 `EditorWindow` 都会 CS0118。这与 `RD.Inspector.Editor` 里裸写 `Editor` 的坑同源（见 inspector 教程 0.3 节）。程序集名不参与查找，保持单数 `RD.EditTool.Editor`。

## EditorWindowBase

普通窗口；`OnGUI` 里自动用引擎绘制**窗口自身**的成员：

```csharp
using RD.Draw;
using RD.EditTool.Editor;
using UnityEditor;

public class MyWindow : EditorWindowBase
{
    [MenuItem("Tools/My Window")]
    private static void OpenWindow() => GetWindow<MyWindow>("My").Show();

    [Button("点我", 30)]
    private void DoSomething() { }

    protected override void Initialize() { }        // 首次显示前调用一次
    protected override void OnWindowDestroy() { }   // 窗口销毁时调用

    // 如需自定义内容：重写 OnDrawContent() 画自身成员（绘制细节收在 DrawWindow，子类不感知后端）
}
```

## MenuEditorWindow

左菜单树 + 右内容面板；右侧调 `DrawObject(选中项.Value)`，所以**菜单项的 `Value` 可以是任意普通 C# 对象** —— 做工具面板时每个菜单项挂一个模块对象即可。

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
        tree.Add("模块A", new ModuleA(), EditorIcons.Folder);
        tree.Add("模块B", new ModuleB());
        return tree;
    }
}
```

## GenericToolWindow（通用工具窗口）

`GenericToolWindow` 在 `MenuEditorWindow` 之上再包一层"工具窗口套路"：反射扫描各程序集里带 `[ToolMenu]` 的模块类 → 按 `Order` 排序挂进左侧菜单树 → 选中时自动做初始化与生命周期调度。窗口自身仍然只需要一个带 `[MenuItem]` 的入口：

```csharp
using RD.EditTool.Editor;

public class MyToolWindow : GenericToolWindow
{
    [MenuItem("Tools/My Tool")]
    public static void OpenWindow() => GetWindow<MyToolWindow>("My Tool").Show();
}
```

模块类继承 `BaseToolModule`（只嵌在父模块里、不进菜单的可直接继承 `ToolModuleBase`），挂上特性即入树：

```csharp
[ToolMenu("资源检查", order: 10, Scope = "loader")]
public class ResCheckModule : BaseToolModule
{
    public override void Initialize() { }      // 首次被选中时调用一次
    public override void SelectionMenu() { }   // 每次被选中时调用
    public override void OnDestroy() { }
}
```

| 扩展点 | 说明 |
|---|---|
| `protected override MenuTree BuildToolMenuTree(MenuTree tree)` | 先调 `base` 拿到反射扫描结果，再在头部/尾部追加自己的根节点 |
| `protected virtual string AcceptScope` | 限定本窗口只加载 `Scope` 匹配的模块（按包位置拆分模块用）；null/空 = 接受所有 |
| `protected virtual void OnToolInitialize() / OnToolDestroy()` | 窗口自身内嵌模块（挂在 `[ShowInInspector]` 字段上的面板）的装配 / 清理 |
| `public void SelectModule(string moduleName)` | 外部代码按模块名跳转选中 |
| `protected BaseTreeMenuItem NewTreeMenuItem(Type moduleType, string moduleName)` | 在 `BuildToolMenuTree` 里手动加模块（走它才会进生命周期清单） |

约定：

- 菜单项是 `TreeMenuItem<T>`（`BaseTreeMenuItem` 的泛型封装）：**首次选中 `Initialize()` 一次**，之后每次选中走子类的 `Select()`；窗口销毁时基类统一调所有模块的 `OnDestroy()`；
- 上次选中项（菜单名 + 模块路径）持久化在 EditorPrefs，重开窗口自动恢复并展开父级；
- 建树 / 选中 / 销毁的异常都被接住并记日志，不会把窗口打炸；
- `[ToolMenu]` 的名字与文件 `ToolMenuAttribute.cs` 一致，框架本身是通用的（历史名 UIAutoMenu 已废弃）。

## 绘制后端：单一 UIElements 后端

`DrawWindow`（本包的宿主基类）接住 Unity 的 `CreateGUI` 入口，在 `rootVisualElement` 上建一棵 UIElements 树，引擎把窗口内容画进树并由 `Driver` 按位置复用元素、由重绘回调驱动更新。子类只重写 `OnDrawContent()` 画自身成员，不感知后端、无需切换。

两种窗口的分工（均走 UIElements）：

- **普通窗口**（只用 `DrawObject()` 画引擎内容）：默认整窗一个滚动区，引擎内容画进去；
- **`MenuEditorWindow`**：左侧菜单树与右侧内容都排进 UIElements 树 —— 分割条与右侧字段控件全是 UIElements 控件。

写自定义窗口时注意：

- 基类（`DrawWindow`）接管 `rootVisualElement`：要加自己的 UIElements 布局，请重写 `SetupUI(VisualElement root)`，并用 `CreateContentArea(parent, draw, scroll)` 把引擎内容摆进自己的布局；
- 自定义内容请重写 `OnDrawContent()`；它既被默认布局的绘制入口调用，也被自定义布局的内容区回调调用，一份代码两处复用。

## MenuTree（菜单树数据模型）

```csharp
var tree = new MenuTree();
tree.Selection.SelectionChanged += OnSelectionChanged;   // 选中项变化事件

tree.Add("全局设置", this, EditorIcons.SettingsCog);          // 加一项（末段为名字，值为任意对象）
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

`Tests/Editor/MenuTreeTests.cs`（程序集 `RD.EditTool.Tests.Editor`）：1 个夹具 14 例，全部纯逻辑，用 Unity Test Runner（`Window > General > Test Runner` → `EditMode`）运行；窗口绘制部分没有自动化覆盖，改 `Window/` 请手工回归。

## 与引擎的接口

`EditorWindowBase` 内部已经持有一份 `HostState`（折叠 / 滚动状态各窗口独立、不串台）并在绘制时把它传到 `Engine.DrawObject`，子类不需要直接管 session：

| 子类会调用的入口 | 说明 |
|---|---|
| `protected virtual void OnDrawContent()` | 默认实现：画窗口自身的成员；一般不需要重写。默认布局的绘制入口与自定义布局的内容区都调它 |
| `protected void DrawObject(object target)` | 以本窗口的会话绘制任意对象（`MenuEditorWindow` 右侧用它画选中菜单项的值） |
| `protected void ReportViewportWidth(float scrollY = 0f)` | 画完滚动区后调一次，把视口宽 / 可用下沿报给引擎（表格据此排版）；与后端无关 |
| `protected virtual void SetupUI(VisualElement root)` | 搭建窗口 UI 的钩子；默认整窗一个滚动区，自己有布局（如菜单 + 分割条）的窗口重写它 |
| `protected VisualElement CreateContentArea(VisualElement parent, Action draw, bool scroll = true)` | 在自定义布局里创建一块引擎内容容器并接上重绘链（`scroll=false` 不带滚动） |

要直接调 `Engine.DrawObject`（例如在 `EditorWindow` 体系外自己造一个宿主）走 `Engine.CreateHostState()` + 带 `HostState` 的重载 —— 那是 cn.rendu.draw 的事，详见 [cn.rendu.draw README §8](../cn.rendu.draw/README.md#8-辅助-api)。
