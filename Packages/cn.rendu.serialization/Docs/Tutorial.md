# cn.rendu.serialization 使用教程

`cn.rendu.serialization` 是**独立序列化器**（位于 `cn.rendu.draw` 之下，仅依赖 `cn.rendu.utilities`）。Runtime 提供 `[Serialize]` 特性与值包判定；Editor 提供 JSON 序列化内核与资产载体 `SerializedScriptableObject`。绘制引擎向上依赖本包取这两样，双方共用同一条值包判定。

## 第 1 章 继承 SerializedScriptableObject 做编辑器资产（最常见）

```csharp
using RD.Serialization;
using RD.Serialization.Editor;

public class MyConfig : SerializedScriptableObject
{
    [Serialize] public Dictionary<string, PackageHubData> AllData = new();
    public string Note;                       // public 字段默认进 JSON
    [SerializeField] int hidden;             // SerializeField 也算数
    [NonSerialized][Serialize] public long LastUpdateTime; // 也可标非序列化
}
```

- 自动落盘：基类在 `OnBeforeSerialize` → `m_Data = Serialization.Serialize(this)`；`OnAfterDeserialize` → `Deserialize` 后调 `protected virtual OnAfterInspectorDeserialize()`（读盘后重算派生值的钩子）。
- 资产存盘由 Unity 原生序列化（JSON 在 `m_Data` 字符串字段里），无需你手写读写。

## 第 2 章 纯 JSON 往返（脱离 ScriptableObject）

```csharp
using RD.Serialization;            // [Serialize]（真实命名空间，非 RD.Draw）
using RD.Serialization.Editor;     // Serialization

[Serialize] public class PlayerData
{
    [Serialize] public int Level = 5;
    [Serialize] public List<string> Tags = new();
    [Serialize] public Dictionary<string, int> Score = new();
}

string json = Serialization.Serialize(new PlayerData());   // 序列化
var target = new PlayerData();
Serialization.Deserialize(target, json);                  // 读回（同构、按成员名匹配）
```

**成员筛选三条规则**（`GetDataMembers`）：
1. Unity 可序列化值类型（Vector/Quaternion/Rect/Color…）按公开/私有实例字段；
2. `ValueBagTypes.IsUnityValueBagType` 命中（AnimationCurve / Gradient）只取公开读写属性；
3. 其余用户类型只认 `[Serialize]` 字段与属性。

> ⚠️ **坑**：忘了标 `[Serialize]` 的成员会静默写成 `{}`，读回是 default——不是报错。

## 第 3 章 两个必须预先点明的坑

1. **特性命名空间是 `RD.Serialization`，不是 `RD.Draw`**。写 `[Serialize]` 要 `using RD.Serialization;`（本包 `README.md` 里 `using RD.Draw;` 那行是错的）。
2. **`Serialization` 同名类/命名空间 → CS0118**：`Serialization` 既是类（`RD.Serialization.Editor.Serialization`）又是命名空间片段。在 `RD.*` 命名空间的文件里裸写 `Serialization` 会先命中命名空间。两种写法：
   - 全限定：`RD.Serialization.Editor.Serialization.Serialize(obj)`
   - 别名：`using SerializationKernel = RD.Serialization.Editor.Serialization;`

## 交叉引用

- `cn.rendu.draw` —— 绘制引擎，向上依赖本包取 `[Serialize]` 与值包判定。
- `cn.rendu.inspector` —— `SerializedScriptableObject` 的整族检视器由它一键登记。
- `cn.rendu.utilities` —— `SerializationUtility.CreateCopy` 是深拷贝门面（区别于本包 JSON 序列化）。
