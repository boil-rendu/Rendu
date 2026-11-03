# cn.rendu.utilities 使用教程

`cn.rendu.utilities` 是**通用工具库（运行时）**：集合扩展、反射深拷贝、字符串/数值/类型工具、强类型 `PlayerPrefs` 封装。零业务包依赖（但仍依赖 `UnityEngine`，故 Unity 构建内可用，纯非 Unity 上下文不行）。

命名空间统一 `RD.Utilities`；只有一个程序集 `RD.Utilities`，`autoReferenced: true`，所有平台（含 Editor 和运行时构建）都编译。

## 第 1 章 ObjectExtensions.Clone<T> 反射深拷贝

最独特、最易误用的能力：

```csharp
using RD.Utilities;

var copy = original.Clone();   // 深拷任意对象图
```

语义：
- 逐字段复制（含私有字段 / 自动属性后备字段 / 继承成员）；数组 / `IList` / `IDictionary` 递归深拷；
- 环引用靠 `visited` 表兜底；超深（64 层 `MaxDepth`）按浅拷截断；
- `string` / 枚举 / 基元 / 委托 / `UnityEngine.Object` 按引用共享（不复制）。

> 与 `cn.rendu.serialization` 的 `SerializationUtility.CreateCopy` 不同：`Clone` 是**对象图复制**（不转 JSON），适合在内存里做快照/分支。

## 第 2 章 PrefsValue<T> 强类型偏好（含分组）

```csharp
using RD.Utilities;

private readonly PrefsValue<int> m_Width = new("MyTool_Width", null, 300);
int w  = m_Width.Value;     // 读（首次惰性加载 + 缓存）
m_Width.Value = 400;        // 写（即时进 PlayerPrefs，刷盘由 Unity 负责）
int w2 = m_Width;           // 隐式转换，等价于读
m_Width.Delete();
```

- `PrefsValue<T>(string key, IGroupKey groupKey = null, T defValue = default)`，`T` 支持 string/int/float/bool/枚举。
- 分组：传 `IGroupKey`（如按 userid）隔离同一 key 的多份值。数组版 `ArrPlayerPrefsValue<T>` 把数组序列化进单条偏好。

## 第 3 章 字符串与类型工具链

```csharp
using RD.Utilities;

var text = StrUtil.Concat("a", "b");             // 拼接（走池，免分配）
var num  = StrConv.ToI32("123");                 // 安全数值解析（带 out 版）
var nt   = TypeExt.GetNumericType(typeof(int));  // 数值类型判定
var sb   = SbPool.Get(); sb.Append("..."); string s = SbPool.PutAndToStr(sb);  // StringBuilder 对象池
```

- `StrUtil`：拼接 / 占位符 `{%key%num}`（`GetStrChunk`）/ 路径扩展名替换等。
- `StrConv`：U16/U32/U64/I32/Float/Byte 解析 + 千分比 `ToPer1000`。
- `TypeExt`：数值类型判定与字段/属性读写。
- `SbPool`：StringBuilder 对象池，配合 `StrUtil` 免 GC。
- `CollectionExtensions.AddRange<T>`：`HashSet<T>` 批量添加（BCL 只给 `List` 加）。

> 当前**没有自动化测试**；上面示例可作为首批 EditMode 用例。

## 交叉引用

- `cn.rendu.serialization/README.md` —— 说明 `Clone` 是对象图复制、非序列化落盘，两者功能不同。
- `cn.rendu.packagemanager` —— `SbPool` / `StrUtil` / `StrConv` / `TypeExt` / `PrefsValue` 的真实调用方示例。
