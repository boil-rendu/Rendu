# UI 自动化工具 ·「常量」面板 — 重复项整理与优化清单

> 状态：**§2.4 + §4.5 已实施**（commit `341c94e`） · 41 项 → **38 项**
> 范围：截图中的「常量」面板（`RD.UI.Editor.UIConstModule`）
> 分析时间：2026-09-24 · 最近更新：2026-09-24（实施 `UIUI*` 字段删除）
>
> | 节 | 状态 | 提交 |
> | --- | :--: | --- |
> | §2.1 死字段 `UIUIViewName` | ✅ 已删 | `341c94e`（同批） |
> | §2.4.1 `UIUIPanelSourceName` 撞车 | ✅ 已删 | `341c94e` |
> | §2.4.2 `UIUIViewName` / `UIUIViewParentName` 撞车 | ✅ 已删 | `341c94e` |
> | §4.5 双 `UIUI*` 前缀统一 | ✅ 已完成 | `341c94e` |
> | 其余项 | ⏸ 待拍板 | — |

---

## 0. 面板定位（实测）

| 项 | 结论 |
| --- | --- |
| 所属窗口 | `Rendu / UI 自动化工具` → `UIAutoTool`（`Packages/cn.rendu.ui/Editor/UIAutoTool/Window/UIAutoTool.cs`） |
| 左侧导航来源 | 固定项「发布 / 全局设置」+ 反射收集所有 `[UIAutoMenu]` 类（`ET生成 / 宏设置 / 检查 / 常量 / 系统图标`） |
| 面板逻辑 | `Packages/cn.rendu.ui/Editor/UIAutoTool/Window/UIConst/UIConstModule.cs` |
| 数据类 | `Packages/cn.rendu.ui/Runtime/Core/UIBase/Asset/UIConstAsset.cs` — `RD.Client.UIConstAsset`（`partial`，**全仓只有这一个定义，无扩展文件**） |
| 存档位置 | `Packages/cn.rendu.gameres/UI/UISettings/UIConstAsset.txt` |
| 存档现状 | **内容为 `{}`** — 即面板上显示的值**全部来自代码里的字段初始化器**，没有一条是真实配置数据 |
| 项数 | `#region 项目配置` 共 **38** 项（原 41 项，删 `UIUIPanelSourceName` / `UIUIViewName` / `UIUIViewParentName` 三处冗余字段）：**27 项 `[ReadOnly]`** + **11 项可编辑** |
| 标签风格 | 前 15 项有中文 `[LabelText]`；后 23 项没有 → 由 RD.Inspector 按字段名自动分词显示（`UIRootName` → `UI Root Name`），**同一面板中文/英文标签混排** |

> 截图首行「UI根目录名称」= `UIProjectName`（值 `UI`，`[ReadOnly]`）。

---

## 1. 全量清单（38 项 · 带引用统计）

「引用」= 全仓 `Packages/**` + `Assets/**` 的 `*.cs` 中该标识符出现次数（**含其自身声明**，故 1 表示**只有声明、无任何调用点**；排除 `obj/`）。

| # | 面板显示 | 字段 | 当前值 | 只读 | 引用 | 判定 |
| --: | --- | --- | --- | :--: | --: | --- |
| 1 | UI根目录名称 | `UIProjectName` | `UI` | ✅ | 26 | 保留（生成物命名前缀；现在还作为 `GetDefaultName` 的基） |
| 2 | UI项目命名空间 | `UINamespace` | `RD.Client` | ❌ | 7 | 保留 |
| 3 | UI项目编辑器资源 | `UIProjectEditorPath` | `Assets/Editor/UI` | ❌ | 2 | 保留（仅 1 处使用） |
| 4 | UI项目指定包编辑器资源 | `UIProjectPackageEditorPath` | `Assets/../Packages/cn.rendu.{0}/Assets/Editor/UI` | ❌ | **1** | **保留** |
| 5 | UI项目资源 | `UIProjectResPath` | `Assets/GameRes/UI` | ❌ | 28 | 保留 |
| 6 | UI项目指定包资源 | `UIProjectPackageResPath` | `Assets/../Packages/cn.rendu.{0}/Assets/GameRes/UI` | ❌ | 3 | 保留 |
| 7 | UI项目生成包名 | `UIETCreatePackageName` | `uiproject` | ❌ | 2 | 与 #8 合并 |
| 8 | UI项目生成默认包 | `UIETCreatePackagePath` | `Assets/../Packages/cn.rendu.uiproject` | ❌ | 2 | 与 #7 合并（可由 #7 推导） |
| 9 | UI项目脚本 | `UIETComponentGenPath` | `.../Scripts/ModelView/Client/UIGen` | ❌ | 5 | 保留 |
| 10 | UI项目ET组件 | `UIETComponentPath` | `.../Scripts/ModelView/Client/UIComponent` | ❌ | 2 | 保留 |
| 11 | UI项目自定义脚本 | `UIETSystemGenPath` | `.../Scripts/HotfixView/Client/UIGen` | ❌ | 3 | 保留 |
| 12 | UI项目ET系统 | `UIETSystemPath` | `.../Scripts/HotfixView/Client/UISystem` | ❌ | 4 | 保留 |
| 13 | UI框架所处位置 | `UIFrameworkPath` | `Assets/../Packages/cn.rendu.ui` | ✅ | **1** | **保留** |
| 14 | UI项目代码模板 | `UITemplatePath` | `.../cn.rendu.ui/Editor/UIAutoTool/Template` | ✅ | 12 | 保留（可派生） |
| 15 | UI项目Root预制 | `UIRootPrefabPath` | `Packages/cn.rendu.ui/Editor/UIRootPrefab/UIRoot.prefab` | ✅ | 3 | 保留（可派生，路径格式与其它项不一致） |
| 16 | UI Packages | `UIPackages` | `Packages` | ✅ | 2 | 与 #17 合并 |
| 17 | UIET Packages Format | `UIETPackagesFormat` | `cn.rendu.` | ✅ | 4 | 与 #16 合并 / 下沉为常量 |
| 18 | UI Root Name | `UIRootName` | `UIRoot` | ✅ | 4 | 保留 |
| 19 | UI Layer Root Name | `UILayerRootName` | `UILayerRoot` | ✅ | 2 | 与 #18 同组 |
| 20 | UI Root Pkg Name | `UIRootPkgName` | `Common` | ✅ | 2 | **与 #22 值相同** → 合并 |
| 21 | UI Panel Name | `UIPanelName` | `Panel` | ✅ | 10 | 保留 |
| 22 | UI Common Name | `UICommonName` | `Common` | ✅ | 3 | **与 #20 值相同** → 合并 |
| 23 | UI View Name | `UIViewName` | `View` | ✅ | 6 | 保留 |
| 24 | UI Parent Name | `UIParentName` | `Parent` | ✅ | 11 | 保留 |
| 25 | UI Prefabs | `UIPrefabs` | `Prefabs` | ✅ | 15 | 保留（见 2.4） |
| 26 | UI PrefabsCN | `UIPrefabsCN` | `预制` | ✅ | 2 | 中英别名对 |
| 27 | UI Sprites | `UISprites` | `Sprites` | ✅ | 10 | 保留 |
| 28 | UI SpritesCN | `UISpritesCN` | `精灵` | ✅ | 2 | 中英别名对 |
| 29 | UI Atlas | `UIAtlas` | `Atlas` | ✅ | 6 | 保留 |
| 30 | UI AtlasCN | `UIAtlasCN` | `图集` | ✅ | 2 | 中英别名对 |
| 31 | UI Source | `UISource` | `Source` | ✅ | 12 | 保留 |
| 32 | UI SourceCN | `UISourceCN` | `源文件` | ✅ | 2 | 中英别名对 |
| 33 | UI Atlas Ignore | `UIAtlasIgnore` | `AtlasIgnore` | ✅ | 4 | 保留 |
| 34 | UI Sprites Atlas1 | `UISpritesAtlas1` | `Atlas1` | ✅ | 2 | 保留（命名带序号，扩展性差） |
| 35 | UI All View Parent Name | `UIAllViewParentName` | `AllViewParent` | ✅ | 6 | 保留 |
| 36 | UI All Popup View Parent Name | `UIAllPopupViewParentName` | `AllPopupViewParent` | ✅ | 6 | 保留 |
| 37 | ~~UIUI Panel Source Name~~ | ~~`UIUIPanelSourceName`~~ | ~~`UIPanelSource`~~ | ✅ | — | **✅ 已删**（`341c94e`）· 派生自 `UIProjectName + UIPanelSourceName` |
| 38 | UI Panel Source Name | `UIPanelSourceName` | `PanelSource` | ✅ | 10 | 保留（10 处使用，含 `GetDefaultName` 调用） |
| 39 | ~~UIUI View Name~~ | ~~`UIUIViewName`~~ | ~~`UIView`~~ | ✅ | — | **✅ 已删**（`341c94e`）· 死字段 + 派生自 `UIProjectName + UIViewName` |
| 40 | UI View Parent Name | `UIViewParentName` | `ViewParent` | ✅ | 9 | 保留（9 处使用，含 `GetDefaultName` 调用） |
| 41 | ~~UIUI View Parent Name~~ | ~~`UIUIViewParentName`~~ | ~~`UIViewParent`~~ | ✅ | — | **✅ 已删**（`341c94e`）· 派生自 `UIProjectName + UIViewParentName` |

---

## 2. 重复内容分类（按 §1 全量清单的「判定」列重组）

> 每节标题直接对应 §1 表中的判定值；无对应判定的细枝末节（展示层等）收进 §2.6 保留项批注或 §4 优化方案。

### 2.1 死字段（2 项待删 / 1 项已删）

| # | 字段 | 面板显示 | 值 | 状态 | 说明 |
| ---: | --- | --- | --- | :--: | --- |
| 4 | `UIProjectPackageEditorPath` | UI项目指定包编辑器资源 | `Assets/../Packages/cn.rendu.{0}/Assets/Editor/UI` | ⏸ 待删 | 全仓 0 调用点。与之成对的 #6 `UIProjectPackageResPath` 有 3 处调用 → 疑似"包内编辑器资源"功能没做或已废弃 |
| 13 | `UIFrameworkPath` | UI框架所处位置 | `Assets/../Packages/cn.rendu.ui` | ⏸ 待删 | 全仓 0 调用点；框架内其它路径（#14/#15）也没用它拼接，是"想当然的中间层" |
| — | ~~`UIUIViewName`~~ | ~~UIUI View Name~~ | ~~`UIView`~~ | ✅ **已删** | `341c94e`：全仓 0 调用点 + 与 #23 `UIViewName` 语义重叠（同时也是 §2.4 撞车一例） |

> 实际剩 **2 项**待删；面板少 1 行。`UIProjectPackageEditorPath` 是"未实现功能"还是"废弃字段"仍待你拍板（§6 Q1）。

### 2.2 与 X 合并（3 对 / 6 项）

#### 2.2.1 #7 + #8 — 路径可由包名推导

| # | 字段 | 当前值 | 推导关系 |
| ---: | --- | --- | --- |
| 7 | `UIETCreatePackageName` | `uiproject` | 真值（包名） |
| 8 | `UIETCreatePackagePath` | `Assets/../Packages/cn.rendu.uiproject` | = `"Assets/../"` + `#16 UIPackages` + `#17 UIETPackagesFormat` + `#7`，完全吻合 |

> 两个字段必须手工保持一致，改一个忘一个就炸。处置：只留 #7，#8 改为运行期拼接。

#### 2.2.2 #16 + #17 — 框架级常量，下沉到 `PackageHelper`

| # | 字段 | 当前值 | 备注 |
| ---: | --- | --- | --- |
| 16 | `UIPackages` | `Packages` | Unity Packages 根目录 |
| 17 | `UIETPackagesFormat` | `cn.rendu.` | 包名命名空间前缀 |

> 这两个值的"事实"不属于 UI 面板，是工程级常量；§3 中还有 4 处独立硬编码副本。处置：下沉到 `RD.Core/PackageHelper`，面板不再展示。

#### 2.2.3 #20 + #22 — 值撞车 `Common`，合并为 `UICommonPkgName`

| # | 字段 | 当前值 | 引用 |
| ---: | --- | --- | ---: |
| 20 | `UIRootPkgName` | `Common` | 2 |
| 22 | `UICommonName` | `Common` | 3 |

> **值完全相同**；且 `CreateUIBaseModule.cs:53` 还有第三个副本 `private const string m_CommonPkg = "Common";`。处置：合并为单一字段 `UICommonPkgName`，同时干掉 `CreateUIBaseModule.cs:53` 的硬编码副本。

### 2.3 与 X 同组（1 对 / 2 项）

| # | 字段 | 值 | 备注 |
| ---: | --- | --- | --- |
| 18 | `UIRootName` | `UIRoot` | 同层级概念 |
| 19 | `UILayerRootName` | `UILayerRoot` | 命名不一致：一个带 `Name`，一个带 `RootName` |

> 值不同（`UIRoot` vs `UILayerRoot`），不算真重复。处置：**统一命名**（如 `UIRootName` / `UILayerRootName` → `RootName` / `LayerRootName`），方便对称查阅。

### 2.4 与 X 撞车（✅ 全部已删 / 改为派生）

> **原计划**：保留两个语义但改名区分（`UIPanelSourceObjectName` / `UIPanelSourceSuffix` 等）。
> **实际决策**：**直接删 `UIUI*` 字段**——它们并非互补的"物体名/后缀"，而是 `$"{UIProjectName}{<后缀>}"` 的派生值（详见 §2.4.3）。

#### 2.4.1 #37 + #38 — Panel 源（已合并）

| # | 字段 | 值 | 状态 | 备注 |
| ---: | --- | --- | ---: | --- |
| ~~37~~ | ~~`UIUIPanelSourceName`~~ | ~~`UIPanelSource`~~ | ✅ **已删** | 与 #38 撞车 + 是 `UIProjectName + UIPanelSourceName` 的派生 |
| 38 | `UIPanelSourceName` | `PanelSource` | ✅ **保留** | 后缀本身。`GetDefaultName(...)` 现唯一拼接来源 |

#### 2.4.2 #40 + #41 — View 父节点（已合并）

| # | 字段 | 值 | 状态 | 备注 |
| ---: | --- | --- | ---: | --- |
| 40 | `UIViewParentName` | `ViewParent` | ✅ **保留** | 后缀本身。`GetDefaultName(...)` 现唯一拼接来源 |
| ~~41~~ | ~~`UIUIViewParentName`~~ | ~~`UIViewParent`~~ | ✅ **已删** | 与 #40 撞车 + 是 `UIProjectName + UIViewParentName` 的派生 |

#### 2.4.3 推导关系（已落地）

```
UIProjectName     = "UI"                          ← 唯一项目前缀
UIPanelSourceName = "PanelSource"                  ← 后缀
↓ GetDefaultName(suffix) = UIProjectName + suffix
                          = "UI" + "PanelSource"
                          = "UIPanelSource"         ← 这正是旧 UIUIPanelSourceName 的值
```

3 处 `UIUI*` 字段值都能从这条公式**精确还原**，完全没有信息损失**；且不再有"双前缀"这种命名事故。

#### 2.4.4 落地改动（commit `341c94e`）

| 文件 | 改动 |
| --- | --- |
| `UIConstAsset.cs` | +`public string GetDefaultName(string suffix) => UIProjectName + suffix;`（行 22）；删 `UIUIPanelSourceName` / `UIUIViewName` / `UIUIViewParentName` 三字段 |
| `UIConstAsset.txt` | 删对应 3 行 JSON 键 |
| `MenuItemUIPanelSource.cs` | L42, L71 → `GetDefaultName(UIPanelSourceName)` |
| `MenuItemUIView.cs` | L29, L79, L104 → `GetDefaultName(UIViewName / UIViewParentName)` |
| `UIPanelSplitData_Editor.cs` | L223, L389 → `GetDefaultName(...)` |

**调用前 / 调用后对比**：

```csharp
// Before — 双前缀字段（命名事故 + 重复事实）
panelObject.name = UIConstHelper.Const.UIUIPanelSourceName;          // "UIPanelSource"

// After — 派生
panelObject.name = UIConstHelper.Const.GetDefaultName(
    UIConstHelper.Const.UIPanelSourceName);                         // "UIPanelSource"
```

### 2.5 中英别名对（4 对 / 8 字段）

| # | 英文 | 值 | # | 中文 | 值 | 各自引用 | 使用方 |
| ---: | --- | --- | ---: | --- | --- | ---: | --- |
| 25 | `UIPrefabs` | `Prefabs` | 26 | `UIPrefabsCN` | `预制` | 15 / 1 | 英文用于代码寻找；中文用于发布时收集资源目录（`UIPublishPackageModule.cs:207`） |
| 27 | `UISprites` | `Sprites` | 28 | `UISpritesCN` | `精灵` | 10 / 1 | 同上（`:215`） |
| 29 | `UIAtlas` | `Atlas` | 30 | `UIAtlasCN` | `图集` | 6 / 1 | 同上（`:203`） |
| 31 | `UISource` | `Source` | 32 | `UISourceCN` | `源文件` | 12 / 1 | 同上（`:211`，另 `UICreateResModule` 也在用） |

> 8 个字段占面板 **20% 行数**，语义上只有 4 个概念；CN 值是硬编码中文（无法本地化），实际只是"产物目录要中文名"这一项需求。处置：收成 `[Serializable] DirNamePair` 子对象（每对 → 1 行）。

### 2.6 保留项子批注（按 §1 括号备注分类）

下列字段 **判定为「保留」**，但备注中点出了额外的风险或可优化项，集中整理：

| # | 字段 | 批注类型 | 说明 |
| ---: | --- | --- | --- |
| 3 | `UIProjectEditorPath` | 仅 1 处使用 | 唯一调用点：`UISetting/UIAtlasModule.cs:110`（构造 `GlobalSpriteAtlasSettings.txt` 路径）。功能完整，不需重改 |
| 14 | `UITemplatePath` | 可派生 | 实际 = `UIFrameworkPath` + `"/Editor/UIAutoTool/Template"`。但 `UIFrameworkPath` 是死字段（§2.1），这个"派生关系"从来没连上 → 应在删 #13 时同步处理：要么补一个 `UIFrameworkPath` 的真实拼接，要么把 `#14` 也改硬编码 |
| 15 | `UIRootPrefabPath` | 可派生 + 路径格式不一致 | 框架内固定路径；且是面板中唯一一个**不带 `Assets/../` 前缀**的路径项（#3~#12 都带），风格不统一 |
| 34 | `UISpritesAtlas1` | 命名带序号·扩展性差 | 值 `Atlas1`，注释："不需要华丽的取名 每个包内的自定义图集就按顺序就好"。后续若出现 `Atlas2/3/...`，会再添字段 → 应改为数组或枚举（`AtlasNames = {"Atlas1", "Atlas2", ...}`） |

---

## 3. 跨模块重复（面板之外的副本）

这些值在面板里是"可配置常量"，在别的模块里又被硬编码了一遍 —— 属于**同一事实的多处定义**：

| 值 | 面板字段 | 其它副本 |
| --- | --- | --- |
| `cn.rendu.` | `UIETPackagesFormat`（#17） | `Packages/com.rendu.packagemanager/Editor/Helpers/CreatePackage/PackageDefaultInfo.cs:9`（`formalPrefix`）、`PackageViewModel.cs:48`（fallback 字面量）、`Packages/cn.rendu.core/Scripts/Core/Share/Helper/PackageHelper.cs:53`（`Replace("cn.rendu.", "")`） |
| `Common`（包名） | `UIRootPkgName`(#20) / `UICommonName`(#22) | `Packages/cn.rendu.ui/Editor/UIAutoTool/Window/UIBase/CreateUIBaseModule.cs:53`（`m_CommonPkg`）、`Packages/cn.rendu.yooasset/Editor/BundleCollector/BundleCollectorConfig.cs:18`（`XmlCommon`） |
| `Packages`（根） | `UIPackages`（#16） | `cn.rendu.loader/Editor/Command/RunInitScriptsCommand.cs:18`、`ScriptsReferencesCommand.cs:23/46`、`ProjectInitCommand.cs:82` 等直接写 `"Packages"` |

> 结论：`UIETPackagesFormat` / `UIPackages` / `UIRootPkgName` 这类"框架级常量"不该由 UI 面板持有，应下沉到 `RD.Core`（如 `PackageHelper`）做**单一来源**，面板只做只读展示或干脆不展示。

---

## 4. 优化方案（建议，未执行）

目标：**38 项 → 21 项**（可编辑项保持 11 项不变，减的全是只读噪音与重复；扣除已实施的 `UIUI*` 删除）

### 4.1 直接删除（2 项待删）

`UIProjectPackageEditorPath`、`UIFrameworkPath`

> 前置确认：`UIProjectPackageEditorPath` 是"待实现功能"还是"废弃字段"？若是待实现，则改为**补使用点**而不是删。
>
> ~~`UIUIViewName`~~ — ✅ **已删**（`341c94e`）· §2.4 同批。

### 4.2 合并（5 组 → 省 5 行）

| 合并项 | | 处置 |
| --- | :--: | --- |
| `UIETCreatePackageName` + `UIETCreatePackagePath` | ⏸ 待合 | 只留 `UIETCreatePackageName`（`uiproject`），路径改为运行期拼 `Packages/cn.rendu.{包名}` |
| `UIRootPkgName` + `UICommonName` | ⏸ 待合 | 合并为 `UICommonPkgName`（值 `Common`），同时干掉 `CreateUIBaseModule.cs:53` 的硬编码副本 |
| `UIPackages` + `UIETPackagesFormat` | ⏸ 待合 | 下沉到 `PackageHelper`，面板不再展示 |
| `UIUIPanelSourceName` + `UIPanelSourceName` | ✅ **已合**（`341c94e`）| **不是 rename，是 delete** — `UIUIPanelSourceName` = `UIProjectName + UIPanelSourceName`，无信息损失。详见 §2.4 |
| `UIUIViewParentName` + `UIViewParentName` | ✅ **已合**（`341c94e`）| 同上：delete + `GetDefaultName` 派生。`UIUIViewName` 同步删除（§2.4.2） |

### 4.3 收进"只读常量"子组（16 项，减少视觉密度）

建议用一个 `[BoxGroup("命名常量（只读）")]` 折叠组收纳，或在 `[Title]` 下分层：

`UIRootName`、`UILayerRootName`、`UIPanelName`、`UIViewName`、`UIParentName`、`UIPrefabs`、`UISprites`、`UIAtlas`、`UISource`、`UIAtlasIgnore`、`UISpritesAtlas1`、`UIAllViewParentName`、`UIAllPopupViewParentName`、`UIPanelSourceName`、`UIViewParentName`、`UIProjectName`

### 4.4 中英别名对 → 收成子对象（8 项 → 4 行）

```csharp
[Serializable]
public class DirNamePair        // 面板显示为一行折叠：预制 Prefabs / 精灵 Sprites ...
{
    public string EN;           // Prefabs
    public string CN;           // 预制
}
public DirNamePair PrefabsDirs; // + Dirs.Sprites / Dirs.Atlas / Dirs.Source
```

### 4.5 统一命名与标签

- 面板标签统一：可编辑项用中文 `[LabelText]`，只读常量组用英文原名（现状混排是"加了一半标签"造成的）
- ✅ **去掉所有 `UIUI*` 双前缀**（`341c94e`：3 个 `UIUI*` 字段已删，§2.4）
- 路径类值统一前缀风格（`Assets/../Packages/...` 或 `Packages/...`，不要两种混用 —— #15 是唯一的异类）

### 4.6 目标面板结构（21 项）

```
【项目配置 · 可编辑 11 项】                          ← 不动
  UI根目录名称 / UI项目命名空间
  UI项目编辑器资源 / UI项目指定包编辑器资源(待定)
  UI项目资源 / UI项目指定包资源
  UI项目生成包名
  UI项目脚本 / UI项目ET组件 / UI项目自定义脚本 / UI项目ET系统

【命名常量（只读折叠 · 13 项）】
  UIRoot / UILayerRoot / Panel / View / Parent / Common(包名)
  AllViewParent / AllPopupViewParent
  PanelSource(后缀) / ViewParent(后缀)               ← 物体名由 GetDefaultName 派生
  AtlasIgnore / Atlas1(序列)

【资源目录名（子对象 · 4 行）】
  Prefabs(EN/CN) / Sprites(EN/CN) / Atlas(EN/CN) / Source(EN/CN)
```

> 与原目标对比：§2.4 + §2.1(1 项) 已实施，面板从 **41 → 38**；剩余若全做，则到 **21**。

---

## 5. 风险与前置条件

1. **存档迁移成本 ≈ 0，但要确认**：`UIConstAsset.txt` 当前是 `{}`（全部走代码默认值），此时改字段名/删字段不会丢任何真实配置。若其它分支或同事机器上有填过值的存档，改名会导致那部分值回落到默认。
2. **引用面广**：41 个字段全仓约 **200 处**引用（`UIProjectResPath` 28 处、`UIProjectName` 24 处最多）。重命名必须全仓替换 + 重新构建 13 个 DotNet 工程验证。
3. **`UIConstAsset` 位于 `Runtime/`**（`Packages/cn.rendu.ui/Runtime/...`）：服务端/Roslyn 侧若也编译该目录，改动需两侧验证；`UIConstModule`（Editor）只是展示层。
4. **CN 别名对不能直接删**：`UIPublishPackageModule` / `UICreateResModule` 依赖它们收集中文目录，合并时必须同步改这两处。
5. **`[ReadOnly]` 属性来自 `RD.Inspector`（Odin 兼容层）**：结构调整（如 `[BoxGroup]` 改名、改嵌套子对象）需确认 `RD.Inspector` 支持 `[Serializable]` 子对象的 BoxGroup 渲染。
6. ~~**`UIPanelSplitData_Editor.cs`** 依赖 `UIUIPanelSourceName` / `UIViewParentName` / `UIUIViewParentName` / `UIAllViewParentName` 做"物体名识别"~~ — ✅ **已迁移**（`341c94e`）：`UIUIPanelSourceName` / `UIUIViewParentName` 改用 `GetDefaultName(...)`，`UIAllViewParentName` 与单 `UIViewParentName` 保持原状。后续若还要重命名单前缀字段，需逐个核对"是物体名还是后缀"，但**不再是双前缀命名事故风险**。

---

## 6. 待确认（需你拍板）

> ✅ **本轮已闭环**：`UIUI*` 字段删除 + `GetDefaultName` 派生（`341c94e`）。
> ⏸ **仍待拍板**：

1. 死字段（剩 2 项）：**补使用点**（尤其 `UIProjectPackageEditorPath` 是否是未完成功能）；`UIFrameworkPath` 已被 `UIProjectName` 通过 `GetDefaultName` 部分取代 → 可顺势删
2. 4 组中英别名对：保持现状
3. 30 项只读常量：**收进折叠组** / **移出面板**（改为 `UIConstHelper` 里的 `const`）？
4. 跨模块硬编码（`cn.rendu.` / `Common` / `Packages`）：这轮是否一并下沉到 `PackageHelper`？（会影响 loader / packagemanager，范围比面板大）
5. **§2.4 实施后副作用排查**：`GetDefaultName` 在编辑器侧每次访问都拼接字符串；7 处调用都是命名初始化/校验，**无热路径**，可接受。若后续发现热路径使用，再考虑缓存。

> 其余项（合并 / 折叠 / 子对象）确认清单后我再出具体改动方案（按 §4.2 → §4.3 → §4.4 顺序，含全仓替换 + 构建验证）。
