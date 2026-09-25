# Rendu

基于 **Tuanjie（团结引擎）2022.3** + **HybridCLR 热更** + **YooAsset 资源**的 Unity 客户端 / .NET 服务端**全栈共库**游戏框架。核心思路：一套 Entity / Scene / Fiber 框架代码，通过 UPM 包组织，客户端与服务端共享 `Share/**` 源码，各自编译到不同目标平台。

## 技术栈

| 层 | 技术 |
| --- | --- |
| 引擎 | Tuanjie 2022.3.62t10（Unity 2022.3 分支，Roslyn 4.3.1，C# 上限 10） |
| 热更 | HybridCLR（客户端热更程序集 `RD.Model` / `RD.ModelView` / `RD.Hotfix` / `RD.HotfixView`） |
| 资源 | YooAsset |
| 服务端 | .NET 10（`RD.AppHost` 宿主 + `RD.Loader` AssemblyLoadContext 热重载） |
| 序列化 | MemoryPack / MongoDB.Bson |
| 寻路 | Recast / Detour（DotRecast 移植） |

## 架构

### 共享热更程序集

四个热更程序集的 asmdef 定义在 `cn.rendu.game`，各功能模块包通过 `*.asmref` 把代码贡献进去：

```
RD.Model        // 数据层：Entity / Component 定义（分析器强制：仅 Object 子类或 [EnableClass]）
RD.ModelView    // 数据层（View）
RD.Hotfix       // 逻辑层：System 实现（Model 的部分类）
RD.HotfixView   // 逻辑层（View）
```

### 源码分层（每个包内）

```
Scripts/
├── Model/          // 数据定义（编译进 RD.Model）
├── ModelView/      // 视图数据（编译进 RD.ModelView）
├── Hotfix/         // 逻辑实现（编译进 RD.Hotfix）
├── HotfixView/     // 视图逻辑（编译进 RD.HotfixView）
├── Share/          // 客户端 / 服务端共享（两侧都编译，语法上限 C# 9）
└── Server/         // 仅服务端（仅 .NET 编译，可用新语法）
```

## 包依赖（从底层到顶层）

```
sourcegenerator → init → inspector → invoke → core
                                     └→ mathematics / referenceregistry
ui → yooasset → hybridclr → loader
excel / protocol / recast            // 工具链
console / numeric / netinner / router / startconfig / http
move / ai / aoi / actorlocation / unit / login   // 功能模块（贡献进共享程序集）
gameres → game → packagemanager      // 资源 / 主入口 / 工具
```

| 包 | 说明 |
| --- | --- |
| `cn.rendu.sourcegenerator` | Roslyn 分析器与源生成器（RD0032 规则、EntitySystemOf 等） |
| `cn.rendu.core` | 核心框架：Entity / Scene / Fiber / Promise / 网络 / 定时器 / 序列化 |
| `cn.rendu.ui` | UI 框架：Panel / Bind 源生成 / 事件 |
| `cn.rendu.invoke` | Invoke 源生成器与运行时 |
| `cn.rendu.yooasset` | YooAsset 资源系统封装 |
| `cn.rendu.hybridclr` | HybridCLR 集成 |
| `cn.rendu.loader` | 服务端热重载加载器（AssemblyLoadContext） |
| `cn.rendu.recast` | Recast / Detour 寻路 |
| `cn.rendu.excel` / `cn.rendu.protocol` | Excel 配置导出 / 协议导出工具 |
| `cn.rendu.game` | 游戏主包：定义四个共享热更程序集与入口 |
| `com.rendu.init` / `com.rendu.packagemanager` | 启动引导 / 包管理工具 |

## DotNet~ 服务端与工具链

每个包的 `DotNet~/` 目录下是独立的 .NET 工程（13 个）：

| 工程 | TFM | 说明 |
| --- | --- | --- |
| `RD.Core` / `RD.Hotfix` / `RD.Model` / `RD.Mathematics` / `RD.Recast` | net10.0 | 共享库 |
| `RD.Loader` | net10.0 | 服务端程序集热重载 |
| `RD.AppHost` | net10.0 | 服务端宿主（Exe） |
| `RD.ExcelExporter` / `RD.ProtocolExporter` | net10.0 | 导出工具（Exe） |
| `RD.SourceGenerator` / `RD.SourceGeneratorAttribute` / `RD.UI.BindSourceGenerator` / `RD.InvokeSourceGenerator` | **netstandard2.0（硬约束）** | Roslyn 分析器 |

**构建与验证**：

```bash
# 全量构建（需传 SolutionDir）
for p in $(git ls-files "*DotNet~*/*.csproj"); do
    dotnet build "$p" -p:SolutionDir="$(pwd)/"
done
```

## 重要约束

- **Roslyn 分析器工程必须 `netstandard2.0`**：net10.0 编译的分析器 dll 引用 `System.Collections.Immutable 10.0`，Tuanjie 的 Roslyn 4.3.1 环境加载失败（CS8032 / RD0032 误报）。
- **Unity 侧 C# 上限 10**（Roslyn 4.3.1 实测 `-langversion:11` 即 CS1617）；DotNet 侧为 C# 12。共享源码 `Scripts/*/Share/**` 语法跟随 Unity 侧。
- **NuGet 漏洞审计**：`dotnet list <proj> package --vulnerable --include-transitive`（逐项目跑；`Rendu.sln` 有嵌套项目报错无法整体加载）。

## 开发约定

- **提交信息**：`<emoji> <type>(<Scope>): <Subject>`（gitmoji 规范，emoji 与 type 间一个空格），如 `🐞 fix(Core): 修复 MongoRegister 空引用`。
- **仓库仅本地提交，不 push**。
- 调试日志保留，不删。

## 目录速览

```
Rendu/
├── Assets/                 # HybridCLR 生成物 / 插件 / NuGet
├── Packages/
│   ├── cn.rendu.*/         # 框架与功能包（含 DotNet~/ .NET 工程）
│   ├── com.rendu.*/        # 启动引导 / 包管理
│   └── manifest.json       # UPM 清单
├── ProjectSettings/
└── Bin/                    # DotNet 构建输出（不入库）
```

## 许可

见 [LICENSE](LICENSE)。
