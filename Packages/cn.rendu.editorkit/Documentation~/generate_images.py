#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""生成 README 用的界面示意图。

这些图是**示意图**（不是截图）：用代码画出引擎的布局规则，好处是标注文字精确、
可随实现一起改、可重复生成，不会像手工截图那样过期。

用法:
    python3 -m venv .venv && .venv/bin/pip install Pillow
    .venv/bin/python "Documentation~/generate_images.py"

输出: Documentation~/images/*.png
"""

import os

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "images")

# 中文用冬青黑体（macOS 自带，与 Unity 中文界面风格接近），代码用等宽 Menlo
FONT_CJK = "/System/Library/Fonts/Hiragino Sans GB.ttc"
FONT_MONO = "/System/Library/Fonts/Menlo.ttc"

CANVAS = "#2b2b2b"
PANEL = "#383838"
BAR = "#454545"
FIELD = "#2f2f2f"
GROUP = "#3f3f3f"
BORDER = "#5a5a5a"
TEXT = "#e8e8e8"
LABEL = "#a8a8a8"
MUTED = "#8a8a8a"
CODE = "#9ecf9e"
ACCENT = "#3f6ea8"
ACCENT_HI = "#7fb0e8"
WARN = "#d8b45a"
DIM_TEXT = "#7a7a7a"

_FONTS = {}


def font(size, mono=False, bold=False):
    key = (size, mono, bold)
    if key in _FONTS:
        return _FONTS[key]
    path = FONT_MONO if mono else FONT_CJK
    index = 1 if (bold and not mono) else 0
    try:
        f = ImageFont.truetype(path, size, index=index)
    except Exception:
        f = ImageFont.truetype(path, size)
    _FONTS[key] = f
    return f


def canvas(w, h):
    im = Image.new("RGB", (w, h), CANVAS)
    return im, ImageDraw.Draw(im)


def txt(d, x, y, s, size=14, fill=TEXT, mono=False, bold=False, anchor="la"):
    d.text((x, y), s, font=font(size, mono, bold), fill=fill, anchor=anchor)


def box(d, x, y, w, h, fill=None, outline=None, r=3, width=1):
    d.rounded_rectangle((x, y, x + w, y + h), radius=r, fill=fill, outline=outline, width=width)


def title_of(d, x, y, s, size=20):
    txt(d, x, y, s, size, TEXT, bold=True, anchor="la")
    d.line((x, y + 30, x + 60, y + 30), fill=ACCENT_HI, width=3)


def badge(d, x, y, n, r=9, fill=WARN):
    d.ellipse((x - r, y - r, x + r, y + r), fill=fill)
    txt(d, x, y, str(n), 11, "#2b2b2b", bold=True, anchor="mm")


def legend(d, x, y, n, s, gap=36, size=12):
    """右侧图例：带序号圆圈 + 说明文字（自动换行到 2 行）。"""
    badge(d, x + 9, y + 8, n)
    txt(d, x + 26, y, s, size, LABEL, anchor="la")
    return y + gap


def field_row(d, x, y, w, label, value=None, lw=120, h=26, disabled=False, size=14,
              placeholder=False, badge_n=None, badge_x=None):
    txt(d, x, y + h / 2, label, size, DIM_TEXT if disabled else LABEL, anchor="lm")
    right = x + w
    if badge_n is not None and badge_x is not None:
        right = badge_x - 22
    if value is not None:
        box(d, x + lw, y, right - (x + lw), h, fill=FIELD, outline=BORDER)
        txt(d, x + lw + 8, y + h / 2, value, size,
            DIM_TEXT if disabled else (MUTED if placeholder else TEXT), anchor="lm")
    if badge_n is not None and badge_x is not None:
        badge(d, badge_x, y + h / 2, badge_n)
    return y + h + 7


def group_frame(d, x, y, w, h, title, size=13):
    box(d, x, y, w, h, fill=GROUP, outline=BORDER)
    txt(d, x + 10, y + 15, title, size, TEXT, bold=True, anchor="lm")


def button(d, x, y, w, s, h=28, hot=False):
    box(d, x, y, w, h, fill=ACCENT if not hot else "#4a7fbe", outline=ACCENT_HI, r=4)
    txt(d, x + w / 2, y + h / 2, s, 13, TEXT, anchor="mm")


def slider(d, x, y, w, label, ratio, value_text, lw=120, h=26, badge_n=None, badge_x=None):
    txt(d, x, y + h / 2, label, 14, LABEL, anchor="lm")
    right = x + w
    if badge_n is not None and badge_x is not None:
        right = badge_x - 22
    tx, tw = x + lw, right - (x + lw) - 52
    d.rectangle((tx, y + h / 2 - 2, tx + tw, y + h / 2 + 2), fill="#4a4a4a")
    d.rectangle((tx, y + h / 2 - 2, tx + ratio * tw, y + h / 2 + 2), fill=ACCENT_HI)
    d.ellipse((tx + ratio * tw - 6, y + h / 2 - 6, tx + ratio * tw + 6, y + h / 2 + 6), fill="#dcdcdc")
    box(d, right - 46, y, 46, h, fill=FIELD, outline=BORDER)
    txt(d, right - 23, y + h / 2, value_text, 13, TEXT, anchor="mm")
    if badge_n is not None and badge_x is not None:
        badge(d, badge_x, y + h / 2, badge_n)
    return y + h + 7


def footer(d, w, h, s="示意图 · 非实际截图"):
    txt(d, w - 12, h - 10, s, 11, "#6a6a6a", anchor="rs")


# ---------------------------------------------------------------- 1. 总览
def fig_overview():
    W, H = 1040, 700
    im, d = canvas(W, H)
    title_of(d, 40, 30, "检视器面板：一个类能画成什么样子")

    x, y, w = 40, 90, 660
    panel_h = 560
    box(d, x, y, w, panel_h, fill=PANEL, outline=BORDER)
    d.rectangle((x + 1, y + 1, x + w - 1, y + 34), fill=BAR)
    txt(d, x + 14, y + 18, "EnemyConfig (Script)", 15, TEXT, bold=True, anchor="lm")
    for i in range(3):
        d.ellipse((x + w - 30 + i * 8, y + 14, x + w - 26 + i * 8, y + 22), fill=MUTED)

    cy = y + 50
    bx = x + w - 34          # 序号标记的横坐标

    # InfoBox
    box(d, x + 14, cy, w - 28, 32, fill="#3d3620", outline="#6a5a2a")
    d.rectangle((x + 14, cy, x + 17, cy + 32), fill=WARN)
    txt(d, x + 26, cy + 16, "数据以 JSON 存在资产自身（.asset 内），无需旁挂文件", 13, "#e0cf9a", anchor="lm")
    badge(d, bx, cy + 16, 1)
    cy += 44

    cy = field_row(d, x + 14, cy, w - 28, "名字", "Slime", badge_n=2, badge_x=bx)

    # BoxGroup + Min/MaxValue → Slider
    gh = 108
    group_frame(d, x + 14, cy, w - 28, gh, "基础")
    gy = cy + 26
    gy = slider(d, x + 24, gy, w - 48, "血量", 0.62, "62", lw=110, badge_n=3, badge_x=bx)
    gy = field_row(d, x + 24, gy, w - 48, "攻击力", "12", lw=110, badge_n=4, badge_x=bx)
    cy += gh + 10

    # FoldoutGroup（折叠态）
    box(d, x + 14, cy, w - 28, 30, fill=GROUP, outline=BORDER)
    txt(d, x + 24, cy + 15, "▶", 11, LABEL, anchor="lm")
    txt(d, x + 42, cy + 15, "高级（折叠组，展开状态按实例记在会话里）", 13, LABEL, anchor="lm")
    badge(d, bx, cy + 15, 5)
    cy += 40

    # HorizontalGroup + 按钮
    gw = (w - 28 - 16) / 3
    for i, s in enumerate(["重置", "随机化", "导出 JSON"]):
        button(d, x + 14 + i * (gw + 8), cy, gw, s)
    badge(d, bx, cy + 14, 6)
    cy += 42

    # TableList
    th = 146
    box(d, x + 14, cy, w - 28, th, fill=GROUP, outline=BORDER)
    txt(d, x + 24, cy + 15, "掉落表", 13, TEXT, bold=True, anchor="lm")
    badge(d, bx, cy + 15, 7)
    ty = cy + 28
    cols = [70, 240, 130, 176]
    heads = ["索引", "物品", "权重", "备注"]
    d.rectangle((x + 22, ty, x + w - 22, ty + 26), fill="#4a4a4a")
    cx = x + 30
    for c, s in zip(cols, heads):
        txt(d, cx, ty + 13, s, 12, LABEL, anchor="lm")
        cx += c + 6
    for r in range(3):
        ry = ty + 26 + r * 26
        if r % 2 == 1:
            d.rectangle((x + 22, ry, x + w - 22, ry + 26), fill="#3a3a3a")
        vals = [[str(r), "金币", "100", "—"], [str(r), "药水", "30", "商店不卖"], [str(r), "宝石", "5", "稀有"]][r]
        cx = x + 30
        for c, s in zip(cols, vals):
            txt(d, cx, ry + 13, s, 12, TEXT, anchor="lm")
            cx += c + 6
    bar_y = ty + 26 + 3 * 26 + 6
    d.rectangle((x + 22, bar_y, x + 46, bar_y + 24), fill="#4a4a4a")
    txt(d, x + 34, bar_y + 12, "+", 14, TEXT, anchor="mm")
    d.rectangle((x + 50, bar_y, x + 74, bar_y + 24), fill="#4a4a4a")
    txt(d, x + 62, bar_y + 12, "−", 14, TEXT, anchor="mm")
    txt(d, x + w - 30, bar_y + 12, "1 / 3", 12, MUTED, anchor="rm")

    # 右侧图例
    lx, ly = 730, 100
    for n, s in [
        (1, "InfoBox 提示"),
        (2, "LabelText 改显示名"),
        (3, "MinValue + MaxValue → Slider"),
        (4, "属性/字段按声明顺序排"),
        (5, "折叠状态存会话（弱引用分桶）"),
        (6, "HorizontalGroup 让按钮排一行"),
        (7, "TableList：索引 / 工具栏 / 分页"),
    ]:
        ly = legend(d, lx, ly, n, s)

    ly += 14
    txt(d, lx, ly, "右列由 [PropertyOrder] 排序，", 12, MUTED, anchor="la")
    txt(d, lx, ly + 18, "分组由 GroupID 路径（\"父/子\"）表达。", 12, MUTED, anchor="la")

    footer(d, W, H)
    im.save(os.path.join(OUT, "01-overview.png"))
    return im.size


# ------------------------------------------------------- 2. 成员可见性规则
def fig_visibility():
    W, H = 1040, 620
    im, d = canvas(W, H)
    title_of(d, 40, 30, "引擎画什么：成员可见性规则")

    cols = [
        ("字段", [
            ("画", ["[ShowInInspector]", "[SerializeField]", "[InspectorSerialize]", "public 且非 static"]),
            ("不画", ["[HideInInspector]", "编译器生成字段", "声明类型在 UnityEngine /", "UnityEditor / mscorlib 里"]),
        ]),
        ("属性", [
            ("画", ["有 getter，且带", "[ShowInInspector]", "或 [InspectorSerialize]"]),
            ("不画", ["索引器", "只写不读的属性", "[HideInInspector]"]),
        ]),
        ("方法", [
            ("画", ["无参方法 + [Button]", "无参方法 + [ButtonGroup]", "[OnInspectorGUI] 插入自定义 GUI"]),
            ("不画", ["任何带参数的方法", "属性访问器 / 运算符", "（IsSpecialName）"]),
        ]),
    ]

    cw = (W - 80 - 2 * 20) / 3
    for i, (name, groups) in enumerate(cols):
        x = 40 + i * (cw + 20)
        box(d, x, 110, cw, 420, fill=PANEL, outline=BORDER)
        d.rectangle((x + 1, 111, x + cw - 1, 145), fill=BAR)
        txt(d, x + 14, 128, name, 16, TEXT, bold=True, anchor="lm")
        cy = 162
        for tag, items in groups:
            color = "#5c8f5c" if tag == "画" else "#8f5c5c"
            box(d, x + 14, cy, 42, 22, fill=color, r=4)
            txt(d, x + 35, cy + 11, tag, 12, "#ffffff", anchor="mm")
            cy += 30
            for it in items:
                mono = it.startswith("[") or it.startswith("public") or "IsSpecialName" in it
                txt(d, x + 18, cy, "· " + it, 13, CODE if mono else LABEL, mono=mono, anchor="la")
                cy += 24
            cy += 16

    fy = 552
    box(d, 40, fy, W - 80, 44, fill=GROUP, outline=BORDER)
    txt(d, 56, fy + 22, "顺序：先按 [PropertyOrder] 升序，同值时按声明顺序（字段 → 属性 → 方法）", 14, TEXT, anchor="lm")
    txt(d, 56 + 620, fy + 22, "private 字段必须挂特性才可见", 14, WARN, anchor="lm")

    footer(d, W, H)
    im.save(os.path.join(OUT, "02-visibility-rules.png"))
    return im.size


# ------------------------------------------------------------- 3. 分组效果
def fig_groups():
    W, H = 1040, 660
    im, d = canvas(W, H)
    title_of(d, 40, 30, "分组：六种容器与它们的样子")

    cw, ch = 300, 230
    gap = 20
    specs = [
        ("BoxGroup", "可折叠盒子容器"),
        ("FoldoutGroup", "默认收起，状态按实例记"),
        ("TitleGroup", "带横线的大标题"),
        ("VerticalGroup", "竖排（默认就是竖排）"),
        ("HorizontalGroup", "一行排开，Width 控宽"),
        ("TabGroup", "当前降级为垂直组"),
    ]

    for i, (name, note) in enumerate(specs):
        col, row = i % 3, i // 3
        x = 40 + col * (cw + gap)
        y = 100 + row * (ch + gap)
        box(d, x, y, cw, ch, fill=PANEL, outline=BORDER)
        d.rectangle((x + 1, y + 1, x + cw - 1, y + 32), fill=BAR)
        txt(d, x + 12, y + 17, "[" + name + "]", 13, CODE, mono=True, anchor="lm")
        txt(d, x + 12, y + 50, note, 12, MUTED, anchor="lm")

        if name == "BoxGroup":
            group_frame(d, x + 14, y + 68, cw - 28, 118, "基础")
            fy = y + 94
            fy = field_row(d, x + 24, fy, cw - 48, "血量", "100", lw=70, h=24, size=12)
            field_row(d, x + 24, fy, cw - 48, "攻击", "12", lw=70, h=24, size=12)
        elif name == "FoldoutGroup":
            box(d, x + 14, y + 68, cw - 28, 30, fill=GROUP, outline=BORDER)
            txt(d, x + 24, y + 83, "▶", 11, LABEL, anchor="lm")
            txt(d, x + 42, y + 83, "高级", 13, TEXT, anchor="lm")
            box(d, x + 14, y + 106, cw - 28, 80, fill="#343434", outline="#4a4a4a")
            txt(d, x + 28, y + 130, "（展开后子成员画在这里）", 12, MUTED, anchor="lm")
        elif name == "TitleGroup":
            txt(d, x + 14, y + 86, "外观设置", 15, TEXT, bold=True, anchor="lm")
            d.line((x + 14, y + 100, x + cw - 14, y + 100), fill=BORDER, width=2)
            fy = y + 112
            fy = field_row(d, x + 14, fy, cw - 28, "颜色", "", lw=70, h=24, size=12)
            field_row(d, x + 14, fy, cw - 28, "缩放", "1.0", lw=70, h=24, size=12)
        elif name == "VerticalGroup":
            fy = y + 74
            for i2, (lb, v) in enumerate([("A", "1"), ("B", "2"), ("C", "3")]):
                fy = field_row(d, x + 14, fy, cw - 28, lb, v, lw=70, h=24, size=12)
            txt(d, x + 14, y + 180, "组本身不画框，只影响布局", 12, MUTED, anchor="lm")
        elif name == "HorizontalGroup":
            for i2, s in enumerate(["A", "B", "C"]):
                box(d, x + 14 + i2 * 92, y + 84, 84, 26, fill=FIELD, outline=BORDER)
                txt(d, x + 14 + i2 * 92 + 42, y + 97, "字段" + s, 12, TEXT, anchor="mm")
            txt(d, x + 14, y + 132, "[HorizontalGroup(\"row\", Width = 84)]", 11, CODE, mono=True, anchor="lm")
            txt(d, x + 14, y + 156, "按钮也能进同一行", 12, MUTED, anchor="lm")
        elif name == "TabGroup":
            for i2, s in enumerate(["第一页", "第二页"]):
                hot = i2 == 0
                box(d, x + 14 + i2 * 84, y + 70, 80, 26, fill=BAR if hot else GROUP, outline=BORDER)
                txt(d, x + 14 + i2 * 84 + 40, y + 83, s, 12, TEXT if hot else MUTED, anchor="mm")
            box(d, x + 14, y + 96, cw - 28, 76, fill="#343434", outline="#4a4a4a")
            txt(d, x + 28, y + 120, "实际按垂直组绘制：", 12, WARN, anchor="lm")
            txt(d, x + 28, y + 142, "两页的内容会依次排下来", 12, MUTED, anchor="lm")

    footer(d, W, H)
    im.save(os.path.join(OUT, "03-groups.png"))
    return im.size


# ----------------------------------------------------------- 4. 条件显隐
def fig_conditions():
    W, H = 1000, 580
    im, d = canvas(W, H)
    title_of(d, 40, 30, "条件：显隐与启禁的四种写法")

    rows = [
        ("[ShowIf(\"IsBoss\")]", "条件为真 → 显示", "条件为假 → 整行消失", "show"),
        ("[HideIf(\"IsLocked\")]", "条件为真 → 整行消失", "条件为假 → 显示", "hide"),
        ("[EnableIf(\"CanEdit\")]", "条件为真 → 可编辑", "条件为假 → 置灰（仍可见）", "enable"),
        ("[DisableIf(\"ReadOnly\")]", "条件为真 → 置灰（仍可见）", "条件为假 → 可编辑", "disable"),
    ]

    y = 104
    for code, t1, t2, kind in rows:
        box(d, 40, y, W - 80, 88, fill=PANEL, outline=BORDER)
        txt(d, 58, y + 26, code, 14, CODE, mono=True, anchor="lm")
        txt(d, 58, y + 54, t1 + "   |   " + t2, 12, MUTED, anchor="lm")

        # 三种视觉状态：正常 / 消失（虚框）/ 置灰
        cx = 470
        for slot, state in enumerate([("显示", "normal"), ("置灰", "dim"), ("消失", "gone")]):
            sx = cx + slot * 160
            if state[1] == "gone":
                for seg in range(10):
                    d.line((sx + seg * 14, y + 44, sx + seg * 14 + 7, y + 44), fill="#5a5a5a", width=1)
                txt(d, sx, y + 62, "不绘制（不留空位）", 11, "#7a7a7a", anchor="lm")
            else:
                dim = state[1] == "dim"
                txt(d, sx, y + 44, "攻击力", 12, DIM_TEXT if dim else LABEL, anchor="lm")
                box(d, sx + 60, y + 32, 84, 24, fill="#333333" if dim else FIELD, outline=BORDER)
                txt(d, sx + 102, y + 44, "12", 12, DIM_TEXT if dim else TEXT, anchor="mm")
                txt(d, sx, y + 62, state[0], 11, DIM_TEXT if dim else MUTED, anchor="lm")
        y += 100

    box(d, 40, y, W - 80, 52, fill=GROUP, outline=BORDER)
    txt(d, 56, y + 26, "成员名既可以是普通成员/无参方法名，也可以是 @ 表达式；解析不到时视为可见并 WarnOnce 一次",
        13, TEXT, anchor="lm")
    footer(d, W, H)
    im.save(os.path.join(OUT, "04-conditions.png"))
    return im.size


# ------------------------------------------------------------- 5. 表达式
def fig_expressions():
    W, H = 1040, 660
    im, d = canvas(W, H)
    title_of(d, 40, 30, "表达式：@ 语法与求值管线")

    # 管线
    blocks = [
        ("\"@Count > 0\"", "特性里的字符串", CODE),
        ("IsExpression", "以 @ 开头才算表达式\n否则按成员名反射", TEXT),
        ("GetAst", "词法 + 语法，只做一次\n按源码字符串缓存", TEXT),
        ("Evaluate", "每次求值都重读成员 / 重调方法\n所以 @Count 跟着字段变", TEXT),
        ("Truthy", "null→false、bool 原值、\n字符串非空、集合 Count>0、其它→true", TEXT),
    ]
    bw, gap = 178, 22
    x = 40
    y = 104
    for i, (name, note, color) in enumerate(blocks):
        box(d, x, y, bw, 96, fill=PANEL, outline=BORDER)
        d.rectangle((x + 1, y + 1, x + bw - 1, y + 30), fill=BAR)
        txt(d, x + 10, y + 16, name, 12, color, mono=True, anchor="lm")
        for j, line in enumerate(note.split("\n")):
            txt(d, x + 10, y + 48 + j * 18, line, 11, MUTED, anchor="lm")
        if i < len(blocks) - 1:
            ax = x + bw + 3
            d.line((ax, y + 48, ax + gap - 8, y + 48), fill=ACCENT_HI, width=2)
            d.polygon([(ax + gap - 8, y + 44), (ax + gap, y + 48), (ax + gap - 8, y + 52)], fill=ACCENT_HI)
        x += bw + gap

    # 缓存说明
    y2 = 224
    box(d, 40, y2, 520, 92, fill=GROUP, outline=BORDER)
    txt(d, 56, y2 + 22, "缓存与容错", 13, TEXT, bold=True, anchor="lm")
    txt(d, 56, y2 + 46, "解析失败也缓存（负缓存）：写错的表达式只报一次警告", 12, LABEL, anchor="lm")
    txt(d, 56, y2 + 66, "上限 512 条，超出整体清空；求值异常只记警告并返回 null", 12, LABEL, anchor="lm")

    box(d, 580, y2, W - 620, 92, fill=GROUP, outline=BORDER)
    txt(d, 596, y2 + 22, "求值不短路", 13, WARN, anchor="lm")
    txt(d, 596, y2 + 46, "@A() && B() 两边都会执行；", 12, LABEL, anchor="lm")
    txt(d, 596, y2 + 66, "有副作用的方法别写进条件里", 12, LABEL, anchor="lm")

    # 上下文变量
    y3 = 340
    box(d, 40, y3, W - 80, 236, fill=PANEL, outline=BORDER)
    d.rectangle((40, y3, W - 40, y3 + 34), fill=BAR)
    txt(d, 56, y3 + 17, "上下文变量与运算符", 14, TEXT, bold=True, anchor="lm")

    left = [
        ("$value", "当前成员的值（可写：@$value = $value + 1）"),
        ("$property.Value", "同 $value"),
        ("$property.State.Expanded", "折叠状态（可读写）"),
        ("$property.State.Visible", "可见状态（可读写）"),
        ("$root / this", "当前被绘制的对象"),
        ("Foo", "先找当前对象的成员，找不到再当全局类型短名"),
    ]
    cy = y3 + 48
    for k, v in left:
        txt(d, 56, cy, k, 12, CODE, mono=True, anchor="lm")
        txt(d, 230, cy, v, 12, LABEL, anchor="lm")
        cy += 26

    ops = [
        ("=", "赋值（$value 或链末端成员）"),
        ("? :", "三目"),
        ("|| &&", "逻辑（不短路）"),
        ("== !=", "相等（数值相对容差 1e-6）"),
        ("< <= > >=", "关系"),
        ("+ -", "加减（含字符串拼接）"),
        ("* / %", "乘除模（除数为 0 得 0）"),
        ("! -", "一元"),
        ("() . 方法调用", "后缀"),
    ]
    txt(d, 640, y3 + 48, "优先级（低 → 高）", 12, TEXT, anchor="lm")
    cy = y3 + 72
    for sym, desc in ops:
        txt(d, 640, cy, sym, 11, CODE, mono=True, anchor="lm")
        txt(d, 735, cy, desc, 11, LABEL, anchor="lm")
        cy += 22

    footer(d, W, H)
    im.save(os.path.join(OUT, "05-expressions.png"))
    return im.size


# --------------------------------------------------------------- 6. 架构
def fig_architecture():
    W, H = 1040, 620
    im, d = canvas(W, H)
    title_of(d, 40, 30, "包结构：三层，业务零耦合")

    # Runtime
    box(d, 40, 104, W - 80, 118, fill=PANEL, outline=BORDER)
    d.rectangle((40, 104, W - 40, 138), fill=BAR)
    txt(d, 56, 121, "Runtime / RD.EditorKit", 14, TEXT, bold=True, anchor="lm")
    txt(d, W - 60, 121, "autoReferenced = true，可被 Runtime 代码引用", 12, MUTED, anchor="rm")
    items = ["LabelText", "ShowIf / HideIf", "BoxGroup / TabGroup", "Button / InlineButton",
             "TableList", "ValueDropdown", "ShowInInspector", "InspectorSerialize"]
    for i, s in enumerate(items):
        cx = 56 + (i % 4) * 234
        cy = 152 + (i // 4) * 32
        box(d, cx, cy, 218, 26, fill=FIELD, outline="#4a4a4a")
        txt(d, cx + 10, cy + 13, s, 12, CODE, mono=True, anchor="lm")

    # 箭头
    d.line((W / 2, 222, W / 2, 262), fill=ACCENT_HI, width=2)
    d.polygon([(W / 2 - 5, 258), (W / 2, 268), (W / 2 + 5, 258)], fill=ACCENT_HI)
    txt(d, W / 2 + 12, 242, "引用（单向）", 12, MUTED, anchor="lm")

    # Editor
    box(d, 40, 268, W - 80, 146, fill=PANEL, outline=BORDER)
    d.rectangle((40, 268, W - 40, 302), fill=BAR)
    txt(d, 56, 285, "Editor / RD.EditorKit.Editor", 14, TEXT, bold=True, anchor="lm")
    txt(d, W - 60, 285, "Editor-only，不 using 任何业务类型", 12, WARN, anchor="rm")
    mods = [
        ("Engine", "反射收集成员 + 绘制（唯一入口 DrawObject）"),
        ("InspectorExpression", "表达式求值（含 AST 缓存）"),
        ("Serialization", "JSON 序列化与读回"),
        ("EditorWindowBase", "窗口基类（含左树右面板）"),
        ("HostSession / Log", "会话状态 / 日志出口"),
    ]
    cy = 312
    for k, v in mods:
        txt(d, 56, cy + 11, k, 12, CODE, mono=True, anchor="lm")
        txt(d, 300, cy + 11, v, 12, LABEL, anchor="lm")
        cy += 20

    # 宿主
    hy = 452
    hosts = [
        ("InspectorEditor", "给 MonoBehaviour /", "ScriptableObject 用"),
        ("InspectorSerialized\nScriptableObject", "编辑器数据资产", "（自带整族检视器）"),
        ("EditorWindowBase", "窗口自绘", "（工具窗口）"),
        ("MenuEditorWindow", "左菜单树 +", "右内容面板"),
    ]
    hw = (W - 80 - 3 * 16) / 4
    for i, (name, a, b) in enumerate(hosts):
        x = 40 + i * (hw + 16)
        box(d, x, hy, hw, 116, fill=GROUP, outline=BORDER)
        first = name.split("\n")[0]
        txt(d, x + 12, hy + 24, first, 12, TEXT, bold=True, anchor="lm")
        if "\n" in name:
            txt(d, x + 12, hy + 44, name.split("\n")[1], 12, TEXT, bold=True, anchor="lm")
            txt(d, x + 12, hy + 70, a, 11, MUTED, anchor="lm")
            txt(d, x + 12, hy + 88, b, 11, MUTED, anchor="lm")
        else:
            txt(d, x + 12, hy + 48, a, 11, MUTED, anchor="lm")
            txt(d, x + 12, hy + 66, b, 11, MUTED, anchor="lm")

    footer(d, W, H)
    im.save(os.path.join(OUT, "06-architecture.png"))
    return im.size


# ----------------------------------------------------------- 7. 菜单树窗口
def fig_menu_window():
    W, H = 1040, 620
    im, d = canvas(W, H)
    title_of(d, 40, 30, "菜单树窗口：左树 + 右面板")

    x, y, w, h = 40, 104, 560, 440
    box(d, x, y, w, h, fill=PANEL, outline=BORDER)
    d.rectangle((x + 1, y + 1, x + w - 1, y + 32), fill=BAR)
    txt(d, x + 12, y + 16, "My Tool", 14, TEXT, bold=True, anchor="lm")

    # 左树
    tx = x + 8
    tree = [
        (0, "全局设置", False, True),
        (0, "模块A", True, False),
        (1, "子项 A1", False, False),
        (1, "子项 A2", False, False),
        (0, "模块B", True, False),
        (0, "资源", True, False),
    ]
    ty = y + 42
    for depth, name, folder, sel in tree:
        if sel:
            d.rectangle((tx, ty, tx + 176, ty + 24), fill=ACCENT)
        icon = "▾" if folder else "·"
        txt(d, tx + 8 + depth * 16, ty + 12, icon + " " + name, 12, TEXT if sel else LABEL, anchor="lm")
        ty += 26

    # 分割条
    split = x + 200
    d.rectangle((split, y + 34, split + 4, y + h - 2), fill="#4a4a4a")
    for i in range(3):
        d.line((split - 2, y + 200 + i * 6, split + 6, y + 200 + i * 6), fill=MUTED, width=1)

    # 右面板
    rx = split + 16
    txt(d, rx, y + 52, "模块A", 16, TEXT, bold=True, anchor="lm")
    d.line((rx, y + 68, rx + 280, y + 68), fill=BORDER, width=2)
    ry = y + 84
    ry = field_row(d, rx, ry, 328, "启用", "true", lw=90, h=26, size=13)
    ry = field_row(d, rx, ry, 328, "备注", "调试用", lw=90, h=26, size=13)
    gw = (328 - 8) / 2
    button(d, rx, ry, gw, "执行", hot=True)
    button(d, rx + gw + 8, ry, gw, "重置")
    ry += 40
    box(d, rx, ry, 328, 30, fill="#3d3620", outline="#6a5a2a")
    d.rectangle((rx, ry, rx + 3, ry + 30), fill=WARN)
    txt(d, rx + 12, ry + 15, "面板内容用 DrawObject 画：任意 C# 对象都行", 11, "#e0cf9a", anchor="lm")

    # 右侧图例
    lx = 660
    ly = 120
    for n, s in [
        (1, "BuildMenuTree() 建树：路径写法"),
        (2, "“父/子”自动补出父级文件夹"),
        (3, "选中项变化 → SelectionChanged"),
        (4, "右面板画当前选中项的 Value"),
        (5, "MenuWidth 可拖拽，默认 ≥150"),
        (6, "选中项/宽度要持久化请用 *Prefs"),
    ]:
        ly = legend(d, lx, ly, n, s, gap=40)

    ly += 10
    txt(d, lx, ly, "值可以是任何对象：", 12, MUTED, anchor="la")
    txt(d, lx, ly + 18, "tree.Add(\"Global\", new GlobalSettings(), icon)", 12, CODE, mono=True, anchor="la")
    txt(d, lx, ly + 40, "本包不提供任何菜单入口，", 12, MUTED, anchor="la")
    txt(d, lx, ly + 58, "窗口入口由使用方自己挂 [MenuItem]。", 12, MUTED, anchor="la")

    footer(d, W, H)
    im.save(os.path.join(OUT, "07-menu-window.png"))
    return im.size


def main():
    os.makedirs(OUT, exist_ok=True)
    for fn in [fig_overview, fig_visibility, fig_groups, fig_conditions,
               fig_expressions, fig_architecture, fig_menu_window]:
        size = fn()
        print("%-24s %s" % (fn.__name__, "%dx%d" % size))


if __name__ == "__main__":
    main()
