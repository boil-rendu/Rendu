#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using RD.Draw;

namespace RD.Loader.Editor
{
    [HideLabel]
    [Serializable]
    [HideReferenceObjectPicker]
    public abstract class MacroDataBase
    {
        public abstract void         Initialize();
        public abstract List<string> GetAll();
        public abstract List<string> GetSelect();
    }

    /// <summary>
    /// 宏数据基类
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class MacroDataBase<T> : MacroDataBase where T : struct
    {
        /// <summary>
        /// 当前已勾选的宏组合(Flags 枚举)。
        /// 这里**不要**写 [LabelText]: 本类是所有宏分组共用的泛型基类, 写死标签会让
        /// N 个分组显示成同一行文字("宏 枚举" × N, 看不出哪行是哪个)。
        /// 标签交给引擎回退到各自枚举类型上的 [LabelText]
        /// (EMacroType="RD" / EUIMacroType="UI" / EUIAssetsMacroType="资源加载")。
        /// </summary>
        [ShowInInspector]
        protected T MacroEnumType;

        public override void Initialize()
        {
            var value = MacroHelper.InitEnumValue<T>(MacroModule.BuildTargetGroup);
            Enum.TryParse(value.ToString(), out this.MacroEnumType);
        }

        /// <summary>
        /// 获取当前枚举所有宏
        /// </summary>
        /// <returns></returns>
        public override List<string> GetAll()
        {
            return MacroHelper.GetEnumAll<T>();
        }

        /// <summary>
        /// 获取选择宏
        /// </summary>
        /// <returns></returns>
        public override List<string> GetSelect()
        {
            return MacroHelper.GetEnumSelect(MacroEnumType);
        }
    }
}
#endif
