# RD.Utilities

通用工具库（**运行时**，零依赖）：不引用任何业务包，只用 BCL + `UnityEngine`，构建里也能用。

## 包结构

```
cn.rendu.utilities/
├── package.json                     包元信息（零依赖）
├── Runtime/
│   ├── RD.Utilities.asmdef          程序集：RD.Utilities（运行时平台，autoReferenced）
│   ├── CollectionExtensions.cs      集合扩展：HashSet<T>.AddRange(IEnumerable<T>)
│   ├── ObjectExtensions.cs          反射深拷：T.Clone<T>()
│   ├── StrUtil.cs                   字符串拼接/处理（partial，与 StrUtil_StrChunk 同类）
│   ├── StrUtil_StrChunk.cs          StrUtil 的 chunk 部分（partial）
│   ├── StrConv.cs                   字符串 ↔ 数值/类型转换（含 GetNumericType 分发）
│   ├── TypeExt.cs                   类型反射帮助（数值类型判定等）
│   ├── SbPool.cs                    StringBuilder 对象池（Get / Put / PutAndToStr）
│   └── PrefsValue.cs          PlayerPrefs 强类型包装（PrefsValue<T> + ArrPlayerPrefsValue<T> + 分组）
└── Scripts/Editor/Share/            AssemblyReference.asmref（引用 RD.Editor，供使用方归并 Editor 程序集）
```

来源：`CollectionExtensions` / `ObjectExtensions` 原在 `cn.rendu.draw/Runtime/Utilities/`，其余 6 个文件原在 `cn.rendu.packagemanager/Editor/Tools/`（2026-10 提取到本包，两个包都不再各自持有）。

## 提供什么

| API | 功能 |
|---|---|
| `CollectionExtensions.AddRange<T>(this HashSet<T>, IEnumerable<T>)` | `HashSet` 批量添加（BCL 只有 `List` 有 `AddRange`） |
| `ObjectExtensions.Clone<T>(this T)` | 反射深拷：逐字段复制（含私有字段 / 自动属性后备字段 / 继承成员），数组 / `IList` / `IDictionary` 递归深拷，环引用靠 visited 表兜底，超深（64 层）按浅拷截断；不可变或不可复制的类型（string / 枚举 / 基元 / 委托 / `UnityEngine.Object`）按引用共享。目标类型没有实例 `Clone()` 时，`x.Clone()` 解析到本扩展方法 |
| `StrUtil.Concat(...)` / `FormatName` / `ReplaceText` / `ToLower` 等 | 字符串拼接 / 模板替换 / 路径扩展名替换（内部走 `SbPool`，免分配） |
| `StrConv.ToI32/ToU32/ToFloat` 等 | 字符串 ↔ 数值转换（带 `out` 安全版与直接版） |
| `TypeExt.GetNumericType(this Type)` 等 | 数值类型判定与类型反射帮助 |
| `SbPool.Get() / Put() / PutAndToStr()` | `StringBuilder` 池，配 `StrUtil` 使用 |
| `PrefsValue<T>` | `PlayerPrefs` 强类型包装：把一个设置项当变量用（首次惰性读并缓存、值没变跳过写、可 `Delete()`、支持隐式转换 `T v = prefs;`），支持按 `IGroupKey`（如 userid）分组；T 支持 string/int/float/bool/枚举；构造签名 `(key, groupKey = null, defValue = ...)` |
| `ArrPlayerPrefsValue<T>` | 数组偏好：`T[]` 序列化成一条字符串存（空数组 = 删偏好），配 `Get() / Set() / Delete()` |

`Clone` 是**对象图复制**，不涉及任何格式与落盘 —— 与 [cn.rendu.serialization](../cn.rendu.serialization/README.md)（JSON 序列化）是两个功能，这也是它独立成包、不住进 Serialization 的原因。

## 用法

```csharp
using RD.Utilities;

var copy   = original.Clone();                    // 深拷任意对象
myHashSet.AddRange(someEnumerable);               // HashSet 批量添加
var text   = StrUtil.Concat("a", "b");            // 字符串拼接（走池）
var num    = TypeExt.GetNumericType(typeof(int)); // 数值类型判定

// PlayerPrefs：设置项即变量（可带分组，构造签名 (key, groupKey, defValue)）
private readonly PrefsValue<int> m_Width = new("MyTool_Width", null, 300);
int w = m_Width.Value;                            // 读（首次惰性加载，之后走缓存）
m_Width.Value = 400;                              // 写（即时进 PlayerPrefs，刷盘由 Unity 负责）
int w2 = m_Width;                                 // 隐式转换，等价于读
```

## 测试

当前**没有自动化测试**。`Clone` 的环引用 / 深度截断 / 不可变共享语义、`PrefsValue<T>` 的分组读写值得补 EditMode 用例，随第一个生产调用方一起补。

## 已知调用方

- **cn.rendu.serialization** 的 `SerializationUtility.CreateCopy`（深拷贝门面）转调 `Clone`；
- **cn.rendu.packagemanager**：`SbPool` / `StrUtil` / `StrConv` / `TypeExt` / `PrefsValue<T>`（原包内自实现，2026-10 提取到本包）。
