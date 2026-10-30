# cn.rendu.inspector 使用教程

`cn.rendu.inspector` 是**检视器宿主胶水**：把 `cn.rendu.draw` 的绘制引擎挂进 Unity 原生检视器。你不用写任何 GUI 代码——给对象标 `[Button]` / `[ShowInInspector]` 等特性，面板自动出来。

依赖：`cn.rendu.draw`（特性 + 引擎）、`cn.rendu.serialization`（`SerializedScriptableObject`）。

## 第 1 章 给已有 MonoBehaviour / ScriptableObject 一键上检视器

```csharp
using RD.Inspector.Editor;
using RD.Draw;            // 特性
using UnityEditor;

public class PlayerConfig : ScriptableObject
{
    public int Level;
    [ShowInInspector] public string Computed => $"Lv{Level}";
    [Button("打印配置")] void Print() => Debug.Log(Computed);
}

[CustomEditor(typeof(PlayerConfig))]
public class PlayerConfigEditor : InspectorEditor { }   // 一行即可
```

`PlayerConfig` 上的 `[Button]` / `[ShowInInspector]` / `[ShowIf]` / `[TableList]` 等 draw 特性即刻生效。

> 必须**显式**用 `[CustomEditor(typeof(X))]` 注册。本包刻意不全局替换原生检视器（`InspectorEditor` 不挂 `[CustomEditor(typeof(MonoBehaviour), true)]`），避免污染全工程。

## 第 2 章 做需要 JSON 持久化的编辑器资产

Unity 原生序列化不认 `Dictionary` / 嵌套 POCO / `HashSet`，用 `SerializedScriptableObject` + `[Serialize]` 解决：

```csharp
using RD.Serialization.Editor;   // SerializedScriptableObject + [Serialize]
using RD.Draw;

public class LootTable : SerializedScriptableObject
{
    [Serialize] public Dictionary<string, DropEntry> Drops = new();
    [Serialize] public List<DropEntry> Flat = new();

    [Button("重置")] void ResetTable() => Drops.Clear();

    // 读盘后重算派生值
    protected override void OnAfterInspectorDeserialize() { /* 重算 */ }
}
// 无需写 Editor 类：SerializedScriptableObjectEditor 已用
// [CustomEditor(typeof(SerializedScriptableObject), true)] 整族登记，子类免登记
```

要点：
- 继承 `SerializedScriptableObject` 即自带整族检视器，**子类无需各自登记**（在 `cn.rendu.inspector` 里一次性覆盖，避免循环依赖）。
- 普通 `public` 字段走 Unity 原生序列化；`Dictionary` / 嵌套 POCO / `HashSet` 等复杂类型走 JSON（由基类在 `OnBeforeSerialize` / `OnAfterDeserialize` 自动处理）。
- **仅限编辑器资产**（基类在 Editor 程序集）。

## 第 3 章 进阶与限制

- 覆盖继承族：`[CustomEditor(typeof(MyBaseBehaviour), true)]`（`true` = 含所有子类）。
- 多选：`InspectorEditor` 带 `[CanEditMultipleObjects]`，但多选时只画第一个对象并给 HelpBox 警告。
- Undo 由 `InspectorEditor` 在写入前自行拍快照，粒度与引擎 `BeginChange` 不同。

## 交叉引用

- `cn.rendu.draw/README.md` —— 特性手册 + 成员可见性规则。
- `cn.rendu.draw/Docs/Tutorial.md` —— 从零接入到工具窗口的实操步骤 + 排错手册。
- `cn.rendu.serialization/README.md` —— `[Serialize]` 成员筛选规则、JSON 容错、命名空间坑。
