# RD 绘制生态 · 统一设计方案（分包 + 宿主契约 + 菜单差距）

> **定位**：本文件是**决策与现状的唯一权威**（为什么这么做、现在走到哪一步）。
> **执行步骤一律在 `Docs/Execution/*`**（索引 `Docs/Execution/00-执行索引.md`），此处不重复步骤细节。
> **引用约定**：代码位置以**符号名**为准（`Engine.DrawObject`、`MenuEditorWindow.ApplyFilter`…），行号只作当时的查找提示——本文档多处行号已随代码漂移过一次，不再以行号为据。
> **校订基线**：2026-10-31。**§1.2 是改造前的实测基线**，当前状态见 §0.2 / §0.3。全文分三档表述，请勿混淆：
> - **代码已落地**：读代码 / grep 确认存在，未跑过；
> - **已验证**：batchmode 编译 + EditMode 测试跑过（见 §0.2 末行）；
> - **待人工验收**：撤销 / 标脏 / 菜单交互 / 三包行为，必须开编辑器手点才算数。

---

## 0. 总览

### 0.1 两条不可动摇的原则
1. **整体设计（包生态）**：`utilities`（最底）→ `serialization`（独立序列化器）→ `draw`（检视器框架）→ `edittool` / `inspector`（宿主），五包作为整体演进，不可各自为政。
2. **检视器框架一体、序列化器独立**：特性 + 引擎同在 `cn.rendu.draw`（Runtime 特性 + Editor 引擎，同包分层）；`cn.rendu.serialization` 是**独立包**，**不依赖 `cn.rendu.draw`**。

### 0.2 状态总览（一屏看完）

| 项 | 内容 | 状态 |
|---|---|---|
| **S1** | Undo / 脏标记共享胶水 | 代码已落地（`Engine.BeginChange/EndChange`；`InspectorEditor` 已接线；`DrawWindow` 传 `null` 合法）—— **待人工验收**（`01` 文档 1.4 四项） |
| **S2** | `HostState` 旋钮归属 | **完成** —— 两个 `SetXxx` 均有语义 + 默认值注释 |
| **S3** | 引导样板共享 | **决议不做 / 待定**：提议名与 `DrawWindow.CreateContentArea` 撞名，且两宿主 `Driver` 生命周期不同（§2.6 D4） |
| **S4** | 术语统一 | **完成** —— 四词统一；`rootNamespace` 已修正 |
| **S5** | 序列化判定同源 | **完成** —— 判定下沉 `RD.Serialization.ValueBagTypes`，draw 的 `DefaultTypeShape` / `Engine` 转发；19 个 `SerializationTests` 通过。回归测试守护待 06 档 |
| **S6** | 多对象编辑策略 | **决议不做** —— 不上提引擎，维持 `InspectorEditor` 现状（§2.6 D5） |
| **Phase 0** | serialization 建 Runtime 程序集 + 接收位 | **完成** —— `Runtime/RD.Serialization.asmdef` + `SerializeAttribute` + `ValueBagTypes` |
| **Phase 1** | `[Serialize]` 迁 serialization | **完成** —— draw 侧文件已删，5 包消费点改完，draw 保留 57 个 Runtime 特性并声明依赖 serialization |
| **Phase 2** | serialization 去 draw 依赖 + 判定下沉 | **完成** —— serialization 仅依赖 utilities；资产载体按决议留 Editor（§1.7）；版本号不自洽按遗留处理 |
| **Phase 3** | 校验 | **编译与往返测试已验证**：batchmode 编译 0 错误；EditMode 213 用例 / 208 通过（5 个失败与本次无交集）；**剩人工行为回归**（`03` 文档 3.6 三项） |
| **菜单 P0** | 搜索框、键盘导航、自动滚动可见 | 代码已落地（`m_SearchFilter` / `ApplyFilter` / `OnMenuKeyDown` / `ScrollSelectedIntoView`）—— **待人工验收**（`02` 文档 2.4） |
| **菜单 P1** | 右键、拖拽、每项自绘、多选 UI | 未做（`04`） |
| **菜单 P2** | 持久化、置灰、面包屑、自动图标、双击 | 未做（`05`；虚拟化已决议不做） |

### 0.3 层序速览（目标已达成）

| 包 | 当前状态 | 目标 | 达成 |
|---|---|---|---|
| `cn.rendu.utilities` | `dependencies:{}`，Runtime，零依赖 | 不变 | — |
| `cn.rendu.serialization` | Runtime（`[Serialize]` + 值包判定 `ValueBagTypes`）+ Editor（JSON 内核 + 资产载体 `SerializedScriptableObject`）；**仅依赖 utilities** | 独立序列化器，位于 draw 之下 | 达成（载体按决议留 Editor，§1.7） |
| `cn.rendu.draw` | Runtime **57** 个检视特性（不含 `[Serialize]`）+ Editor 引擎；**依赖 serialization** | 检视器框架，向上依赖 serialization | 达成 |
| `cn.rendu.edittool` / `cn.rendu.inspector` | 依赖 draw；inspector 另显式依赖 serialization | 宿主仍是宿主，互不依赖 | 达成 |
| `cn.rendu.packagemanager`（消费方） | 30 处 `[Serialize]` 全部走 `RD.Serialization` | 同上 | 达成（迁移的最大外部消费点） |

最终拓扑（无环，已核验）：`utilities → serialization → draw → {edittool, inspector}`，packagemanager 作为消费方挂在 draw / edittool / serialization 上。

### 0.4 文档地图
- 第 1 章：分包架构（现状 → 目标 → 迁移清单 → Phase）。
- 第 2 章：宿主契约（术语 / 参数 / Undo / `HostState` / 接缝清单 / 决议）。
- 第 3 章：菜单系统差距（与成熟检视器菜单对照，含已落地项）。

---

# 第 1 章 分包架构

## 1.1 参考架构（已核实）
成熟开源检视器（Odin）：`Sirenix.OdinInspector` 承载**全部检视特性**，引擎在同包的 `.Editor` 子命名空间——**特性与引擎同包，靠 Runtime/Editor 分层，无独立契约包**；宿主基类（`OdinEditorWindow` / `OdinMenuEditorWindow`）也在 `Sirenix.OdinInspector.Editor` 内。其序列化器 `OdinSerializer` 是**完全独立的包**，不依赖检视器，且自带自己的序列化属性。

要点：把特性放进检视器框架包解决"共享词汇"；让序列化器完全独立、自带所需解决"序列化依赖检视器"。本方案据此定层序。

## 1.2 改造前的实测基线（2026-10-30）

> 以下记录的是**分包改造前**的状态，用于对照与追溯；当前状态见 §0.2 / §0.3。

**依赖与程序集**
| 包 / 程序集 | references |
|---|---|
| `RD.Utilities`（Runtime） | 无 |
| `RD.Draw`（Runtime） | 无 |
| `RD.Draw.Editor` | `RD.Draw` |
| `RD.Serialization.Editor` | `RD.Draw`、`RD.Draw.Editor`、`RD.Utilities` ← **倒置根因** |
| `RD.Inspector.Editor` | `RD.Draw`、`RD.Draw.Editor`、`RD.Serialization.Editor` |
| `RD.EditTool.Editor` | `RD.Utilities`、`RD.Draw`、`RD.Draw.Editor` |
| `RD.PackageManager.Editor`（消费方） | `RD.Draw`、`RD.Draw.Editor`、`RD.EditTool.Editor`、`RD.Serialization.Editor`、`RD.Utilities` |

**事实清单（影响方案，必须纳入）**
1. **`cn.rendu.serialization` 没有 Runtime 程序集**：只有 `Editor/`（`SerializedScriptableObject.cs`、`Serialization.cs`、`SerializationUtility.cs`、`AssemblyInfo.cs`）。因此 `SerializedScriptableObject` 当前是 **Editor-only** 资产载体——目标方案要求它进 Runtime，这是"新增程序集 + 迁文件"，不是"确认目录已存在"。
2. **倒置只剩一处**：`Serialization.cs` 的 `IsUnityValueBagType` 判定经 `Engine` 取（`using RD.Draw.Editor;`）。`[Serialize]` 特性本体在 `draw/Runtime/Attributes/Core/SerializeAttribute.cs`。
3. **`[Serialize]` 全仓 74 处、跨 4 包**：`serialization/Tests`（30）、**`packagemanager`（30：HubData 11 / VersionData 9 / DependencyInfo 4 / VersionAsset 3 / HubAsset 3）**、`serialization/Editor`（8）、`inspector/Editor`（3）、`draw/Editor/MemberCollector`（2）、`draw/Tests`（1）。**packagemanager 是与 serialization 并列的最大消费方，设计方案原先完全没列它。**
4. **版本号不自洽**：`cn.rendu.draw/package.json` 是 `0.0.0`，而 serialization / edittool / inspector 都声明 `"cn.rendu.draw": "3.0.0"`。改依赖声明时须一并定夺（对齐实版 or 统一抬版本）。
5. **共享源码 `asmref`**：serialization / inspector / edittool 三包都有 `Scripts/Editor/Share/AssemblyReference.asmref → RD.Editor`（init 包）。这些 `Share` 源码编译进 `RD.Editor`，不进本包程序集——**去 draw 依赖时要单独确认 `Share/` 下没有 `RD.Draw` 引用**，否则本包清零了、编译仍失败。
6. **`RD.EditTool.Editor` 的 `rootNamespace` 是 `RD`**，与源码命名空间 `RD.EditTool.Editor` 不一致（术语与命名清理项）。

## 1.3 目标依赖图（最终层序）

```
依赖方向（底 → 顶，无环）：

cn.rendu.utilities          通用工具（零业务、零依赖）                        [Runtime]
      │
      ▼
cn.rendu.serialization      独立序列化器（位于 draw 之下）
  · Runtime: [Serialize] 特性 + 值包判定 ValueBagTypes（命名空间 RD.Serialization）
  · Editor:  JSON 序列化内核 + SerializedScriptableObject 资产载体（编辑器数据资产，依赖内核，故留 Editor，§1.7）
  · 依赖:    utilities（仅此），不依赖 draw
      │
      ▼
cn.rendu.draw               检视器框架（向上依赖 serialization）
  · Runtime: 检视/布局特性（[ShowInInspector]/[ReadOnly]/[Button]/[BoxGroup]/[TableList]/[InfoBox]… 57 文件，不含 [Serialize]）
  · Editor:  渲染引擎 + DefaultTypeShape/ITypeShape（渲染形态分类，留 draw）
      │
      ▼
cn.rendu.inspector   cn.rendu.edittool      （宿主：Editor / EditorWindow）
```

要点：`serialization` 在 `draw` 之下，`draw` 向上依赖它；无中间契约包。倒置消除（serialization 不再引用 draw，draw 反向依赖它），`draw` 的 Runtime 特性纯粹是"如何渲染/检视"的词汇。

## 1.4 边界判定规则（放哪个包）
- **数据事实 / 资产载体 / 序列化行为** → `serialization`（`[Serialize]`、`SerializedScriptableObject`、值包判定）。判据：脱离任何 UI 也有意义。
- **"如何渲染 / 如何检视"的词汇（特性）与实现（引擎）** → `draw`。判据：只在绘制上下文里有意义（`[BoxGroup]`、`[TableList]`、`DefaultTypeShape`）。
- **谁来调引擎、Undo 怎么接、窗口长什么样** → 宿主（`inspector` / `edittool`）。
- **共用判定冲突时**：下层定义事实，上层复用（见 §2.6 D6）——绝不允许下层反向引用上层。

## 1.5 迁移清单（逐包）

| 包 | 动作 |
|---|---|
| **cn.rendu.serialization** | ① **新建** `Runtime/RD.Serialization.asmdef`（references: `RD.Utilities`）；② 接收 `SerializeAttribute`（命名空间 `RD.Serialization`）；③ `SerializedScriptableObject` **按决议留在 `Editor/`**（详见 §1.7）——它依赖 JSON 内核且定位是编辑器数据资产；④ 新增 `IsUnityValueBagType`（AnimationCurve / Gradient，public，供本包与 draw 复用）；⑤ Editor 层去 `RD.Draw` / `RD.Draw.Editor` 引用，仅留 `RD.Utilities`；⑥ `package.json` 去掉 `cn.rendu.draw`；⑦ 确认 `Scripts/Editor/Share/` 内无 `RD.Draw` 引用；⑧ Editor asmdef references 加 `RD.Serialization`。 |
| **cn.rendu.draw** | ① 迁出 `Runtime/Attributes/Core/SerializeAttribute.cs`（**保留其余 57 个检视特性与 Runtime 程序集，不变纯 Editor 包**）；② `MemberCollector` 两处 `typeof(SerializeAttribute)` 改 `using RD.Serialization;`；③ `DefaultTypeShape.IsUnityValueBagType` 改为复用 `RD.Serialization` 的判定（`Engine.IsUnityValueBagType` 保留为转发，注释同步）；④ `package.json` 增加 `cn.rendu.serialization`；`Editor/RD.Draw.Editor.asmdef` 的 `references` 加 `RD.Serialization`（Runtime 侧无消费点，不加）；⑤ `Tests` 里 `EngineReflectionTests` 的 `[Serialize]` 改 `using`。 |
| **cn.rendu.packagemanager** | **最大外部消费点**：5 个文件（HubData / VersionData / DependencyInfo / VersionAsset / HubAsset）改 `using RD.Serialization;`；asmdef 需能访问 `RD.Serialization`（Runtime 程序集）。 |
| **cn.rendu.inspector** | `SerializedScriptableObjectEditor` 等 3 处 `[Serialize]` 改 `using RD.Serialization;`；依赖不变（显式保留 serialization 声明，不靠 draw 传递）。 |
| **cn.rendu.edittool** | 无 `[Serialize]` 消费；依赖不变。 |

**注意项**
- `[Serialize]` 命名空间改 `RD.Serialization` 是**破坏性变更**：上表 5 个包、74 处标记点需改 `using`。零破坏备选：物理迁入 serialization 包但保留 `RD.Draw` 命名空间（命名空间 ≠ 包名）——**决策：接受破坏性变更**，一次性改完，发布说明给迁移指南；理由是"命名空间跟着包走"是后续唯一能自解释的约定。
- **draw 的 Runtime 程序集保留**：仅 1 个文件迁出，draw 仍是 Runtime + Editor 同包分层。
- **注释同步清单（易漏）**：`Engine.IsUnityValueBagType` 的 XML 注释当前写着"宿主 serialization 要跟引擎用同一条判定，不能各写一份"，`DefaultTypeShape` 注释写着"全仓仍只有一份实现"——二者都描述**旧决策**，Phase 2 时必须改写为"转发下层 `RD.Serialization` 判定"。
- 版本号不自洽（§1.2-4）在本次改动中一并定夺，否则新依赖声明又写出一个对不上的版本号。

## 1.6 分阶段实施路线（Phase 0~3 已完成）
- **Phase 0 — serialization 接收位（纯增量）**：新建 `Runtime/RD.Serialization.asmdef` + `Attributes/SerializeAttribute.cs`（命名空间 `RD.Serialization`）+ 值包判定；不删旧文件、可随时编译验证。
- **Phase 1 — 迁 `[Serialize]`**：draw 侧文件迁出并改命名空间；draw 的 `MemberCollector`、`Tests` 与**全部消费方**（serialization / inspector / packagemanager）改 `using RD.Serialization;`；draw 的 package.json + 两个 asmdef 增加 serialization 依赖。**draw 保留 Runtime 程序集（57 属性）**。
- **Phase 2 — serialization 去 draw 依赖**：Editor 层去掉 `RD.Draw` / `RD.Draw.Editor`；`SerializedScriptableObject` 已在 Runtime；判定本地化；`package.json` 去 draw；`Share/` 复核；同步 §1.5 注释清单。
- **Phase 3 — 校验**：全仓编译；grep 确认 serialization 内 `RD.Draw` 引用清零、draw 内 `SerializeAttribute` 已迁出但 Runtime 仍在；序列化往返结果不变；inspector / edittool / packagemanager 行为不变。

## 1.7 风险与取舍
- **`[Serialize]` 破坏性变更**：面 5 包 74 处，已列清单，逐包改；提供迁移指南。
- **新建 Runtime 程序集**：`SerializedScriptableObject` 从 Editor-only 变 Runtime，需确认其在 Editor 侧的用法（`SerializedScriptableObjectEditor`、序列化内核）不依赖 Editor 程序集；若有，考虑拆分或保留 Editor 扩展方法。
- **值包判定下沉**（§2.6 D6）：语义上"AnimationCurve / Gradient 是 Unity 原生值包"是**数据事实**，不是绘制逻辑，放在下层成立；draw 的 `DefaultTypeShape` 保留"渲染形态分类"（容器 / 标量 / 可内联），只把值包这一条复用下层。
- **宿主包拆分偏差**：成熟检视器把宿主基类放进引擎包内；我们拆成 `inspector` / `edittool` 两个独立包。属可接受偏差（宿主可独立发布），不强制合并。
- **已消解的原风险**：旧方案为换独立而"牺牲 `IsUnityValueBagType` 单一来源"——下沉方案下无需牺牲（§2.6 D6）。
- **资产载体不下沉（实施决议）**：`SerializedScriptableObject` 与 JSON 内核留在 Editor。它调用 `Serialization.Serialize/Deserialize`（Editor 程序集），下沉就得连内核一起搬；且它定位是"编辑器数据资产载体"，`[CustomEditor]` 与消费方（packagemanager）都在 Editor 侧，没有运行时场景。**Runtime 只承载真正被两侧共用的数据事实：`[Serialize]` 与值包判定。** 若日后需要运行时可用的载体，再连内核一起下沉。
- **日志通道不能下沉**：去 draw 依赖时发现 `Serialization` 还用了 `DrawLog`（Warn/WarnOnce）。判定可以下沉共用（数据事实），日志通道不行——否则下层就要依赖上层。已在 serialization 建本地 `SerializationLog` 替代。
- **版本号不自洽（遗留）**：`cn.rendu.draw` 实版 `0.0.0` 与四包声明的 `3.0.0` 仍未统一。本次新增依赖均写实版，未动既有声明；抬版或改声明属发布策略，留待发布时定。

---

# 第 2 章 宿主契约

## 2.1 依赖视角（宿主）
宿主只认识 `draw` 的门面 `Engine`；完整五包拓扑见 §1.3（`serialization` 在 `draw` 之下、`utilities` 在最底）。两个宿主都只依赖 `draw`（inspector 另显式声明 serialization），互不依赖。引擎内部无进程级可变绘制状态。

## 2.2 统一术语表（Glossary）
跨包（draw / serialization / edittool / inspector / utilities）注释、文档、代码必须用以下统一词：

| 术语 | 含义 |
|---|---|
| **引擎 Engine** | 绘制引擎门面（`RD.Draw.Editor.Engine`），宿主唯一直接依赖的类型。 |
| **宿主 Host** | 调用 `Engine.DrawObject` 的一方。两类：窗口宿主（`EditorWindow`，`DrawWindow`）、检视器宿主（`UnityEditor.Editor`，`InspectorEditor`）。 |
| **后端 Driver** | 保留模式渲染器，按位置复用 `VisualElement`。每次绘制显式传入。 |
| **会话 HostState / Session** | 每宿主一份的折叠/滚动状态，**互不串台**，且不透明（宿主可位于其它程序集）。 |
| **根对象 target** | 被绘制的任意 C# 对象。 |
| **内容区 Content Area** | Driver 挂载的容器（默认整窗 root；菜单窗口为右面板）。 |
| **特性 Attribute** | Runtime 层纯声明，**宿主无关**（检视器面板、工具窗口、任何直接调用 `Engine.DrawObject` 的宿主都生效）。 |

> EditTool 内的"外壳 / Shell"**仅指窗口 UIElements 布局**（菜单 + 分割条），不可与引擎的"宿主 / 后端 / 会话"混淆。该约定已在代码注释落地。

## 2.3 宿主契约

### 2.3.1 参数约定
`Engine.DrawObject` 是一族重载（4 参 / +`HostState` / +`Driver` / +`HostState`+`Driver` 六个参），6 参为完整形态：
```csharp
Engine.DrawObject(object target, EditorWindow host,
                  Action<object> onBeforeChange,
                  Action<object> onChanged,
                  HostState state,
                  Driver driver);
```
- **host**：窗口宿主传 `this`；检视器宿主传 `null`（`Editor` 非窗口），引擎据此放开 `MonoBehaviour` / `ScriptableObject` 根对象。
- **state**：每宿主持一份 `Engine.CreateHostState()`，生命周期 = 宿主实例。
- **driver**：每内容区一份；**绘制中与事件回调必须复用同一份**。
- **onBeforeChange / onChanged**：见 §2.3.2。

### 2.3.2 Undo / 脏标记策略（S1）
引擎已提供宿主无关的变更胶水（`Engine.BeginChange` / `Engine.EndChange` / `ChangeScope`）：构造/调用时 `Undo.RecordObject`，结束时 `SetDirty` + `PrefabUtility` + `Repaint`。

| 宿主 | host | onBeforeChange | onChanged | 状态 |
|---|---|---|---|---|
| `InspectorEditor` | `null` | `Engine.BeginChange(target)` | `Engine.EndChange(target)` | 已接线 |
| `DrawWindow` | `this` | `null` | `null` | 纯内存窗口，合法 |

**契约补充（从实现中提炼，此前未写入文档）**
1. **必须成对**：回调风格下 `BeginChange` 返回的 `ChangeScope` 被丢弃，脏标记靠 `EndChange` 补上——只用其一会丢一半。宿主不得拆开用。
2. **传序列化根，不传引擎回传的 owner**：引擎回调给的 `owner` 可能是组件内部的普通 C# 对象（如 `UIData`），对它 Undo / SetDirty 无效。宿主一律传自己的 `target`。
3. **纯内存窗口规则**：仅当宿主确实不编辑可序列化对象时方可传 `null`，且必须在宿主的 XML 注释写明"本窗口仅改内存状态、不接变更回调"（`DrawWindow.DrawObject` 已如此提示子类）。若 EditTool 日后出现"编辑序列化资产"的窗口，直接接这对胶水，不要重抄 Inspector 的旧实现。

### 2.3.3 HostState 会话字段归属（S2）
两个旋钮均已文档化（语义 + 默认值），宿主只设自己用得到的那个，未设的取默认：
- `SetViewportWidth(width)`：滚动区视口宽，供表格均分列宽；**默认 0 = 不提供**，后端自量（检视器显式清 0）。
- `SetContentBottom(bottom)`：内容区可用下沿，供表格填满剩余高度；**默认 0 = 不提供**，退回"按行数估高"（菜单窗口按滚动位置折算后上报）。

引擎只读自己需要的 facet，互不串台。

### 2.3.4 Driver + Session + Redraw 引导样板（S3）
两宿主的引导实质相同：`m_Session` 字段初始化器 + `new Driver(panel, Redraw)` + `Redraw()`。

> **复核结论（见 §2.6 D4）**：样板本身只有两行、两处；且两宿主 `Driver` 的**生命周期不同**（窗口跨帧复用、`OnDisable` 清空；检视器每次 `CreateInspectorGUI` 重建）。因此不建议为"消重"而强行抽 helper；若仍要抽，必须先改名避开 `DrawWindow.CreateContentArea`。

### 2.3.5 与 serialization 的关系（S5）
旧方案让 serialization 自带一套判定、draw 留一套，"接受 2 行重复"。**已优化为判定下沉**（§2.6 D6）：serialization 定义值包判定，draw 的 `DefaultTypeShape` 与 `Engine.IsUnityValueBagType` 转发复用——方向合法（上层用下层），单一来源保留，无重复。

## 2.4 接缝清单与修复路线

| # | 接缝 | 现状 | 建议 | 优先级 |
|---|---|---|---|---|
| **S1** | Undo/脏标记策略分裂 | 引擎已提供胶水；`InspectorEditor` 已接线；`DrawWindow` 传 `null`（合法） | 契约三条已写入 §2.3.2；EditTool 出现序列化编辑窗口时接线 | 守成（剩人工验收，见 `01` 文档 1.4） |
| **S2** | `HostState` 字段归属不清 | 已完成（注释含默认值语义） | 无需动作 | 已解 |
| **S3** | 引导样板重复 | 两处各写两行；**提议名与 `DrawWindow.CreateContentArea` 撞名** | 改名后再议（§2.6 D4） | P2 |
| **S4** | 术语不一致 | 已落地 | 清理 `RD.EditTool.Editor` 的 `rootNamespace` | P2 |
| **S5** | 序列化判定须与引擎同源 | **已完成**：判定下沉 `RD.Serialization.ValueBagTypes`，draw 转发；19 个 `SerializationTests` 通过 | 回归测试守护待 06 档 | P1（仅剩测试） |
| **S6** | 多对象编辑策略 | Inspector 用 `[CanEditMultipleObjects]` + UIElements `HelpBox` 自处理；EditTool 无多对象场景 | **建议不上提引擎**（§2.6 D5）；只补注释说明 | P2（维持现状） |

## 2.5 落地路线（Roadmap）
- **已完成**：Phase 0~3（分包重设计 + 编译与测试验证）；S2、S4、S5；菜单 P0（搜索 / 键盘 / 自动滚动）。
- **当前**：人工验收 —— `01` 文档 1.4（撤销 / 标脏 / 预制体 / 工具窗口）、`02` 文档 2.4（菜单交互）、`03` 文档 3.6（三包行为回归）。
- **下一步**：`04` 菜单 P1（右键、拖拽、每项自绘、多选 UI）；随后 `05` 菜单 P2（虚拟化已决议不做）、`06` 回归测试守护。
- **不做**：S3（改名待定，建议不做，§2.6 D4）、S6（不上提引擎，§2.6 D5）、菜单虚拟化。
- **不上提的**：搜索 / 键盘 / 右键等是菜单**外壳**的交互能力，与"任意调用方都能用引擎绘制"无关；上提会污染引擎。只有宿主无关特性（§2.2）才在 Draw。

## 2.6 设计决议（已拍板 + 本轮新增）
1. **不引入 `IDrawHost` 接口**。两宿主基类不同（`EditorWindow` / `UnityEditor.Editor`），无法靠继承统一；契约已由 `Engine.DrawObject` 的参数（host / state / driver / 回调）表达，再加接口只增仪式。维持参数化契约。
2. **Undo/脏标记采用引擎共享胶水**（方案 X）：`Engine.BeginChange/EndChange`，两宿主共用；纯内存窗口传 `null` 须显式注释前提。已落地，补契约三条。
3. **交互增强不宿主无关化、不上提 Draw**：外壳交互归宿主。
4. **（新增）S3 的引擎 helper 必须改名**：`Engine.CreateContentArea` 与 `DrawWindow.CreateContentArea(parent, draw, scroll)` 同名但语义不同（后者返回 `VisualElement`、建滚动容器），在 `DrawWindow` 内会直接撞名/歧义。若实施，改名为 `Engine.CreateHostContent(panel, redraw) → (HostState state, Driver driver)`；并先确认它能容纳两种 `Driver` 生命周期，否则维持现状更划算。
5. **（新增）S6 不上提引擎**：多对象只有 `InspectorEditor` 一处、本质是 3 行 `HelpBox`；引擎侧新增 `DrawTargets` 要再铺一套（state / driver / 回调）重载族，收益低于成本。维持宿主自处理 + 注释说明；若 EditTool 日后真出现多对象场景再上提。
6. **（新增）值包判定下沉 serialization**（替代"各自实现、接受重复"）：`RD.Serialization` 提供 `IsUnityValueBagType`（AnimationCurve / Gradient），draw 的 `DefaultTypeShape` 与 `Engine` 转发复用。方向 `draw → serialization` 合法，单一来源保留，`Engine.IsUnityValueBagType` 保留公开以免破坏既有调用方，但注释须改为"转发下层判定"。

---

# 第 3 章 菜单系统差距（与成熟检视器菜单对照）

> 目的：`MenuEditorWindow` + `MenuTree` 采用成熟检视器菜单的左右分栏范式（左树 UIElements 手搭、右栏交引擎绘制）。本章梳理现有能力并对照成熟检视器菜单列出差距。
> 范围：只针对**左侧菜单树**这一外壳 UI 层；右侧"引擎绘制 Value"由 `DrawObject` 实现，与成熟检视器右栏等价，不在此章讨论。

## 3.1 现有架构（与成熟检视器范式一致）

| 模块 | 位置 |
|---|---|
| 菜单数据模型 | `Editor/Menu/MenuTree.cs`、`MenuTreeItem.cs`、`MenuTreeSelection.cs` |
| 资源挂载扩展 | `Editor/Menu/MenuTreeAssetLoader.cs`、`MenuTreePathUtil.cs` |
| 菜单渲染 + 外壳 | `Editor/Window/MenuEditorWindow.cs` |
| 模块注册框架 | `Editor/Tool/BaseTreeMenuItem.cs`、`ToolMenuAttribute.cs`、`GenericToolWindow.cs` |
| 选型弹窗（含过滤实现） | `Editor/Selectors/TypeSelectorWindow.cs` |

数据流：`BuildMenuTree()` → `BuildMenuUI()` 渲染成 UIElements → 选中变化经 `Selection.SelectionChanged` 双分发（`OnSelectionChanged` 业务层 + `OnMenuSelectionChanged` UI 层）→ 右栏 `RedrawContent()` 用 `DrawObject(SelectedValue)` 交引擎。

## 3.2 已实现能力

| 能力 | 实现位置 |
|---|---|
| 路径式增删 + 自动建文件夹 | `MenuTree.Add` / `EnsureFolder` |
| 图标（`Texture2D` / `EditorIcon`） | `MenuTreeItem.Icon` |
| 单选 + 高亮刷新（不重建整树） | `MenuTreeSelection` + `RefreshMenuSelection` |
| 展开 / 折叠 | `MenuTreeItem.Toggled` + foldout 箭头（`ApplyToggle`） |
| **搜索框 / 过滤**（只改行 display，不重建树；命中项强制展开祖先） | `m_SearchFilter` + `ApplyFilter` + `MenuTreePathUtil` |
| **键盘导航**（按可见顺序跳过隐藏项；搜索框输入中不拦截） | `OnMenuKeyDown` + `m_VisibleOrder` / `RebuildVisibleOrder` |
| **选中后自动滚到可见** | `ScrollSelectedIntoView`（选中变化与键盘移动后调用） |
| 搜索态跨重建保持（关键字回填 + 幂等重套过滤） | `RebuildMenuTree` 中回填 `m_Filter` |
| Value 任意对象 → 右栏引擎绘制 | `DrawObject(target)`（`RedrawContent`） |
| 资源批量挂载 | `MenuTreeAssetLoader.AddAllAssetsAtPath` 等 |
| 属性驱动注册 + Order 排序 | `ToolMenuAttribute` + `GenericToolWindow` 排序 |
| 分割条拖拽改宽 + 空态提示 | `BuildSplitter` / `m_ContentEmptyHint` |
| 选中事件业务/UI 双分发 | `SubscribeSelectionEvents` |
| 内容区下沿上报（表格填满剩余高度） | `ReportContentBottom` → `HostState.SetContentBottom` |

## 3.3 差距清单（对照成熟检视器）
编号沿用执行文档引用；"状态"列为本轮校订结果。

| # | 能力 | 说明 | 状态 |
|---|---|---|---|
| ① | 搜索框 / 过滤 | 对标 Odin `DrawSearchToolbar` | 已落地 |
| ② | 键盘导航 | 方向键移动 / 首字母定位 / Enter 确认 | 已落地（首字母定位未做） |
| ③ | 右键上下文菜单 | 对标 Odin `OdinMenuItem.OnContextMenu` | 未做 |
| ④ | 拖拽 | 对标 `EnableDragSupport` / `EnableDropSupport`（项重排、拖资源进菜单） | 未做 |
| ⑤ | 多选 UI 语义 | 数据层 `MenuTreeSelection.Select(MenuTreeItem[])` 已支持；UI 无 Ctrl/⌘ 多选，右栏只取首项 | 未做 |
| ⑥ | 每项自定义绘制 | 对标 `OnDrawMenuItem`；当前行模板写死（箭头+图标+文字） | 未做 |
| ⑦ | 选中后自动滚到可见 | — | 已落地 |
| ⑧ | 展开 / 选中态持久化 | 搜索关键字已持久化；`Toggled` / 选中态重建仍会丢 | 部分 |
| ⑨ | 面包屑 / 标题栏 | 对标 `DrawBreadCrumbs` | 未做 |
| ⑩ | 项置灰 / 禁用 | 对标 Odin `enabled`；无 `Enabled` 标记 | 未做 |
| ⑪ | 类型自动图标 | 对标按对象类型自动选图标；当前需手动传 | 未做 |
| ⑫ | 大数据树虚拟化 | 当前 `BuildMenuItems` 全量递归构建 | **决议不做**（见 `05-菜单打磨.md`） |
| ⑬ | 双击确认 vs 单击选中 | 对标 `ConfirmSelectionOnDoubleClick`；当前单击即选中 | 未做 |

## 3.4 优先级（按当前状态重排）
- **已完成**：① 搜索框、② 键盘导航、⑦ 自动滚动可见（P0 三项）。
- **P1（交互完整度，当前重点）**：③ 右键菜单、④ 拖拽、⑥ 每项自绘、⑤ 多选 UI。
- **P2（打磨）**：⑧ 持久化、⑩ 置灰、⑨ 面包屑、⑪ 自动图标、⑬ 双击语义；⑫ 虚拟化不做。

> 落地步骤与验收属执行层：`Docs/Execution/02-菜单搜索与键盘.md`（① ② ⑦）、`04-菜单交互完整度.md`（③ ④ ⑤ ⑥）、`05-菜单打磨.md`（⑧ ⑨ ⑩ ⑪ ⑬）；总序见 `00-执行索引.md`。
