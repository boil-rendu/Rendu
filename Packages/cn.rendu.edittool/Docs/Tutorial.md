# cn.rendu.edittool 使用教程

`cn.rendu.edittool` 是**编辑器窗口宿主框架**：在 `cn.rendu.draw`（绘制引擎）之上，提供“左树菜单 + 右面板”的工具窗口骨架。你写的业务模块挂进左树，右面板由 `Engine.DrawObject` 把任意 C# 对象画成属性面板。

> 本包窗口基类为 `DrawWindow`（旧文档曾误称 `EditorWindowBase`），持久化偏好用 `PrefsValue<T>`（来自 `cn.rendu.utilities`，旧文档曾误称 `EditorPrefsValue<T>`）。本文以源码为准。

## 第 0 章 这是什么

继承关系：`DrawWindow(abstract : EditorWindow)` → `MenuEditorWindow(partial)` → `GenericToolWindow(abstract)` → 你的窗口。
模块：`BaseToolModule` → `BaseTreeMenuItem` → `TreeMenuItem<T>`。

## 第 1 章 三步开一个工具窗口

1. 窗口类继承 `GenericToolWindow`，加一个 `[MenuItem]` 静态入口即可——左树、选中持久化、生命周期全由基类包办。

   ```csharp
   using RD.EditTool.Editor;   // GenericToolWindow
   using UnityEditor;

   public class MyToolWindow : GenericToolWindow
   {
       [MenuItem("RD/MyTool/打开")]
       public static void Open() => GetWindow<MyToolWindow>();
   }
   ```

2. 模块类继承 `BaseToolModule`，挂 `[ToolMenu]` 即入左树：

   ```csharp
   using RD.EditTool.Editor;

   [ToolMenu("我的模块")]          // 也可 [ToolMenu("我的模块", order: 0)] 控制顺序
   public class MyModule : BaseToolModule
   {
       public override void Initialize() { }   // 首次选中时一次
       public override void OnDestroy() { }     // 窗口销毁时统一调用
   }
   ```

   `[ToolMenu]` 属性：`ToolMenuAttribute(string menuName, int order = 0)`。`order` 越小越靠前；可写 `-int.MaxValue`（恒最前）、`1000`（最后）。`Scope` 非空时仅出现在 `AcceptScope` 匹配的窗口。

3. 右面板内容：模块里放一个公开字段/属性，引擎会按成员画出来（见第 2 章）。

> 最干净的现成范例：`cn.rendu.packagemanager` 的 `RDPackageManagerWindow`（窗口入口）+ 任一 `RDPackage*Module`（模块）。直接抄它。

## 第 2 章 用 MenuTree 建树 + 用 DrawObject 画面板

- 左树是数据模型 `MenuTree`：`Add("A/B/C", value, icon)` 按路径自动补中间层、同名就地替换；`SelectFirstMenuItem` / `SelectMenuItem` / `FindMenuItem` / `MoveItem` / `ForEach`。
- 菜单项的 `Value` 可以是**任意普通 C# 对象**——选中后 `MenuEditorWindow.DrawSelectedTarget(target)` 调 `Engine.DrawObject(target)` 把它画成右面板。
- 面板上出控件靠 `cn.rendu.draw` 的 Runtime 特性：`[Button]`、`[ShowInInspector]`、`[HideIf]`、`EditorIcons.*` 等。

   ```csharp
   public class MyModule : BaseToolModule
   {
       [Button("执行")] void Do() { }
       [ShowInInspector] public int Count = 0;
   }
   ```

- **编辑可序列化资产要标脏**：默认 `DrawObject` 只改内存；改 `UnityEngine.Object` 请用 `Engine.BeginChange/EndChange` 包住（Undo / 脏标记 / 重绘由引擎统一管）。纯 C# 对象走 `onBeforeChange` / `onChanged` 回调。详见 `cn.rendu.draw/Docs/Execution/01-共享胶水与术语.md`。

## 第 3 章 生命周期与扩展点

- `Initialize()`：模块首次被选中时调用一次。
- `OnDestroy()`：窗口销毁时统一调用（装配/清理内嵌面板放这）。
- `OnToolInitialize` / `OnToolDestroy`：窗口级装配（挂在自身 `[ShowInInspector]` 字段上的内嵌面板）。
- `BuildToolMenuTree(MenuTree)`：先 `base` 拿到反射结果，再头部/尾部加根节点。
- `AcceptScope`：限定本窗口只加载某作用域模块。
- `NewTreeMenuItem(Type, string)`：**手动加模块必须走它**，否则窗口销毁时该模块 `OnDestroy` 不会被调用。

## 第 4 章 持久化（上次选中 / 菜单宽）

用 `PrefsValue<T>`（`cn.rendu.utilities`，包裹 `PlayerPrefs`），支持 string/int/float/bool/枚举：

```csharp
using RD.Utilities;
private readonly PrefsValue<string> m_LastSelect = new("MyTool_LastSelect", null, "");
string last = m_LastSelect;          // 读（首次惰性加载 + 缓存）
m_LastSelect.Value = "我的模块";     // 写
```

## 交叉引用

- `cn.rendu.draw/Docs/Execution/01-共享胶水与术语.md` —— `BeginChange/EndChange` 与术语。
- `cn.rendu.draw/Docs/Execution/03-分包重设计与宿主契约.md` —— `DrawWindow` / `GenericToolWindow` 宿主契约（本框架自身结构）。
- `cn.rendu.draw/README.md` —— 绘制引擎特性手册。
