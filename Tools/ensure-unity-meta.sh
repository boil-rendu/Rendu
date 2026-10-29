#!/usr/bin/env bash
# 为新增的脚本/目录补齐 .meta。
#
# 为什么必须入库: Unity 用 .meta 里的 GUID 标识资源。缺 .meta 时 Unity 会现场生成,
# 于是每台机器、每次克隆都会得到不同的 GUID —— 场景/预制体/asmdef 里按 GUID 的引用
# 换个机器就断。目录的 .meta 同理, 少了它目录本身在 Unity 眼里就是"新资源"。
#
# 只补"确实不存在"的, 不覆盖已有文件(GUID 不能变)。
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

new_guid() { python3 -c "import uuid;print(uuid.uuid4().hex)"; }

cs_meta() {
cat <<META
fileFormatVersion: 2
guid: $1
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
META
}

folder_meta() {
cat <<META
fileFormatVersion: 2
guid: $1
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
META
}

count=0
for target in "$@"; do
    [ -e "$target" ] || { echo "目标不存在: $target" >&2; exit 1; }
    if [ -f "$target.meta" ]; then continue; fi
    if [ -d "$target" ]; then
        folder_meta "$(new_guid)" > "$target.meta"
    else
        cs_meta "$(new_guid)" > "$target.meta"
    fi
    echo "  新建 $target.meta"
    count=$((count + 1))
done
echo "补齐 $count 个 .meta"
