using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;
using RD.Draw.Editor.Tables;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 集合渲染: 列表 / 字典 / 字符串集合。
    ///
    /// "一个容器成员怎么画"由 <see cref="ContainerRenderer"/> 决定;
    /// 本类负责"容器里那些东西怎么画"。
    ///
    /// 写集合一律经 <see cref="CollectionMutator"/>, 本类不直接改集合。
    ///
    /// 产树约定: 所有控件都经 <see cref="WidgetBuilder"/> 产出节点, 一帧里一次都不碰后端;
    /// "点了 × / 敲了字 / 拖了一下"这类结果由解释器渲染后回填, 由 <see cref="ChangeApplier"/>
    /// 统一写回, 循环不再中途退出。
    /// </summary>
    internal sealed class CollectionRenderer
    {
        private readonly DrawContext m_Ctx;
        private readonly WidgetBuilder m_Builder;
        private readonly ValueAccessor m_Values;
        private readonly ILabelFormatter m_Labels;
        private readonly ITypeShape m_Shape;
        private readonly CollectionMutator m_Mutator;
        private readonly TableRenderer m_Tables;
        private readonly ListPager m_Pager;
        private readonly IMemberDrawHost m_Drawer;

        public CollectionRenderer(DrawContext ctx, WidgetBuilder builder, ValueAccessor values, ILabelFormatter labels,
                                  ITypeShape shape, CollectionMutator editor, TableRenderer tables, ListPager pager,
                                  IMemberDrawHost drawer)
        {
            m_Ctx    = ctx;
            m_Builder = builder;
            m_Values = values;
            m_Labels = labels;
            m_Shape  = shape;
            m_Mutator = editor;
            m_Tables = tables;
            m_Pager  = pager;
            m_Drawer = drawer;
        }

        //-------------------------------------- 字符串集合 --------------------------------------

        /// <summary>字符串集合(HashSet&lt;string&gt; 之类): 逐条列出, 不可编辑。</summary>
        public void RenderStringSet(object target, CachedMember member, IEnumerable collection)
        {
            var count = 0;
            foreach (var _ in collection) count++;

            m_Builder.BeginVertical(true);
            if (m_Labels.ShouldDrawLabel(member))
            {
                m_Builder.Label(m_Labels.CountedLabel(member, count), RDTheme.CountedHeader);
            }

            foreach (var item in collection)
            {
                m_Builder.KeyValueLabel("", item?.ToString() ?? "null");
            }

            m_Builder.EndVertical();
        }

        //-------------------------------------- 字典 --------------------------------------

        /// <summary>字典: 折叠头 + 逐键值对(可编辑/可删除)。</summary>
        public void RenderDictionary(object target, CachedMember member, IDictionary dict)
        {
            var readOnly = member.GetAttr<ReadOnlyAttribute>() != null;
            var settings = member.GetAttr<DictionaryDrawerSettingsAttribute>();
            if (settings != null && settings.IsReadOnly) readOnly = true;

            m_Builder.BeginVertical(true);

            // 折叠头的"读→画→比较→写回→重绘"见 CollapseState: 四处容器共用同一份实现。
            var expanded = CollapseState.Build(m_Ctx, target, member.RawName, true,
                                               m_Labels.CountedLabel(member, dict.Count), m_Builder);

            if (expanded)
            {
                var keys = new List<object>();
                foreach (var k in dict.Keys) keys.Add(k);

                foreach (var k in keys)
                {
                    var v = dict[k];
                    m_Builder.BeginHorizontal();
                    var keyText = k?.ToString() ?? "";

                    if (readOnly)
                    {
                        // 只读分支画的是"能编辑但不会写回"的输入框: 与改造前一致(返回值本来就被丢掉了)
                        if (v != null && m_Shape.IsScalarLike(v.GetType()))
                        {
                            m_Builder.TextField(keyText, v.ToString(), 0f, null);
                        }
                        else
                        {
                            m_Builder.KeyValueLabel(keyText, v?.ToString() ?? "null");
                        }
                    }
                    else
                    {
                        m_Builder.Button("-", RemoveButtonSpec, () =>
                        {
                            m_Mutator.RemoveKey(target, dict, k);
                            m_Ctx.Driver.RequestRepaint();
                        });

                        if (v != null && m_Shape.IsScalarLike(v.GetType()))
                        {
                            var oldText = v.ToString();
                            m_Builder.TextField(keyText, oldText, 0f, newText =>
                            {
                                var parsed = ScalarParser.Parse(v.GetType(), newText);
                                if (parsed != null && Equals(parsed, v) == false)
                                {
                                    m_Mutator.SetKey(target, dict, k, parsed);
                                }
                            });
                        }
                        else
                        {
                            m_Builder.KeyValueLabel(keyText, v?.ToString() ?? "null");
                        }
                    }

                    m_Builder.EndHorizontal();
                }
            }

            m_Builder.EndVertical();
        }

        /// <summary>字典键值对的删除按钮: 定宽 22, 不参与水平撑开。</summary>
        private static readonly DrawButtonSpec RemoveButtonSpec =
            DrawButtonSpec.Of(width: 22, expandWidth: false);

        //-------------------------------------- 列表 --------------------------------------

        /// <summary>列表绘制入口: 带 [TableList] 且元素类型有可表格化成员的走表格, 否则走普通列表。</summary>
        public void RenderList(object target, CachedMember member, IList list, EditorWindow host, int depth)
        {
            var table = member.GetAttr<TableListAttribute>();
            if (table != null)
            {
                var elementType = m_Shape.GetElementType(member.ValueType);
                var tableColumns = elementType != null ? m_Tables.GetColumns(elementType) : null;
                if (tableColumns != null && tableColumns.Count > 0)
                {
                    m_Tables.RenderTable(target, member, list, host, depth, table, tableColumns);
                    return;
                }
            }

            DrawPlainList(target, member, list, host, depth);
        }

        private void DrawPlainList(object target, CachedMember member, IList list, EditorWindow host, int depth)
        {
            // Unity 对象列表: 只读列表展示
            if (list.Count > 0 && list[0] is UnityEngine.Object)
            {
                m_Builder.BeginVertical(true);
                if (m_Labels.ShouldDrawLabel(member))
                {
                    m_Builder.Label(m_Labels.CountedLabel(member, list.Count), RDTheme.CountedHeader);
                }
                // 下标循环: foreach 一个 IList 会走 IEnumerable.GetEnumerator(), 每帧装箱一次枚举器
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item is UnityEngine.Object uo)
                    {
                        m_Builder.ObjectField(uo.name, uo, uo.GetType());
                    }
                    else
                    {
                        m_Builder.KeyValueLabel("", item?.ToString() ?? "null");
                    }
                }

                m_Builder.EndVertical();
                return;
            }

            var listSettings = member.GetAttr<ListDrawerSettingsAttribute>();
            var readOnly = member.GetAttr<ReadOnlyAttribute>() != null;
            if (listSettings != null && listSettings.IsReadOnly) readOnly = true;

            // 拖拽排序: 只读列表不给拖, [ListDrawerSettings(DraggableItems = false)] 也不给 ——
            // 不给的时候连手柄都不画: 画一条按不动的假手柄, 比没有更让人困惑。
            // 驱动抽象删除后只剩 Driver(支持排序), "后端没有拖拽能力"这分支已不存在。
            var draggable = readOnly == false
                            && (listSettings == null || listSettings.DraggableItems);
            var listId    = draggable ? ReorderId(member, list) : 0;

            // 默认展开态: [ListDrawerSettings(DefaultExpandedState = true)] 与 FoldoutGroup.Expanded 同一语义。
            //
            // 它只能当"第一次看到时的默认值", 不能与当前折叠态用 || 拼 —— 那样默认展开的列表
            // 永远收不起来: 点了三角写进会话的 false, 下一帧又被这里拉回 true, 箭头弹回 ▼。
            var defaultExpanded = listSettings == null || listSettings.DefaultExpandedState;
            var expanded        = m_Ctx.Session.GetToggle(target, member.RawName, defaultExpanded);

            // 分页: 元素数超过 NumberOfItemsPerPage(默认 15) 自动启用分页器。
            // 计算提前到头部之前: 分页器要内联进"▼ N items ◀ 1/2 ▶ +"头那一行(观感要求), 不是列表下方。
            //
            // "展开全部"是另一档开关(头右侧那枚 ▲/▼): 打开时整列按实际条数铺开, 分页器随之退场。
            // 它与折叠态分开存(键加后缀) —— 收起列表不该把"全部显示"也一起清掉。
            var pageSize     = listSettings != null && listSettings.NumberOfItemsPerPage > 0 ? listSettings.NumberOfItemsPerPage : 15;
            var overflows    = listSettings != null && list.Count > pageSize;
            var expandAll    = overflows && m_Ctx.Session.GetToggle(target, ExpandAllKey(member), false);
            var pageEnabled  = listSettings != null && (listSettings.ShowPaging || overflows) && expandAll == false;

            // 页数/页索引/本页区间: 与表格共用同一份计算(边界不变式见 PagingRange)。
            var page      = PagingRange.Compute(list.Count, pageSize, pageEnabled,
                                                m_Ctx.Session.GetPage(target, member.RawName));
            var pageCount = page.PageCount;
            var pageIndex = page.PageIndex;
            var start     = page.Start;
            var end       = page.End;

            m_Builder.BeginVertical(true);

            // 列表头(观感要求): [折叠三角 计数] ‖ [◀ [页] / 总页 ▶] ……… [▲] [+]
            //
            // 左边读"这是什么、有几条": 折叠三角后面紧跟计数(参考界面就是 "▼ 20 items"),
            // 计数不再飘到最右 —— 那样读起来像"最右那个数字属于加号"。
            // 分页器与计数之间画一条 1px 竖线(同一件事的两个面); 翻页键无底纹,
            // 行尾的 ▲(展开全部)带框 —— 它换的是显示范围, 不是页码。
            var headerName = m_Labels.LabelOrEmpty(member)?.Trim();
            var unnamed    = string.IsNullOrEmpty(headerName);

            // 整行按"单行控件高度"钉齐: 计数与页码是文字(自然高只有一行字), 翻页键/三角/加号是按钮,
            // 不钉成同一高度时各自贴着自己那条边, 一排看过去就是"七上八下"。
            var rowHeight = m_Ctx.Driver.LineHeight;
            var countSpec = DrawTextSpec.Of(mini: true, height: rowHeight);

            m_Builder.BeginHorizontal();

            if (list.Count == 0)
            {
                // 空列表不画折叠三角: 没有可折叠的内容, 三角点了也不会有任何变化。
                // 折叠状态不动: 加进第一条后面折叠头回来, 仍是用户上次的展开态。
                if (unnamed == false)
                {
                    // 名字的字体与折叠头一致(非加粗), 只是少了前面那枚三角
                    m_Builder.Label(headerName, DrawTextSpec.Of(height: rowHeight));
                    DrawToolbarSeparator();
                }
            }
            else
            {
                expanded = CollapseState.Build(m_Ctx, target, member.RawName, defaultExpanded,
                                               unnamed ? "" : headerName, m_Builder);
                DrawToolbarSeparator();
            }

            // 计数段: 段后跟 FlexibleSpace, 由它吃掉整行余量 —— 右侧各段才能一路贴到行尾。
            // 参考界面就是这么一条"被竖线切开的分段栏", 而不是左边挤一团、右边孤立两枚。
            m_Builder.Label(m_Labels.CountedText(list.Count), countSpec);
            m_Builder.FlexibleSpace();

            // 分页器: ◀ [页] / 总数 ▶, 自成一段
            if (pageEnabled && pageCount > 1)
            {
                DrawToolbarSeparator();
                m_Pager.Build(m_Builder, target, member.RawName, pageIndex, pageCount);
            }

            // 展开/收起全部: 超过一页才有意义(否则本来就是一屏)。
            // 图标即当前状态: ▲ = 现在按每页条数收着, ▼ = 已经按实际条数铺开。
            if (overflows)
            {
                DrawToolbarSeparator();
                var spec = RDTheme.ExpandButton.WithHeight(rowHeight);
                spec.Tooltip = expandAll ? $"收起为每页 {pageSize} 条" : $"展开显示全部 {list.Count} 条";
                m_Builder.Button(expandAll ? "▼" : "▲", spec, () =>
                {
                    m_Ctx.Session.SetToggle(target, ExpandAllKey(member), expandAll == false);
                    m_Ctx.Driver.RequestRepaint();
                });
            }

            if (readOnly == false)
            {
                DrawToolbarSeparator();
                m_Builder.Button("+", RDTheme.AddButton.WithHeight(rowHeight), () =>
                {
                    m_Mutator.AddElement(target, member, list, listSettings);
                    m_Ctx.Driver.RequestRepaint();
                });
            }

            m_Builder.EndHorizontal();

            if (expanded)
            {
                // 头与体之间一条淡线: 参考界面的工具条下面有一道横向切分, 没有它时
                // "1 items" 与第一条依赖糊在一起, 像两行数据。
                m_Builder.Separator(1f, RDTheme.ListItemLineColor);
                m_Builder.Space(2f);

                // 先把本轮要画的条目清干净: 同 id 的上一轮区间不能混进来(嵌套列表尤其如此)
                if (draggable) m_Builder.BeginReorder(listId);

                for (var i = start; i < end; i++)
                {
                    // 闭包要按下标捕获: for 的 i 是共享变量, 直接用会全部指到最后一条
                    var index   = i;
                    var element = list[index];
                    var card    = false;

                    // 条目区间的登记必须在"条目画完"之后(见 ReorderItemWidget), 因此包一层节点
                    if (draggable) m_Builder.BeginReorderItem(listId, index);

                    m_Builder.BeginVertical(false);

                    if (element == null)
                    {
                        // 用户在选择器里选了 "None" —— 这确实是一条元素, 但必须能删掉,
                        // 否则加进去就成了删不掉的死数据。
                        m_Builder.BeginHorizontal();
                        DrawListDragHandle(draggable, listId, index, GhostText(element));
                        m_Builder.KeyValueLabel(IndexText.Of(index), "None");
                        if (readOnly == false)
                        {
                            m_Builder.Button("×", RDTheme.ItemRemoveButton, () =>
                            {
                                m_Mutator.RemoveAt(target, list, index);
                                m_Ctx.Driver.RequestRepaint();
                            });
                        }

                        m_Builder.EndHorizontal();
                    }
                    else if (m_Shape.IsScalarLike(element.GetType()))
                    {
                        m_Builder.BeginHorizontal();
                        DrawListDragHandle(draggable, listId, index, GhostText(element));

                        var oldText = element.ToString();
                        m_Builder.TextField(IndexText.Of(index), oldText, 0f, newText =>
                        {
                            if (readOnly) return;

                            var parsed = ScalarParser.Parse(element.GetType(), newText);
                            if (parsed != null && Equals(parsed, element) == false)
                            {
                                m_Mutator.SetAt(target, list, index, parsed);
                            }
                        });

                        if (readOnly == false)
                        {
                            m_Builder.Button("×", RDTheme.ItemRemoveButton, () =>
                            {
                                m_Mutator.RemoveAt(target, list, index);
                                m_Ctx.Driver.RequestRepaint();
                            });
                        }

                        m_Builder.EndHorizontal();
                    }
                    else
                    {
                        // 观感要求: 条目 = 一条通栏卡片[拖拽手柄][成员竖排][删除键]
                        //   成员(名称/版本/同步)竖向堆叠, 各自占满成员块的宽 ——
                        //   "名称 + 完整包名"要完整可见, "同步"是通栏按钮;
                        //   卡片是浅一档的底 + 一条下边线(参考界面就是一格一格的横带), 删除键在带内右端 ——
                        //   放到带外要多占 16px, 值框正好差这一截(包名被截断)。
                        //   删除键顶对齐首条成员行, 不另起一行, 否则每个条目多出一行高度。
                        //
                        // 成员块必须 expandWidth: 横向组里它既无 min 也无 max,
                        // 不撑开就会被压成 0 宽(值框被砍没)。
                        card = true;
                        if (card) m_Builder.BeginCard();

                        m_Builder.BeginHorizontal();

                        DrawListDragHandle(draggable, listId, index, GhostText(element));

                        m_Builder.BeginVertical(false, 0f, false, true);
                        m_Builder.PushLabelWidth(NestedItemLabelWidth);
                        try
                        {
                            m_Drawer.DrawNested(element, host, depth + 1, target, false);
                        }
                        finally
                        {
                            m_Builder.PopScope();
                        }

                        m_Builder.EndVertical();

                        if (readOnly == false)
                        {
                            // 删除键: 一枚 × 字形(无底纹), 定宽 16 —— 其余横向空间全部留给值框。
                            m_Builder.BeginVertical(false, 0f);
                            m_Builder.Button("×", RDTheme.ItemRemoveButton, () =>
                            {
                                m_Mutator.RemoveAt(target, list, index);
                                m_Ctx.Driver.RequestRepaint();
                            });
                            m_Builder.EndVertical();
                        }

                        m_Builder.EndHorizontal();

                        if (card) m_Builder.EndCard();
                    }

                    m_Builder.EndVertical();

                    if (draggable) m_Builder.EndReorderItem();

                    // 条目之间一条淡线: 只留空隙时整列读起来是一块, 分不清哪一行属于哪一条。
                    // 最后一条后面不画(否则线压在列表底边上)。
                    //
                    // 卡片自带边框时只留 2px 空: 边框 + 分隔线并排就是两条线, 比参考界面厚一倍。
                    if (index + 1 < end)
                    {
                        if (card) m_Builder.Space(2f);
                        else DrawItemSeparator();
                    }
                }

                // 结果要等所有条目都画完才问得出来: 落点得先有全部条目的区间。
                // 松手那一帧在这里写数据(经 Mutate → 宿主拍 Undo 快照), 下一帧就是新顺序。
                if (draggable)
                {
                    m_Builder.EndReorder(drag =>
                    {
                        if (drag.Dropped && drag.Target >= 0)
                        {
                            m_Mutator.MoveElement(target, list, drag.Source, drag.Target);
                            m_Ctx.Driver.RequestRepaint();
                        }
                    });
                }
            }

            m_Builder.EndVertical();
        }

        /// <summary>
        /// 列表条目左侧的拖拽手柄列: 一条 "≡" 竖线, 垂直居中于整个条目。
        /// 每个条目左侧都有这个手柄(名称/版本/同步每一行左边都有一条)。
        ///
        /// 能拖时由后端画(它才知道指针按在哪、落在哪条缝上)。驱动抽象删除后只剩 Driver(支持排序),
        /// "后端没有拖拽能力"那一支已不存在; 不可拖的列表(<paramref name="draggable"/> 为 false)
        /// 不画手柄: 只读列表上的手柄等于一个按了没反应的控件。
        /// </summary>
        private void DrawListDragHandle(bool draggable, int listId, int index, string ghost)
        {
            if (draggable)
            {
                m_Builder.ReorderHandle(listId, index, ListHandleColumnWidth, ghost);
            }
        }

        /// <summary>
        /// 拖拽浮层上的那一句: 条目本身可能裹着十几行成员, 浮层只放得下一行摘要。
        ///
        /// 字符串直接用它自己; 其余类型用 ToString() —— 编辑器里的数据类大多已经把它
        /// 重写成"名字 + 版本"这类人话; 没重写时 ToString() 返回全限定类型名, 那就退回短类型名。
        /// </summary>
        private static string GhostText(object element)
        {
            if (element == null) return "None";
            if (element is string text) return text;

            var summary = element.ToString();
            return string.IsNullOrEmpty(summary) || summary == element.GetType().FullName
                ? element.GetType().Name
                : summary;
        }

        /// <summary>
        /// 列表的拖拽标识: 集合实例的引用哈希, 再混入成员名压低哈希碰撞的概率。
        ///
        /// 只用成员名会在嵌套列表里撞(两个对象的同名列表同时可见时, 是一张图上的两张表);
        /// 只用引用哈希则两个并存的实例理论上可能撞上同一个值。
        /// 两者异或之后: 同一张列表跨帧不变, 并存的列表互不相同。
        /// </summary>
        private static int ReorderId(CachedMember member, IList list)
        {
            return member.RawName.GetHashCode()
                   ^ System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(list);
        }

        /// <summary>
        /// "展开全部"状态的会话键: 与折叠态共用同一张 Toggle 表, 靠后缀区分(成员名里不会有 '#')。
        /// </summary>
        private static string ExpandAllKey(CachedMember member)
        {
            return member.RawName + "#expandAll";
        }

        //-------------------------------------- 工具条分段与条目分隔 --------------------------------------

        /// <summary>
        /// 工具条的分段竖线: 与表格列边界同一条 1px 线(后端不支持时就没有, 布局不受影响)。
        ///
        /// 高度取行高 —— 传 0 会让线去撑满整行高度(数据行那种用法), 头部会被顶高一块。
        /// 左右各 2px: 单元格窄, 间距给大了分段线会把计数与分页器推散。
        /// </summary>
        private void DrawToolbarSeparator()
        {
            var height = m_Ctx.Driver.LineHeight;
            if (height <= 1f) return;

            m_Builder.Space(2f);
            m_Builder.ColumnSeparator(height);
            m_Builder.Space(2f);
        }

        /// <summary>
        /// 条目之间的分隔线: 上下各 2px 空隙 + 1px 线, 与表格网格线同色。
        /// </summary>
        private void DrawItemSeparator()
        {
            m_Builder.Space(2f);
            m_Builder.Separator(1f, RDTheme.ListItemLineColor);
            m_Builder.Space(2f);
        }

        //-------------------------------------- 集合增删按钮的规格 --------------------------------------

        /// <summary>
        /// 嵌套列表项的标签列宽: 名称/版本 两个字的宽度足够(约 28px), 多给的每一像素
        /// 都从值框里扣 —— 表格单元格里"完整包名 + 删除键"就是被这里挤掉的。
        /// </summary>
        private const float NestedItemLabelWidth = 44f;

        /// <summary>列表条目左侧拖拽手柄列的宽(像素): "≡" 竖线所在的位置。</summary>
        private const float ListHandleColumnWidth = 12f;
    }
}
