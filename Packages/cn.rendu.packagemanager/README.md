# RD.PackageManager

RD 系 Unity 包管理器。把 `cn.rendu.*` 多包工程里的“版本 / 依赖 / 包生成 / 更新”收敛到一个集成工具里，避免散落在菜单各处手动改 `package.json`。

## 它能干嘛

| 能力 | 入口 | 说明 |
|---|---|---|
| 集成工具窗口 | `RD/PackageManager/包管理集成工具/打开` | 左树菜单 + 右面板，含 5 个模块（见下） |
| 包创建窗口 | `RD/PackageManager/包创建` | 简写驱动生成新包骨架 |
| 更新检查窗口 | `RD/PackageManager/更新检查` | 列出远端高于本地的包 |

### 集成窗口的 5 个模块

| 模块 | 作用 | 数据资产 |
|---|---|---|
| 文档 | 跳飞书/本地文档的入口 | 无 |
| 库（Hub） | 包清单与依赖总览 | `PackageHubAsset` / `PackageHubData` |
| 创建 | 生成新包 | 直接写 `Packages/<name>` |
| 更新 | 检查远端更新 | 即时请求 |
| 版本管理 | **副本编辑**版本与依赖，再统一落盘 | `PackageVersionAsset` / `PackageVersionData` / `DependencyInfo` |

## 关键设计点

- **版本管理是副本编辑**：界面上改版本号 / 依赖，只改 `PackageVersionAsset` 的深拷贝，磁盘不动；只有点 `同步生成` 才会写回各包 `package.json`。
- **依赖传播**：`强制依赖同步` 把版本变更沿依赖图递归传播到引用方。
- **覆盖更新有风险**：`更新当前所有` 会删旧目录再写，弹窗已警告；更新前先 `强制依赖同步`。
- **入口不自启**：三个入口都在 `RD/PackageManager/` 菜单，需手动打开。

## 文档

- `Docs/Tutorial.md` —— 实操教程：升版本并同步依赖 / 新建包 / 批量查更新，从打开到落盘一步步写。
- `Docs/ArchitectureDesign.md` —— 架构设计：三层分工、数据流向、设计约束。

## 依赖

`cn.rendu.draw`（绘制面板）、`cn.rendu.edittool`（窗口壳 `GenericToolWindow`）、`cn.rendu.serialization`、`cn.rendu.utilities`。
