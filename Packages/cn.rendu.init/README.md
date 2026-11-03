# RD.Init

RD 多包 Unity 工程的**初始化与治理工具（编辑器侧）**。负责把 `cn.rendu.*` 从包缓存“解包”为可编辑本地包、选定主包、切换代码模式、管理全局配置，以及读取包级 Git 信息。

## 功能清单

| 能力 | 入口（`RD/Init/...`） | 产物 |
|---|---|---|
| 依赖自动解包 | `RepairDependencies` | 把 `Library/PackageCache/cn.rendu.*` 移入 `Packages/`（由 `[InitializeOnLoad]` 在包注册时自动跑，异常时手动触发） |
| 移入包窗口 | `MoveToPackage` | 手动挑包从缓存移入 `Packages/` |
| 主包选择 | （`GlobalConfig` 检视器内 `Set Main Package`） | 根目录 `MainPackage.txt` + 硬链 `RD.sln` |
| 全局配置 | `GlobalConfig` | 定位 `Resources/GlobalConfig.asset` |
| 代码模式切换 | （`GlobalConfig` 检视器内 `Set Code Mode`） | 改写各包 `AssemblyReference.asmref`，切换 Client / Server / ClientServer 编译范围 |
| 包 Git 信息 | `PackageGitHelper.Load(path)` | 读取各包 `packagegit.json` |

## 关键说明

- **`MainPackage.txt`**：首行主包名，其后逐行是主包的 `cn.rendu.*` 依赖清单；代码模式切换时按它取目标包集合。
- **`packagegit.json` 契约**：由 `cn.rendu.packagemanager` 在创建/更新包时生成，`cn.rendu.init` 只负责读取（BSON 反序列化）。两包以此解耦。
- **仅编辑器侧**：`DependencyResolver` / `MainPackageSelector` / 代码模式切换都跑在 Editor，不进运行时构建。
- **跨平台**：`ProcessHelper` 在 Windows 走 `powershell.exe` / `dotnet.exe`，macOS 走 `/usr/local/bin/pwsh` / `/usr/local/share/dotnet/dotnet`。
- **MongoDB 驱动是工具配套**：`Plugins/MongoDB/*` 只服务于包治理（读 `packagegit.json`、远端注册表），不进入游戏运行时。
- **`ProcessHelper` 为 `internal`**，不是对外公共 API，使用者无需直接调用。

## 文档

- `Docs/Tutorial.md` —— 实操：新工程初始化、配置主包与代码模式、验证与排错。

## 依赖

独立包，`package.json` 的 `dependencies` 为空；仅依赖 `UnityEngine`（运行时与编辑器均可编译）。
