# RD.Panel 使用教程

这份文档**教怎么用**；`README.md` 是**特性手册**（全部特性、参数、每个参数的生效情况）。两者分工：

| 你现在的状态 | 该看哪份 |
|---|---|
| 第一次接触，想十分钟内把一个面板跑起来 | 本教程第 1 章 |
| 想做一个完整的工具窗口 / 编辑器资产 | 本教程第 8～12 章 |
| 想查"`[TableList]` 有哪些参数、哪个没实现" | `README.md` §4 速查表 |
| 面板没反应 / 值不落盘 / 表达式不生效 | 本教程第 13 章排错手册 |

**本教程里所有代码都按当前源码逐条核对过**（引擎实际读什么、不读什么，见第 15 章"能力边界"）。写作原则：不写"照 Odin 文档应该如此"的推断，只写"引擎真的会这么画"的结论。

---

## 目录

- [第 0 章 先建立三个认知](#第-0-章-先建立三个认知)
- [第 1 章 十分钟跑通第一个检视器](#第-1-章-十分钟跑通第一个检视器)
- [第 2 章 基本功：标签、排序、数值、提示](#第-2-章-基本功标签排序数值提示)
- [第 3 章 分组：把长面板拆成可读的块](#第-3-章-分组把长面板拆成可读的块)
- [第 4 章 条件显隐与启禁](#第-4-章-条件显隐与启禁)
- [第 5 章 按钮、回调、自定义 GUI](#第-5-章-按钮回调自定义-gui)
- [第 6 章 列表、表格、字典、下拉](#第-6-章-列表表格字典下拉)
- [第 7 章 表达式系统（`@`）](#第-7-章-表达式系统)
- [第 8 章 编辑器数据资产：SerializedScriptableObject](#第-8-章-编辑器数据资产inspectorserializedscriptableobject)
- [第 9 章 工具窗口：EditorWindowBase](#第-9-章-工具窗口inspectoreditorwindow)
- [第 10 章 菜单树窗口：做一个真正的工具](#第-10-章-菜单树窗口做一个真正的工具)
- [第 11 章 自己写宿主：直接用 Engine.DrawObject](#第-11-章-自己写宿主直接用-inspectorenginedrawobject)
- [第 12 章 综合实战：从零做一个关卡配置面板](#第-12-章-综合实战从零做一个关卡配置面板)
- [第 13 章 排错手册（症状 → 原因 → 处理）](#第-13-章-排错手册症状--原因--处理)
- [第 14 章 选型速查（我要 X → 怎么写）](#第-14-章-选型速查我要-x--怎么写)
- [第 15 章 当前版本的能力边界（避免踩空）](#第-15-章-当前版本的能力边界避免踩空)
- [第 16 章 继续深入](#第-16-章-继续深入)

---

## 第 0 章 先建立三个认知

### 0.1 它能画什么：四种宿主

`cn.rendu.panel` = **一套特性**（`Runtime/`，纯数据）+ **一个反射绘制引擎**（`Editor/`，把特性解释成 IMGUI）。引擎的唯一入口是一个静态方法 `Engine.DrawObject`，谁来调它决定了"面板长在哪儿"（检视器宿主在 cn.rendu.inspector 包、窗口宿主在 cn.rendu.editorwindow 包）：

| 宿主 | 你会用到的基类 / API | 典型用途 |
|---|---|---|
| `MonoBehaviour` / `ScriptableObject` | 继承 `InspectorEditor` + `[CustomEditor]`（**cn.rendu.inspector** 宿主包） | 替换某个组件原生检视器 |
| 编辑器数据资产 | 继承 `SerializedScriptableObject`（**cn.rendu.inspector** 宿主包） | 带 JSON 落盘的编辑器配置资产 |
| `EditorWindow` 自身 | 继承 `EditorWindowBase`（**cn.rendu.editorwindow** 包） | 纯工具窗口（不必是 Unity 对象） |
| **任意普通 C# 对象** | 继承 `MenuEditorWindow`（**cn.rendu.editorwindow** 包），或直接调 `Engine.DrawObject(obj, window)` | 工具窗口右半边的"模块面板" |

最后一行是关键差异：**被画的对象不必是 `UnityEngine.Object`**。一个普通 `class LevelModule { [Button("执行")] void Run() {} }` 就能当面板内容，`[Button]` 照常出按钮。这是做编辑器工具时最省事的地方。

![包结构：三层，业务零耦合](images/06-architecture.png)

*Runtime 只放特性定义，Editor 放引擎与宿主；业务类型永远不进入这个包。*

### 0.2 它刻意不做什么

接入前请确认这四条可以接受（这是"反射写值"路线的固有代价，不是 bug）：

1. **不显示预制体 override 标记**，也无法右键还原单个属性；
2. **不做多对象差异编辑**：多选时只画第一个对象，并给一条 `HelpBox` 警告；
3. **不注册任何全局检视器**：本包没有 `[CustomEditor(typeof(MonoBehaviour), true)]` 这种一行改全工程的动作，用哪个类型要你自己登记（唯一的例外是 `SerializedScriptableObject` 的继承族自带检视器，见 8.2）；
4. **不提供任何菜单入口**：全包没有 `[MenuItem]`，窗口入口由你写。

### 0.3 依赖与 asmdef 引用

| 你的代码在哪 | 需要引用 |
|---|---|
| Runtime 代码（只用特性） | `RD.Panel` |
| Editor 代码（直接用引擎） | `RD.Panel.Editor`（它已依赖 `RD.Panel`） |
| 检视器（`InspectorEditor` / `SerializedScriptableObject`） | `RD.Inspector.Editor`（cn.rendu.inspector 包，它已依赖前两者） |
| 工具窗口（`EditorWindowBase` / `MenuEditorWindow` / `MenuTree` / `EditorPrefsValue<T>`） | `RD.EditorWindow.Editor`（cn.rendu.editorwindow 包，它已依赖特性与引擎） |

如果工程用 asmdef 组织，Editor 程序集的 `references` 里要加这两项。本仓库里的真实写法是 `Packages/com.rendu.packagemanager/Editor/RD.PackageManager.Editor.asmdef`（只摘录关键字段）：

```json
{
    "name": "RD.PackageManager.Editor",
    "references": [
        "RD.Panel",
        "RD.Panel.Editor"
    ],
    "includePlatforms": [ "Editor" ]
}
```

命名空间速查（后面所有代码示例都假设已 `using`）：

| 命名空间（= 程序集） | 里面有什么 |
|---|---|
| `RD.Panel`（`RD.Panel.asmdef`） | **全部特性**（`Runtime/Attributes/*`）+ `CollectionExtensions` —— 只有这些，没有任何逻辑代码 |
| `RD.Panel.Editor`（cn.rendu.panel 的引擎程序集） | `Engine`、`ExpressionEvaluator`、`HostState`（`CreateHostState` 发的不透明句柄）、`Log`、`Icons` |
| `RD.Inspector.Editor`（cn.rendu.inspector 宿主包，程序集 `RD.Inspector.Editor`） | `InspectorEditor`、`Serialization` 与 `SerializedScriptableObject`（含它的 Editor）—— 与引擎同名不同包，别引错 |
| `RD.EditorWindows.Editor`（cn.rendu.editorwindow 包） | `EditorWindowBase`、`MenuEditorWindow`、`MenuTree`、`EditorPrefsValue<T>` —— 注意是复数 `EditorWindows`：单数 `RD.EditorWindow` 会遮蔽 `UnityEditor.EditorWindow` 类型（坑同下） |

> 坑：`InspectorEditor` 继承 `UnityEditor.Editor`。因为文件位于 `RD.Inspector.Editor` 命名空间内，**简单名 `Editor` 会被解析成命名空间 `RD.Inspector` 下的 `Editor`**，写 `: Editor` 报 CS0118。基类一定写全：`: InspectorEditor`（本包已提供），自己写别的基类时写 `: UnityEditor.Editor`。

---

## 第 1 章 十分钟跑通第一个检视器

### 1.1 写一个组件

新建 `Assets/Scripts/EnemyConfig.cs`：

```csharp
using RD.Panel;
using UnityEngine;

public class EnemyConfig : MonoBehaviour
{
    [LabelText("等级")]
    [MinValue(1), MaxValue(99)]
    public int Level = 1;

    public string DisplayName;

    [SerializeField]              // 私有字段要露出来必须挂标记
    [LabelText("攻击速度")]        // 不写这行的话标签会是 "m_ Attack Speed"（见下）
    private float m_AttackSpeed = 1f;
}
```

### 1.2 登记检视器

> `InspectorEditor` 在检视器宿主包 **cn.rendu.inspector**（程序集 `RD.Inspector.Editor`）；命名空间 `RD.Inspector.Editor` 不变。

新建 `Assets/Scripts/Editor/EnemyConfigEditor.cs`：

```csharp
using RD.Inspector.Editor;
using UnityEditor;

[CustomEditor(typeof(EnemyConfig))]
public class EnemyConfigEditor : InspectorEditor { }
```

### 1.3 放哪里 / 注意事项

- **`[CustomEditor]` 必须在 Editor 程序集里**（`Editor/` 目录，或 `includePlatforms: ["Editor"]` 的 asmdef），否则打包时会报找不到 `UnityEditor`；
- 类名与文件名要一致（Unity 的常规要求）；
- `InspectorEditor` 内部已经处理好了：
  - 绘制前 `Undo.RecordObject(target, "Inspector")`（写值前的快照 → Ctrl+Z 可用）；
  - 绘制后 `EditorUtility.SetDirty(target)` + `PrefabUtility.RecordPrefabInstancePropertyModifications`（改动能落盘、预制体实例能登记 override）；
  - 外面还套了一层 `EditorGUI.BeginChangeCheck()` / `EndChangeCheck()`：控件式修改由它兜底，按钮与集合增删由引擎的 `onChanged` 回调兜底（两条路都通，漏不掉）；
  - 基类上标了 `[CanEditMultipleObjects]`，但**多选时只画第一个对象**，并在顶部给一条黄色 `HelpBox` 警告；
  - 想一次覆盖整个继承族，可以写 `[CustomEditor(typeof(EnemyConfigBase), true)]`（本包刻意不做 `typeof(MonoBehaviour)` 那种全局注册，以免顶掉工程里其它脚本的原生检视器）。

### 1.4 预期结果

选中挂了这个组件的物体，Inspector 会画出：

```text
等级        |============○========|   (1~99 滑杆)
Display Name| [                 ]   (字符串输入框)
攻击速度     | [ 1.0 ]              (float 输入框)
```

三条规则当场体感：

- `Level` 同时有 `[MinValue]` / `[MaxValue]` → 画成**滑杆**（两个都要有，缺一个就是普通输入框）；
- `DisplayName` 是 public 实例字段 → **自动出现**；
- `m_AttackSpeed` 是 private，靠 `[SerializeField]` 才出现。

**标签从哪来**：按 **成员自身的 `[LabelText]` → 成员「类型」上的 `[LabelText]`（只认枚举）→ `FriendlyName`** 三级回退。最后一级是把成员名"友好化"（`FriendlyName`，正则 `(?<!^)([A-Z][a-z0-9])` → ` $1`），规则是**在"大写字母 + 小写字母/数字"这一对之前插一个空格，且首字符除外**：

| 成员名 | 默认标签 | 说明 |
|---|---|---|
| `AttackSpeed` | `Attack Speed` | 正常驼峰能拆开 |
| `DisplayName` | `Display Name` | 同上 |
| `m_AttackSpeed` | `m_ Attack Speed` | **不会去掉 `m_` 前缀**，所以私有字段默认标签很难看 |
| `MaxHP` | `MaxHP`（不变） | 连续大写不拆：`H` 后面还是大写 |
| `WaveId` | `Wave Id` | 大写后面跟小写/数字才拆 |
| `WaveID` | `WaveID`（不变） | `I` 后面是大写 `D`，不满足拆分条件 |

实践建议：私有字段与连续大写的字段一律配 `[LabelText]`。按钮文字省略时走的是同一套规则（`AddWave` → `Add Wave`，见 5.1）。

`[LabelText]` 不会按名字递归覆盖同类字段。唯一跨成员的一级是上面第二级：**枚举类型**上的 `[LabelText]` 会成为该类型**所有**"没自带标签"的字段的标签（`FitMode` / `EMacroType` 这类场合 —— 枚举自己写不了成员名，类型名是它唯一自然的标签）。类/结构上的 `[LabelText]` 不参与这一级，它是类型自身的显示名（`[AddComponentMenu]` 那一类语义）。

### 1.5 没生效？按这个顺序查

1. **面板还是 Unity 原生样式** → 检视器没被登记（类名/文件名不一致、没在 Editor 程序集、类型写错），或 Editor 程序集没引用 `RD.Inspector.Editor`；
2. **整个面板空白** → `target` 为 null，或该类型一个可见成员都没有（见 1.6）；
3. **部分字段没出来** → 对照 1.6 的可见性规则；
4. **编译报错 CS0118 `Editor`** → 见第 0 章末尾的命名空间坑；
5. **控制台有 `[Inspector]` 开头的日志** → 引擎把异常都收敛到 `Log`（不会打断绘制），日志内容通常直接指出原因。

### 1.6 为什么私有字段没出来：成员可见性规则（最小版）

引擎按类型缓存成员清单，判定如下（照 `Editor/Engine/Engine.cs` 的 `GetMembers` 逐条核对）：

**字段**

- 任一命中 → 不画：`[HideInInspector]`（优先级最高）、编译器生成字段（自动属性背后的字段、闭包）、声明类型所在程序集以 `UnityEngine` / `UnityEditor` / `mscorlib` 开头（`IsEngineInternal` —— 所以继承 `MonoBehaviour`/`ScriptableObject`/`EditorWindow` 不会把引擎的基类字段带进面板）；
- 任一命中 → 画：`[ShowInInspector]`、`[SerializeField]`、`[InspectorSerialize]`、**public 且非 static**。

> 推论：**private 字段必须挂标记**；**public static 字段默认不画** —— 但它是"默认不画"，不是"画不了"：加 `[ShowInInspector]`（或 `[SerializeField]`）就会被收集，所以静态字段**不需要**改写成属性。这条有测试钉住：`EngineReflectionTests.Members_StaticField_IsSkippedUnlessExplicitlyShown`。

**属性**

- 必须带 `[ShowInInspector]` 或 `[InspectorSerialize]`（这点和字段不同：普通 `public int X { get; set; }` 不画）；
- 必须有 getter，**并且**带 `[ShowInInspector]` 或 `[InspectorSerialize]`；没有 setter 的属性自动按只读绘制（不会报错）；
- 访问器可见性不影响收集：`[ShowInInspector] private int ShownPrivateProperty { get; set; }` 一样会被画出来（测试 `Members_PropertyNeedsExplicitAttribute` 钉了这条）；
- **索引器**：引擎的收集阶段**不专门过滤**索引器（序列化器会跳过 `GetIndexParameters().Length > 0` 的属性），但索引器要挂标记才会被收集，而读它的值必须带索引实参 —— 所以结论是**别给索引器挂标记**；要在条件里读索引器，用 `@this[0]` 表达式（见 7.4）。

**方法**

- **只有无参方法**会被收集（带参的直接忽略，这就是 `[Button]` 必须挂在无参方法上的原因）；
- 带 `[Button]` / `[ButtonGroup]` → 画成按钮；带 `[OnInspectorGUI]` → 在该位置调用；
- 属性访问器、运算符等 `IsSpecialName` 方法跳过。

**继承**：**你自己的基类**成员会和派生类成员一起被收集（反射带 `FlattenHierarchy`，`@` 表达式里也能读到）；被排除的只有上面 `IsEngineInternal` 那些引擎程序集里的成员。⚠️ 一个例外（实测过 `GetFields` 的行为）：**基类里的 `private` 字段收不到** —— `FlattenHierarchy` 对非公开实例成员只在 `protected` 这一档生效。所以：

- 基类 `protected` / `public` 字段 → 派生类面板里照常显示（标 `[SerializeField]` 的也照常被 JSON 序列化，见第 8 章）；
- 基类 `private` 字段 → **既不显示、也不会被 `[InspectorSerialize]` 写进 JSON**（哪怕标了标记）。要让它参与，改成 `protected`，或在派生类里另开一个字段转发。

**顺序**：先按 `[PropertyOrder(double)]` 升序，相同 order 时按"声明顺序"（字段 → 属性 → 方法）——**注意不是源码里的书写顺序**，所以想精确控制位置就显式给 order。

![成员可见性规则](images/02-visibility-rules.png)

*字段 / 属性 / 方法各自的"画与不画"判定。*

---

## 第 2 章 基本功：标签、排序、数值、提示

延续第 1 章的 `EnemyConfig`，一步步把它变好用。

### 2.1 改显示名与标签

```csharp
[LabelText("显示名称")] public string DisplayName;
[HideLabel] public bool EnableDebug;      // 不画左边的标签，值独占整行
[LabelWidth(120)] public int MaxHp;       // 只影响本行标签宽度
```

### 2.2 排序

```csharp
[PropertyOrder(-100)] public int Id;      // 负数排最前
[PropertyOrder(10)]   public string DisplayName;
[PropertyOrder(20)]   public int MaxHp;
```

不写 `[PropertyOrder]` 的成员 order 为 0。实战中的用法不是"给每个字段编号"，而是**只给需要挪位的少数字段编号**（典型是按钮，见 5.3）。

order 相同时的次序由**收集顺序**决定，规则是：**字段 → 属性 → 方法**（`GetMembers` 依次反射这三类，`Seq` 递增），同类内部是反射返回的元数据顺序（Roslyn 编译结果通常就是声明顺序，但这不是语言保证），并且**自己声明的成员排在基类成员之前**。所以"顺序要精确可控"就写 `[PropertyOrder]`，不要指望源码行号。

### 2.3 私有成员怎么露面：三种标记怎么选

| 标记 | 作用范围 | 什么时候用 |
|---|---|---|
| `[SerializeField]` | 字段 | 运行时要用的私有字段（Unity 自己也序列化它） |
| `[ShowInInspector]` | 字段 / 属性（**放方法上无效**） | 只想在编辑器里看或改，不需要 Unity 序列化（属性也必须用它） |
| `[InspectorSerialize]` | 字段 / 属性 | 配合 `SerializedScriptableObject` 走的 JSON 落盘通路（第 8 章） |

三者可以叠加。真实项目里常见写法是 `[NonSerialized] + [InspectorSerialize]`（数据不交给 Unity 存，交给本包的 JSON 存）。

### 2.4 数值、校验、颜色

```csharp
[MinValue(0), MaxValue(100)]  public int Weight;              // 变滑杆
[MinValue(0), MaxValue(1)]    public float Progress01;        // float 同样适用
[Required("必须填一个名字")]   public string DisplayName;       // 空/默认值时在面板上提示
[GUIColor(1f, 0.4f, 0.4f)]    public int MaxHp;               // 行着色
[ReadOnly]                    public int RuntimeInstanceId;   // 只读
```

细节（都可在源码里核对）：

- `[MinValue]` / `[MaxValue]` **必须两个都存在且都传数值**，才走 `EditorGUILayout.Slider`；任一侧用 `MemberName` 重载（`[MinValue("m_MinHp")]`）就退回普通输入框，且成员名不会被取值——**这是最容易踩空的参数**；
- int 的滑杆内部按 float 滑动再转回 int；
- `[Required]` 的 `ErrorMessage` 与 `InfoMessageType` 都生效；**在表格单元格里不绘制**（避免撑高行）；
- `[GUIColor]` **只读取数值与数字串**两种写法：`[GUIColor(1f, 0.4f, 0.4f)]`、`[GUIColor("1,0,0,1")]`（`a` 可省，默认 1）；
- ⚠️ `GUIColorAttribute` 上还有一个 `MethodName` 属性（想写"返回 `Color` 的方法名"），但**引擎不读取它**：写了不会有颜色也不会报错。需要动态颜色请用 `[OnInspectorGUI]` 手写 GUI，或改数据驱动的显隐（`[EnableIf]` 等）来传达状态；
- `[GUIColor]` 只影响该成员所在行（不递归到子对象）。

### 2.5 编辑期 / 运行期显隐

```csharp
[HideInPlayMode]  public string EditorOnlyMemo;    // 运行时隐藏
[HideInEditorMode] public int RuntimeScore;        // 编辑期隐藏
```

### 2.6 提示与标题

```csharp
[Title("战斗参数", "改这些要重新导出配置")]
[InfoBox("改动会影响所有关卡。", InfoMessageType.Warning)]
[DetailedInfoBox("为什么有这个限制？", "详细解释写在这里。", InfoMessageType.Info)]
[InfoBox("只在高级模式下提示", InfoMessageType.Info, VisibleIfMemberName = "ShowAdvanced")]
public int Damage;
```

- `InfoMessageType`：`None` / `Info` / `Warning` / `Error`；
- `[InfoBox]` / `[DetailedInfoBox]` 的 `VisibleIfMemberName` **已生效**，支持裸成员名与 `@` 表达式；条件不满足时整条提示不画；
- `VisibleIfMemberName` 是个**可写属性**，所以既能用上面的命名参数写法，也能用位置参数省掉它：`[InfoBox("提示", 条件)]`（`InfoBox` 有 `(message, visibleIfMemberName)` 重载）、`[DetailedInfoBox("摘要", "详情", InfoMessageType.Info, 条件)]`（第 4 个参数）；
- ⚠️ `[DetailedInfoBox]` **不是"可展开"控件**：引擎画的是一个 `HelpBox`，文本是 `Message + "\n" + Details` 拼起来的，没有折叠/展开交互（名字来自 Odin，行为不是）；要真正的可折叠说明，自己用 `[FoldoutGroup]` + `[InfoBox]` 拼；
- `[DisplayAsString]` 的 `Alignment`（`TextAlign.Center` / `Right`）**生效**，`Overflow` / `AlignLabel` 未读取；
- `[Title]` 没有条件参数（始终显示）；`Subtitle`、`HorizontalLine`、`Bold`、`Alignment` 均生效 —— `Left`/`Centered`/`Right` 整体对齐，`Split` = 主标题靠左、副标题同一行靠右；
- Unity 自带的 `[Tooltip]` **不读取**，悬停提示请用 `[InfoBox]`，按钮用 `Tooltip = "..."` 命名参数。

---

## 第 3 章 分组：把长面板拆成可读的块

### 3.1 分组的三条核心规则

所有分组特性都继承 `PropertyGroupAttribute`，理解这三条就够用了：

1. **`GroupID` 支持 `"父/子"` 路径**，用它表达嵌套；`GroupName` 只是路径最后一段（用于显示）；
2. **同类型 + 同 `GroupID` 的分组会被合并**成同一个组 —— 所以同组的多个字段各写一次特性是**正常写法**，不是重复；
3. `Order` 决定同级分组先后；`HideWhenChildrenAreInvisible`（默认 `true`）表示"组内成员全部不可见时整组不画"。

### 3.2 六种容器

| 特性 | 效果 | 备注 |
|---|---|---|
| `[BoxGroup("名字")]` | 带框的盒子 | `[BoxGroup(" ", showLabel: false)]` 可做无标题分隔盒 |
| `[FoldoutGroup("名字", expanded: true)]` | 可折叠组 | 第二个参数是**默认展开状态**，用户折叠后由会话记住 |
| `[VerticalGroup("名字")]` | 垂直排列（一列） | 最常用的"排版容器" |
| `[HorizontalGroup("名字")]` | 水平排列（一行） | 把几个字段/按钮排一行；⚠️ `Width` **未读取**，而且本包**没有任何宽度控制特性**（没有 `MinWidth`/`Width` 之类的成员特性）—— 一行里的宽度只能靠内置的伸缩行为，要精确列宽请用 `[TableList]` + `[TableColumnWidth]` |
| `[ButtonGroup("名字")]` | 同组按钮排一行 | 只对按钮有意义（见 5.4） |
| `[TitleGroup("名字")]` | 带横线的大标题分组 | `Alignment` / `HorizontalLine` / `BoldTitle` 生效（分组没有副标题，`Split` 退化为 `Left`），`IndentChildren` 未读取 |
| `[TabGroup("页签名")]` | ⚠️ **降级为垂直组** | 当前实现没有页签，源码注释原话："页签分组暂按垂直组降级" |

![六种分组容器的样子](images/03-groups.png)

*注意 `TabGroup` 这一步的降级：想要真正的页签，目前只能靠 `FoldoutGroup` 模拟。*

### 3.3 嵌套分组

```csharp
[BoxGroup("战斗/伤害")]
public int Damage;

[BoxGroup("战斗/伤害")]
public float DamageRate;

[BoxGroup("战斗/抗性")]
public int Armor;
```

`"战斗/伤害"` 会在 `"战斗"` 盒子里再套一个 `"伤害"` 盒子；`GroupName` 显示为最后一段。

### 3.4 整组条件显隐：`VisibleIf`

```csharp
[FoldoutGroup("高级", VisibleIf = "@ShowAdvanced")]   // 表达式
[BoxGroup("外观", VisibleIf = "HasAppearance")]        // 裸成员名
public int Tint;
```

- 条件不满足时**整组不绘制**，且与 `HideWhenChildrenAreInvisible` 无关，所以不会留下一个空框；
- 子组被它隐藏时，父组也不会再被判定为"还有内容"；
- 写法同 `[ShowIf]`（支持 `@` 表达式与裸成员名）；
- 同在这条基类上的 `AnimateVisibility` **未读取**（显隐没有过渡动画）。

### 3.5 什么时候用 `HorizontalGroup` 而不是表格

`HorizontalGroup` 适合"两三个相关字段排一行"（例如 `宽 / 高`、`最小 / 最大`）。**要"多行 × 多列 + 增删"，用 `[TableList]`（第 6 章）**，别用嵌套的 `VerticalGroup` 硬凑——不适合纯数据表。

### 3.6 分组常见坑

- 组名字符串**必须一字不差**，`"战斗"` 与 `"战斗 "`（多个空格）是两个组；
- 组内成员全部被条件藏起来时，组会消失（默认行为），如果你希望"空框还在"，把 `HideWhenChildrenAreInvisible = false`；
- 展开状态存在会话对象 `HostSession` 里：**按"被绘制的对象 → 组路径 / 成员名"两级分桶**（`ConditionalWeakTable<object, ObjectState>` + 两个 `Dictionary<string,bool>`）。所以：
  - 同窗口内两个同类型对象（或同一列表的两行元素）的折叠**互不影响**；
  - **两个窗口各自持有一个会话，状态不共享**（窗口 A 里展开的组，窗口 B 里还是默认状态）；
  - 会话不写盘、域重载后回到默认；对象被回收时它的状态桶自动释放。

---

## 第 4 章 条件显隐与启禁

### 4.1 四种特性、两种参数形态

```csharp
[ShowIf("ShowDeleteBtn")]                  // 成员为真 → 显示
[ShowIf("ViewMode", ViewMode.Advanced)]    // 成员等于该值 → 显示
[HideIf("IsLocked")]                       // 成员为真 → 隐藏
[EnableIf("CanEdit")]                      // 成员为真 → 可编辑
[DisableIf("ShowDeleteBtn")]               // 成员为真 → 禁用（变灰）
```

四种签名完全一致：`(string memberName)` 或 `(string memberName, object value)`。

### 4.2 用成员名：90% 的场景

`memberName` 可以是：

- **字段 / 属性**（有无 setter 都行，只读值）；
- **无参方法**（例如 `private bool ShowIfAuthorURL() => !string.IsNullOrEmpty(PackageAuthorURL);`，这是表格行的典型写法）；
- **`@` 开头的表达式**（见 4.3）。

值比较走统一的 `Value.Equal`：数值用**随量级放大的相对容差**（1e-6，所以 `float` 字段 `0.1f` 和字面量 `0.1` 能相等）、枚举可以与字符串名比较、bool 按 bool 比。

### 4.3 用 `@` 表达式：组合条件时

只有"以 `@` 开头"的字符串才会走表达式求值。典型场景：

```csharp
[ShowIf("@Level >= 10 && IsReady()")]     // 组合条件、算术、方法调用
[ShowIf("@State == \"Running\"")]         // 枚举与字符串名比较
[HideIf("@!_isExpanded")]                 // 取反
```

条件每帧重算，所以数据变了面板会立刻跟着变。

### 4.4 多个条件 = 与

同一个成员上挂多个条件特性时，**全部成立才可见/可编辑**：

```csharp
[ShowIf("IsElite")]
[ShowIf("@Level > 5")]
public int EliteSkillId;
```

### 4.5 写成不存在的名字会怎样

语义是**视为可见**（宁可不隐藏，也不因为一个拼错的名字把成员整片吞掉），并在首次遇到时记一条 `WarnOnce`，控制台里明确告诉你"哪个类型、哪个名字没解析到"。所以：

- 如果条件"看起来完全不生效"，先去控制台搜 `[Inspector]`；
- 这条日志**只报一次**（`WarnOnce` 去重），不会刷屏。

### 4.6 不生效的开关

`[ShowIf].Animate` **未读取**（不校验、不报错，就是没有动画）。

### 4.7 条件的"作用域"：它在哪个对象上求值

这一条决定了嵌套场景能不能用对，值得单独记一页。

引擎求值条件时，作用域**就是当前正在绘制的那个对象**（`ConditionValue(target, …)`）：

- 画组件自身成员时 → 是这个 `MonoBehaviour` / `ScriptableObject` / 窗口；
- 画**嵌套对象**的成员时 → 是那个嵌套实例（不是你挂在脚本上的外层对象）；
- 画**表格行 / 列表元素**的成员时 → 是那一行的元素实例。

后果（很多人第一次都会踩）：

```csharp
public class HpModule            // 嵌套对象
{
    [ShowIf("EnableHp")]         // ❌ EnableHp 在外层组件上 → 解析不到
    public int MaxHp;
}

public class PlayerConfig : MonoBehaviour
{
    public bool EnableHp = true;
    public HpModule Hp = new();
}
```

这份代码的 `[ShowIf]` **不会按预期工作**：引擎在 `HpModule` 上找不到 `EnableHp`，于是记一条警告后按"视为可见"处理（`MaxHp` 一直显示）。三种可行改法：

1. 把条件搬到同一作用域：让 `HpModule` 自己持有一个 `EnableHp`（由外层同步，或在 `OnValueChanged` 里写过去），条件里写 `"EnableHp"` / `"@EnableHp"`；
2. 换成能被本对象解析的条件：例如 `[ShowIf("@MaxHp > 0")]` 这类只读自己字段/方法的表达式；
3. 数据层不做条件，改在宿主层（`[OnInspectorGUI]` 或自定义检视器）手写这段判断——嵌套对象的条件的本质就是"局部作用域"，硬跨层反而更难维护。

同样的作用域规则适用于分组上的 `VisibleIf`、`[EnableIf]`、`[DisableIf]`、`[InfoBox].VisibleIfMemberName`。

表达式里的 `$root` / `this` 也指向这个作用域对象（`$root` 不是"序列化根"，就是当前绘制的对象），所以 `@$root.Xxx` 与裸写 `Xxx` 等价。

---

## 第 5 章 按钮、回调、自定义 GUI

### 5.1 最小按钮

```csharp
[Button("重置")]
private void ResetValues() { /* 改字段即可，宿主会处理 Undo / SetDirty */ }
```

规则：**方法必须无参**（带参方法不会被收集），private 完全可以。

按钮文字可以省略（`[Button]`），省略时用方法名的友好化结果，例如 `AddWave` → `Add Wave`。实践上仍建议显式写中文名。

### 5.2 控制外观

```csharp
[Button("打开文档", 30, Icon = SdfIconType.Info, Tooltip = "在浏览器打开设计文档")]
private void OpenDocument() { Application.OpenURL("https://example.com"); }
```

- 第 1 个参数是按钮文字，第 2 个是**高度**（默认 24）；
- `Icon` 取 `SdfIconType`（`Runtime/Attributes/SdfIconType.cs` 里的枚举），但**只有少数字形族有映射**：`Folder*`、`Settings*`/`Gear*`、`Save*`、`Play*`/`Pause*`/`Stop*`、`Refresh`/`Sync`/`ArrowRepeat`、`Search*`、`Plus*`、`Minus*`、`Trash*`、`Exclamation*`、`Info`/`InfoCircle`、`Question*`、`Eye*`/`EyeSlash*`、`Check*`、`ArrowUp/Down/Left/Right`、`X*`、`Calendar*`；
- ⚠️ 其它值（`Link45deg`、`Github` 这类"看起来很常用"的）`Icons.FromSdf` 返回 `null` → **按钮只显示文字**（不报错）。要加图标就往 `Icons.FromSdf` 的 switch 里补一条映射；
- `Tooltip` 生效；
- `ButtonStyle`、`IconAlignment` **未读取**（图标统一居左）。

### 5.3 按钮放哪儿：靠 `PropertyOrder`

按钮是成员清单里的一员，按 `PropertyOrder` 参与排序。真实场景是"把按钮放在它操作的数据旁边"：

```csharp
[Button("添加一行", 10)]
[PropertyOrder(1000)]              // 排在表格(1001)之前
private void AddWave() => Waves.Add(new WaveData());

[TableList(AlwaysExpanded = true)]
[PropertyOrder(1001)]
public List<WaveData> Waves = new();
```

### 5.4 一排按钮

```csharp
[ButtonGroup("操作")] [Button("应用", 28)] private void Apply() { }
[ButtonGroup("操作")] [Button("回滚", 28)] private void Revert() { }
```

`[ButtonGroup("同名")]` 的按钮排在同一行；`HorizontalGroup` 也能做到同样效果（它更通用，能一行里混排字段和按钮）。单行里建议 2～4 个，多了会被挤压变形。

### 5.5 字段旁边的按钮：`InlineButton`

```csharp
[InlineButton("PickRandomName", "随机")]
public string DisplayName;

private void PickRandomName() { DisplayName = "Enemy_" + Random.Range(1000, 9999); }
```

⚠️ **只在标量成员（数值/字符串/枚举/bool 等，非容器）这一行里绘制**。挂在 `List<T>` / 字典 / 嵌套对象上不会出现——容器走的是另一条绘制分支。忘了这一点会以为特性失效。

### 5.6 值变化后重算：`OnValueChanged`

```csharp
[OnValueChanged("OnTeamChanged")]
public int TeamId;

private void OnTeamChanged()   // 必须无参
{
    RefreshTeamMembers();
}
```

触发时机：控件报告"值有变化"（`EditorGUI.BeginChangeCheck` / `EndChangeCheck`）之后，引擎先写值、再调用方法（方法名解析同样只认无参版本）。`IncludeChildren` **未读取**。

### 5.7 插入原生 GUI：`OnInspectorGUI`

想在面板中间插一段自己的 `EditorGUILayout` 代码：

```csharp
[OnInspectorGUI]
[PropertyOrder(50)]
private void DrawCustomSection()
{
    EditorGUILayout.HelpBox("这段是手写的原生 GUI", MessageType.Info);
    if (GUILayout.Button("手写按钮")) { /* ... */ }
}
```

方法名省略时用声明的方法名；可以挂多个；用 `[PropertyOrder]` 决定插入位置。

### 5.8 按数据自动折叠：`OnStateUpdate`

```csharp
[OnStateUpdate("@$property.State.Expanded = Count > 0")]
public List<WaveData> Waves = new();
```

**这里必须讲清实情**：引擎每帧为该成员构造一个状态对象、执行表达式，然后**只把 `State.Expanded` 读回去**（`RunStateUpdates` 的最后一步是 `SetExpanded(target, m, state.Expanded, session)`）。

| 写法 | 结果 |
|---|---|
| `@$property.State.Expanded = 条件` | **生效**：按数据自动展开 / 收起 |
| `@$property.State.Visible = 条件` | 能写，但引擎不读回 → **无效果**（自动显隐请用 `[ShowIf]`） |
| `@$property.State.Enabled = 条件` | 同上，无效果（请用 `[EnableIf]` / `[DisableIf]`） |
| `@$property.State.Selected = 条件` | 同上，无效果 |

参数不是 `@` 开头时，按"本对象上的无参方法名"直接调用。

### 5.9 按钮 / 回调里改数据会遇到什么

- **Undo 由宿主负责**：`InspectorEditor` 在写值前拍快照，所以按钮里改字段 Ctrl+Z 可回退；
- **落盘由宿主的 `onChanged` 负责**：`InspectorEditor` 会 `SetDirty` + 登记 prefab override。**自己写宿主（第 11 章）就必须自己实现 `onChanged`，否则改动不落盘**；
- 折叠状态、滚动位置是**会话内缓存**（不写盘、域重载后回到默认）；
- 成员清单、分组树、反射查找、表达式 AST 都是**按类型缓存**的：给类**新增成员后需要等域重载**（改代码本来就会触发编译，通常无感）。

---

## 第 6 章 列表、表格、字典、下拉

### 6.1 默认列表

`List<T>` / 数组默认是带折叠标题的列表（标题形如 `名字 (个数)`，右侧一个 `+`），不需要任何特性：

```csharp
public List<WaveData> Waves = new();
```

它的真实能力边界（都来自 `DrawPlainList`，逐条核对过）：

- **增删**：只有标题栏的 `+`（追加）与每个元素的 `-`（删除该元素）。**没有拖拽排序、没有中间插入、没有复制元素**——整个引擎里没有任何拖拽代码，`ListDrawerSettings.DraggableItems` 也未读取；
- `+` 的默认行为是 `Activator.CreateInstance(T)`（所以 `T` 得有无参构造）；想自定义就配 `[ListDrawerSettings(CustomAddFunction = "...")]`（见 6.3）。实例化失败只记一条警告，不会崩；
- **元素是标量**（`int`/`float`/`bool`/`string`/枚举…）时，每个元素是一个**文本输入框**（标签 `#0`、`#1`…），输入内容由 `ParseSimple` 解析回该类型：**解析失败就静默不写**（`List<int>` 里敲字母不生效，也没有红色提示）；
- ⚠️ `Vector2` / `Vector3` / `Color` 被算作"标量"，但 `ParseSimple` 对它们返回 `null` —— 于是 **`List<Vector2>`、`List<Color>` 的元素是"只能看、敲了不生效"的文本框**；
- **元素是 `UnityEngine.Object`**（材质、Prefab、其它资产…）时走一个特殊分支：**只读展示**——用 `ObjectField` 列出来，但**没有 `+`/`-`、没有折叠标题，而且控件返回值被丢弃（拖进去的引用不会写回）**。⚠️ 判据是**"第 0 个元素是不是 `UnityEngine.Object`"**，所以一个**空**的 `List<Material>` 一开始是普通列表（有 `+`），一旦有了元素就退化成只读展示，**此后再也删不掉里面的元素**；
- 元素为 `null` 时只画一行 `#i = null` 的只读文本，**连 `-` 按钮都没有** —— `List<自定义类>` 里的空槽位在面板上删不掉（请在外面把 `null` 清掉）；
- 元素是**自定义类**时，展开绘制该元素的可见成员（可见性规则同 1.6）；
- 🤯 **不对称之处**：`Vector4`/`Quaternion`/`Rect`/`LayerMask` 等**不满足**引擎的"标量"判据（那里只认 `Vector2`/`Vector3`/`Color` + 基元 + `string`/`decimal`/`DateTime` + 枚举 + `UnityEngine.Object`），所以它们走**嵌套展开** —— 结果 **`List<Vector4>`、`List<Quaternion>` 反而可以编辑**（x/y/z/w 各自一个输入框），而 `List<Vector3>` 只能看。要存坐标/颜色列表，不如包一层自己的类（`[Serializable] struct WavePoint { public Vector3 Pos; }`）；
- 挂了 `[TableList]` 但元素类型**没有任何可表格化成员**时，会**静默退回普通列表**（不报错，只是没表格）。

### 6.2 表格：`[TableList]`

```csharp
[TableList(DrawScrollView = true, AlwaysExpanded = true, HideToolbar = false)]
public List<WaveData> Waves = new();
```

| 参数 | 作用 | 状态 |
|---|---|---|
| `DrawScrollView` | 是否套滚动视图 | 生效 |
| `AlwaysExpanded` | 展开后保持展开（不随选中变化收起） | 生效 |
| `HideToolbar` | 隐藏工具栏（= 隐藏添加/删除按钮） | 生效 |
| `IsReadOnly` | 只读（与工具栏联动） | 生效 |
| `ShowIndexLabels` | 显示行号 | 生效 |
| `NumberOfItemsPerPage` | 默认 15；**仅配合 `DrawScrollView`** 决定滚动视图一次显示几行。可见行数 = `Clamp(数据条数, 3, NumberOfItemsPerPage)`，**空表也留 3 行** | 生效（但这不是"分页"） |
| `ShowPaging` / `ShowItemCount` | 想当然的"分页 / 显示条目数" | **未读取**，写了没效果 |

列定义来自**行对象自己的成员**（可见性规则同 1.6），列宽用行成员上的 `[TableColumnWidth(int)]`：

```csharp
[Serializable]
public class WaveData
{
    [TableColumnWidth(120)]
    public string Name;

    [TableColumnWidth(60), MinValue(1), MaxValue(20)]
    public int Count;
}
```

- `Width` 生效；`Resizable` **未读取**（列不可拖拽调宽）；
- `[HideInTables]` **未实现**：想"某成员不参与表格列"，用 `[HideInInspector]` 或干脆别让它可见；
- 表格单元格里不画 `[Required]` 提示；
- **单元格里的条件显隐是"保留列位、单元格留空"**：`DrawTableCell` 会调 `CheckVisibility(列成员, 行元素)`，条件不满足时该格为空，但**列本身仍在**（列集合由行类型决定，是全局的）。所以"某行勾了 `IsBoss` 就出现 BOSS 名称输入框"是可以做的，"这一列按行出现/消失"做不到——那需要按条件分两张表；
- 单元格里条件的**作用域是那一行的元素**（见 4.7）：`[ShowIf("IsBoss")]` 里的 `IsBoss` 必须在行类型上有定义。


### 6.3 自定义"添加"按钮

```csharp
[ListDrawerSettings(CustomAddFunction = "AddWave")]
public List<WaveData> Waves = new();

private void AddWave() => Waves.Add(new WaveData { Name = "New" });
```

`ListDrawerSettings` 里**只有 `CustomAddFunction` 与 `IsReadOnly` 被读取**，其余参数（`ShowPaging`/`ShowItemCount`/`ShowFoldout`/`DefaultExpandedState`/`HideAddButton`/`DraggableItems`/`CustomRemoveIndexFunction`/`CustomRemoveElementFunction`/`OnBeginListElementGUI`/`OnEndListElementGUI`/`OnTitleBarGUI`…）当前**全部未读取**——**注意：`[TableList]` 的 `ShowPaging` 同样未读取**（见 6.2），所以"列表分页"当前没有开箱支持，需要分页只能自己用 `[OnInspectorGUI]` 手写，或把数据拆成多个字段/多级结构。

### 6.4 字典

```csharp
public Dictionary<string, int> Weights = new();

// 需要整个字典只读时才用得上这个特性：
[DictionaryDrawerSettings(IsReadOnly = true)]
public Dictionary<string, int> LockedWeights = new();
```

`Dictionary<TKey, TValue>` 会画成可折叠的列表（标题 `名字 (条目数)`），每条一行。**它的能力比列表还窄，用之前先看完这份清单**（照 `DrawDictionary` 逐条核对）：

- **不能新增条目**：整个字典抽屉**没有 `+` 按钮**，只有每行的 `-`（删掉该键值对）；
- **键不可编辑**：键只作为行的标签显示（`k.ToString()`），面板上改不了；
- **值按类型分成两条路**：
  - **标量值**（数值 / 字符串 / bool / 枚举…）→ 文本框 + `ParseSimple`，解析失败静默丢弃（和列表标量同一个坑）；
  - **非标量值**（自定义类、集合、`Vector2`/`Color`…）→ **只读 `LabelField`，显示成 `ToString()`，完全不可编辑**；
- `[DictionaryDrawerSettings]` 只有 `IsReadOnly` 生效（整字典变只读，值连文本框都不给）；`KeyLabel` / `ValueLabel` / `DisplayMode` **未读取**。

结论：**字典适合"看 + 改标量值 + 删条目"，不适合当可增删的配置表**。要做真正的配置表，请用 `List<你自己的 Entry 类型>` + `[TableList]`（第 6.2 节）——那样才有 `+`/`-`、可编辑的列和条件显隐。

### 6.5 下拉：`[ValueDropdown]`

```csharp
[ValueDropdown("GetTeamSelectList")]
[OnValueChanged("OnTeamChanged")]
public int TeamId;

private ValueDropdownList<int> GetTeamSelectList()
{
    var list = new ValueDropdownList<int>();
    foreach (var team in m_AllTeams)
    {
        list.Add(team.DisplayName, team.Id);   // (显示文本, 值)
    }
    return list;
}
```

约定（照 `ResolveDropdownItems` / `DrawValueDropdown` 逐条核对）：

- 参数是**取值成员名**（字段 / 属性 / 无参方法都可以，内部就是同一套成员解析）；
- 返回值最好是 `ValueDropdownList<T>`；返回其它 `IEnumerable` 时，元素若有 `Text` / `Value` 属性就按它们取值，否则 `ToString()` 当显示文本、元素本身当值；元素是 `null` 时文本为 `(null)`；
- 返回**非集合**的单个值 → 下拉里只有这一项（很少见，但不会崩）；
- 返回 `null` 或空集合 → **退化为普通 `TextField`**（注意：是文本框，不是"原类型该有的控件"；也不报错）；
- 匹配当前值时用统一的值比较；**匹配不上 → 选择框显示空白（`Popup` 传 -1），并且不动数据**；
- 用户选择后写回走的是**正常写值通路**（`Mutate` → Undo / `SetDirty` / 落盘全都生效）——这是它和表达式赋值的关键区别；
- ⚠️ **只对标量成员生效**：`[ValueDropdown]` 是在标量控件入口里读的，所以挂在 `List<T>` / 字典 / 自定义类成员上**不会画下拉**（那些成员走容器 / 嵌套对象分支，和 `[InlineButton]` 的坑一样）；
- `AppendNextDrawer` / `IsUniqueList` / `ExcludeDuplicates` / `DoubleClickToConfirm` / `HideValuesInChildTree` 当前**未读取**。

### 6.6 枚举

```csharp
public ViewMode Mode;                              // 默认：下拉（Popup）
[EnumToggleButtons] public ViewMode ToggleMode;    // 一排按钮（选项少时更直观）

public enum ViewMode
{
    [LabelText("简单")] Simple,
    [LabelText("高级")] Advanced,
}
```

关于枚举绘制的五条事实（照 `DrawEnum` / `GetEnumLabel`）：

- 默认是 `EditorGUILayout.Popup`，选项顺序 = `Enum.GetValues` 顺序（**没有** Unity 原生那种"按位展开"的 flags 折叠界面）；
- `[EnumToggleButtons]` 换成 `GUILayout.Toolbar`（一排按钮，选项多会挤在一起，建议 ≤4 个）；
- **枚举值上可以挂 `[LabelText("中文")]` 定制显示文本** —— 这是做中文界面的正经办法，不用改枚举名；
- **枚举「类型」上也可以挂 `[LabelText("中文")]`** —— 它不当值用，而是成为该枚举字段的**字段标签**（字段自己没写 `[LabelText]` 时）。典型用途：一个泛型基类承载 N 个枚举，N 个字段声明的是同一个成员、名字都一样，只有枚举类型能区分它们（`cn.rendu.ui` 的 `MacroDataBase<T>` 就这么显示 RD / UI / 资源加载）；
- ⚠️ `[Flags]` 枚举当前**只能选一个值**（Popup / Toolbar 都是单选），需要多选位请自己拆成多个 bool 字段或用 `[InspectorSerialize]` + 自己的控件；
- 当前值匹配不上任何选项时（例如运行期被赋了非法值），下拉会**显示第 0 项，但不会改数据**（只影响显示）。

### 6.7 实战：掉落表（完整代码）

```csharp
using System;
using System.Collections.Generic;
using RD.Panel;
using UnityEngine;

[Serializable]
public class DropEntry
{
    [TableColumnWidth(140)]
    public string ItemId;

    [TableColumnWidth(70), MinValue(0), MaxValue(1000)]
    public int Weight;

    public bool Rare;

    [ShowIf("Rare")]                 // 只有稀有物品才填备注
    public string Note;
}

public class DropTable : MonoBehaviour
{
    [FoldoutGroup("掉落", expanded: true)]
    [TableList(DrawScrollView = true, AlwaysExpanded = true)]
    [ListDrawerSettings(CustomAddFunction = "AddEmptyEntry")]
    public List<DropEntry> Entries = new();

    [FoldoutGroup("掉落")]                       // 同名组 → 合并进同一个折叠组
    [HorizontalGroup("掉落/按钮")]                // 组内再排一行
    [Button("添加一行", 24)]
    private void AddEntry() => Entries.Add(new DropEntry { Weight = 100 });

    [HorizontalGroup("掉落/按钮")]
    [Button("清空", 24)]
    [GUIColor(1f, 0.5f, 0.5f)]
    [ShowIf("@Entries.Count > 0")]               // 空表时不显示"清空"
    private void Clear() => Entries.Clear();

    private void AddEmptyEntry() => Entries.Add(new DropEntry());
}
```

这段代码同时用到了：分组嵌套、表格、列宽、条件显隐（成员名 + 表达式）、`ListDrawerSettings` 的自定义添加、水平组排按钮、`GUIColor`。照抄能直接跑。

---

## 第 7 章 表达式系统（`@`）

### 7.1 哪些地方会用到表达式

只有三处：

1. `[OnStateUpdate("...")]` 的 `Action`；
2. 条件类特性的成员名：`[ShowIf]` / `[HideIf]` / `[EnableIf]` / `[DisableIf]`；
3. 分组特性上的 `VisibleIf`，以及 `[InfoBox]` / `[DetailedInfoBox]` 的 `VisibleIfMemberName`。

**判定规则：字符串以 `@` 开头才算表达式**。不带 `@` 的一律按"成员名 / 方法名"反射处理——这是"表达式不生效"的第一大原因（见 13 章）。

### 7.2 上下文变量

| 写法 | 含义 |
|---|---|
| `$value` | 当前成员的值（**只读**，见 7.5） |
| `$property.State.Expanded` | 当前成员的折叠状态（读写，且是唯一会被引擎读回的状态） |
| `$root` / `this` | 当前被绘制的对象 |
| 裸标识符 `Foo` | 先在当前对象上找字段 / 属性（含静态），找不到再当**全局类型短名**（扫所有程序集，例如 `Mathf.Max(1, 2)`、`Color.red`） |
| `true` / `false` / `null` | 字面量 |
| `'文本'` / `"文本"` | 字符串（支持 `\n \t \r` 转义） |
| `123` / `1.5` / `1e3` | 数字（**一律是 double**） |

没有 `target` / `value` 这类不带 `$` 的保留名——写 `target` 会去当前对象上找名叫 `target` 的成员。

### 7.3 运算符（优先级由低到高）

```text
=                        赋值
? :                      三目
||                       逻辑或
&&                       逻辑与
== !=                    相等（数值相对容差 1e-6；枚举可与字符串名比较）
< <= > >=                关系
+ -                      加减（任一侧是 string 则字符串拼接）
* / %                    乘除模（**浮点语义**：`%` 在除数接近 0 时返回 0；`/` 除以 0 得到 `Infinity`/`NaN`，不抛异常也不是 0）
! -                      一元
() . [] 方法调用          后缀：分组、成员链、索引、方法调用
```

索引器已支持：`@Items[0]`、`@Map['key']`、`@Child.Items[1]`、`@GetScaled(2)[1]`，覆盖数组 / 字符串 / `List<T>` / `Dictionary<K,V>` 与自定义 `this[...]` 索引器；**越界或键缺失返回 `null`**，不会打断绘制。

### 7.4 不支持的语法

`++` / `--`、复合赋值（`+=`）、按位运算（`& | ^ ~ << >>`）、类型转换 `(T)x`、`is` / `as`、数组/集合字面量。

另外：解析会**校验收尾**，写了一半的表达式（如 `@Items[0`）整体返回 `null`，而不是被截断成错值。

### 7.5 六个必知陷阱

1. **`Truthy(0)` 为真**。真值规则（`Value.Truthy`）是：`null` / `false` / 空串 / **空集合**（`ICollection.Count == 0`，数组 / `List` / `Dictionary` 都算）/ 已销毁的 Unity 对象为假，**其余全为真**（包括数值 0、枚举 0）。所以 `[ShowIf("@Count")]` 在 `Count == 0` 时**依然显示**；要判非零请写 `@Count > 0` 或 `@Count != 0`。注意判空只认 `ICollection`，延迟执行的 `IEnumerable`（比如 `Where(...)` 的结果）会被当成真；
2. **求值不短路**：`@A() && B()` 两边都会执行。条件表达式里不要放有副作用的方法；
3. **数字字面量都是 double**：`@Flag == 1` 比较 int 字段没问题（有相对容差），但别指望 `int` 溢出语义；
4. **`$value` 不可写回**：`@$value = $value + 1` 只改表达式内部的临时值，**不会改你的字段**；`$property.Value = x` 也一样（`$property.Value` 读写的就是同一份上下文值）。要写数据得写链末端成员（见下一条）；
5. **赋值只认"链末端成员"**：`@$property.State.Expanded = false`、`@$root.SomeField = 1` 这类带 `.` 的赋值才会真正写；裸 `@Count = 1` 不写（无链可写）。而且表达式赋值**不经过宿主的 Undo / SetDirty 回调**，每帧执行还会覆盖用户输入——只建议在 `$property.State.Expanded` 这种"视图状态"上使用；
6. **方法调用按"名字 + 参数个数"匹配**，不校验类型；找不到就返回 `null`。`@Foo(1, 2)` 可以调用 `Foo(int, int)`（引擎会按形参类型收窄数值/枚举），但是**调用不存在的成员不会报错，只会静默为 null**，条件里就是"假"。

### 7.6 常用句式速抄

```csharp
[ShowIf("@Count > 0")]
[ShowIf("@Level >= 10 && IsReady()")]
[ShowIf("@State == \"Running\"")]
[ShowIf("@Name != null && Name != ''")]
[HideIf("@!_isExpanded")]
[EnableIf("@Level >= 10 ? true : false")]
[ShowIf("@Items[0] != null")]
[ShowIf("@Mathf.Approximately(Speed, 0f) == false")]
[OnStateUpdate("@$property.State.Expanded = Errors.Count > 0")]
```

### 7.7 怎么调试表达式

- 表达式被 `try/catch` 包着：词法/求值出错只记一条警告、返回 `null`，**不会中断绘制**；去控制台搜 `[Inspector]`；
- **解析结果（AST）按源码字符串缓存**（上限 512 条，满了整体清空），解析失败也缓存（负缓存）：写错的表达式只报**一次**（`表达式语法错误, 已忽略: …`），之后按"空表达式（恒为假）"处理。所以改完表达式记得等编译/域重载；
- 想单独验证一个表达式，可以在自己的编辑器代码里直接调（`Evaluate` / `EvaluateBool` 都是 public，且**传入时可以不带 `@`**——引擎内部会剥掉）：

```csharp
bool r = ExpressionEvaluator.EvaluateBool("@Count > 0", myObject, null, null);
bool r2 = ExpressionEvaluator.EvaluateBool("Count > 0", myObject, null, null);   // 等价
```

- 缓存的是**元数据**，不是取值结果：每次求值都重新读成员、重新调方法，所以 `@Count` 能跟着字段实时变化。

---

## 第 8 章 编辑器数据资产：`SerializedScriptableObject`

> 本章的主角（`SerializedScriptableObject` / `Serialization`）在检视器宿主包 **cn.rendu.inspector**；用法与规则不变。

### 8.1 什么时候用它

适合：**编辑器侧的配置资产**（工具配置、包版本表、检查规则表…），成员类型比较简单，希望数据"就在 `.asset` 里、肉眼可读"。

不适合：运行时资产（基类在 Editor 程序集，打包报错）、需要保存 `UnityEngine.Object` 引用的资产（第 8.5 条限制 1）。

### 8.2 三步上手

```csharp
using System;
using System.Collections.Generic;
using RD.Panel;

public class MyToolConfig : SerializedScriptableObject
{
    [InspectorSerialize]
    [LabelText("服务器地址")]
    public string ServerUrl = "http://localhost:8080";

    [InspectorSerialize]
    [LabelText("重试次数")]
    [MinValue(0), MaxValue(10)]
    public int RetryCount = 3;

    [NonSerialized]
    [InspectorSerialize]
    [LabelText("缓存")]
    public Dictionary<string, int> Cache = new();

    protected override void OnAfterInspectorDeserialize()
    {
        // 读盘后重算派生数据（可选）
    }
}
```

1. 继承 `SerializedScriptableObject`；
2. 想存/想显示的成员标 `[InspectorSerialize]`；
3. 在 Project 里右键 `Create > ...`（或 `ScriptableObject.CreateInstance` + `AssetDatabase.CreateAsset`）建一个 `.asset`。

**不需要写检视器**：基类自带 `[CustomEditor(typeof(SerializedScriptableObject), true)]`，覆盖了全部子类（范例源码见 cn.rendu.inspector 包的 `Editor/Inspector/SerializedScriptableObjectEditor.cs`，只有一行）。

### 8.3 数据存在哪

```csharp
public class SerializedScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector] private string m_InspectorData = string.Empty;

    [NonSerialized] private bool m_Deserializing;

    public void OnBeforeSerialize()
    {
        if (m_Deserializing) return;                     // 读盘过程中不回写, 避免把半成品写回去
        m_InspectorData = Serialization.Serialize(this);
    }

    public void OnAfterDeserialize()
    {
        if (string.IsNullOrEmpty(m_InspectorData)) return;   // 还没有数据的资产直接跳过
        m_Deserializing = true;
        try { Serialization.Deserialize(this, m_InspectorData); }
        finally { m_Deserializing = false; }
        OnAfterInspectorDeserialize();
    }

    protected virtual void OnAfterInspectorDeserialize() { }
}
```

- 序列化结果是 **JSON 文本**，存在资产自身那个 `[SerializeField] string` 里 —— 打开 `.asset` 文件就能看到那串 JSON，**不是旁挂文件**；
- 成员筛选标准就是 `[InspectorSerialize]`（字段/属性，含继承链）；
- 两个容易困惑的点都在上面的源码里：**数据还是空串时 `OnAfterDeserialize` 直接返回**（所以刚 `CreateInstance` 出来、一次都还没存过的资产，`OnAfterInspectorDeserialize` **不会被调用**）；**读盘期间 `OnBeforeSerialize` 被守卫生效跳过**（否则读一半的数据会被写回，覆盖真正的存量数据）。

### 8.4 能存什么类型

数值 / 字符串 / 布尔 / 枚举 / 字符（存为 int）/ 一维数组 / `List<T>` / `Dictionary<TKey,TValue>`（键为字符串或数值）/ 嵌套 POCO。

特殊处理（记住结论即可）：

- **Unity 值类型**（`Vector2/3/4`、`Quaternion`、`Rect`、`Bounds`、`Color`、`LayerMask`…）按它们自己的数据字段存取，能正常往返；
- **曲线 / 渐变**（`AnimationCurve` / `Gradient`）按公开读写属性存取（数据在原生对象里），能往返；
- `HashSet<T>` 这类只实现 `ICollection<T>` 的集合走 `Add(T)` 填充。

### 8.5 硬限制（每条都说清后果）

1. **`UnityEngine.Object` 引用会被写成 `null`**：材质、Prefab、其它 SO 的引用**无法**通过这条通路保存。需要引用就改用 Unity 原生 `[SerializeField]` 字段（不标 `[InspectorSerialize]`，由 Unity 自己存，本包只负责画它——注意 `[SerializeField]` 的成员同样会被引擎画出来）；
2. **成员名就是 JSON 的 key**：**字段改名 = 旧数据丢失**，且没有版本迁移机制。改名要自己写迁移（例如加 `OnAfterInspectorDeserialize` 里做兼容）；
3. 无 setter 的属性在反序列化时**静默跳过**（只有 getter 的属性在面板上能看到，但读盘时写不回去，也没有警告）；
4. 嵌套对象默认用 `FormatterServices.GetUninitializedObject` 创建：**不执行构造函数、不初始化字段初始值**（靠 JSON 覆盖）。所以别把"必须有构造函数才能成立的类"塞进去。唯一例外是曲线 / 渐变这类内部持有原生数据的类型，它们**走真实构造函数**（引擎注释写明了原因：`AnimationCurve` 若绕过构造函数，内部 `m_Ptr` 为空，随后写 keys 就是在对空指针操作）；
5. 递归写出有**深度上限 32 层**，环引用会被剪成 `null`（而不是栈溢出）；
6. **单个成员失败不拖垮整份数据**：读/写某一成员失败只记一条警告后跳过；某个成员的 getter 抛异常时该键写成 `null`；
7. **一个成员都取不到的类型会被当场点出来**：某类型的成员集合为空（例如嵌套 POCO 忘了标 `[InspectorSerialize]`）时会记一条 `WarnOnce`，告诉你"本次只会写成 `{}`、读回来是 default"。看到这条日志就去补标记；
8. 无法实例化的目标类型（接口、无默认构造的字典）保持字段默认值；
9. ⚠️ **继承体系里基类的 `private` 字段不参与**：序列化器的成员扫描用的是 `GetFields(Public|NonPublic|Instance|FlattenHierarchy)`，而 `FlattenHierarchy` 对非公开**实例**成员只覆盖 `protected`（实测过）—— 所以基类的 `private` 字段即使标了 `[InspectorSerialize]` 也**不会进 JSON**（面板上也不显示，见 1.6）。继承体系里请把数据成员写成 `protected` / `public`。

### 8.6 反序列化后重算：`OnAfterInspectorDeserialize`

读盘后自动调用一次，适合重建缓存 / 重算派生值。本仓库的真实用法（`PackageVersionAsset`）是"重算一次派生版本号，因为 JSON 写入不触发 setter"：

```csharp
protected override void OnAfterInspectorDeserialize()
{
    foreach (var pair in AllPackageVersionData)
    {
        if (pair.Value == null) continue;
        pair.Value.Version = pair.Value.Version;   // 触发 setter 重算
    }
}
```

### 8.7 什么时候不要用它

- 需要保存资源引用 → 用普通 `ScriptableObject` + `[SerializeField]`（本包照画，配上第 1 章的 `InspectorEditor` 登记）；
- 需要 Unity 原生 `SerializedProperty` 生态（override、多选差异）→ 用原生检视器；
- 运行时资产 → 绝不能用（基类在 Editor 程序集）。

---

## 第 9 章 工具窗口：`EditorWindowBase`

> **本章与第 10 章的内容已迁至独立包 `cn.rendu.editorwindow`**（程序集 `RD.EditorWindow.Editor`），见该包的 `README.md`。迁移后不变的承诺：
>
> - 引擎仍接受 `EditorWindow host`（传窗口实例表示窗口自绘）—— 窗口包的基类就是用它画窗口自身与右侧面板；
> - 跨程序集宿主各持独立会话：`Engine.CreateHostState()` 拿不透明句柄，绘制时传 `DrawObject(target, host, onBeforeChange, onChanged, state)`；
> - 命名空间是 `RD.EditorWindows.Editor`（**复数**）—— 单数 `RD.EditorWindow` 会遮蔽 `UnityEditor.EditorWindow` 类型：任何 `RD.*` 命名空间里裸写 `EditorWindow` 都会 CS0118（同 0.3 节末尾 `Editor` 的坑）。

---

## 第 10 章 菜单树窗口：做一个真正的工具

> **已迁至 `cn.rendu.editorwindow`**。
> `MenuEditorWindow` 的"左树 + 右面板"、`MenuTree` 的建树规则（路径自动补齐、同名就地替换、深度优先遍历、选中事件）与 `EditorPrefsValue<T>` 都在 `cn.rendu.editorwindow`，行为契约由随包迁移的 `MenuTreeTests`（14 例）钉住。
>
> 本仓库的成熟用法仍可对照阅读：`Packages/cn.rendu.ui/Editor/UITool/Window/UITool.cs`。

---


## 第 11 章 自己写宿主：直接用 `Engine.DrawObject`

### 11.1 公开签名

```csharp
public static void DrawObject(object target,
                              EditorWindow host,
                              Action<object> onBeforeChange = null,
                              Action<object> onChanged = null);
```

| 参数 | 含义 |
|---|---|
| `target` | 要绘制的对象（可以是窗口自身） |
| `host` | 宿主窗口；传 `null` 表示"宿主是 `UnityEditor.Editor`"（根对象可为 `MonoBehaviour` / `ScriptableObject`）；传窗口实例表示窗口自绘 |
| `onBeforeChange` | 数据**即将写入前**回调 → 宿主在此拍 Undo 快照 |
| `onChanged` | 数据**已修改后**回调 → 宿主标记脏、登记 prefab override、请求重绘 |

### 11.2 宿主必须自己承担的三件事

反思一下 `InspectorEditor` 做了什么，你自建宿主就得做同样的事：

```csharp
private void OnBeforeChange(object owner) => Undo.RecordObject(target, "Inspector");

private void OnChanged(object owner)
{
    EditorUtility.SetDirty(target);
    if (PrefabUtility.IsPartOfPrefabInstance(target))
    {
        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
    }
    Repaint();
}
```

**漏掉 `onChanged` 的后果：改动不落盘**（面板上看着变了，磁盘上没变，切场景就没了）。另外注意注释里那条经验：用 `Editor.target`（序列化根）而不是引擎回传的 `owner` 去 Undo / SetDirty —— `owner` 可能是组件内部的普通 C# 对象，对它做 Undo 不起作用。

### 11.3 示例：在原生检视器里嵌一块本包面板

```csharp
[CustomEditor(typeof(LevelRoot))]
public class LevelRootEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();       // 上半部分：Unity 原生

        var root = (LevelRoot)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("关卡模块", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        Engine.DrawObject(root.Module, null, o => Undo.RecordObject(target, "Module"), o =>
        {
            EditorUtility.SetDirty(target);
            Repaint();
        });
        if (EditorGUI.EndChangeCheck()) Repaint();
    }
}
```

### 11.4 什么时候值得这么做

- 想在**原生检视器**里嵌入一小块本包面板（其余部分保留原生体验）；
- 想用自定义布局把多个对象拼在一个窗口里；
- 想给引擎接上自己的持久化通路（例如把改动写进你自己的数据系统）。

其余场景优先用第 1 / 8 / 9 / 10 章的现成宿主，少写 Undo 逻辑。

---

## 第 12 章 综合实战：从零做一个关卡配置面板

需求：一个 `LevelConfig` 组件，要有

1. 基础信息（名字 / 模式 / 关卡 ID），关卡 ID 用下拉；
2. 波次表（增删、列宽、权重滑杆）；
3. 只在"高级模式"下出现的调参项；
4. BOSS 波次才填的字段；
5. 按钮：添加波次 / 一键清空（空表时隐藏）；
6. 校验：名字必填；
7. 波次数量 > 0 时自动展开波次表。

### 12.1 组件代码

`Assets/Scripts/LevelConfig.cs`：

```csharp
using System;
using System.Collections.Generic;
using RD.Panel;
using UnityEngine;

public enum LevelMode { Normal, Endless, Boss }

[Serializable]
public class WaveData
{
    [TableColumnWidth(120)]
    [LabelText("敌人 ID")]
    public string EnemyId;

    [TableColumnWidth(70), MinValue(1), MaxValue(100)]
    public int Count = 1;

    [TableColumnWidth(70), MinValue(0), MaxValue(1000)]
    public int Weight = 100;

    public bool IsBoss;

    [ShowIf("IsBoss")]                 // 条件字段
    [LabelText("BOSS 名称")]
    public string BossName;
}

public class LevelConfig : MonoBehaviour
{
    // ---- 基础 ----
    [BoxGroup("基础")]
    [Required("关卡必须有个名字")]
    [LabelText("关卡名")]
    public string LevelName = "New Level";

    [BoxGroup("基础")]
    [LabelText("模式")]
    public LevelMode Mode = LevelMode.Normal;   // 枚举可与字符串名比较：@Mode == 'Endless'

    [BoxGroup("基础")]
    [LabelText("关卡 ID")]
    [ValueDropdown("GetIdSelectList")]
    [OnValueChanged("OnIdChanged")]
    public int LevelId = 1001;

    // ---- 高级（整组条件显隐）----
    [FoldoutGroup("高级", VisibleIf = "@Mode != 'Normal'")]
    [LabelText("难度系数")]
    [MinValue(0.5f), MaxValue(3f)]
    public float DifficultyScale = 1f;

    [FoldoutGroup("高级", VisibleIf = "@Mode != 'Normal'")]
    [LabelText("无限模式循环波次")]
    [ShowIf("@Mode == 'Endless'")]
    public bool LoopWaves;

    // ---- 波次表 ----
    [BoxGroup("波次")]
    [TableList(DrawScrollView = true, AlwaysExpanded = true, ShowIndexLabels = true)]
    [ListDrawerSettings(CustomAddFunction = "AddEmptyWave")]
    [OnStateUpdate("@$property.State.Expanded = Waves.Count > 0")]
    [LabelText("波次列表")]
    public List<WaveData> Waves = new();

    [BoxGroup("波次")]
    [HorizontalGroup("波次/按钮")]
    [Button("添加波次", 26)]
    [PropertyOrder(100)]
    private void AddWave() => Waves.Add(new WaveData());

    [BoxGroup("波次")]
    [HorizontalGroup("波次/按钮")]
    [Button("清空", 26)]
    [GUIColor(1f, 0.5f, 0.5f)]
    [ShowIf("@Waves.Count > 0")]
    [PropertyOrder(101)]
    private void ClearWaves() => Waves.Clear();

    private void AddEmptyWave() => Waves.Add(new WaveData());

    private ValueDropdownList<int> GetIdSelectList()
    {
        var list = new ValueDropdownList<int>();
        for (var i = 0; i < 10; i++)
        {
            list.Add($"关卡 {1001 + i}", 1001 + i);
        }
        return list;
    }

    private void OnIdChanged()
    {
        if (string.IsNullOrEmpty(LevelName) || LevelName == "New Level")
        {
            LevelName = $"Level_{LevelId}";
        }
    }
}
```

### 12.2 检视器

`Assets/Scripts/Editor/LevelConfigEditor.cs`：

```csharp
using RD.Inspector.Editor;
using UnityEditor;

[CustomEditor(typeof(LevelConfig))]
public class LevelConfigEditor : InspectorEditor { }
```

### 12.3 逐段说明了什么

| 写法 | 为什么这么写 |
|---|---|
| `[BoxGroup("基础")]` 挂 3 次 | 同类型同 GroupID 会合并成一个盒子，不需要额外声明 |
| `[ValueDropdown]` + `[OnValueChanged]` | 下拉取值与"改完重算派生字段"配对出现，是本包最常见的组合 |
| `VisibleIf = "@Mode != 'Normal'"` | 分组级条件（写在 `PropertyGroupAttribute` 上，所有分组特性都能用）：不满足时**整组消失**，不留空框。枚举既可跟字符串名比（`'Normal'`），也可写 `@Mode != LevelMode.Normal`（裸标识符会解析到全局类型 `LevelMode` 的静态成员） |
| `[TableList(ShowIndexLabels = true)]` | 波次是有序的，行号有帮助；`AlwaysExpanded` 避免选中变化导致收起 |
| `[ListDrawerSettings(CustomAddFunction)]` | 工具栏的"添加"按钮走你自己的方法（这里只是 `new WaveData()`，实战里常带默认值/校验） |
| `OnStateUpdate("@$property.State.Expanded = ...")` | 唯一会被读回的表达式状态：数据为空时自动收起 |
| `[HorizontalGroup("波次/按钮")]` | 按钮排一行；同组的第二个按钮会自动接在后面 |
| `[ShowIf("@Waves.Count > 0")]` | 空表时"清空"没有意义，直接不显示 |
| `[GUIColor(1f, 0.5f, 0.5f)]` | 破坏性操作用暖色提示 |
| `[PropertyOrder(100)/(101)]` | 把按钮排在表格之后（表格默认 order 0，所以这两个是"往后推"） |

### 12.4 自测清单

照这个清单点一遍，能发现 90% 的用法错误：

1. 关掉 `Mode = Normal` → "高级"整组是否消失？
2. `Mode = Endless` → 是否出现"无限模式循环波次"？
3. 波次表清空 → "清空"按钮是否消失？再添加一行 → 是否又出现？
4. 波次数量从 0 变 1 → 波次表是否自动展开？
5. 某行勾上 `IsBoss` → 该行是否出现"BOSS 名称"输入框？
6. 删掉 `LevelName` → 是否出现红色必填提示？
7. 连续改 `LevelId` → 下拉选项是否正常切换、`LevelName` 是否跟着变？
8. Ctrl+Z 能否回退按钮造成的数据变更？

---

## 第 13 章 排错手册（症状 → 原因 → 处理）

| 症状 | 原因 | 处理 |
|---|---|---|
| 面板还是 Unity 原生样子 | 检视器没登记；或程序集没引用 `RD.Inspector.Editor`；或脚本没在 Editor 程序集 | 检查 `[CustomEditor]` 类、asmdef `references`、目录是否 `Editor/` |
| 面板全空 | `target == null`；或一个可见成员都没有 | 确认至少有 public 字段 / `[SerializeField]` / `[ShowInInspector]` |
| private 字段不显示 | 没挂 `[SerializeField]` / `[ShowInInspector]` / `[InspectorSerialize]` | 三选一（见 2.3） |
| public 字段不显示 | 是 `static`（静态字段默认不收集）；或挂了 `[HideInInspector]` | 给静态字段直接加 `[ShowInInspector]` 即可（不用改写成属性）；`HideInInspector` 优先于一切 |
| 属性不显示 | 缺 getter；或没挂 `[ShowInInspector]` / `[InspectorSerialize]` | 补上 |
| 属性画出来但不能改 | 没有 setter（引擎自动只读） | 加 setter，或接受只读 |
| 带参方法不出按钮 | 引擎只收集无参方法 | 改成无参，参数从字段读 |
| `[MinValue]`/`[MaxValue]` 没变滑杆 | 只写了一个；或用了 `MemberName` 重载 | 两个都写数值版 |
| `[TabGroup]` 没有页签 | 当前实现降级为垂直组 | 用 `FoldoutGroup` 模拟 |
| `[HideReferenceObjectPicker]`/`[HideMonoScript]`/`[Searchable]`/`[OnInspectorInit]`/`[HideInTables]` 没反应 | 未实现 | 见第 15 章清单 |
| 条件显隐完全不生效 | 表达式忘了 `@` 前缀（被当成成员名）；或成员名拼错；或条件恒为真 | 看控制台 `[Inspector]` 警告；数值判断记得 `> 0`（`Truthy(0)` 为真） |
| 条件在嵌套对象 / 表格行里不生效 | 条件是在**当前被绘制的那个对象**上求值的（嵌套实例 / 行元素），外层字段不在作用域 | 见 4.7：把条件搬到同一对象，或改在宿主层手写判断 |
| 条件每帧报错刷屏 | 不会刷屏：AST 负缓存 + `WarnOnce` | 只需修一次，改完等编译 |
| `[InlineButton]` 不出现 | 挂在容器成员（`List<T>`/字典/嵌套对象）上了 | 只对**标量成员**有效 |
| 列表元素不能拖拽排序 / 不能中间插入 | 引擎没有实现（无任何拖拽代码） | 想要排序只能自己在元素上开 `int Order` 字段 + 按钮 / 或改数据结构 |
| `List<int>` 里敲了字没反应、`List<Vector2>`/`List<Color>` 完全改不动 | 标量元素是文本框 + `ParseSimple` 解析，解析失败静默丢弃；`Vector2`/`Vector3`/`Color` 没有解析分支 | 复杂元素请用自定义类（`[Serializable]`）而不是裸 `Vector2`/`Color`（`List<Vector4>`/`List<Quaternion>` 反而能编辑，见 6.1） |
| `List<Material>` 这类列表加进去就删不掉、没有 `+`/折叠 | 首个元素是 `UnityEngine.Object` → 整列表走只读展示分支 | 别用对象列表；要引用多个资产就用 `List<MyRef>` 包一层，或自己写宿主 GUI |
| 列表里的 `null` 元素删不掉 | `null` 元素只画只读文本，没有 `-` 按钮 | 在代码里清掉 `null`（或在 setter/校验里统一处理） |
| 列表里的材质 / Prefab 引用拖进去不生效 | 同上：只读展示分支里 `ObjectField` 的返回值被丢弃 | 改用单个字段引用，或自定义包装类 |
| `[TableList]` 没变成表格 | 元素类型的成员一个都不可见（没有可表格化成员），静默退回普通列表 | 检查行类型的成员可见性 |
| 字典加不了新键、键改不了 | 字典抽屉**没有 `+`**，键只是标签；只有标量值可编辑 | 需要增删/编辑复杂值就用 `List<Entry>` + `[TableList]`（见 6.4） |
| 字典里自定义类的值只能看 | 非标量值退化成只读 `LabelField`（`ToString()`） | 同上，或把值展平成标量字段 |
| `[Flags]` 枚举只能选一个 | Popup / Toolbar 都是单选，引擎没有按位多选界面 | 拆成多个 bool，或自己写 `[OnInspectorGUI]` |
| `[TableList]` 某成员没成列 | 该成员不可见；或用了 `[HideInTables]`（未实现）/`[HideInInspector]` | 让它按可见性规则可见 |
| 表格列宽调不动 | `TableColumnWidth.Resizable` 未读取 | 目前只能改 `Width` 重编译 |
| `[ListDrawerSettings]` 大部分参数无效 | 只实现 `CustomAddFunction` / `IsReadOnly` | 见 6.3；`[TableList]` 的 `ShowPaging` 也未读取（没有内置分页） |
| 下拉是空的 | getter 返回 `null` 或空集合（退化为输入框）；或当前值匹配不上（选择框空白） | 检查 getter 返回值；确保候选里有当前值 |
| 按钮点了没反应 | 方法有参数；或方法名拼错（`[InlineButton]`/`[OnValueChanged]` 都是按名字找） | 无参 + 名字精确 |
| 改了值但重启编辑器就没了 | 宿主没实现 `onChanged`（自己写宿主时） | 见 11.2 |
| 编辑器资产里的值读回来是 0/空 | 成员没标 `[InspectorSerialize]`；或该成员是**基类的 `private` 字段**（扫描不到） | 控制台会有 `WarnOnce` 提示"只会写成 `{}`"；继承体系请用 `protected`/`public`（见 8.5 第 9 条） |
| 资产里的材质/Prefab 引用丢了 | 这条通路不保存 `UnityEngine.Object` 引用 | 改用 `[SerializeField]` 字段由 Unity 存 |
| 基类里标的字段在面板上/资产里都不见 | 它是基类的 `private` 字段（`FlattenHierarchy` 拿不到非公开实例字段的 private 档） | 改成 `protected` / `public`，或写到派生类里 |
| 资产字段改名后数据没了 | 成员名就是 JSON key，无迁移机制 | 写迁移或别改名 |
| `@` 表达式一直返回假 | 表达式有语法错（负缓存后恒为假）；或调用了不存在的方法（静默 null） | 看首次警告；用 `EvaluateBool` 手动试 |
| `@$value = ...` 改了没效果 | `$value` 只读，赋值只改表达式内部临时值 | 用 `$property.State.Expanded` 或链末端成员赋值 |
| `State.Visible` / `State.Enabled` 写了没效果 | 引擎只读回 `Expanded` | 显隐用 `[ShowIf]`，启禁用 `[EnableIf]`/`[DisableIf]` |
| 折叠状态莫名其妙复位 | 会话状态不写盘，域重载后回默认 | 需要持久化请自己用 `*Prefs` |
| 新增成员后不显示 | 成员清单按类型缓存 | 等编译/域重载（正常流程会自动发生） |
| 多选时只有一个对象生效 | 设计如此，并会显示黄色 `HelpBox` | 需要多选编辑请用原生检视器 |
| 预制体实例上看不到 override 标记 | 反射写值的固有限制 | 已知差异，接入前评估 |
| 报 CS0118 关于 `Editor` | 命名空间遮蔽 | 基类写 `UnityEditor.Editor` |
| Unity 的 `[Tooltip]` / `[Range]` / `[TextArea]` / `[Header]` / `[Delayed]` 没反应 | 引擎不读取 Unity 原生装饰特性 | 用本包对应特性（`[InfoBox]`、`[MinValue]`+`[MaxValue]`、`[DisplayAsString]`、`[Title]`…） |

---

## 第 14 章 选型速查（我要 X → 怎么写）

| 我要… | 写法 |
|---|---|
| 改显示名 | `[LabelText("…")]` |
| 不显示标签 | `[HideLabel]`（用在列表/字典/表格上时：折叠头只剩 `▼ (3)`，纯文本头整行不画） |
| 控制先后 | `[PropertyOrder(n)]` |
| 让私有字段出现 | `[SerializeField]` / `[ShowInInspector]` / `[InspectorSerialize]` |
| 只读 | `[ReadOnly]` |
| 数值滑杆 | `[MinValue(a), MaxValue(b)]` |
| 必填校验 | `[Required("提示")]` |
| 行着色 | `[GUIColor(r,g,b)]` 或 `[GUIColor("1,0,0,1")]`（只接受数值，不支持方法名） |
| 值以文本显示（超长截断） | `[DisplayAsString(false, 20, TextAlign.Left, true)]` |
| 运行时/编辑时隐藏 | `[HideInPlayMode]` / `[HideInEditorMode]` |
| 盒子/折叠/一列/一行/标题/页签 | `[BoxGroup]` / `[FoldoutGroup]` / `[VerticalGroup]` / `[HorizontalGroup]` / `[TitleGroup]` / `[TabGroup]`(降级) |
| 一键按钮 | `[Button("文字", 高度, Icon = SdfIconType.X)]` |
| 按钮排一行 | `[ButtonGroup("组")]` 或 `[HorizontalGroup("组")]` |
| 字段旁边加按钮 | `[InlineButton("方法名", "标签")]` |
| 值变化后回调 | `[OnValueChanged("无参方法")]` |
| 插入原生 GUI | `[OnInspectorGUI("可选方法名")]` |
| 按数据自动折叠 | `[OnStateUpdate("@$property.State.Expanded = 条件")]` |
| 条件显隐/启禁 | `[ShowIf]` / `[HideIf]` / `[EnableIf]` / `[DisableIf]` |
| 整组条件显隐 | 分组特性的 `VisibleIf = "@…"` |
| 下拉选择 | `[ValueDropdown("取值方法")]` + `ValueDropdownList<T>` |
| 枚举按钮组 | `[EnumToggleButtons]` |
| 枚举项显示中文 | 在枚举值上挂 `[LabelText("中文")]` |
| 多行多列表格 | `[TableList]` + 行成员上 `[TableColumnWidth(n)]` |
| 键值对展示 | `Dictionary<K,V>`（注意：不能加新条目、键不可改、非标量值只读，见 6.5） |
| 自定义"添加"按钮 | `[ListDrawerSettings(CustomAddFunction = "方法名")]` |
| 提示 / 两段式提示 / 标题 | `[InfoBox]` / `[DetailedInfoBox]`（不是可展开控件，见 2.6）/ `[Title]` |
| 编辑器数据资产 | 继承 `SerializedScriptableObject` + `[InspectorSerialize]` |
| 组件检视器 | 继承 `InspectorEditor` + `[CustomEditor]` |
| 工具窗口 | 继承 `EditorWindowBase`（cn.rendu.editorwindow） |
| 左树右面板工具 | 继承 `MenuEditorWindow`（cn.rendu.editorwindow）+ `BuildMenuTree()` |
| 持久化上次选项/窗口宽度 | `EditorPrefsValue<T>`（cn.rendu.editorwindow） |
| 图标枚举 → Texture | `Icons.FromSdf(SdfIconType.X)`（只映射少数字形族，其余返回 `null`） |

---

## 第 15 章 当前版本的能力边界（避免踩空）

### 15.1 已声明但完全未实现

引擎源码里有一段"有意不实现的属性"注释，逐条给了理由（照抄要点）：

| 特性 | 引擎给出的理由 | 对你的影响 |
|---|---|---|
| `[HideMonoScript]` | 本检视器只画窗口自身与它反射出的成员，**从不画 MonoBehaviour 的 Script 引用栏**，没有可隐藏的对象 | 写了等于没写，行为本来就是"隐藏" |
| `[HideReferenceObjectPicker]` | 本检视器**没有引用对象选择器**（基类 / 接口的派生类型下拉），用户类一律按实际类型**内联展开**，等价于"始终隐藏选择器" | 写了等于没写；但**内联展开是默认行为**（`TreeMenuItem<T>` 那种"包装对象展开成模块面板"正是靠它） |
| `[HideInTables]` / `[Searchable]` / `[OnInspectorInit]` | 注释原话：业务侧目前无使用，**暂不实现** | 写了等于没写；表格"某列不参与"请用 `[HideInInspector]` |

（另一条例外：`[InlineEditor]` 的 Runtime 注释写着"占位 / 空操作"，但那是**过时注释** —— 引擎确实实现了内联展开，见 `DrawInlineEditor`。）

### 15.2 实现了但部分参数不读

`TabGroup`（降级为垂直组）、`PropertyGroup.VisibleIf`（**生效**）/ `AnimateVisibility`（不生效）、`ShowIf.Animate`、`OnValueChanged.IncludeChildren`、`Button.ButtonStyle` / `Button.IconAlignment`、`TableColumnWidth.Resizable`、`TableList` 的 `ShowPaging`/`ShowItemCount`、`Title` 的 `Alignment`、`HorizontalGroup` 的 `Width`、`DictionaryDrawerSettings` 的 `KeyLabel`/`ValueLabel`/`DisplayMode`、`ListDrawerSettings` 除 `CustomAddFunction`/`IsReadOnly` 外的参数、`ValueDropdown` 除 `ValuesGetter` 外的参数、`FilePath.ParentFolder`、`FolderPath` 的全部参数、`DisplayAsString.Overflow`/`AlignLabel`、`VerticalGroup.PaddingTop/PaddingBottom`、`MinValue`/`MaxValue` 的 `MemberName` 重载、`GUIColorAttribute.MethodName`、`TitleGroup.IndentChildren`。

写这些参数**不会报错**，只是没有效果 —— 这是"照 Odin 文档猜"最容易掉进的坑。

### 15.3 Unity 原生特性不生效

`[Tooltip]`、`[Range]`、`[TextArea]`、`[Multiline]`、`[Header]`、`[Space]`、`[Delayed]`、`[ColorUsage]` 等一律不读取。引擎只认：`[HideInInspector]`（跳过）、`[SerializeField]`（可见性）、`[NonSerialized]`（对引擎无影响，但影响 Unity 自己的序列化）。

### 15.4 引擎层面的固有限制

- **不内窥 `UnityEngine.Object` 成员**（只画对象引用控件，`[InlineEditor]` 才展开）；用户自定义类则**默认内联展开**；
- **容器能力有限**：列表没有拖拽排序/中间插入，标量元素走文本框（`Vector2`/`Vector3`/`Color` 不可编辑），`UnityEngine.Object` 列表只读；字典不能新增条目、键不可改、非标量值只读（详见 6.1 / 6.4 与 README §4.6）；
- 曲线/渐变用专用控件（可编辑，key 与 color/gradient 都能改）；
- 嵌套深度上限 + 环引用剪断（**绘制侧 `Engine.MaxDepth = 24`，序列化侧 `Serialization.MaxDepth = 32`**，两个数字不一样）；
- 多选只画第一个对象（并给 HelpBox 警告）；
- 不显示 prefab override 标记，也无法右键还原单个属性；
- `[Flags]` 枚举只能单选；
- 成员清单/分组树/反射查找条件/AST 都是**按类型缓存**的，靠域重载刷新；
- `HostSession` 承担折叠与滚动状态（宿主弱引用分桶，宿主回收即释放，不写盘）。

---

## 第 16 章 继续深入

### 16.1 源码地图

- **特性定义**：`Runtime/Attributes/*.cs`（纯数据，加特性先看这里）；
- **绘制引擎**：`Editor/Engine/Engine.cs` —— `GetMembers`（可见性/排序）、`GetGroupTree`（分组）、`DrawValueMemberBody`、`DrawScalarControl`、`DrawTableList`、`CheckVisibility`、`RunStateUpdates`；
- **表达式**：`Editor/Engine/Expression.cs` —— `Value`（值语义）、`GetAst`（缓存）、`Evaluate`、`SetLeft`；
- **序列化**：cn.rendu.inspector 包 `Editor/Serialization.cs`；
- **检视器宿主**：cn.rendu.inspector 包 `Editor/Inspector/InspectorEditor.cs`（窗口宿主已迁至 cn.rendu.editorwindow）；
- **逐符号索引**：`README.md` §12。

### 16.2 想给引擎加东西时的推荐流程

1. 在 `Runtime/Attributes/` 加/改特性（纯数据，别放逻辑）；
2. 在 `Engine` 里找到读取它的位置（新特性通常要动 `GetMembers`/`DrawValueMemberBody`/`DescribeGroup` 之一）；
3. **先在 `Tests/Editor/` 写断言**（固化"应该是什么行为"，尤其是你改变既有行为时）；
4. 跑离线回归（不用开 Unity，几秒钟）：

```bash
cd "Packages/cn.rendu.panel/Tools~/Verify" && dotnet run              # 跑全部
cd "Packages/cn.rendu.panel/Tools~/Verify" && dotnet run -- InspectorExpression   # 只跑一个夹具（参数是夹具名子串）
```

正常输出结尾是 `== passed: 128, failed: 0, skipped: 2 ==`（2 例依赖 Unity 运行时，脱机跑会 SKIP，见 16.3）；

5. GUI 部分没有自动化覆盖（`Begin/End` 配平、布局），改绘制流程请用 Unity Test Runner 或手工回归；
6. 如果引擎行为与文档不一致，**改文档**（本包 README/教程里"生效情况"这一列就是靠这个纪律维持可信度的）。

### 16.3 相关文档

| 文档 | 内容 |
|---|---|
| `README.md` | 特性手册：全部特性、参数、逐项生效情况、§12 符号索引、§13 测试说明 |
| 本文件 | 教程：从接入到自建工具窗口的完整路径 |
| `Tests/Editor/*.cs` | 四块纯逻辑的行为契约（表达式 44、成员收集 37、条件显隐 20、折叠状态 14，共 115 例；序列化 19 例随宿主迁至 cn.rendu.inspector，菜单树 14 例在 cn.rendu.editorwindow） |
| `Tools~/Verify/` | 离线校验工程（纯 .NET 跑同一批用例；脱机 128 通过、2 例需 Test Runner；需 .NET 8 + `UnityEngine.dll`/`UnityEditor.dll`） |

---

> 反馈：如果你照着本教程做，发现某处"文档说生效但实际没效果"（或反之），那是文档 bug —— 请按 16.2 的流程把它改对，并在 `Tests/Editor/` 留一条断言钉住。
