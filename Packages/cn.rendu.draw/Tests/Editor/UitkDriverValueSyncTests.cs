#if UNITY_EDITOR

using NUnit.Framework;
using RD.Draw;
using RD.Draw.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Draw.Tests.Editor
{
    /// <summary>
    /// UITK 后端的"值同步"回归基线。
    ///
    /// 保留模式与立即模式的本质差别: IMGUI 的改动发生在 OnGUI 执行期间, 同一个帧里
    /// BeginChangeCheck/EndChangeCheck 直接比得出差值; 这里改动发生在**两次绘制之间** ——
    /// 用户点完/输完那一刻控件里已经是新值, 而引擎手里的模型值还是旧的。
    /// 因此控件不能无条件 SetValueWithoutNotify(模型值): 那会把用户刚弄的内容在重绘里抹掉,
    /// 并把旧值当"新值"交回引擎写回模型 —— 表现就是控件怎么点/怎么打字都没反应。
    ///
    /// 注意: 两次绘制必须用**同一个驱动实例**(m_ChangedElements 记在驱动上, 换实例就丢了);
    /// 驱动不传 requestRedraw, 因此 NoteChanged 里的 RequestRepaint 会直接返回, 不碰调度器。
    /// </summary>
    public class UitkDriverValueSyncTests
    {
        [Test]
        public void TextField_KeepsUserEditUntilEngineConsumesIt()
        {
            var root   = new VisualElement();
            var driver = new UitkDriver(root);

            driver.BeginDraw();
            driver.TextField("标签", "旧值");
            driver.EndDraw();

            var field = root.Q<TextField>();
            Assert.AreEqual("旧值", field.value);

            // 模拟用户输入: BaseField.value 的 setter 会触发 RegisterValueChangedCallback
            // (即 NoteChanged: 记下这个控件), 与真实编辑行为一致
            field.value = "新值";

            // 引擎此时还没写回模型, 仍拿旧值来画
            driver.BeginDraw();
            var returned = driver.TextField("标签", "旧值");
            driver.EndDraw();

            // 未消费的用户改动不许被旧值覆写; 且要把用户的当前值交给引擎写回
            Assert.AreEqual("新值", returned, "用户输入不许被模型旧值覆写");
            Assert.AreEqual("新值", root.Q<TextField>().value);
        }

        [Test]
        public void TextField_AppliesModelValueWhenNoPendingChange()
        {
            var root   = new VisualElement();
            var driver = new UitkDriver(root);

            driver.BeginDraw();
            driver.TextField("标签", "甲");
            driver.EndDraw();

            // 没有未消费改动时正常同步模型值(代码里改了模型, 界面要跟着变)
            driver.BeginDraw();
            var returned = driver.TextField("标签", "乙");
            driver.EndDraw();

            Assert.AreEqual("乙", returned);
        }

        [Test]
        public void Popup_KeepsUserSelectionUntilEngineConsumesIt()
        {
            var root   = new VisualElement();
            var driver = new UitkDriver(root);

            driver.BeginDraw();
            driver.Popup("选择", 0, new[] { "甲", "乙", "丙" });
            driver.EndDraw();

            var popup = root.Q<PopupField<string>>();
            Assert.AreEqual("甲", popup.value);

            // 模拟用户改选: 同上, 触发 NoteChanged
            popup.value = "丙";

            driver.BeginDraw();
            var index = driver.Popup("选择", 0, new[] { "甲", "乙", "丙" });   // 模型还是旧 index 0
            driver.EndDraw();

            Assert.AreEqual(2, index, "用户的改选要被交给引擎, 而不是被旧 index 抹掉");
        }

        [Test]
        public void Foldout_KeepsUserToggleUntilEngineConsumesIt()
        {
            var root   = new VisualElement();
            var driver = new UitkDriver(root);

            driver.BeginDraw();
            var first = driver.Foldout(false, "折叠");
            driver.EndDraw();
            Assert.IsFalse(first);

            var foldout = root.Q<Foldout>();
            foldout.value = true;   // 模拟用户展开

            driver.BeginDraw();
            var second = driver.Foldout(false, "折叠");   // 引擎状态还是收起
            driver.EndDraw();

            Assert.IsTrue(second, "用户展开过的折叠头不许被引擎旧状态覆写回去");
        }
    }
}

#endif
