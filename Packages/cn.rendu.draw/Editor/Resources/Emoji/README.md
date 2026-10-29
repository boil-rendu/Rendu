# Emoji 贴图

面板文字里的 emoji 不走字体渲染（编辑器默认字体没有 emoji 字形，Unity 2022.3 也不支持彩色位图字体），
改由 `Editor/Emoji/DrawEmoji.cs` 把文字切成"文本段 + emoji 贴图段"后内联绘制。

## 来源与注意

- 由脚本从本机系统字体 **Apple Color Emoji** 的 `sbix` 表抽取（64×64 PNG）。
- Apple Color Emoji 的版权归 Apple Inc. 所有，**不允许随产品分发**。
  这里只是本地开发/自用的编辑器贴图；正式发布或开源前请换成可自由使用的替代
  （Noto Color Emoji / OpenMoji / Twemoji 等），保持同样的文件名（`码位小写十六进制.png`）即可直接替换。

## 补充 emoji

往本目录放 `<码位小写十六进制>.png`（例如 `1f600.png`）即可，`DrawEmoji` 启动时扫描目录建立码位表。
