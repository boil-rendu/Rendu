using System;

namespace RD.Draw
{
    /// <summary>将方法显示为可点击的按钮。</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ButtonAttribute : Attribute
    {
        /// <summary>
        /// 显示排序号(与 [PropertyOrder] 同一排序通道), 未指定时按声明顺序。
        /// 注意: 构造函数的第二个位置参数是按钮高度而不是排序号 ——
        /// 要指定排序请用 [PropertyOrder](或命名参数 Order)。
        /// </summary>
        public int Order { get; set; }
        public string ButtonName { get; set; }
        public int ButtonHeight { get; set; }
        public ButtonStyle ButtonStyle { get; set; }
        public SdfIconType Icon { get; set; }
        public IconAlignment IconAlignment { get; set; }
        public string Tooltip { get; set; }

        public ButtonAttribute()
        {
        }

        /// <summary>
        /// buttonHeight 是按钮高度(像素), 0 = 不锁高(紧凑的文字宽按钮)。
        /// 业务代码一律按此约定书写: [Button("同步生成", 50)] = 50px 高的占行宽按钮。
        /// </summary>
        public ButtonAttribute(string buttonName = null, int buttonHeight = 0)
        {
            ButtonName   = buttonName;
            ButtonHeight = buttonHeight;
        }

        /// <summary>只指定高度的写法: [Button(50)]。</summary>
        public ButtonAttribute(int buttonHeight)
        {
            ButtonHeight = buttonHeight;
        }
    }
}
