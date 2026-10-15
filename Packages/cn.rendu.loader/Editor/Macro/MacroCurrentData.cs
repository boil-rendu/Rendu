#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using RD.Init.Editor;
using RD.Draw;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 宏管理面板的数据部分: 显示已有宏数量, 并提供「添加 / 移除」单个宏的交互。
    /// 增删后通过构造时传入的回调直接刷新整个界面。
    /// </summary>
    [Serializable]
    [HideLabel]
    [HideReferenceObjectPicker]
    public class MacroCurrentData
    {
        /// <summary>当前已存在的全部宏(只读展示, 分号分隔) —— 让"现在有哪些宏"一目了然, 不用点开下拉看。</summary>
        [LabelText("当前全部宏")]
        [ReadOnly]
        public string CurrentMacros;

        /// <summary>待移除的宏(下拉选择)</summary>
        [LabelText("需要移除的宏")]
        [ValueDropdown("GetValues")]
        [InlineButton("Remove", "移除")]
        public string RemoveMacro;

        /// <summary>待添加的宏(手填名称)</summary>
        [LabelText("需要添加的宏")]
        [InlineButton("Add", "添加")]
        public string AddMacro;

        private List<string> AllMacro;

        //整个界面刷新 本来应该用通知的 这里偷懒直接回调了
        //不会有其他需求了
        private Action RefreshAction;

        /// <summary>读取当前目标平台已定义的全部宏, 并缓存刷新回调</summary>
        public MacroCurrentData(Action refreshAction)
        {
            RefreshAction     = refreshAction;
            RemoveMacro       = null;
            AddMacro          = null;
            AllMacro          = MacroHelper.GetSymbols(MacroModule.BuildTargetGroup);
            CurrentMacros     = string.Join("; ", AllMacro);
        }

        /// <summary>下拉框数据源: 全部已定义宏</summary>
        private IEnumerable<string> GetValues()
        {
            return AllMacro;
        }

        /// <summary>添加 AddMacro 指定的宏(为空则提示), 随后刷新界面</summary>
        public void Add()
        {
            if (string.IsNullOrEmpty(AddMacro))
            {
                UnityTipsHelper.Show("请填写需要添加的宏名称");
                return;
            }

            MacroHelper.AddMacro(AddMacro, MacroModule.BuildTargetGroup);
            RefreshAction?.Invoke();
        }

        /// <summary>移除 RemoveMacro 指定的宏(未选择则提示), 随后刷新界面</summary>
        public void Remove()
        {
            if (string.IsNullOrEmpty(RemoveMacro))
            {
                UnityTipsHelper.Show("请选择需要移除的宏");
                return;
            }

            MacroHelper.RemoveMacro(RemoveMacro, MacroModule.BuildTargetGroup);
            RefreshAction?.Invoke();
        }
    }
}
#endif
