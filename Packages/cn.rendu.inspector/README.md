# RD.Inspector

检视器宿主包：把 [cn.rendu.panel](../cn.rendu.panel/README.md) 的绘制引擎（`Engine.DrawObject`）挂到 Unity 的两个原生位置——**组件/资产的 Inspector 面板**与**带 JSON 落盘的编辑器资产**。本包不含任何绘制逻辑。

## 包结构

```
cn.rendu.inspector/
├── Editor/
│   ├── RD.Inspector.Host.asmdef        程序集：RD.Inspector.Host（Editor 平台）
│   ├── Inspector/
│   │   ├── InspectorEditor.cs          InspectorEditor：挂到 UnityEditor.Editor 的检视器基类
│   │   ├── InspectorBackend.cs         绘制后端开关（IMGUI / UI Toolkit，含菜单项）
│   │   └── SerializedScriptableObjectEditor.cs   为序列化基类整族注册检视器
│   └── Serialization.cs                SerializedScriptableObject + Serialization(JSON 读写规则)
└── Tests/Editor/                       EditMode 测试（RD.Inspector.Host.Tests，19 例）
```

## 依赖

| 程序集 | 引用 | 说明 |
|---|---|---|
| `RD.Inspector.Host` | `RD.Inspector`、`RD.Inspector.Editor` | 引擎与特性都在 cn.rendu.panel |

## 给 MonoBehaviour / ScriptableObject 接检视器

写一个继承 `InspectorEditor` 的类，用 Unity 原生 `[CustomEditor]` 注册：

```csharp
using RD.Inspector.Editor;
using UnityEditor;

[CustomEditor(typeof(EnemyConfig))]
public class EnemyConfigEditor : InspectorEditor { }
```

`EnemyConfig` 上的 `[Button]` / `[ShowIf]` / `[TableList]` 等特性即刻生效。已知限制（反射写值的代价）：不显示 prefab override 标记、不做多对象差异编辑（多选时只画第一个并给 HelpBox 警告）、Undo 由本类在"写入前"自行拍快照（一次交互一条）。

### 切换绘制后端（IMGUI / UI Toolkit）

切换收在包内，业务代码零改动 —— 所有继承 `InspectorEditor` 的检视器一起跟着开关走：

- 菜单 **Tools / RD Inspector / UI Toolkit 绘制**（带勾选态）；
- 或任意编辑器代码里 `InspectorBackend.UseUIToolkit = true`。

选择持久化在 EditorPrefs，重启后保持。切换在检视器重建时生效（菜单会尝试重绘所有视图，个别没变的重选一下对象即可）。

两条分支都在 `InspectorEditor` 内部分叉：IMGUI 分支用 `IMGUIContainer` 承载原来的 `OnInspectorGUI`（逐帧行为与从前一致），UI Toolkit 分支画进 `VisualElement` 树并由 `UitkDriver` 的重绘回调驱动。Undo / 写脏 / prefab override 登记在两种后端下完全相同，写入语义不变。

注意：UI Toolkit 模式下走的是引擎的反射绘制，子类自己重写的 `OnInspectorGUI` 不会再被执行；要画自定义内容的检视器请保持 IMGUI 模式或自行处理。UI Toolkit 后端的其它差异（富文本降级、按钮图标只支持 `Texture2D` 等）见 panel 包 README 的「绘制后端」一节。

## 编辑器数据资产：SerializedScriptableObject

```csharp
public class MyConfig : SerializedScriptableObject
{
    [InspectorSerialize] public List<DropEntry> Drops = new();
    public string Note;                       // public 字段默认进 JSON
    [SerializeField] int hidden;              // SerializeField 同样算数
}
```

- **普通字段走 Unity 原生序列化**，`[InspectorSerialize]` 标记的、以及 public / `[SerializeField]` 的**复杂成员**（Dictionary、嵌套 POCO、HashSet 等 Unity 序列化器不认的类型）走 JSON 落盘（`EditorJsonUtility`），存进同一个 asset；
- 继承族自带检视器（`[CustomEditor(typeof(SerializedScriptableObject), true)]`），子类不用再登记；
- 可选钩子：`OnAfterInspectorSerialize()` / `OnAfterInspectorDeserialize()`（读盘后重算派生值用）；
- **仅限编辑器资产**：基类在 Editor 程序集，运行时资产绝不能用。

序列化成员筛选的三条规则（特性 / Unity 值类型 / 属性包类型）、写出与读回的容错行为，由 `SerializationTests`（19 例）钉住。

## 测试

**不开 Unity 的快速回归**（`Tools~/Verify/`，与 cn.rendu.panel 同一套路）：

```bash
cd "Packages/cn.rendu.inspector/Tools~/Verify" && dotnet run
```

当前规模：1 个夹具 19 例，脱机 17 通过 / 2 跳过（依赖 Unity 运行时的 `Serialize_UnityObjectReferenceIsNotPersisted`、`RoundTrip_Gradient` 留给 Unity Test Runner）。

## 与其他两包的分工

| 包 | 职责 |
|---|---|
| `cn.rendu.panel` | 特性 + 绘制引擎：任意对象 → 属性面板 |
| `cn.rendu.inspector`（本包） | 检视器宿主：面板挂到 `UnityEditor.Editor` / 编辑器资产 |
| `cn.rendu.editorwindow` | 窗口宿主：左菜单树 + 右面板的工具窗口 |
