#!/usr/bin/env bash
# 用真实文件树同步 Unity(Rider) 生成的 csproj 里的 <Compile Include> 清单。
#
# 为什么需要: Packages/ 下的脚本不在 Assets/ 里, csproj 由 Rider 按 .meta 索引生成,
# 重构期间批量增删/移动文件后它会有延迟 —— 表现为 CS2001(源文件找不到)或新文件根本没编译。
# 本脚本在每次 dotnet build 前跑一次, 让编译清单与磁盘一致。
#
# 收集规则严格按 asmdef 边界: 只取"离现有 Compile 项最近的、含 .asmdef 的祖先目录"作为根,
# 并且递归时不跨越嵌套的 .asmdef(那是另一个程序集, 由另一个 csproj 负责)。
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

for proj in "$@"; do
    [ -f "$proj" ] || continue
    python3 - "$proj" <<'PY'
import os, re, sys, pathlib

proj = pathlib.Path(sys.argv[1])
text = proj.read_text(encoding="utf-8")
existing = [m.group(1) for m in re.finditer(r'<Compile Include="([^"]+)"', text)]

# 本 csproj 代表的程序集名 —— 只有同名 asmdef 才是它的编译根。
m = re.search(r"<AssemblyName>([^<]+)</AssemblyName>", text)
asm_name = m.group(1) if m else ""

def asmdef_name(d):
    for f in d.glob("*.asmdef"):
        n = re.search(r'"name"\s*:\s*"([^"]+)"', f.read_text(encoding="utf-8"))
        if n:
            return n.group(1)
    return None

def asmdef_root(rel):
    """从 Compile 项回溯到最近的 asmdef 目录; 名字与本程序集不符则返回 None。"""
    parts = rel.split("/")
    for i in range(len(parts), 0, -1):
        d = pathlib.Path("/".join(parts[:i]))
        if d.is_dir() and any(d.glob("*.asmdef")):
            return d if asm_name == "" or asmdef_name(d) == asm_name else None
    return None

roots = {r for r in (asmdef_root(p) for p in existing) if r is not None}
if not roots:
    sys.exit(0)

wanted = set()
for root in roots:
    for dirpath, dirnames, filenames in os.walk(str(root)):
        # 嵌套 asmdef = 另一个程序集, 不归本 csproj。
        if dirpath != str(root) and any(
            name.endswith(".asmdef") for name in os.listdir(dirpath)
        ):
            dirnames[:] = []
            continue
        for f in filenames:
            if f.endswith(".cs"):
                wanted.add((pathlib.Path(dirpath) / f).as_posix())

missing = sorted(wanted - set(existing))
stale = sorted(p for p in existing if p not in wanted)
if not missing and not stale:
    sys.exit(0)

for p in stale:
    text = re.sub(r'^[ \t]*<Compile Include="%s" />\r?\n' % re.escape(p), "", text, flags=re.M)

lines = text.splitlines(keepends=True)
idx = max(i for i, l in enumerate(lines) if "<Compile Include=" in l)
indent = re.match(r"\s*", lines[idx]).group(0)
lines[idx + 1:idx + 1] = ['%s<Compile Include="%s" />\n' % (indent, p) for p in missing]
proj.write_text("".join(lines), encoding="utf-8")
print("%s: +%d -%d" % (proj.name, len(missing), len(stale)))
PY
done