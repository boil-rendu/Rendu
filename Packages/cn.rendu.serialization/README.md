# RD.Serialization

编辑器序列化层：把"数据怎么存"从绘制引擎和两个宿主里摘出来，单独成包。

- **`Serialization`**：`[Serialize]` 成员的 JSON 序列化内核（成员筛选、写出与读回）；
- **`SerializedScriptableObject`**：带 JSON 落盘的编辑器资产载体（`ScriptableObject` + `EditorJsonUtility`）。

特性与绘制引擎在 [`cn.rendu.draw`](../cn.rendu.draw/README.md)；检视器宿主 [`cn.rendu.inspector`](../cn.rendu.inspector/README.md) 与窗口宿主 [`cn.rendu.edittool`](../cn.rendu.edittool/README.md) **都可引用本包，互不依赖**。

## 包结构

```
cn.rendu.serialization/
├── package.json                     包元信息（依赖 cn.rendu.draw + cn.rendu.utilities）
├── Editor/
│   ├── RD.Serialization.Editor.asmdef   程序集：RD.Serialization.Editor（Editor 平台）
│   ├── AssemblyInfo.cs              开放 Serialization 的 internal 给测试程序集（不扩公共 API）
│   ├── Serialization.cs             [Serialize] 成员的 JSON 序列化内核
│   ├── SerializedScriptableObject.cs  资产载体：ScriptableObject + JSON 落盘
│   └── SerializationUtility.cs      深拷贝门面：CreateCopy 转调 RD.Utilities 的深拷
├── Scripts/Editor/Share/            AssemblyReference.asmref（引用 RD.Editor，供使用方归并 Editor 程序集）
└── Tests/Editor/                    EditMode 测试（程序集 RD.Serialization.Tests）
```

分层：`cn.rendu.draw`（特性 + 绘制引擎）← **本包**（序列化）← `cn.rendu.inspector` / `cn.rendu.edittool`（两个并列宿主）。

## 命名空间的坑：`Serialization` 是类，也是命名空间的一段

本包的命名空间 `RD.Serialization.Editor` 里有一个**同名的类** `Serialization`。在 `RD.*` 命名空间下的文件里**裸写 `Serialization`**，名称解析会先在外层命名空间 `RD` 上命中命名空间成员 `RD.Serialization`，报 CS0118（"Serialization 是命名空间，但用法像类型"）——`using RD.Serialization.Editor;` 救不了它（using 要等所有外层命名空间都找不到才生效）。两种写法二选一：

- 全限定：`RD.Serialization.Editor.Serialization.Serialize(obj)`；
- 别名：`using SerializationKernel = RD.Serialization.Editor.Serialization;`。

`SerializedScriptableObject`（用户真正继承的类）名字唯一，没有这个问题。

## 依赖

| 程序集 | 引用 | 说明 |
|---|---|---|
| `RD.Serialization.Editor` | `RD.Draw`、`RD.Draw.Editor`、`RD.Utilities` | `[Serialize]` 特性在 Runtime 侧；成员规则与引擎同源（复用引擎公开的 `Engine.IsUnityValueBagType`）；`RD.Utilities` 供 `SerializationUtility.CreateCopy` 转调深拷 |
| `RD.Serialization.Tests` | `RD.Serialization.Editor`、`RD.Draw` | 仅测试用；测试与被测代码同命名空间（见上节的坑） |

## 深拷贝门面：SerializationUtility

`SerializationUtility.CreateCopy<T>(obj)` 由本包的同名静态类承接，内部转调 [cn.rendu.utilities](../cn.rendu.utilities/README.md) 的 `ObjectExtensions.Clone`：

```csharp
using RD.Serialization.Editor;

var copy = SerializationUtility.CreateCopy(original);   // = original.Clone()
```

深拷实现只有一份（在 `RD.Utilities`，运行时可用），这里只是编辑器侧的兼容入口。

## 用法

```csharp
using RD.Draw;                // 特性命名空间
using RD.Serialization.Editor; // SerializedScriptableObject（本包）
using UnityEditor;

public class MyConfig : SerializedScriptableObject
{
    [Serialize] public List<DropEntry> Drops = new();
    public string Note;                       // public 字段默认进 JSON
    [SerializeField] int hidden;              // SerializeField 同样算数
}
```

- **普通字段走 Unity 原生序列化**，`[Serialize]` 标记的、以及 public / `[SerializeField]` 的**复杂成员**（Dictionary、嵌套 POCO、HashSet 等 Unity 序列化器不认的类型）走 JSON 落盘，存进同一个 asset；
- **继承族自带检视器**：`[CustomEditor(typeof(SerializedScriptableObject), true)]` 注册在 cn.rendu.inspector（`SerializedScriptableObjectEditor`），子类散布各包（如 packagemanager 的 PackageHubAsset / PackageVersionAsset）无需再登记；
- 可选钩子：`OnAfterInspectorDeserialize()`（读盘后重算派生值用；与 Unity 的 `OnAfterDeserialize` 接口方法同名冲突，故保留 Inspector 前缀）；
- **仅限编辑器资产**：基类在 Editor 程序集，运行时资产绝不能用。

## 测试

`Tests/Editor/SerializationTests.cs`（程序集 `RD.Serialization.Tests`，命名空间与被测代码同层）：1 个夹具 19 例，钉住成员筛选三条规则（特性 / Unity 值类型 / 属性包类型）、写出与读回的容错行为。用 Unity Test Runner 跑（`Window > General > Test Runner` → `EditMode`）：

- 其中 2 例（`Serialize_UnityObjectReferenceIsNotPersisted`、`RoundTrip_Gradient`）依赖 Unity 运行时对象（`ScriptableObject` / 曲线 / 渐变在非 Unity 进程里 `new` 不出来），**只能在 Test Runner 内跑**；
- 测试通过 `Editor/AssemblyInfo.cs` 的 `InternalsVisibleTo` 访问 `Serialization` 的 internal 成员规则（`GetDataMembers` 等），不扩大公共 API。

## 与其他包的分工

| 包 | 职责 |
|---|---|
| `cn.rendu.draw` | 特性 + 绘制引擎：任意对象 → 属性面板 |
| `cn.rendu.serialization`（本包） | JSON 序列化内核 + 资产载体（两个宿主都能用，互不依赖） |
| `cn.rendu.inspector` | 检视器宿主：面板挂到 `UnityEditor.Editor`（含 `SerializedScriptableObject` 整族登记） |
| `cn.rendu.edittool` | 窗口宿主 + 工具模块框架：左菜单树 + 右面板的工具窗口 |
