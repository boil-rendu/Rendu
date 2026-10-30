# RD.Inspector

检视器宿主包：把 [cn.rendu.draw](../cn.rendu.draw/README.md) 的绘制引擎（`Engine.DrawObject`）挂到 Unity 的**组件/资产的 Inspector 面板**上，并为 [cn.rendu.serialization](../cn.rendu.serialization/README.md) 的编辑器数据资产做整族 `[CustomEditor]` 登记。本包不含任何绘制逻辑、不含序列化内核。

## 包结构

```
cn.rendu.inspector/
├── Editor/
│   ├── RD.Inspector.Editor.asmdef     程序集：RD.Inspector.Editor（Editor 平台）
│   ├── AssemblyInfo.cs                预留：开放 internal 给本包测试程序集（当前无用例）
│   ├── InspectorEditor.cs             InspectorEditor：挂到 UnityEditor.Editor 的检视器基类（CreateInspectorGUI + Driver）
│   └── SerializedScriptableObjectEditor.cs   为 cn.rendu.serialization 的资产载体整族注册检视器
├── Scripts/Editor/Share/               AssemblyReference.asmref（引用 RD.Editor，供使用方归并 Editor 程序集）
└── Tests/Editor/                       预留测试程序集（RD.Inspector.Tests，当前无用例：宿主是纯 GUI 胶水）
```

三个 .cs 平铺在 `Editor/` 下（不设 `Inspector/` 子文件夹 —— 包名/命名空间已经是 Inspector，再套一层只是 stutter）：全部是"把引擎挂到 Unity 检视器"的宿主胶水，不含绘制逻辑、不含特性定义、不含序列化内核。

本包只有三个 .cs 文件，全部是"把引擎挂到 Unity 检视器"的宿主胶水：不含绘制逻辑、不含特性定义、不含序列化内核。

## 依赖

| 程序集 | 引用 | 说明 |
|---|---|---|
| `RD.Inspector.Editor` | `RD.Draw`、`RD.Draw.Editor`、`RD.Serialization.Editor` | 引擎与特性在 cn.rendu.draw；资产载体在 cn.rendu.serialization（整族登记要用到） |

## 给 MonoBehaviour / ScriptableObject 接检视器

写一个继承 `InspectorEditor` 的类，用 Unity 原生 `[CustomEditor]` 注册：

```csharp
using RD.Inspector.Editor;
using UnityEditor;

[CustomEditor(typeof(EnemyConfig))]
public class EnemyConfigEditor : InspectorEditor { }
```

`EnemyConfig` 上的 `[Button]` / `[ShowIf]` / `[TableList]` 等特性即刻生效。已知限制（反射写值的代价）：不显示 prefab override 标记、不做多对象差异编辑（多选时只画第一个并给 HelpBox 警告）、Undo 由本类在"写入前"自行拍快照（一次交互一条）。

### 绘制后端：单一 UIElements 后端

`InspectorEditor` 走保留模式：重写 `CreateInspectorGUI()` 返回一棵 `VisualElement` 树，引擎把目标对象画进树里的容器，由 `Driver` 按位置复用元素。业务代码零感知、无需选择后端。

与 Unity 原生检视器的差异（反射写值的固有限制，接入前需确认可接受）：

- 不显示预制体覆盖（override）标记，也无法用右键菜单还原单个属性；
- 不做多对象差异编辑，多选时只绘制第一个对象并给 HelpBox 警告（见 `CreateInspectorGUI`）；
- Undo 由本类在"写入前"自行拍快照（一次交互一条），与 `SerializedProperty` 的行为略有差别。

引擎绘制的其它降级差异（富文本降级、按钮图标只支持 `Texture2D` 等）见 [cn.rendu.draw README §2.4](../cn.rendu.draw/README.md#24-绘制后端-单一-uitelements-后端)。

## 编辑器数据资产：SerializedScriptableObject

> 载体 `SerializedScriptableObject` 与 JSON 内核 `Serialization` 在独立包 **cn.rendu.serialization**（`Editor/`，程序集 `RD.Serialization.Editor`，命名空间 `RD.Serialization.Editor`）—— 数据资产层不依赖任何宿主，检视器与工具窗口都能用；
> 本包只保留 `[CustomEditor(typeof(SerializedScriptableObject), true)]` 的整族登记（子类散布各包，如 packagemanager 的 PackageHubAsset / PackageVersionAsset，在此一次覆盖，见 Inspector/SerializedScriptableObjectEditor.cs）。
> 用法（继承 + `[Serialize]`）完全不变，示例里的类型从 `using RD.Serialization.Editor;` 引入。

```csharp
using RD.Serialization.Editor;                  // SerializedScriptableObject（cn.rendu.serialization）

public class MyConfig : SerializedScriptableObject
{
    [Serialize] public List<DropEntry> Drops = new();
    public string Note;                       // public 字段默认进 JSON
    [SerializeField] int hidden;              // SerializeField 同样算数
}
```

- **普通字段走 Unity 原生序列化**，`[Serialize]` 标记的、以及 public / `[SerializeField]` 的**复杂成员**（Dictionary、嵌套 POCO、HashSet 等 Unity 序列化器不认的类型）走 JSON 落盘（cn.rendu.serialization 的序列化内核），存进同一个 asset；
- 继承族自带检视器（`[CustomEditor(typeof(SerializedScriptableObject), true)]`），子类不用再登记；
- 可选钩子：`OnAfterInspectorDeserialize()`（读盘后重算派生值用；与 Unity 的 `OnAfterDeserialize` 接口方法同名冲突，故保留 Inspector 前缀）；
- **仅限编辑器资产**：基类在 Editor 程序集，运行时资产绝不能用。

序列化成员筛选的三条规则（特性 / Unity 值类型 / 属性包类型）、写出与读回的容错行为，由 cn.rendu.serialization 的 `SerializationTests`（19 例）钉住。

## 测试

本包**不设自己的用例**：检视器宿主是纯 GUI 胶水（挂 `Engine.DrawObject` + Undo/SetDirty），没有可脱离 GUI 自动化的逻辑，改它请手工回归。相关行为契约在：

- **cn.rendu.serialization**（`SerializationTests` 19 例）：JSON 序列化内核的成员规则 / 写出读回容错；
- **cn.rendu.draw**（`Tests/Editor`）：引擎侧成员收集 / 条件显隐 / 折叠状态 / 表达式；
- **cn.rendu.edittool**（`MenuTreeTests` 14 例）：菜单树契约。

## 与其他包的分工

| 包 | 职责 |
|---|---|
| `cn.rendu.draw` | 特性 + 绘制引擎：任意对象 → 属性面板 |
| `cn.rendu.serialization` | 编辑器数据资产 + JSON 序列化内核（两个宿主都能用，互不依赖） |
| `cn.rendu.inspector`（本包） | 检视器宿主：面板挂到 `UnityEditor.Editor`；含资产载体的整族登记 |
| `cn.rendu.edittool` | 窗口宿主 + 工具模块框架：左菜单树 + 右面板的工具窗口 |
