# cn.rendu.init 使用教程

`cn.rendu.init` 是 RD 多包 Unity 工程的**初始化与治理工具（编辑器侧）**：把 `cn.rendu.*` 从包缓存解包为可编辑本地包、选定主包、切换代码模式、管理全局配置。

> 仅编辑器侧，不进运行时构建。`ProcessHelper` 为 `internal`，使用者无需直接调用。

## 第 1 章 新工程初始化：把包从缓存移入 Packages/

`DependencyResolver`（`[InitializeOnLoad]`）在包管理器注册包时**自动**把 `Library/PackageCache/cn.rendu.*` 移入 `Packages/`。

- 异常/不全时用菜单 `RD/Init/RepairDependencies` 重新移一遍。
- 或手动开 `RD/Init/MoveToPackage` 窗口：扫描缓存 → 按词过滤 → 多选勾选 → 一键移入 `Packages/`。

## 第 2 章 配置主包与代码模式

1. `RD/Init/GlobalConfig` 定位 `Resources/GlobalConfig.asset`，在 Inspector 填：
   - `SceneName`（如 `StateSync` → 映射规则 `cn.rendu.statesync`）
   - `CodeMode`（`Client=1` / `Server=2` / `ClientServer=3`）
   - `Address`（如 `127.0.0.1:10101`）
2. Inspector 点 **Set Main Package** → `MainPackageSelector.SetAsMainPackage` 写出根目录 `MainPackage.txt`（首行主包名，其后逐行主包的 `cn.rendu.*` 依赖），并把主包 `RD.sln` 硬链到工程根。
3. Inspector 点 **Set Code Mode** → 执行 `dotnet run --project Packages/cn.rendu.init/DotNet~/RD.CodeMode/RD.CodeMode.csproj -- --CodeMode=X` → 读 `MainPackage.txt` → `CodeModeChangeHelper` 重写各包 `AssemblyReference.asmref`，切换 Client / Server / ClientServer 的编译范围。

## 第 3 章 验证与排错

- 无 `MainPackage.txt` 时静默跳过代码模式切换（日志 `MainPackage.txt not found / is empty`）。
- `packagegit.json` 缺失时 `PackageGitHelper.Load` 报错 *retry refresh unity*——该文件由 `cn.rendu.packagemanager` 在创建/更新包时生成，是两包之间的数据契约。
- 版本差异：Unity 6000+ 与旧版对应的 `MoveToPackages` / `MoveToPackages_6.ps1` 脚本不同，注意 `DependencyResolver` 调用的是哪个。

## 交叉引用

- `../../cn.rendu.packagemanager/Docs/Tutorial.md` —— `packagegit.json` 由该包生成。
- 包根 `packagegit.json`（示例见 `Packages/cn.rendu.init/packagegit.json`：`{"Id":11,"Name":"Init"}`）。
