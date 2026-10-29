#!/usr/bin/env bash
# Unity 包内安全移动: 同时搬走源文件与同名 .meta。
#
# 为什么不能只用 mv: Unity 用 .meta 里的 GUID 标识资源, 只搬 .cs 会让 Unity 重新生成
# .meta、GUID 随之变化 —— 场景/预制体/asmdef 里按 GUID 的引用会静默失效。
# 只搬 .cs 不搬 .meta 等于"删掉一个资源再加一个资源", 引用全丢。
#
# 用法: Tools/mv-unity-file.sh <旧路径> <新路径>
set -euo pipefail
src="$1"
dst="$2"
[ -f "$src" ] || { echo "源文件不存在: $src" >&2; exit 1; }
mkdir -p "$(dirname "$dst")"
mv "$src" "$dst"
[ -f "$src.meta" ] && mv "$src.meta" "$dst.meta"
echo "$src -> $dst"