# RD.PackageManager 架构设计

本文件记录 `cn.rendu.packagemanager` 的模块拆分、数据流向与设计约束，对应 `Tutorial.md` 的“怎么用”。结论按 `Packages/cn.rendu.packagemanager/Editor/` 当前源码核对。

---

## 1. 总览：三层分工

| 层 | 职责 | 关键类型 |
|---|---|---|
| 入口层 | 菜单入口 + 窗口壳 | `RDPackageManagerWindow`（`GenericToolWindow`）、`RDPackageCreateWindow`、`RDPackageUpdateWindow` |
| 模块层 | 各业务能力，靠 `[ToolMenu]` 入树 | `RDPackageHubModule` / `RDPackageVersionModule`(+2 partial) / `PackageCreateModule` / `RDPackageUpdateModule` / `RDPackageDocumentModule` |
| 支撑层 | 数据资产 + 工具链执行 | `PackageHubHelper` / `PackageVersionHelper` / `PackageHelper` / `PackageExecuteMenuItemHelper` / `PackageCreateHelper` |

业务代码**不进入** `cn.rendu.draw` / `cn.rendu.edittool`；本包只做“包管理”这一件事。

## 2. 主窗口：GenericToolWindow 复用

`RDPackageManagerWindow : GenericToolWindow`（来自 `cn.rendu.edittool`）。本类**只提供窗口入口与静态工具方法**（`OpenWindow` / `CloseWindow` / `UnloadAllAssets` / `ReLoadAllAssets`），左树、右面板、模块扫描、选中调度、上次选中持久化全部由基类承担。

> 这是有意为之：包管理只是 GenericToolWindow 的一个“宿主”，后续加能力只需新增 `[ToolMenu]` 模块，不动窗口壳。

模块入树规则（`[ToolMenu(name, order?)]`）：

- `文档`：`-int.MaxValue`（恒最前，纯入口）
- `库` / `创建` / `更新`：`0`（默认序，按声明/字母排）
- `版本管理`：`1000`（最后）

模块统一继承 `BaseToolModule`：`Initialize()` 选中时调用，`OnDestroy()` 离开时调用。

## 3. 模块与数据资产对应关系

| 模块 | 读写的资产 | 落盘时机 |
|---|---|---|
| 库（Hub） | `PackageHubAsset` / `PackageHubData` | `OnDestroy` → `PackageHubHelper.SaveAsset()` |
| 版本管理（Version） | `PackageVersionAsset` / `PackageVersionData` / `DependencyInfo` | 仅 `同步生成` 按钮写回各包 `package.json` |
| 创建（Create） | 直接生成 `Packages/<name>` | 点“创建/更新包”即时 |
| 更新（Update） | 远端请求 | 即时 |
| 文档（Document） | 无（跳飞书） | — |

**关键不变量**：版本管理模块的界面编辑对象是 `PackageVersionAsset` 的**深拷贝**（`m_AllPackageInfoDataDic`）。这意味着：

- 在界面上改版本号 / 依赖，**不会**动磁盘；
- 只有 `同步生成` 才会把版本 / 依赖写回各包 `package.json`；
- 强制依赖同步 = 把版本变更沿依赖图递归传播（`SyncRefPackages` / `ChageDependencies`）；强制本地同步 = 关窗→卸载资源→重开窗（当前不做联网）。

## 4. 数据流向（版本管理）

```
PackageHelper.CheckUpdateAll  ──►  拉取所有 RD 包最新版本
        │
        ▼
PackageVersionAsset（深拷贝成 m_AllPackageInfoDataDic）
        │  界面增删改都在副本上
        ▼
[同步生成]  ──►  写回各包 package.json（版本 + 依赖）
        │
        ├─[强制依赖同步] 沿依赖图递归传播版本
        └─[更新当前所有] 覆盖式批量更新（删旧目录→改依赖→请求服务端）
```

## 5. 工具链串联

`PackageExecuteMenuItemHelper` 把散落在 `RD/` 菜单下的工具串成一条流水线，顺序固定（注释明确“有顺序的 别乱动”）：

```
RepairDependencies → ReGenerateProjectFiles
                   → ReGenerateProjectAssemblyReference
                   → UpdateScriptsReferences
                   → Proto2CS        （Excel 导出已禁用）
```

每个 `ExecuteMenuItem` 包 `try/catch`，单步失败只 `LogError` 不中断。库 / 版本模块的“更新全部”会调用 `RDAll()`。

## 6. 设计约束 / 已知限制

- **版本管理是副本编辑**：忘了点 `同步生成` 等于白改（见 §3）。
- **强制本地同步不是联网同步**：仅重开窗，真正联网逻辑已注释保留在 `RDPackageVersionModule_Sync.cs` 末尾。
- **更新当前所有是覆盖更新**：会删旧目录，合并更新需自行处理；弹窗已明确警告。
- **包名(简写)驱动创建**：创建窗口输入简写后防抖载入已存在信息；正式包名/显示名只读派生。
- **入口不自启**：三个入口都在 `RD/PackageManager/` 菜单，需手动打开。
