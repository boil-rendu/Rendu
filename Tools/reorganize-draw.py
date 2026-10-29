#!/usr/bin/env python3
"""按目标目录重排 cn.rendu.draw 的 Editor 源码, 让命名空间与目录一一对应。

背景: 重构前 90 个文件全部塞在 namespace RD.Draw.Editor 里, 目录树只在磁盘上存在,
IDE 里 200+ 个类型平铺, 依赖方向读不出来。本脚本把两者对齐:

  Editor/Api/          -> RD.Draw.Editor            对外门面(宿主唯一需要认识的东西)
  Editor/Drivers/      -> RD.Draw.Editor            后端契约 + IMGUI/UITK 实现
  Editor/Metadata/     -> RD.Draw.Editor.Metadata   成员发现与缓存
  Editor/Rules/        -> RD.Draw.Editor.Rules      纯规则(形态/标签/分组/可见性/取值/状态)
  Editor/Expressions/  -> RD.Draw.Editor.Expressions
  Editor/Rendering/    -> RD.Draw.Editor.Rendering  绘制流水线
  Editor/Tables/       -> RD.Draw.Editor.Tables     表格结构与渲染

命名空间不再保留"中间层 Engine": 那一层没有信息量, 且会与门面类 Engine 同名冲突。

using 的增删按类型名精确计算, 不靠"多导入几个"蒙混, 免得留下一堆无用 using。

用法: python3 Tools/reorganize-draw.py [--dry]
"""
import pathlib
import re
import shutil
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
PKG = ROOT / "Packages/cn.rendu.draw"
EDITOR = PKG / "Editor"
REN = "Engine/Rendering"

LAYERS = {
    "Api": "RD.Draw.Editor",
    "Drivers": "RD.Draw.Editor",
    "Metadata": "RD.Draw.Editor.Metadata",
    "Rules": "RD.Draw.Editor.Rules",
    "Expressions": "RD.Draw.Editor.Expressions",
    "Rendering": "RD.Draw.Editor.Rendering",
    "Tables": "RD.Draw.Editor.Tables",
}

E, B, R, X, D, S, V = ("Engine/", "Backends/", "Engine/Reflection/", "Engine/Expressions/",
                       "Engine/Diagnostics/", "Engine/Session/", "Engine/Rules/")

MOVES = [
    (E + "Engine.cs", "Api/Engine.cs"),
    (B + "Backend.cs", "Api/Backend.cs"),
    (S + "HostState.cs", "Api/HostState.cs"),
    (S + "HostSession.cs", "Api/HostSession.cs"),
    (E + "DrawLog.cs", "Api/DrawLog.cs"),
    (E + "EngineServices.cs", "Api/EngineServices.cs"),
    (D + "IDiagnosticSink.cs", "Api/IDiagnosticSink.cs"),
    (X + "ExpressionEvaluator.cs", "Api/ExpressionEvaluator.cs"),
    (X + "PropertyState.cs", "Api/ExpressionContext.cs"),
    (X + "PropertyAccessor.cs", "Api/PropertyAccessor.cs"),
    (E + "ValueOps.cs", "Rules/ValueOps.cs"),
    (R + "CachedMember.cs", "Metadata/CachedMember.cs"),
    (R + "MemberCollector.cs", "Metadata/MemberCollector.cs"),
    (R + "GroupNode.cs", "Metadata/GroupNode.cs"),
    (R + "GroupEntry.cs", "Metadata/GroupEntry.cs"),
    (R + "EnumCatalog.cs", "Metadata/EnumCatalog.cs"),
    (V + "ITypeShape.cs", "Rules/ITypeShape.cs"),
    (V + "DefaultTypeShape.cs", "Rules/DefaultTypeShape.cs"),
    (V + "ILabelFormatter.cs", "Rules/ILabelFormatter.cs"),
    (V + "DefaultLabelFormatter.cs", "Rules/DefaultLabelFormatter.cs"),
    (V + "GroupResolver.cs", "Rules/GroupResolver.cs"),
    (V + "GroupTraverse.cs", "Rules/GroupTraverse.cs"),
    (V + "VisibilityEvaluator.cs", "Rules/VisibilityEvaluator.cs"),
    (V + "ValueAccessor.cs", "Rules/ValueAccessor.cs"),
    (V + "StateUpdater.cs", "Rules/StateUpdater.cs"),
    (V + "TableSchema.cs", "Tables/TableSchema.cs"),
    (X + "SyntaxKind.cs", "Expressions/SyntaxKind.cs"),
    (X + "SyntaxNode.cs", "Expressions/SyntaxNode.cs"),
    (X + "TokenKind.cs", "Expressions/TokenKind.cs"),
    (X + "Lexer.cs", "Expressions/Lexer.cs"),
    (X + "Parser.cs", "Expressions/Parser.cs"),
    (X + "Interpreter.cs", "Expressions/Interpreter.cs"),
    (X + "GlobalTypeIndex.cs", "Expressions/GlobalTypeIndex.cs"),
    (E + "Rendering/TableRenderer.cs", "Tables/TableRenderer.cs"),
    (R + "TableColumn.cs", "Tables/TableColumn.cs"),
    (D + "TableGeometrySink.cs", "Tables/TableGeometrySink.cs"),
]
for n in ("IDriver", "IDriverScope", "IDriverControls", "IDriverLayout", "IDriverStateStack",
          "IDriverDiagnostics", "IDriverFactory", "DriverBase", "ImGuiDriver", "ImGuiDriverFactory",
          "UitkDriver", "UitkDriverFactory", "FallbackLayout", "NullStateStack", "DrawMetrics",
          "DrawEmoji", "EditorIcon", "DrawAlign", "DrawMessageKind", "DrawTextSpec", "DrawButtonSpec"):
    MOVES.append((B + n + ".cs", "Drivers/" + n + ".cs"))

TYPE_RE = re.compile(
    r"^\s*(?:public|internal|private|protected)?\s*"
    r"(?:static\s+|sealed\s+|abstract\s+|partial\s+|readonly\s+|ref\s+)*"
    r"(?:class|interface|struct|enum|delegate)\s+([A-Za-z_][A-Za-z0-9_]*)", re.M)


def targets():
    out = [d for _, d in MOVES]
    for p in sorted((EDITOR / REN).rglob("*.cs")):
        out.append(str(p.relative_to(EDITOR)).replace(REN + "/", "Rendering/"))
    return out


def layer_of(rel):
    return rel.split("/")[0]


def type_map():
    m = {}
    for dst in targets():
        layer = layer_of(dst)
        path = resolve(dst)
        if layer not in LAYERS or path is None or not path.exists():
            continue
        for name in TYPE_RE.findall(path.read_text(encoding="utf-8")):
            m.setdefault(name, set()).add(LAYERS[layer])
    return m


def do_moves(dry):
    for src, dst in MOVES:
        s, d = EDITOR / src, EDITOR / dst
        if not s.exists():
            if d.exists():
                continue
            raise SystemExit("源文件缺失: " + src)
        if dry:
            continue
        d.parent.mkdir(parents=True, exist_ok=True)
        shutil.move(str(s), str(d))
        if pathlib.Path(str(s) + ".meta").exists():
            shutil.move(str(s) + ".meta", str(d) + ".meta")


def resolve(dst):
    """目标路径 -> 磁盘上实际存在的文件(预演时文件还在原处)。"""
    p = EDITOR / dst
    if p.exists():
        return p
    if dst.startswith("Rendering/"):
        p = EDITOR / REN / dst[len("Rendering/"):]
        if p.exists():
            return p
    for src, d in MOVES:
        if d == dst:
            return EDITOR / src
    return None


def rewrite(dry):
    tmap = type_map()
    changed = 0
    for dst in targets():
        layer = layer_of(dst)
        path = resolve(dst)
        if layer not in LAYERS or path is None or not path.exists():
            continue
        ns = LAYERS[layer]
        original = path.read_text(encoding="utf-8")
        text = re.sub(r"^namespace RD\.Draw\.Editor\s*$", "namespace " + ns, original, flags=re.M)

        needed = set()
        for name, namespaces in tmap.items():
            if ns in namespaces:
                continue
            if re.search(r"(?<![\w.])" + re.escape(name) + r"\b", text):
                needed |= namespaces - {ns}

        if needed:
            existing = re.findall(r"^using [^\n]+;\n", text, flags=re.M)
            missing = ["using %s;\n" % n for n in sorted(needed)
                       if "using %s;\n" % n not in existing]
            if missing:
                block = "".join(missing)
                if existing:
                    last = max(text.rindex(u) for u in existing)
                    cut = text.index("\n", last) + 1
                    text = text[:cut] + block + text[cut:]
                else:
                    m = re.search(r"^namespace ", text, flags=re.M)
                    text = text[:m.start()] + block + "\n" + text[m.start():]

        if text != original:
            changed += 1
            if not dry:
                path.write_text(text, encoding="utf-8")
    return changed


def prune(dry):
    for d in sorted(EDITOR.rglob("*"), key=lambda p: -len(p.parts)):
        if not d.is_dir() or d.name in LAYERS or d.name in ("Resources", "Scripts"):
            continue
        if any(d.iterdir()):
            continue
        print("  清理空目录", d.relative_to(PKG))
        if not dry:
            d.rmdir()
            meta = pathlib.Path(str(d) + ".meta")
            if meta.exists():
                meta.unlink()


if __name__ == "__main__":
    dry = "--dry" in sys.argv
    do_moves(dry)
    n = rewrite(dry)
    prune(dry)
    print("%s: 移动 %d 个文件, 重写 %d 个文件" % ("预演" if dry else "完成", len(MOVES), n))
