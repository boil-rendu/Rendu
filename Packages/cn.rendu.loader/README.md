# RD.Loader

服务端热重载加载器（AssemblyLoadContext） + **LoaderTool 工具窗口**（7 个挂在 `[UIAutoMenu(Scope="loader")]` 下的特性面板模块）。包内自带一条从 UPM 包到 `RD.Loader` / `RD.AppHost` 工具链的完整闭环：编译 → 引用补全 → 资源刷新 → 链接 SLN → 启动服务/监视器/MongoDB/构建发布 —— 全部在 **Rendu / Loader Tool** 窗口里。

工具窗口与模块机制本身在 [cn.rendu.editorwindow](../cn.rendu.editorwindow/README.md)；特性与绘制引擎在 [cn.rendu.panel](../cn.rendu.panel/README.md)。本包只承载"loader 域"模块的具体业务。

## 包结构

```
cn.rendu.loader/
├── Runtime/                              RD.Loader（运行时程序集）
│   ├── Define.cs                         AppDllPath / CodeDir 等常量
│   └── GlobalConfig.cs                   全局配置 ScriptableObject (CodeMode / SceneName / Address)
│
├── Editor/                               RD.Loader.Editor（Editor 程序集）
│   ├── RD.Loader.Editor.asmdef
│   ├── LoaderMenu.cs                     顶层菜单 Rendu/Loader/（只留 F6/F7 两个全局热键）
│   ├── Tool/LoaderTool.cs                Rendu/Loader Tool 工具窗口（AcceptScope="loader"）
│   │
│   ├── Macro/                            100  宏定义（Phase 3 从 cn.rendu.ui 迁入）
│   │   ├── MacroModule.cs                [UIAutoMenu("宏定义", 100, Scope="loader")]
│   │   ├── MacroHelper.cs                收集所有 [EnumMacro] 枚举
│   │   ├── MacroDataBase.cs              枚举→字典基类
│   │   ├── MacroCurrentData.cs           当前值持久化（EditorPrefs）
│   │   ├── EnumMacroAttribute.cs         标记哪些枚举是宏
│   │   └── Demo/MacroData.cs             使用示例
│   │
│   ├── GlobalConfig/                     200  全局配置
│   │   ├── GlobalConfigModule.cs         [UIAutoMenu("全局配置", 200, Scope="loader")]
│   │   └── GlobalConfigModel.cs          读 / 写 GlobalConfig.asset
│   │
│   ├── ProjectInit/                      300  项目初始化（一次性管线：6 步）
│   │   ├── ProjectInitModule.cs          [UIAutoMenu("项目初始化", 300, Scope="loader")]
│   │   └── ProjectInitCommand.cs         FindPackageName / CheckIsInited / ChangeCodeMode
│   │
│   ├── ProjectFiles/                     400  项目文件管理
│   │   ├── ProjectFilesModule.cs         [UIAutoMenu("项目文件", 400, Scope="loader")]
│   │   ├── ProjectFilesCommand.cs        ReGenerateProjectFiles / GetIgnoredCsprojFiles
│   │   ├── LinkSlnCommand.cs             链接 SLN（让 Rider / VS 能打开）
│   │   └── RegenerateProjectFilesCommand.cs
│   │
│   ├── Build/                            500  构建
│   │   ├── BuildModule.cs                [UIAutoMenu("构建", 500, Scope="loader")]
│   │   └── BuildApplicationCommand.cs    BuildPipeline.BuildPlayer 包装
│   │
│   ├── Server/                           600  服务端管理
│   │   ├── ServerModule.cs               [UIAutoMenu("服务器", 600, Scope="loader")]
│   │   ├── StartServerCommand.cs         启动游戏服务器
│   │   └── StartMongoCommand.cs          启动 MongoDB
│   │
│   ├── ScriptsReferences/                700  脚本引用（四个 RD.* 程序集定义）
│   │   ├── ScriptsReferencesModule.cs    [UIAutoMenu("脚本引用", 700, Scope="loader")]
│   │   ├── ScriptsReferencesCommand.cs   读 cn.rendu.*/packagegit.json 收集引用
│   │   └── UpdateScriptsReferencesCommand.cs
│   │
│   ├── Define/                           三个跨模块用的工具类（不是窗口）
│   │   ├── DefineCommand.cs              static EnableDefineSymbols(symbol, enable)
│   │   ├── EnableDefineCommand.cs        CommandBase 包装：启用宏
│   │   └── ToggleDefineCommand.cs        CommandBase 包装：切换宏
│   │
│   ├── Command/                          8 个跨模块用的命令（按"动作"分，不绑定到具体窗口）
│   │   ├── ChangeCodeModeCommand.cs
│   │   ├── GenerateAssemblyReferenceCommand.cs
│   │   ├── RefreshAssetDatabaseCommand.cs
│   │   ├── RunInitScriptsCommand.cs
│   │   ├── SaveAssetCommand.cs
│   │   ├── SetSceneNameCommand.cs
│   │   └── StartWatcherCommand.cs
│   │
│   ├── Assembly/                         2 个 IPreprocessGenerateProject 钩子
│   ├── Services/                         编译服务接口 + 实现
│   ├── Core/                             EditorTypes (PlatformType / DevelopMode) + RelayCommand
│   └── Utils/                            9 个编辑器工具（AssemblyTool / EditorDialogHelper / ...）
│
├── Scripts/Loader/                       客户端/服务端热重载与启动器（.NET 共享）
│   ├── Client/                           Launcher / CodeLoader / CoroutineHelper / ...
│   ├── Server/                           Launcher / CodeLoader / NLogger
│   └── Share/                            HttpClientHelper
│
├── DotNet~/RD.Loader/                    服务端热重载工程 (net10.0)
└── package.json
```

## 依赖

| 程序集 | 引用 | 说明 |
|---|---|---|
| `RD.Loader` | `RD.YooAsset`、`RD.Init` | 运行时，引擎 + 资源 + 启动引导 |
| `RD.Loader.Editor` | `RD.Panel`、`RD.Panel.Editor`、`RD.EditorWindow.Editor` | 编辑器扩展，挂特性模块到 LoaderTool |

> 注：`RD.Loader.Editor` 不引用 `cn.rendu.ui` —— 工具基建已迁到 `cn.rendu.editorwindow`，避免 ui / loader 互相依赖。

## 工具窗口：Loader Tool

菜单 **Rendu / Loader Tool**（与 Rendu / Loader/ 子菜单并列，定义在 `Editor/Tool/LoaderTool.cs:23`）。

打开后左侧菜单树长这样（按 Order 升序；模块在 `BuildToolMenuTree` 时被反射扫描并按 Scope 过滤，只显示 `Scope == "loader"` 的模块）：

| Order | 模块 | 主要内容 |
|---|---|---|
| 100 | 宏定义 | 列出本程序集内所有 `[EnumMacro]` 枚举；勾选值进 EditorPrefs |
| 200 | 全局配置 | CodeMode / SceneName / Address；改动自动存 GlobalConfig.asset |
| 300 | 项目初始化 | 5 个只读字段（包名/场景/CodeMode/地址/INITED 状态）；按钮跑"6 步初始化管线" |
| 400 | 项目文件 | Sln 状态 + 7 个按钮（LinkSln / 重新生成 / 刷新数据库 / GenRef / UpdateRef / InitScripts / 列出 Ignore csproj） |
| 500 | 构建 | 平台下拉 + Development 开关 + Build / 打开 BuildSettings / 清缓存 |
| 600 | 服务器 | 3 进程状态（🟢/🔴） + 6 按钮（启动/停止/重启 × GameServer / MongoDB / Watcher） |
| 700 | 脚本引用 | 3 按钮（生成 / 更新 / 查看当前引用配置） |

## 顶层菜单：Rendu / Loader/

只保留两个全局热键；其他动作全部收编到上面的模块里：

| 项 | 快捷键 | 行为 |
|---|---|---|
| Compile | F6 | `AssetDatabase.Refresh` + `AssemblyTool.DoCompile` |
| Hot Reload | F7 | 反射拿 `RD.CodeLoader.Instance.Reload` 调用；非 Play Mode 拒绝 |

## 写一个新模块挂在 LoaderTool 下

`BaseToolModule` 在 `cn.rendu.editorwindow`，`[ShowInInspector] / [Button] / [GUIColor] / [ValueDropdown] / [OnValueChanged]` 等特性在 `cn.rendu.panel`。最简例子：

```csharp
using RD.Panel;
using RD.EditorWindows.Editor;
using UnityEngine;

namespace RD.Loader.Editor
{
    [UIAutoMenu("我的模块", 750, Scope = "loader")]
    public class MyModule : BaseToolModule
    {
        [LabelText("配置项")]
        [ShowInInspector]
        public string Config { get; set; } = "default";

        [Button("执行", 10)]
        [GUIColor(0.53f, 0.95f, 0.72f)]
        public void DoSomething() => Debug.Log(Config);
    }
}
```

存盘后 LoaderTool 会在左侧菜单树 Order=750 处多一项，标题"我的模块"。

### 跨模块复用：Command / 工具类

模块的 [Button] 里**直接 `new XxxCommand().Execute()`**，不要自己写实现：

```csharp
[Button("重新生成 AssemblyReference", 10)]
public void Regenerate() => new GenerateAssemblyReferenceCommand(CodeMode.Client).Execute();
```

新增"动作"步骤：
1. 在 `Editor/Command/<Name>Command.cs` 写一个 `class <Name>Command : CommandBase`，实现 `Execute()`；
2. 模块的 [Button] 里 `new <Name>Command().Execute()` 调用即可。

## 项目初始化模块的 6 步管线

`ProjectInitModule.InitProject()` 是从 ui 包历史继承下来的一键初始化流程，按顺序跑：

1. `SetSceneName` — 在 GlobalConfig 里写 SceneName
2. `LinkSln` — 链接 SLN
3. `UpdateScriptsReferences` — 把 `cn.rendu.*/packagegit.json` 里 `ScriptsReferences` 写到 4 个 RD.* 程序集定义
4. `ChangeCodeMode("ClientServer")` — 切到客户端+服务端共编模式
5. `RunInitScripts` — 跑 `Packages/cn.rendu.*/Init.ps1`
6. `EnableDefine("INITED")` — 标记工程"已初始化"（DefineWindow 那个 INITED 开关的去处）

## 与其他包的关系

| 包 | 关系 |
|---|---|
| `cn.rendu.editorwindow` | LoaderTool 继承它的 `GenericToolWindow`；7 个模块都继承它的 `BaseToolModule` |
| `cn.rendu.panel` | 提供 `[Button] / [ShowInInspector] / [GUIColor] / [ValueDropdown]` 等特性 + 绘制引擎 |
| `com.rendu.init` | 提供 `MenuDefine.ProjectRootMenu`（"Rendu"）与 `CommandBase` 基类 |
| `cn.rendu.core` | 提供 `FileHelper`（ScriptsReferencesCommand 收集引用时用） |
| `cn.rendu.ui` | **不依赖**。Phase 1-2 后，工具基建已迁到 editorwindow；cn.rendu.ui 只剩 UI 业务 + AutoTool 薄壳 |
| `cn.rendu.yooasset` | 运行时，资源系统 |
| `DotNet~/RD.Loader` | 服务端热重载工程，编译产物 `Bin/RD.Loader.dll`（= `RD.Loader.StartWatcherCommand` 启动的那个 DLL） |
| `DotNet~/RD.AppHost` | 服务端 Exe 宿主（详见顶层 README） |
