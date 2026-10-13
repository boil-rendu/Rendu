# RD.UI

UI 框架（Panel / Bind 源生成 / 事件，详见飞书 wiki）与 **UI 工具窗口**（`Rendu/UI Tool` —— `UITool` —— 头部"发布" + 中间反射模块 + 尾部"全局设置"）。本 README 不重复 UI 框架本身（飞书 wiki 已有），只讲**本包当前在仓库里承担的两件事**：

1. **Runtime** —— UI 业务代码（`RD.UI` 程序集，运行时）
2. **Editor / UITool** —— 工具窗口与 13 个工具模块（`RD.UI.Editor` 程序集，只在 Editor 平台）
3. **DotNet~/RD.UI.BindSourceGenerator** —— 数据绑定字段/事件表的 Roslyn 源生成器（`netstandard2.0` 硬约束，详见根 [README §重要约束](../../README.md#重要约束)）

工具窗口与模块机制本身在 [cn.rendu.edittool](../cn.rendu.edittool/README.md)；特性与绘制引擎在 [cn.rendu.panel](../cn.rendu.panel/README.md)；检视器宿主在 [cn.rendu.inspector](../cn.rendu.inspector/README.md)。

> 重要：**Phase 1-2 后，本包不再承载"工具窗口基建"**。`BaseToolModule` / `BaseTreeMenuItem` / `BaseCreateModule` / `UIAutoMenuAttribute` 等都已迁到 [cn.rendu.edittool](../cn.rendu.edittool/README.md) 命名空间 `RD.EditTool.Editor`。本包只剩"UI 域"的具体模块与 UITool 容器。

## 包结构

```
cn.rendu.ui/
├── Runtime/                              RD.UI（运行时程序集）
│   └── Core/
│       ├── UISingleton/                  UI 单例 (Manager / Mono Singleton)
│       ├── UIMono/                       UI 控件 (Widget / Material / Shader)
│       └── UIBind/                       数据绑定 (Source Generators 编到这里)
│
├── Editor/                               RD.UI.Editor（Editor 程序集）
│   ├── UITool/                       UITool 容器 + 13 个工具模块
│   │   ├── Window/UITool.cs            工具窗口：头部"发布" + 中间反射 [UIAutoMenu] + 尾部"全局设置"
│   │   │
│   │   ├── Window/Base/                  Create 模块的 base（2 文件）
│   │   │   ├── CreateBaseModule.cs       继承 BaseCreateModule, 写"创建 UI 模块"通用配置
│   │   │   └── CreateBaseEditorData.cs   关联的 ScriptableObject 数据
│   │   │
│   │   ├── Window/Check/                 "检查"模块（Prefabs / Script 预制体/脚本批量检查）
│   │   ├── Window/Const/                 "常量"模块
│   │   ├── Window/Create/                "创建"模块（7 个 CreateXxxCode + CreateModule 协调）
│   │   ├── Window/Publish/               "发布"模块（UITool 头部根；不是 [UIAutoMenu]，手动挂）
│   │   ├── Window/Script/                "代码生成"模块
│   │   ├── Window/Setting/               "全局设置"（UITool 尾部根的子模块：Atlas / Other）
│   │   ├── Window/UnityIcons/            "系统图标"模块（打开 UnityIconsWindow 子窗）
│   │   │
│   │   ├── EditorBase/                   3 个 Create 模块用到的辅助工具（命名误导，不是基类）
│   │   │   ├── BaseTemplate.cs           CreateVo 数据结构
│   │   │   ├── EditorHelper.cs           编辑器 IO / 字符串 / 菜单调用
│   │   │   └── TemplateEngine.cs         简单模板引擎（用于代码生成）
│   │   │
│   │   └── Template/                     13 个 .txt 模板（Create 模块用）
│   │       ├── CreateBaseTemplate.txt
│   │       ├── CreateCommonTemplate.txt
│   │       ├── CreateComponentTemplate.txt
│   │       ├── CreatePanelTemplate.txt
│   │       ├── CreateSystemTemplate.txt ...（共 13 个）
│   │       └── Script/                   子目录
│   │
│   ├── UIRootPrefab/                     资源
│   ├── Resources/USS/UnityIconsWindow.uss   UnityIconsWindow 子窗的样式（被引用）
│   ├── Core/                             其他编辑器扩展
│   │   ├── ContextMenu/                  Inspector 上下文菜单扩展
│   │   ├── Hierarchy/                    Hierarchy 窗口图标
│   │   ├── Inspector/                    Inspector 扩展
│   │   ├── Scene/                        Scene 视图辅助（GraphicSelector / QuickCameraFocus）
│   │   ├── Toolbar/                      顶部工具栏扩展（AutoToolBar / CLI / ToolbarExtender）
│   │   └── Utils/                        杂项
│   └── .Template/cn.rendu.ui.sample/     项目模板样本
│
├── DotNet~/RD.UI.BindSourceGenerator/    数据绑定 Roslyn 源生成器 (netstandard2.0 硬约束)
├── package.json
└── .Template/
```

## 工具窗口：UI Tool

菜单 **Rendu / UI Tool**（定义在 `Editor/UITool/Window/UITool.cs:26`）。

打开后左侧菜单树（由 `BuildToolMenuTree` 拼装）：

```
┌─ UITool ─────────────────────────────────────────────┐
│  ▼ 发布         ← 头部根: PublishModule (手动挂)        │
│      UI 文档                                                │
│      整体发布 / 整体发布图集 / 创建模块(子面板)            │
│                                                            │
│  ▼ 代码生成     ← Order=200000 (最高, 反射)              │
│      ... 子面板                                             │
│                                                            │
│  ▼ 检查         ← Order=Int.MaxValue (反射)                │
│      ... 子面板                                             │
│                                                            │
│  ▼ 常量         ← Order=Int.MaxValue (反射)                │
│      ... 子面板                                             │
│                                                            │
│  ▼ 系统图标     ← Order=Int.MaxValue (反射)                │
│      ... 子面板                                             │
│                                                            │
│  ▼ 全局设置     ← 尾部根: 值为 this, 绘制自身字段         │
│      用户名 / 模板基础模块 / 全局图集设置 / 其他设置        │
└────────────────────────────────────────────────────────────┘
```

| 位置 | 模块 | Order | 来源 |
|---|---|---|---|
| 头部根 | 发布 (PublishModule) | — | UITool.cs:57 手动挂，**不**走 [UIAutoMenu] |
| 中间 | 代码生成 (ScriptModule) | 200000 | [UIAutoMenu] 反射 |
| 中间 | 检查 (CheckModule) | int.MaxValue | [UIAutoMenu] 反射 |
| 中间 | 常量 (ConstModule) | int.MaxValue | [UIAutoMenu] 反射 |
| 中间 | 系统图标 (UnityIconsModule) | int.MaxValue | [UIAutoMenu] 反射 |
| 尾部根 | 全局设置 | — | UITool.cs:63 手动挂，值为 UITool 自身 |
| 尾部 | 模板基础模块 (CreateBaseModule) | — | UITool.m_UIBaseModule（内嵌在尾部） |
| 尾部 | 全局图集设置 (AtlasModule) | — | UITool.AtlasModule（内嵌） |
| 尾部 | 其他设置 (OtherModule) | — | UITool.OtherModule（内嵌） |

> 尾部 3 个内嵌的 `BaseCreateModule` 不走菜单树选中 —— 它们的字段直接画在 UITool 自身尾部"全局设置"页面里。

### UITool 的两个 [MenuItem]

| 项 | 路径 | 用途 |
|---|---|---|
| 打开 | `Rendu/UI Tool` | 正常打开工具窗口 |
| 关闭 | `Rendu/关闭 UI 工具`（注释中，正常菜单不显示） | 工具窗口坏掉时手动用脚本/类名关闭 |

## 依赖

| 程序集 | 引用 | 说明 |
|---|---|---|
| `RD.UI` (Runtime) | `RD.Core` | UI 框架运行时 |
| `RD.UI.Editor` | `RD.Core`、`RD.Panel`、`RD.Panel.Editor`、`RD.Inspector.Editor`、`RD.EditTool.Editor` | 编辑器扩展：工具窗口 + 上下文菜单 + Hierarchy/Scene/Toolbar 扩展 |
| `RD.UI.BindSourceGenerator` (DotNet~) | `Microsoft.CodeAnalysis.CSharp` 4.3.1 | 数据绑定字段/事件表源生成器；**netstandard2.0 硬约束**（见根 README） |

## 与其他包的关系

| 包 | 关系 |
|---|---|
| `cn.rendu.core` | 提供 UI 框架的运行时支撑（Entity / 网络 / Fiber） |
| `cn.rendu.invoke` | Invoke 源生成器（UI Bind 走它的事件回调） |
| `cn.rendu.panel` | 特性 + 绘制引擎（`[Button] / [ShowInInspector] / [GUIColor]` 等） |
| `cn.rendu.inspector` | InspectorEditor，UI 资产检视 |
| `cn.rendu.edittool` | GenericToolWindow + BaseToolModule（**Phase 1-2 后由本包迁出**） |
| `cn.rendu.loader` | **不依赖**。`UITool` 与 `LoaderTool` 是两个独立的工具窗口实例，凭 `AcceptScope=null` / `="loader"` 互不串台 |
