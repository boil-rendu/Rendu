using System.Collections.Generic;
using RD.EditorKit.Editor;
using NUnit.Framework;

namespace RD.EditorKit.Tests.Editor
{
    /// <summary>
    /// MenuTree 的行为基线。
    ///
    /// 这棵树的建树规则有几条容易踩的隐式约定, 都在这里固化:
    /// - Add("A/B/C") 会自动补齐中间层文件夹节点, 且已存在的层级会被复用而非重建;
    /// - 同一父节点下的同名节点会被"就地替换 Value"而不是重复添加;
    /// - ForEach 是深度优先且保持插入顺序(用逆序压栈实现, 不是递归);
    /// - 重复选中同一项不会重复触发 SelectionChanged。
    ///
    /// 未覆盖: AddAllAssetsAtPath / AddAllAssetImporterAtPath(依赖 AssetDatabase),
    /// 以及私有路径工具 NormalizeFolder / BuildChildMenuPath。
    /// </summary>
    public class MenuTreeTests
    {
        //-------------------------------------- 查路径 --------------------------------------

        [Test]
        public void FindMenuItem_ReturnsNullForMissingOrEmptyPath()
        {
            var tree = new MenuTree();

            Assert.IsNull(tree.FindMenuItem(null));
            Assert.IsNull(tree.FindMenuItem(""));
            Assert.IsNull(tree.FindMenuItem("Nope"));
        }

        [Test]
        public void FindMenuItem_ResolvesBySlashPath()
        {
            var tree = new MenuTree();
            var leaf = tree.Add("A/B/C", 42);

            Assert.AreSame(leaf, tree.FindMenuItem("A/B/C"));
            Assert.AreSame(leaf, tree.FindMenuItem("/A/B/C/"), "首尾斜杠应被忽略");
            Assert.IsNotNull(tree.FindMenuItem("A/B"), "中间层文件夹节点也是可查找的节点");
            Assert.IsNull(tree.FindMenuItem("A/B/D"));
            Assert.IsNull(tree.FindMenuItem("A/C"));
        }

        //-------------------------------------- 建树 --------------------------------------

        [Test]
        public void Add_NestedPath_CreatesIntermediateFolders()
        {
            var tree = new MenuTree();
            var leaf = tree.Add("A/B/C", 42);

            Assert.AreEqual("C", leaf.Name);
            Assert.AreEqual(42, leaf.Value);
            Assert.AreSame(tree, leaf.Tree);

            Assert.AreEqual(1, tree.MenuItems.Count, "根下只有 A");
            var a = tree.MenuItems[0];
            Assert.AreEqual("A", a.Name);
            Assert.AreEqual(1, a.ChildMenuItems.Count);

            var b = a.ChildMenuItems[0];
            Assert.AreEqual("B", b.Name);
            Assert.AreEqual(1, b.ChildMenuItems.Count);
            Assert.AreSame(leaf, b.ChildMenuItems[0]);
            Assert.AreSame(b, leaf.Parent);
        }

        [Test]
        public void Add_ReusesExistingFolders()
        {
            var tree = new MenuTree();
            tree.Add("A/B/One", 1);
            tree.Add("A/B/Two", 2);
            tree.Add("A/C/Three", 3);

            Assert.AreEqual(1, tree.MenuItems.Count, "根下只应有 A");

            var a = tree.MenuItems[0];
            Assert.AreEqual(2, a.ChildMenuItems.Count, "A 下应有 B、C");
            Assert.AreEqual("B", a.ChildMenuItems[0].Name);
            Assert.AreEqual("C", a.ChildMenuItems[1].Name);
            Assert.AreEqual(2, a.ChildMenuItems[0].ChildMenuItems.Count, "B 下应有 One、Two");
        }

        [Test]
        public void Add_SameNameReplacesValueInsteadOfDuplicating()
        {
            var tree = new MenuTree();
            var first = tree.Add("A/Item", 1);
            var second = tree.Add("A/Item", 2);

            var a = tree.MenuItems[0];
            Assert.AreEqual(1, a.ChildMenuItems.Count, "同名节点应被复用而不是重复添加");
            Assert.AreSame(first, second);
            Assert.AreEqual(2, a.ChildMenuItems[0].Value, "复用节点时应覆盖 Value");
        }

        [Test]
        public void AddChild_SetsTreeAndParent()
        {
            var tree = new MenuTree();
            var root = tree.Add("A", 1);
            var child = new MenuTreeItem(tree, "X", 2);

            root.AddChild(child);

            Assert.AreSame(tree, child.Tree);
            Assert.AreSame(root, child.Parent);
            Assert.IsTrue(root.HasChildren);
            Assert.AreEqual(1, root.ChildMenuItems.Count);

            root.RemoveChild(child);
            Assert.IsFalse(root.HasChildren);
        }

        //-------------------------------------- 遍历 --------------------------------------

        [Test]
        public void ForEach_VisitsDepthFirstInInsertionOrder()
        {
            var tree = new MenuTree();
            tree.Add("A/One", 1);
            tree.Add("A/Two", 2);
            tree.Add("B/Three", 3);

            var visited = new List<string>();
            tree.ForEach(item => visited.Add(item.Name));

            CollectionAssert.AreEqual(new[] { "A", "One", "Two", "B", "Three" }, visited);
        }

        //-------------------------------------- 选中 --------------------------------------

        [Test]
        public void Selection_SelectSetsIsSelectedAndRaisesEvent()
        {
            var tree = new MenuTree();
            var one = tree.Add("A/One", 1);
            var two = tree.Add("A/Two", 2);

            var events = new List<SelectionChangedType>();
            tree.Selection.SelectionChanged += events.Add;

            tree.Selection.Select(one);
            Assert.AreSame(one, tree.Selection.SelectedValue);
            Assert.IsTrue(one.IsSelected);
            Assert.AreEqual(1, tree.Selection.Count);
            CollectionAssert.AreEqual(new[] { SelectionChangedType.ItemAdded }, events);

            tree.Selection.Select(two);
            Assert.IsFalse(one.IsSelected, "切换选中应清掉旧选中项");
            Assert.IsTrue(two.IsSelected);
            Assert.AreEqual(2, events.Count);
        }

        [Test]
        public void Selection_SelectSameItemTwice_DoesNotRaiseAgain()
        {
            var tree = new MenuTree();
            var one = tree.Add("A/One", 1);

            var count = 0;
            tree.Selection.SelectionChanged += _ => count++;

            tree.Selection.Select(one);
            tree.Selection.Select(one);

            Assert.AreEqual(1, count, "重复选中同一项不应重复触发事件");
        }

        [Test]
        public void Selection_SelectNullClears()
        {
            var tree = new MenuTree();
            var one = tree.Add("A/One", 1);
            tree.Selection.Select(one);

            // 注意: Select(null) 会与 Select(MenuTreeItem[]) 重载产生二义性, 调用方必须显式转型。
            tree.Selection.Select((MenuTreeItem)null);

            Assert.AreEqual(0, tree.Selection.Count);
            Assert.IsFalse(one.IsSelected);
        }

        [Test]
        public void Selection_SelectArrayReplacesSelection()
        {
            var tree = new MenuTree();
            var one = tree.Add("A/One", 1);
            var two = tree.Add("A/Two", 2);

            tree.Selection.Select(one);
            tree.Selection.Select(new[] { one, two });

            Assert.AreEqual(2, tree.Selection.Count);
            Assert.IsTrue(one.IsSelected);
            Assert.IsTrue(two.IsSelected);
        }

        [Test]
        public void SelectFirstMenuItem_OnlyAppliesWhenNothingSelected()
        {
            var tree = new MenuTree();
            tree.Add("A/One", 1);
            var b = tree.Add("B/Two", 2);

            tree.SelectFirstMenuItem();
            Assert.AreSame(tree.MenuItems[0], tree.Selection.SelectedValue,
                "应选中第一个根节点(文件夹), 而不是它的叶子节点");

            tree.Selection.Select(b);
            tree.SelectFirstMenuItem();
            Assert.AreSame(b, tree.Selection.SelectedValue, "已有选中项时不应改动");
        }

        [Test]
        public void SelectMenuItem_ByPath()
        {
            var tree = new MenuTree();
            var leaf = tree.Add("A/B/C", 42);

            tree.SelectMenuItem("A/B/C");
            Assert.AreSame(leaf, tree.Selection.SelectedValue);

            tree.SelectMenuItem("A/B/Missing");
            Assert.AreSame(leaf, tree.Selection.SelectedValue, "找不到路径时不应改变选中");
        }

        [Test]
        public void Clear_EmptiesItemsAndSelection()
        {
            var tree = new MenuTree();
            var leaf = tree.Add("A/B/C", 42);
            tree.Selection.Select(leaf);

            tree.Clear();

            Assert.AreEqual(0, tree.MenuItems.Count);
            Assert.AreEqual(0, tree.Selection.Count);
            Assert.IsFalse(leaf.IsSelected);
        }
    }
}
