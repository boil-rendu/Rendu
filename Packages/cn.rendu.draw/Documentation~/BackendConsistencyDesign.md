# RD.Draw 架构与设计决策

> **状态**：Phase 1 / 2 / 3 / 4 均已落地。本引擎现在是**单一 UIElements（保留模式）后端**。
> 实施：`0ac3c53c`（令牌 / 布局 / 树骨架）+ 后续接入（`Engine` 收口、集合 / 表格 / 分页产树）。
> 范围：`Packages/cn.rendu.draw` 的绘制引擎（`Editor/` 反射引擎 → 单一 `Driver` 后端）。
> 目标读者：参与绘制层评审的开发者。

---



## 目录

- 0 一句话结论
- 1 目标与验收标准
- 2 现状诊断（2.0 Runtime/Editor 架构 · 2.1 保留骨架 · 2.2 收敛前的问题）
- 3 设计模式总览
- 4 目标架构（4.1 分层 · 4.2 RDTheme · 4.3 LayoutPolicy · 4.4 Widget · 4.5 IControlRenderer）
- 5 类结构
- 6 关键设计决策
- 7 迁移复盘（Phase 1~4）
- 8 风险与回滚
- 9 验收清单
- 附录 A：令牌映射清单
- 附录 B：布局规则清单
- 附录 C：`控件词汇表` → `Widget` 树对应表
- 附录 D：文件 / 类 清单核对

---

## 0.1 实施进度

| 阶段 | 状态 | 落地内容 |
|---|---|---|
| Phase 1 设计令牌 | ✅ | `Theme/RDTheme.cs`（颜色 / 几何 / 控件 chrome / 文本按钮规格）、`IMetricsProvider` + `EditorMetricsProvider`；`DrawStyles` / `DrawMetrics` 已删除，后端全部改读 `RDTheme` |
| Phase 2 布局单源 | ✅ | `Theme/LayoutHint.cs` + `LayoutPolicy.cs`（`ForField` / `ForButton` / `ForLabel` / `ForBox` / `ForToolbarItem`），后端只做翻译；`Tests/Editor/LayoutPolicyTests.cs` 14 个用例 |
| Phase 3 声明式控件树 | ✅ | `Widgets/*`（33 个节点）+ `WidgetPool`（节点复用，设计决策 4）+ `WidgetBuilder`（产树 + 变更记录）+ `Renderer` + `RenderContext` + `ChangeApplier`；`InspectorRenderer` 是唯一收口：`Renderer.Render(tree)` → `ChangeApplier.Apply()` → `WidgetPool.Release(tree)`。分组 / 成员 / 值控件 / 集合 / 表格 / 分页 / 批注 / 按钮全部产树，规则层已不再直接调 `Driver` 绘制原语 |
| Phase 4 原生控件接口线 | ✅ 部分自绘 | `Rendering/IControlRenderer.cs`：接口 + 默认实现 `DriverControlRenderer`（控件 → `Driver` 原语）；`ControlRenderer` 对 `Toggle` / `Slider` / `Toolbar` / `ToolbarMulti` 走**自绘**（`PaintedToggleField` / `PaintedSliderField` / `PaintedToolbarField` / `PaintedMultiToolbarField`，颜色尺寸全部取 `RDTheme`，交互仍走 `NoteChanged` / `HasPendingChange` 那套"未消费改动不覆写"），其余控件委托 `Driver` 的原生控件。像素级几何换算在 `Drivers/PaintedSliderMath.cs`，有 8 个用例 |

Phase 3 的已知遗留（不阻塞验收）：

- **`ValueChange` / `ButtonAction` 仍每帧 `new`**：节点已经池化，但每帧的变更记录与按钮记录是普通对象（且它们携带的 `ExtractNewValue` / `OnValue` / `OnClick` 闭包本身也要分配），池化收益有限，暂不做。
- **`Renderer` 是单一解释器**：`Renderer` 直接遍历 `Widget` 树并调用 `Driver` 原语，控件节点交给注入的 `IControlRenderer`。分叉点只在注入，不在类结构。

**自动化覆盖**（本轮新增，`Tests/Editor/`）：`WidgetPoolTests`（8 例）钉住池化不变量 —— 借出即默认值、解释器回填字段不复用、钩子闭包断开、子树清空、长列表不保留容量、重复归还无操作；`WidgetRendererTests`（11 例）钉住整链 —— 遍历顺序、滚动三钩子相对 `BeginScroll` 的位置、`Mark/Restore` 回滚、值/折叠/按钮回放、"值先于按钮"、未变更不写不重绘。已用 Tuanjie 自带 Mono 直接执行测试程序集：**19/19 通过**（Unity Test Runner 里同样可跑）。

**必须人工回归的功能**（自动化测不到观感）：列宽拖拽（`ColumnResizer` 结果下一帧生效）、列表拖拽排序（`ReorderItemWidget` 条目区间在渲染后登记）、分页跳转、折叠头、表格滚动区高度自适应（`ScrollWidget` 的三个测量钩子）。

---

## 0. 一句话结论

把"外观 / 布局 / 控件描述"各自收成**一份单源**（`RDTheme` / `LayoutPolicy` / `Widget` 树）；后端退化成"读同一份数据、翻译成 `Driver` 原语"的哑解释器（`Renderer`）。"一致"由"同一份描述、同一份令牌、同一份规则"从结构上保证，而不是靠两端代码写得像。

---

## 1. 目标与验收标准

### 1.1 目标

> 同一个控件、同样的输入数据，在经过单一 `Driver` 后端渲染时，外观与布局完全由 `RDTheme` / `LayoutPolicy` 决定，后端零重复、零写死。

拆成三条可验收的标准：

- **A. 外观值单源**：颜色 / 尺寸 / 间距 / 圆角 / 悬停色等视觉值，在仓库里只存在一份（都在 `RDTheme`）。
- **B. 布局规则单源**：控件多宽、多大边距、何时撑满/固定，计算逻辑只存在一份（`LayoutPolicy`）。
- **C. 原生控件收敛**：`TextField / Popup / Slider / Foldout / ...` 这些委托给 `Driver` 的控件，其"渲染差异"收敛到一个明确的**接口线**（`IControlRenderer`），可插拔，不侵入引擎与规则层。

### 1.2 非目标（明确不做）

- 不在本方案里展开自绘控件的**具体像素实现**（只通过 `IControlRenderer` 预留接口线，见 §4.5）。
- 不改变对外 API（`Engine.DrawObject` 的宿主签名保持兼容，见 §7 迁移）。

---

## 2. 现状诊断

### 2.0 现有总体架构：Runtime 属性（纯数据） + Editor 反射引擎（解释器）

在进入后端层之前，先对齐本包的整体设计。`cn.rendu.draw` 本来就分两块，且**已经贯彻"数据与解释分离"**：

- **`Runtime/Attributes/`（纯数据，无逻辑）**：写在字段/属性/方法/类上的特性，只携带声明式描述，不碰任何绘制代码。完整词汇表（按目录）：

  | 目录 | 职责 | 特性 / 类型 |
  |---|---|---|
  | `Core/` | 显示名、可见性、顺序、只读 | `LabelText`、`DisplayAsString`、`ShowInInspector`、`PropertyOrder`、`ReadOnly`、`Searchable`、`Serialize`、`HideLabel`、`HideMonoScript`、`HideReferenceObjectPicker`、`InlineEditor` |
  | `Groups/` | 分组（含嵌套路径） | `BoxGroup`、`FoldoutGroup`、`TabGroup`、`HorizontalGroup`、`VerticalGroup`、`TitleGroup`、`ButtonGroup`，基类 `PropertyGroupAttribute`、`GroupKind` |
  | `Conditions/` | 条件显隐 / 启禁 | `ShowIf`、`HideIf`、`EnableIf`、`DisableIf`、`HideInEditorMode`、`HideInPlayMode` |
  | `Buttons/` | 方法按钮 | `Button`、`InlineButton`、`ButtonStyle`、`IconAlignment`、`SdfIconType` |
  | `Collections/` | 列表 / 表格 / 字典 | `TableList`、`ListDrawerSettings`、`DictionaryDrawerSettings`、`DictionaryDisplayOptions`、`EnumToggleButtons`、`HideInTables`、`TableColumnWidth` |
  | `Dropdowns/` | 下拉数据源 | `ValueDropdown`、`ValueDropdownItem`、`ValueDropdownList` |
  | `Events/` | 生命周期回调 | `OnInspectorGUI`、`OnInspectorInit`、`OnStateUpdate`、`OnValueChanged` |
  | `Text/` | 标题 / 提示框 | `Title`、`InfoBox`、`DetailedInfoBox`、`TitleAlignment`、`InfoMessageType` |
  | `Appearance/` | 颜色 / 标签宽 / 进度条 | `GUIColor`、`LabelWidth`、`ProgressBar` |
  | `Paths/` | 路径选择 | `FilePath`、`FolderPath` |
  | `Constraints/` | 数值约束 / 校验 | `MinValue`、`MaxValue`、`Required` |
- **`Editor/`（反射式绘制引擎，解释器）**：用反射读这些特性，产出"描述"（如 `DescribeGroup`），再驱动 `Driver` 落屏。

值得注意：`PropertyGroupAttribute` 已经示范了"渲染语义当数据"的正确做法——`Kind / ShowLabel / Expanded / CenterLabel / BoldTitle / Alignment` 全是虚成员，由派生特性重写，引擎的 `DescribeGroup` 只做搬运、不按类型 `switch`。

**结论：本包的 DNA 就是"声明式数据 → 解释器"。本方案要做的不是引入一个新范式，而是把这个范式补完到最后一跳。**

而且这条链比想象中更接近目标——前半段已经做对了，只差最后一段：

```
Runtime 特性（数据）
  → GroupResolver.DescribeGroup（数据 → GroupDescriptor，不按类型 switch）        ✅ 已声明式
  → GroupNode 树 + GroupTraverse / GroupEntry（★ 已经是声明式树）               ✅ 已声明式
  → GroupRenderer.BeginGroup / EndGroup（switch(Kind) → Driver.Begin*/Label/Foldout）  ❌ 这里断了
  → 后端
```

具体证据（已核对源码）：

- `GroupResolver.DescribeGroup`（`Editor/Rules/GroupResolver.cs`）把 `PropertyGroupAttribute` 的 `Kind/ShowLabel/Expanded/Width/...` 逐项拷进 `GroupDescriptor`，**零 `switch`、零绘制**——"渲染语义当数据"已经做到。
- `GroupResolver.Get` 产出的是 `GroupNode` 树（`Kind/ShowLabel/Width/VisibleIf/Children/Members`），配合 `GroupTraverse` 混排——**这已经是一棵声明式树**。
- 断点只在 `GroupRenderer.BeginGroup/EndGroup`（`Editor/Rendering/GroupRenderer.cs`）：它对 `node.Kind` 做 `switch`，把每个分组转成 `Driver.BeginVertical/BeginHorizontal/Foldout/Label/Separator/Space` 的一串过程式调用。

于是"一个控件长什么样"从 `GroupRenderer` 这一步起就不再是数据，而是散落在后端翻译代码里——这正是 §2.2 问题的源头。本方案要做的，是把 `GroupRenderer` 里那个 `switch(Kind) → Driver 调用` 也换成数据（`Widget` 树），让整条链从头到尾都是"数据 → 解释器"。

**这也大幅降低了 Phase 3 的风险**：`GroupNode`/`GroupTraverse`/`GroupEntry` 已经是"声明式树"的现成雏形，Phase 3 的 `Widget` 树不是从零发明，而是把这个已有的树机制从"只管分组"推广到"管所有控件"，再把 `switch` 搬进共享渲染器。

### 2.1 已经正确的骨架（保留不动）

现有代码的方向是对的，这几块直接保留：

| 组件 | 作用 | 结论 |
|---|---|---|
| `Driver`（具体类，唯一后端） | 控件词汇表 + `VisualElement` 树 | 保留 |
| 元素复用 / 样式 / 布局收口 | `Tree` / `Styler` / `Layout`（`Drivers/`，原拆分契约已并入 `Driver`） | 保留，作为 `Driver` 的一部分 |
| `IControlRenderer`（原生控件边界） | `ControlRenderer` 自绘 / `DriverControlRenderer` 退回原生（原可插拔后端工厂已移除） | 保留 |
| `EditorMetricsProvider` | 运行时刻度（UIElements 默认控件的既定规格） | 保留 |
| `GroupResolver` + `GroupNode`/`GroupEntry`/`GroupTraverse`（Rules/Metadata） | 属性 → 组描述的**声明式树**：`DescribeGroup` 只搬运不 `switch`，`GroupNode` 已是树 | 保留，作为 `Widget` 树的容器子集（§2.0 / 附录 C.6） |
| `Styler` / `Layout` / `Tree`（`Drivers/`） | `Driver` 的元素复用、样式、布局收口 | 保留，作为 `Driver` 的一部分 |

### 2.2 收敛前的问题（历史根因）

下表记录的是**早期多后端并存时**的不一致根因——正是它们推动了"单源"重构。单一后端落地后，根因 4 已自然消失，根因 1~3 由 `RDTheme` / `LayoutPolicy` / `Widget` 树消除：

| # | 根因 | 表现 | 代码证据（历史） |
|---|---|---|---|
| 1 | 外观值在两处**各写一份** | 改一边忘一边 | 紧凑按钮 `padding(2,2,1,1)` 只在 `Driver` 的旧 `CompactButtonStyle`；现已上收进 `RDTheme.CompactButtonPadding` |
| 2 | 无底纹/悬停/折叠等"自绘 chrome"各写一套 | 交互反馈不同 | 无底纹按钮的悬停提亮，现已统一在 `RDTheme.FramelessHoverLerp` |
| 3 | 契约是**过程式调用**，非声明式描述 | 顺序/细节漂移 | 折叠头的三⻆/缩进/行高来源不同，现已统一由 `Widget` 树 + `RDTheme` 决定 |
| 4 | *（历史）* 原生控件委托给宿主**各自主题** | 不同 UI 系统的主题天然不同 | 单一 UIElements 后端后，原生控件只有一套主题，此问题不再存在 |

> 根因 4 是当初无法用"包装"消除的部分：不同 UI 系统的控件是两套主题渲染，不可能靠外层对齐做到像素级一致。这也是当初保留 `IControlRenderer` 接口线的原因。收敛到单一 UIElements 后端后，该接口线退化为"自绘 / 原生"的可插拔边界（见 §4.5），不再承担"跨主题对齐"。

---

## 3. 设计模式总览

| 模式 | 落点 | 解决根因 |
|---|---|---|
| **设计令牌 / 单一真值源** | `RDTheme` | #1 |
| **策略 + 模板方法**（布局策略） | `LayoutPolicy` | #1、#2 的"规则重复" |
| **声明式控件树 + 解释器**（ViewModel） | `Widget` 树 + `Renderer` | #3 |
| **底层原语**（保留） | `Driver` 控件词汇表 + `VisualElement` 树 | 底座 |
| **原生控件边界**（保留） | `IControlRenderer` | 自绘 / 原生可插拔 |
| **策略（预留接口线）** | `IControlRenderer` | #4（现为自绘 / 原生边界） |

---

## 4. 目标架构

### 4.1 分层

```
┌─────────────────────────────────────────────┐
│  Runtime 特性（纯数据，现有，不动）             │  显示名/分组/条件/按钮/下拉/表格
└──────────────────────┬──────────────────────┘
                       ▼
┌─────────────────────────────────────────────┐
│  规则层（Editor 反射引擎，现有）               │  反射、分组、取值、判定
│       产出：声明式控件树                        │
└──────────────────────┬──────────────────────┘
                       ▼
┌─────────────────────────────────────────────┐
│  声明式控件树  Widget（平台无关，单一描述）      │  ★ 新增
└──────────────────────┬──────────────────────┘
                       │ 被解释
                       ▼
┌─────────────────────────────────────────────┐
│  Renderer（具体类，单一解释器）                │  ★ 新增（翻译器）
│       只读三样东西：                           │
│   · RDTheme（外观值）                         │  ★ 新增
│   · LayoutPolicy（布局规则，纯函数）            │  ★ 新增
│   · IControlRenderer（原生控件接口线）         │  ★ 已接线
└──────────────────────┬──────────────────────┘
                       │ 落到
                       ▼
┌─────────────────────────────────────────────┐
│  Driver（唯一后端，保留模式/UIElements）        │  原语 + VisualElement 树
└─────────────────────────────────────────────┘
```

核心关系一句话：**规则层不再直接驱动 `Driver`，而是先产出一棵 `Widget` 树；`Renderer` 消费同一棵树 + 同一份 `RDTheme` + 同一份 `LayoutPolicy`，再调用 `Driver` 原语落屏。** "一致"由"同一份描述、同一份令牌、同一份规则"从结构上保证，而不是靠两端代码写得像。

这句话正好把 §2.0 里那条断裂的链补上：`Runtime 特性（数据）→ 规则层（解释）→ Widget 树（数据）→ Renderer（解释）→ Driver（原语）→ 后端`。从特性到后端的**每一跳都是"数据 → 解释器"**，没有哪一跳再把"长什么样"退化成散落的过程式代码。

### 4.2 设计令牌 `RDTheme`（根因 1）

**职责**：仓库里所有视觉值的唯一入口。渲染器只能 `theme.Xxx`，不允许写死任何像素/颜色。

分两半：

- **运行时度量**（跟随当前编辑器主题，从 UIElements 默认控件的既定规格取）——由 `IMetricsProvider`（实现 `EditorMetricsProvider`）提供。
- **静态令牌**（产品规格：颜色、圆角、图标尺寸等，不随主题变，或只随 `isProSkin` 变）。

```csharp
/// 运行时度量源：取 UIElements 默认控件的既定规格（行高/按钮高/边距/灰度等）。
/// 只有一个实现：EditorMetricsProvider（UIElements 默认皮肤下的常量，不再做皮肤采样）。
public interface IMetricsProvider
{
    float RowSpacing { get; }          // 行距
    float FieldMinHeight { get; }      // 单行字段高
    float ButtonHeight { get; }        // 按钮高
    float BoxSpacing { get; }          // box 内边距
    float BoxMargin { get; }           // box 外边距
    float ButtonMargin { get; }        // 按钮外边距
    float MiniFontSize { get; }        // mini 字号
    float ScrollbarWidth { get; }      // 滚动条宽
    float ScrollbarHeight { get; }     // 滚动条高
    float ScrollViewInsets { get; }    // 滚动区横向 inset
    float BoxInsets { get; }           // 分组盒左右 padding 之和
    float ToolbarHeight { get; }       // 工具栏高
    float MiniFontSize { get; }
    float ButtonTintGray { get; }      // 控件底纹灰度（铺纯色时乘算基准）
    float PopupTintGray { get; }
    float TextFieldTintGray { get; }
    float LabelWidth { get; }          // 标签列宽（现读不缓存）
}
```

```csharp
/// 设计令牌门面：外观值的唯一入口。
/// 静态令牌（本类）+ 运行时度量（Metrics）合并成一次绘制里的完整视觉真值。
public sealed class RDTheme
{
    public IMetricsProvider Metrics { get; }   // 运行时度量（唯一来源，EditorMetricsProvider.Instance）

    // ---- 颜色（随 isProSkin 变）----
    public Color HeaderBackground { get; }
    public Color HeaderBorder { get; }
    public Color CardBackground { get; }
    public Color CardBorder { get; }
    public Color TableLineColor { get; }
    public Color DropIndicatorColor { get; }
    public Color ToolbarToggleOnColor { get; }

    // ---- 静态几何（产品规格，不随主题变）----
    public float IndentWidth => 10f;
    public float PanelHeaderHeight => 20f;
    public float FoldoutArrowWidth => 14f;
    public float ButtonIconSize => 16f;
    public float ButtonIconLeft => 6f;
    public float GhostHeight => 24f;
    public float ColumnResizeHitWidth => 5f;

    // ---- 控件内边距（统一上收，不再分散在各处）----
    public RectOffset CompactButtonPadding { get; }   // (2,2,1,1)
    public RectOffset CompactButtonMargin { get; }    // 0

    // ---- 悬停提亮系数（无底纹按钮）----
    public float FramelessHoverLerp { get; }          // 0.5f
}
```

**迁移映射**：`DrawStyles` 的常量 → `RDTheme` 静态令牌；`DrawMetrics` 的 `Cached` 逻辑 → `EditorMetricsProvider`；后端里所有 `const float` / 写死的 `Color` / `RectOffset` → `RDTheme` 属性。

### 4.3 布局策略 `LayoutPolicy`（根因 1、2）

**职责**：把"控件该多宽、多大边距、何时撑满/固定"这些**纯计算**收成一份。它不碰 `Driver`，返回一个中立的布局描述；后端做翻译。

```csharp
/// 中立的布局描述：渲染器把它翻译成 style.*（保留模式的布局属性）。
public readonly struct LayoutHint
{
    public bool FillWidth;      // 占满可用宽
    public bool FillHeight;     // 占满可用高
    public bool Fixed;          // 不参与主轴拉伸
    public float? Width;
    public float? Height;
    public float? MinWidth;
    public float? MinHeight;
    public float MarginLeft, MarginRight, MarginTop, MarginBottom;
    public float PaddingLeft, PaddingRight, PaddingTop, PaddingBottom;
}

/// 布局策略：每个控件族的布局规则只有这一份。
/// 后端只做"LayoutHint → 本地 API"的机械翻译。
public static class LayoutPolicy
{
    public static LayoutHint ForButton(in DrawButtonSpec spec, RDTheme theme) { ... }
    public static LayoutHint ForField(float width, RDTheme theme) { ... }
    public static LayoutHint ForLabel(in DrawTextSpec spec, RDTheme theme) { ... }
    public static LayoutHint ForToolbar(RDTheme theme) { ... }
    public static LayoutHint ForBox(bool boxed, float width, RDTheme theme) { ... }
    // ...
}
```

**要搬进来的"规则"清单**（当前分散在两处、必须合成一份）：
- 字段 `width<=0` → 撑满行内剩余宽；`width>0` → 定宽（原 `FieldOptions` / `ApplyLabelAndWidth`）。
- 按钮：无底纹或定宽 → margin 0；否则 margin = `ButtonMargin`（原 `ButtonOptions` / `Button` 尾部）。
- 标签：`MaxWidth>0` 是"上限宽"（min=0, max=w），与定宽不同（原 `Label` / `ApplyTextSpec`）。
- 分组盒：`Width+MaxWidth` 双锁，否则只写 width 会被子控件最小宽撑大（原 `BeginVertical`）。
- 工具栏：平分整行、项紧贴、扁高、直角、居中（原 `ToolbarMulti` / `Toolbar`）。

### 4.4 声明式控件树 `Widget`（根因 3）

**职责**：平台无关的单一描述。规则层产树，渲染器解释树。

```csharp
public abstract class Widget
{
    public List<Widget> Children { get; } = new();
}

// ---- 容器 ----
public sealed class ColumnWidget : Widget { public bool Boxed; public float Width; public bool ExpandHeight; public bool ExpandWidth; }
public sealed class RowWidget : Widget { public bool Boxed; }
public sealed class IndentWidget : Widget { }
public sealed class ScrollWidget : Widget { public Vector2 Scroll; public float MaxHeight; }   // Scroll 由会话持久化
public sealed class SpaceWidget : Widget { public float Pixels; }
public sealed class FlexibleSpaceWidget : Widget { }
public sealed class SeparatorWidget : Widget { public float Thickness; public Color Color; }

// ---- 文本 ----
public sealed class LabelWidget : Widget { public string Text; public DrawTextSpec Spec; }
public sealed class KeyValueLabelWidget : Widget { public string Label; public string Value; }
public sealed class HelpBoxWidget : Widget { public string Message; public DrawMessageKind Kind; }
public sealed class ProgressBarWidget : Widget { public float Value; public string Title; public float Height; }

// ---- 控件（语义 + 数据，不含宿主 UI 概念）----
// 控件节点共派生自 ControlWidget（Widget 的控件分支基类），IControlRenderer 按它判别（见 §4.5）。
public abstract class ControlWidget : Widget { }
public sealed class ButtonWidget : ControlWidget { public string Text; public DrawButtonSpec Spec; public int Key; }
public sealed class FoldoutWidget : ControlWidget { public bool Expanded; public string Label; }
public sealed class TextFieldWidget : ControlWidget { public string Label; public string Value; public float Width; }
public sealed class IntegerFieldWidget : ControlWidget { public string Label; public long Value; public float Width; }
public sealed class FloatFieldWidget : ControlWidget { public string Label; public double Value; public float Width; }
public sealed class SliderWidget : ControlWidget { public string Label; public float Value, Min, Max; public float Width; }
public sealed class ToggleWidget : ControlWidget { public string Label; public bool Value; public float Width; }
public sealed class PopupWidget : ControlWidget { public string Label; public int Selected; public string[] Options; public float Width; }
public sealed class ToolbarWidget : ControlWidget { public int Selected; public string[] Options; public float Width; }
// ... EnumFlags / ObjectField / StructField 等，与现有 控件词汇表 一一对应
```

> **兼容点**：`Widget` 树的叶子字段，直接复用现有的 `DrawTextSpec` / `DrawButtonSpec`（它们已经是声明式规格）。这意味着把"过程式调用"改成"产树"时，**规格对象无需重写**。

### 4.5 渲染器 `Renderer` 与原生控件接口线 `IControlRenderer`（根因 4）

**渲染器**是"树 → 原语"的翻译器（具体类，不再有 `IRenderer` / `WidgetTreeRenderer` 抽象层）：

```csharp
/// Widget 树 → Driver 原语 的解释器（具体类）。
/// 按节点类型直接调用 Driver 上的原语, 控件节点交给注入的 IControlRenderer。
public sealed class Renderer
{
    private readonly IControlRenderer m_ControlRenderer = new ControlRenderer();

    internal void Render(Widget root, RenderContext ctx) { ... }
}
```

**原生控件接口线（已接线；默认实现把控件翻译成 `Driver` 原语，自绘实现仍属后续项）**：

```csharp
/// 原生控件的契约边界：把"某个语义控件怎么渲染"这个差异收进一个可插拔实现。
///
/// 当前后端注入的是 ControlRenderer —— 它对 Toggle / Slider / Toolbar 自绘, 其余控件
/// 退回 DriverControlRenderer（翻译成 Driver 原语）。换观感只需换一个注入实现,
/// 树遍历、RDTheme、LayoutPolicy 全部不动。
public interface IControlRenderer
{
    /// 该实现能否渲染这个控件（按控件类型 + 可选能力判别）。
    bool Supports(ControlWidget widget);
    /// 渲染一个控件并回填交互结果。
    void Render(ControlWidget widget, RenderContext ctx);
}
```

> **这条接口线已落地为"部分自绘"**：`DriverControlRenderer` 把控件翻译成 `Driver` 原语；`ControlRenderer` 对 `Toggle` / `Slider` / `Toolbar` / `ToolbarMulti` 自绘（颜色尺寸全部取 `RDTheme`，交互仍走 `NoteChanged` / `HasPendingChange` 那套"未消费改动不覆写"）。"原生控件主题差异"因此**隔离在 `IControlRenderer` 的实现边界之内**，引擎、规则层、`RDTheme`、`LayoutPolicy` 一概不动。
>
> **为什么只自绘这几类**：它们的交互完全发生在控件内部（点击、拖动），自绘不丢任何宿主能力 —— 而这恰好是观感差异最大的地方（勾选框、滑杆、页签选中态）。其余控件都依赖 `Driver` 的原生控件 —— 文本编辑与 IME（`TextField` / `IntegerField` / `FloatField`）、下拉浮层（`Popup`）、位组合菜单（`EnumFlags`）、对象选择与复合控件（`ObjectField` / `StructField`）。自绘这些等于重写输入法与浮层，属于"有界但大"的另一件工程；它们在 `Driver` 侧的外观由 `Styler` 对齐。

---

## 5. 类结构（完整清单）

新增文件（都在 `Packages/cn.rendu.draw/Editor/`）：

```
Editor/
  Theme/
    IMetricsProvider.cs        # 运行时度量接口
    EditorMetricsProvider.cs   # UIElements 默认控件规格（常量，替代原度量层）
    RDTheme.cs                 # 设计令牌门面
    LayoutPolicy.cs            # 布局策略（纯函数）
    LayoutHint.cs              # 中立布局描述
  Widgets/
    Widget.cs                  # 基类（含 RootWidget 与 Reset 契约）
    WidgetPool.cs              # 节点对象池（按类型分栈、归还前 Reset）
    ContainerWidgets.cs        # Column/Row/Indent/Scroll/Space/FlexibleSpace/Separator
    TextWidgets.cs             # Label/KeyValueLabel/HelpBox/ProgressBar
    PanelWidgets.cs            # PanelHeader/ColumnSeparator/ColumnResizer/Card/Reorder/ReorderItem/ReorderHandle/StyledScope
    ControlWidgets.cs          # ControlWidget 基类 + Button/Foldout/TextField/.../StructField
  Rendering/
    Renderer.cs                # 树解释器（具体类，单一后端）
    RenderContext.cs           # 渲染上下文
    IControlRenderer.cs        # 原生控件接口线 + 默认实现（DriverControlRenderer）
    ControlRenderer.cs         # 自绘实现：Toggle / Slider / Toolbar(Multi) 自绘, 其余退回默认实现
    ChangeApplier.cs           # 解释器回填的写回（值/折叠/按钮）
    InspectorRenderer.cs       # 从"驱动 Driver"改为"产出 Widget 树"，唯一收口
```

改造/保留：

```
  Drivers/        # 唯一后端 Driver（控件词汇表 + VisualElement 树；原多后端抽象层已移除）
                  # Styler/Layout/Tree 改读 RDTheme/LayoutPolicy
  Api/DrawStyles.cs  # 常量迁入 RDTheme，本文件删除或改薄封装
  Api/DrawMetrics.cs # 逻辑迁入 EditorMetricsProvider，本文件删除或改成薄封装
  Rendering/InspectorRenderer.cs  # 从"驱动 Driver"改为"产出 Widget 树"
```

依赖方向（必须单向，防止回环）：

```
规则层 → Widget 树 → Renderer → { RDTheme, LayoutPolicy, IControlRenderer } → Driver → 宿主 UI
```

---

## 6. 关键设计决策（评审重点）

1. **保留 `Driver` 作为底层原语（具体类），声明式层叠加其上**。理由：原多后端抽象层已移除，只剩单一 `Driver`；把一致性责任上移到"同一棵树 + 同一份令牌"，而不是推翻原语层重写。

2. **度量以 UIElements 默认控件规格为基准**（`EditorMetricsProvider`），不再做皮肤采样。理由：渲染后端本身就是 UIElements，控件尺寸直接取它的默认规格，避免"两套度量互相追"；兜底常量就是 UIElements 编辑器皮肤的默认值，与量到的真值一致。

3. **`LayoutPolicy` 只产出中立 `LayoutHint`，不做宿主 API 调用**。理由：布局决策必须可单测、可被后端复用；否则又是"两份长得像的代码"。

4. **`Widget` 树复用而非每帧新建**（✅ 已落地为对象池 `Widgets/WidgetPool.cs`）。理由：`Driver` 已是按位置复用（`Tree`），每帧重建对象会有 GC 压力。实现取舍：选"对象池"而不是"树结构复用 + 叶子字段重写" —— 后者要在建树时按位置比对上一帧的节点类型，条件结构一变就得重建，复杂度与收益不成比例。池化的正确性靠两条约定：借出的节点一定被 `Reset()` 成默认值（每个节点类型的 `Reset` 覆盖自己的全部字段，含解释器回填的输出字段），归还时先递归归还子树再清空子节点表；每类型独立栈、上限 512，长列表的 `List` 容量超过 64 就丢弃。

5. **原生控件不追求跨主题像素一致**，只留 `IControlRenderer` 接口线。理由见 §2.2 与 §4.5；收敛单一 UIElements 后端后，跨主题问题已不存在，接口线退化为"自绘 / 原生"的可插拔边界。

6. **对外 API 兼容**。`Engine.DrawObject` 的宿主签名不变（`Driver` 作为后端参数）。只有内部 `InspectorRenderer` 的产出物从"调用序列"变为"控件树"。

---

## 7. 迁移复盘（分阶段，每阶段独立可验收、可回滚）

### Phase 1 — 设计令牌（低风险，先做）
- 新增 `IMetricsProvider` / `EditorMetricsProvider` / `RDTheme` / `LayoutHint` / `LayoutPolicy`。
- 把 `DrawStyles` 常量、`DrawMetrics` 的 `Cached`、后端里写死的 `const`/`Color`/`RectOffset` 全部上收进 `RDTheme`。
- 后端改读 `theme.Xxx`。**此阶段不动调用结构，只换取值来源。**
- 验收：grep 全仓 `DrawStyles` / 写死像素常量归零；观感无回归。

### Phase 2 — 布局规则单源（中）
- 把 `FieldOptions/ButtonOptions/Label 分支/Box 双锁/工具栏几何` 等规则写成 `LayoutPolicy.ForXxx`。
- 后端改成"读 `LayoutHint` → 翻译"。**此阶段仍无 Widget 树，只是消除规则重复。**
- 验收：布局规则在后端零重复；单元测试覆盖 `LayoutPolicy`（纯函数，好测）。

### Phase 3 — 声明式控件树（大，根治）
- 新增 `Widget` 树 + `Renderer` + `RenderContext` + `IControlRenderer`。
- `InspectorRenderer` 改为产树；`Renderer` 改为解释树。
- 原多后端合并为单一 `Driver`（原语后端），由渲染器调用。
- 验收：规则层产出的是 `Widget` 树，观感完全由 `RDTheme`/`LayoutPolicy` 决定。

### Phase 4 — 原生控件接口线落地（已部分落地）
- ✅ `IControlRenderer` 边界接线 + `ControlRenderer`：`Toggle` / `Slider` 由自绘（`RDTheme` 令牌 + `PaintedSliderMath`）。
- ⬜ 其余原生控件（文本编辑 / 下拉浮层 / 位组合菜单 / 对象选择）仍委托 `Driver` 原生控件 —— 若需像素级对齐观感，可换一个自绘实现注入 `Renderer`，与树遍历 / `RDTheme` / `LayoutPolicy` 解耦。

> 评审后先落地 Phase 1（见效最快、风险最低），Phase 2 紧随；Phase 3 作为"根治项"单独排期，因为它改动 `InspectorRenderer` 与全部渲染器，需要更完整的回归验证。

---

## 8. 风险与回滚

| 风险 | 缓解 |
|---|---|
| 多一层抽象引入性能/GC | `Widget` 树复用（见决策 4）；`LayoutHint` 用 `struct`；令牌/度量用缓存，不做每帧反射 |
| `InspectorRenderer` 产树改造引入回归 | 分阶段；Phase 1/2 不碰调用结构，可独立回滚 |
| 原生控件观感未对齐 | 已在 §1.2/§4.5 明确"自绘控件走 `IControlRenderer` 边界"，其余委托 `Driver` 原生控件 |
| 主题切换后令牌失效 | 令牌/度量沿用现有"皮肤实例变了就重建"的失效约定 |

回滚方式：每阶段以独立 commit 落地（遵循仓库 gitmoji 规范），任一阶段验收不过，仅 revert 该阶段。

---

## 9. 验收清单（评审通过后逐项勾）

- [x] 视觉值（颜色/尺寸/间距/圆角/悬停色）仓库内仅 `RDTheme` 一份（含拖拽浮层的尺寸 / 压暗色 / 圆角）。
- [x] 布局规则（宽度/边距/撑满/固定）仓库内仅 `LayoutPolicy` 一份。
- [x] 规则层产出的是 `Widget` 树，不再直接调 `Driver` 绘制原语（`RequestRepaint` 除外）。
- [x] `Renderer` 是单一解释器，不写死任何像素/颜色。
- [x] 原生控件差异被 `IControlRenderer` 边界隔离，引擎与规则层零感知（`Toggle` / `Slider` 已自绘，其余仍走 `Driver` 原生控件 —— 见 §4.5 的范围说明）。
- [x] 对外 API（`Engine.DrawObject`）兼容，宿主无需改动。
- [x] 单一 UIElements 后端，不再有其它后端的分支。

---

## 附录 A：令牌映射清单（现状散落值 → RDTheme）

> 用途：让 Phase 1"把散落值上收进 `RDTheme`"这件事从口号变成可逐项勾的清单。评审时据此核对"搬干净没有"。

### A.1 运行时度量（迁入 `EditorMetricsProvider`）

| 令牌成员 | 现状来源 | 备注 |
|---|---|---|
| `RowSpacing` | 行距（UIElements 默认） | |
| `FieldMinHeight` | 单行控件高（UIElements 默认） | |
| `ButtonHeight` | 按钮高（UIElements 默认） | |
| `BoxSpacing` | box 内边距（UIElements 默认） | |
| `BoxMargin` | box 外边距（UIElements 默认） | |
| `ButtonMargin` | 按钮外边距（UIElements 默认） | |
| `MiniFontSize` | mini 字号（UIElements 默认） | |
| `ScrollbarWidth` | 滚动条宽（UIElements 默认） | |
| `ScrollbarHeight` | 滚动条高（UIElements 默认） | |
| `ScrollViewInsets` | 滚动区横向 inset | |
| `BoxInsets` | 分组盒左右 padding 之和 | |
| `ToolbarHeight` | 工具栏高（UIElements 默认） | |
| `ButtonTintGray` / `PopupTintGray` / `TextFieldTintGray` | 控件底纹灰度（UIElements 默认皮肤下的既定值） | |
| `LabelWidth` | `EditorGUIUtility.labelWidth`（不缓存） | 保留"现读不缓存"语义 |

### A.2 静态令牌（迁入 `RDTheme` 本体）

| 令牌成员 | 现状位置 | 当前值 |
|---|---|---|
| `IndentWidth` | `DrawMetrics.IndentWidth` | 10f |
| `LabelWidthDefault` / `LabelWidthMax` | `DrawMetrics` | 150f / 300f |
| `InputPaddingLeft` | `DrawMetrics` | 0f |
| `PanelHeaderHeight` | `DrawStyles` | 20f |
| `PanelHeaderBackground` | `DrawStyles` | isProSkin ? 0.26 : 0.76 |
| `PanelHeaderBorder` | `DrawStyles` | isProSkin ? 0.32 : 0.60 |
| `CardBackground` | `DrawStyles` | isProSkin ? 0.20 : 0.82 |
| `CardBorder` | `DrawStyles` | isProSkin ? 0.24 : 0.66 |
| `CardPadding` | `DrawStyles` | 3f |
| `TableHeaderLineColor` | `DrawStyles` | (0.32, 0.32, 0.32, 0.5) |
| `TableHeaderLineThickness` | `DrawStyles` | 1f |
| `TitleGroupLineThickness` / `TitleGroupLineColor` | `DrawStyles` | 2f / (0.4, 0.4, 0.4, 0.3) |
| `DropIndicatorColor` | `DrawStyles` | (0.24, 0.49, 0.90, 1) |
| `ToolbarToggleOnColor` | `Driver`（写死） | (0.32, 0.55, 0.85, 1) |
| `AddButton` / `PagerButton` / `ExpandButton` / `ItemRemoveButton` | `DrawStyles`（`DrawButtonSpec`） | 24 / 14 / 18 / 16×18，均无底纹 |
| `CountedHeader` / `TableHeaderText` / `CountLabel` | `DrawStyles`（`DrawTextSpec`） | bold / center×20 / mini |

### A.3 控件 chrome 单边重复（根因 1 的直接证据，现已消除）

下面这些值是 Phase 1 消灭的对象——每一项都应只剩 `RDTheme` 一处定义：

| 值 | 现状（统一后） |
|---|---|
| `FoldoutArrowWidth = 14f` | `RDTheme.FoldoutArrowWidth` |
| `GhostHeight = 24f` | `RDTheme.GhostHeight` |
| 列边界抓取区 | `RDTheme.ColumnResizeHitWidth = 5f` |
| `CompactButtonStyle` `padding(2,2,1,1)` + `margin 0` | `RDTheme.CompactButtonPadding` / `CompactButtonMargin` |
| 无底纹按钮悬停提亮 `Lerp(text, white, 0.5)` / active `white` | `RDTheme.FramelessHoverLerp` |
| `ButtonIconSize=16` / `ButtonIconLeft=6` / `ButtonIconPadLeft=26` | `RDTheme.ButtonIconSize` / `ButtonIconLeft` |
| 表头行 `border/padding (1,1,1,1)` | `RDTheme` 统一 |

> A.3 这张表就是 §2.2 根因 1 的逐条实物。Phase 1 完成后，A.3 里每一项都只剩 `RDTheme` 一处定义。

---

## 附录 B：Phase 2 布局规则清单（散落分支 → LayoutPolicy）

> Phase 2 把"控件多宽 / 多大边距 / 何时撑满"这些**决策**收成一份纯函数。下表逐条列出现状里"同一决策在两处各写一份"的位置，以及它该落进哪个 `LayoutPolicy` 方法。

| 决策（规则） | 原实现 A | 原实现 B | → `LayoutPolicy` |
|---|---|---|---|
| 字段 `width<=0` 撑满行内剩余、`>0` 定宽 | `Driver` 旧 `FieldOptions` | `Styler.ApplyLabelAndWidth` | `ForField(width, theme)` |
| 字段 `min-width=0` 允许收缩（防长值顶宽列） | `s_ExpandSlot` | `minWidth=0, flexShrink=1` | `ForField` 内 |
| 按钮 margin：无底纹/定宽 → 0，否则 `ButtonMargin` | 旧 `ButtonOptions` | `Button` 尾部 margin 三行 | `ForButton(spec, theme)` |
| 按钮 padding：紧凑键 `(2,2,1,1)` | 旧 `CompactButtonStyle` | （缺，走 USS 默认） | `ForButton` 内 |
| 按钮高度：缺省用按钮高 | 旧 `ButtonOptions` 不锁高 | `EditorMetricsProvider.ButtonHeight` | `ForButton` 内 |
| 标签 `MaxWidth>0` 是"上限宽"（min=0, max=w），非定宽 | 旧 `Label` 分支 | `ApplyTextSpec` | `ForLabel(spec, theme)` |
| 标签占满行（stretchWidth / 非定宽时） | 旧 `GUILayout.Label` 默认 | `Layout.FillWidth` | `ForLabel` 内 |
| 分组盒 `Width + MaxWidth` 双锁（防子控件最小宽撑大） | 旧 `BeginVertical` | `BeginVertical` | `ForBox(boxed, width, theme)` |
| 工具栏：平分整行、项紧贴、扁高、直角、居中 | 旧 `ToolbarMulti` | `Toolbar` / `ToolbarMulti` | `ForToolbar(theme)` |
| "占满宽"在竖排里是 cross-axis（alignSelf），横排里是主轴（flexGrow） | （无对应） | `Layout.FillWidth` | `ForFillWidth(inRow)` |

> 关键点：`LayoutPolicy` 的每个方法都**返回中立 `LayoutHint`、不碰任何宿主 API**。`Driver` 只做 `LayoutHint → style.*` 的机械翻译。Phase 2 之后，上表"原实现 A / 原实现 B"两列应合并为同一份 `LayoutPolicy`，两处不再各写一遍。

---

## 附录 C：`控件词汇表` → `Widget` 树对应表（Phase 3 产树打底）

> Phase 3 把 `InspectorRenderer` 的产出从"调用序列"改为"`Widget` 树"。下表把现有 `控件词汇表` 的每个方法一对一映射到 `Widget` 节点，确认"控件词汇表"在树形描述下是完整的、没有遗漏。

### C.1 容器

| `控件词汇表` | `Widget` 节点 | 节点字段（来自方法参数） |
|---|---|---|
| `BeginVertical(boxed, width, expandHeight, expandWidth)` / `EndVertical` | `ColumnWidget` | `Boxed`, `Width`, `ExpandHeight`, `ExpandWidth` |
| `BeginHorizontal(boxed)` / `EndHorizontal` | `RowWidget` | `Boxed` |
| `BeginIndent` / `EndIndent` | `IndentWidget` | — |
| `BeginScroll(ref scroll, maxHeight)` / `EndScroll` | `ScrollWidget` | `Scroll`（会话持久化）, `MaxHeight` |
| `Space(pixels)` | `SpaceWidget` | `Pixels` |
| `FlexibleSpace()` | `FlexibleSpaceWidget` | — |
| `Separator(thickness, color)` | `SeparatorWidget` | `Thickness`, `Color` |

### C.2 文本

| `控件词汇表` | `Widget` 节点 | 节点字段 |
|---|---|---|
| `Label(text, spec)` | `LabelWidget` | `Text`, `DrawTextSpec` |
| `KeyValueLabel(label, value)` | `KeyValueLabelWidget` | `Label`, `Value` |
| `HelpBox(message, kind)` | `HelpBoxWidget` | `Message`, `DrawMessageKind` |
| `ProgressBar(value, title, height)` | `ProgressBarWidget` | `Value`, `Title`, `Height` |

### C.3 控件

| `控件词汇表` | `Widget` 节点 | 节点字段 |
|---|---|---|
| `Button(text, spec)` | `ButtonWidget` | `Text`, `DrawButtonSpec`, `Key` |
| `Foldout(expanded, label)` | `FoldoutWidget` | `Expanded`, `Label` |
| `TextField` / `IntegerField` / `FloatField` | `TextFieldWidget` / `IntegerFieldWidget` / `FloatFieldWidget` | `Label`, `Value`, `Width` |
| `Slider` | `SliderWidget` | `Label`, `Value`, `Min`, `Max`, `Width` |
| `ToggleField` | `ToggleWidget` | `Label`, `Value`, `Width` |
| `Popup` | `PopupWidget` | `Label`, `Selected`, `Options`, `Width` |
| `EnumFlagsField` | `EnumFlagsWidget` | `Label`, `Value`(Enum), `Width` |
| `Toolbar` / `ToolbarMulti` | `ToolbarWidget` / `ToolbarMultiWidget` | `Selected`/`States`, `Options`, `Width` |
| `ObjectField` | `ObjectFieldWidget` | `Label`, `Value`, `Type`, `Width` |
| `StructField` | `StructFieldWidget` | `Label`, `Value`, `Type`, `Width` |

### C.4 可选能力（`面板能力` / `拖拽排序`）

| 能力方法 | `Widget` 节点 | 说明 |
|---|---|---|
| `PanelHeader(text, align, bold)` | `PanelHeaderWidget` | 表头横条 |
| `ColumnSeparator(height)` | `ColumnSeparatorWidget` | 列竖线 |
| `BeginCard` / `EndCard` | `CardWidget` | 列表条目卡片 |
| `ColumnResizer(height)` | `ColumnResizerWidget` | 可拖拽列边界 |
| `BeginReorder / ReorderHandle / EndReorderItem / PollReorder` | `ReorderWidget`（包裹列表子树的特殊节点） | 拖拽排序登记 |

### C.5 状态栈（`状态栈`）在树里的表达

`PushColor/PopColor/PushEnabled/PopEnabled/PushLabelWidth/PopLabelWidth` 不是"控件"，而是**作用于一段子树的外观开关**。在 `Widget` 树里它们不占节点，而是一个**包裹节点** `StyledScopeWidget`（含 `Color?`、`Enabled?`、`LabelWidth?` 三个字段），子树整体继承其外观。渲染器在进入该节点时压状态栈、离开时弹栈——与现在"渲染器按调用序列压栈/弹栈"同义，只是从"过程式压弹"变成"树节点携带状态"。

> C.1～C.5 合起来说明：现有控件词汇表是**完备的**，能无损转成 `Widget` 树，Phase 3 不是"新造一套控件"，而是"把已有词汇表的载体从调用序列换成数据"。这也是 §4.4 说"`Widget` 树叶子直接复用 `DrawTextSpec`/`DrawButtonSpec`"的依据。

### C.6 推广映射：`GroupNode` → `Widget`（避免两棵树并存）

**关键决策：不要并排再造一棵 `Widget` 树，让 `Widget` 树吸收 `GroupNode`。**

§2.0 已核实：`GroupNode` / `GroupEntry` / `GroupTraverse` 本身已经是一棵声明式树。Phase 3 若在它旁边另起一棵 `Widget` 树，就会出现"结构树 + 渲染树"两棵树要同步——那正是我们要消灭的同类重复。因此：

- `Widget` 是 `GroupNode` 的**泛化**：`GroupNode` 是"只描述分组"的容器节点，`Widget` 把"容器节点"推广到"任意控件节点"（成员、值控件、按钮、表格……）。
- `CachedMember` 现在不是树节点，渲染链从"调 Driver"改为"产 `Widget` 子树"，成员于是也变成树的一部分。

**结构侧对应关系：**

| 现状（结构侧） | 目标（`Widget` 树） | 说明 |
|---|---|---|
| `GroupNode` | `Widget` 基类（容器分支） | 一个节点，含 `Children` |
| `GroupNode.Kind` | 建树时按 `Kind` 选 `Widget` 子类型：Box→`BoxWidget`、Vertical→`ColumnWidget`、Horizontal/ButtonGroup→`RowWidget`、Foldout→`FoldoutWidget`、Title→`TitleWidget` | `switch(Kind)` 从"每次绘制"前移到"建树一次" |
| `GroupNode.Children` | `Widget.Children` | 直接对应 |
| `GroupNode.Members` | 成员渲染链产出的 `Widget` 子树 | `CachedMember` 不再是叶子，而是能展开成子树的节点 |
| `GroupNode.Entries` + `GroupEntry` | 合并进 `Widget.Children` | 排序逻辑（先 Order 再 Seq）保留，但"Group XOR Member"双态不再需要 |
| `GroupTraverse.Entries` | 删除（`Widget.Children` 即有序子节点） | |

**成员/渲染侧对应关系：**

| 现状（渲染侧） | 目标 | 说明 |
|---|---|---|
| `MemberRenderChain`（`CanDraw` 链） | 保留链，每档命中后**产出 `Widget` 子树**而非调 Driver | 优先级链语义不变，载体从"调用"换成"子树" |
| `ValueControlChain` | 保留，产出叶子 `Widget`（`TextFieldWidget` 等） | 对应附录 C.3 |
| `CollectionRenderer` / `TableRenderer` / `ButtonPainter` / `AnnotationPainter` | 各自从"调 Driver"改为"产 `Widget` 子树" | |
| `GroupRenderer.BeginGroup/EndGroup` 的 `switch(Kind)` | 消失：`switch` 移到建树阶段，渲染由 `Renderer` 统一解释 | §2.0 定位的断点 |

一句话：**树只有一棵（`Widget`）**——`GroupNode` 是它的容器子集，`CachedMember` 的渲染链是它的"子树构造器"，`Renderer` 是它唯一的解释器。`GroupRenderer` 里那个 `switch(Kind)` 被前移到建树阶段、且只执行一次，不再在每次绘制里跑。

---

## 附录 D：文件 / 类 清单核对（改动文件 / 回滚点）

> 评审落地时逐行核对用。每个被改动的文件/类标注：所属阶段、改动性质、回滚方式。原则：**每个阶段一个独立 commit**（遵循仓库 gitmoji 规范），阶段内任何验收不过，只 revert 该阶段。

| 文件 / 类 | 阶段 | 改动性质 | 回滚方式 |
|---|---|---|---|
| `Api/DrawMetrics.cs` | Phase 1 | 逻辑迁入 `EditorMetricsProvider`，删除或改薄封装 | revert Phase 1 |
| `Rendering/DrawStyles.cs` | Phase 1 | 常量迁入 `RDTheme`，删除或改薄封装 | revert Phase 1 |
| `Drivers/Driver.cs` | Phase 1→3 | 删硬编码 → 读 `theme.Xxx`/`LayoutPolicy` → 合并为唯一后端 | 分阶段 revert |
| `Drivers/Styler.cs` / `Layout.cs` / `Tree.cs` | Phase 1→2 | 改读 `RDTheme`（替代直接读 `DrawMetrics`） | revert 对应阶段 |
| `Theme/IMetricsProvider.cs` | Phase 1 | 新增接口 | 删除 |
| `Theme/EditorMetricsProvider.cs` | Phase 1 | 新增（原 `DrawMetrics` 逻辑，现为 UIElements 默认规格） | 删除 |
| `Theme/RDTheme.cs` | Phase 1 | 新增 | 删除 |
| `Theme/LayoutHint.cs` | Phase 2 | 新增 | 删除 |
| `Theme/LayoutPolicy.cs` | Phase 2 | 新增 | 删除 |
| `Widgets/*.cs` | Phase 3 | 新增（`Widget` 树） | 删除 |
| `Rendering/Renderer.cs` / `RenderContext.cs` | Phase 3 | 新增（单一解释器） | 删除 |
| `Rendering/IControlRenderer.cs` | Phase 4 | 新增（接口线预留） | 删除 |
| `Rendering/ControlRenderer.cs` | Phase 4 | 新增（自绘实现） | 删除 |
| `Rendering/InspectorRenderer.cs` | Phase 3 | 从"驱动 `Driver`"改为"产出 `Widget` 树" | revert Phase 3 |
| `Api/Engine.cs` / `Rendering/DrawContext.cs` | Phase 3 | 内部装配改动（对外签名不变） | revert Phase 3 |
| `Runtime/Attributes/**` | — | **不动**（纯数据层零改动） | — |
| `Drivers/` 旧契约族 | 已删除 | 多后端抽象层一起移除，`Driver` 现为具体类 | 删除 |

> 本方案的一条硬约束：**纯数据层（Runtime 特性）不改**；原多后端抽象层已随收敛移除，`Driver` 现为单一具体类，改动只发生在 `Editor/` 的"解释 → 翻译"这一段。
