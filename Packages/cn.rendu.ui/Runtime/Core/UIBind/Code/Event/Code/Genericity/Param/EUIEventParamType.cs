using RD.EditorKit;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 事件参数类型枚举
    /// 决定动态创建事件（CreatorUIEventBase）时用哪种泛型参数；与 EUIBindDataType 类似但独立
    /// </summary>
    [LabelText("事件参数类型")]
    public enum EUIEventParamType
    {
        /// <summary>ParamVo 泛型类（UIBindParamVo）</summary>
        [LabelText("ParamVo 泛型类")]
        ParamVo = 0,

        /// <summary>System.Object</summary>
        [LabelText("System.Object")]
        Object = 1,

        /// <summary>Bool</summary>
        [LabelText("Bool")]
        Bool = 2,

        /// <summary>String</summary>
        [LabelText("String")]
        String = 3,

        /// <summary>Int</summary>
        [LabelText("Int")]
        Int = 4,

        /// <summary>Float</summary>
        [LabelText("Float")]
        Float = 5,

        /// <summary>Vector3</summary>
        [LabelText("Vector3")]
        Vector3 = 6,

        /// <summary>List_Int</summary>
        [LabelText("List_Int")]
        List_Int = 7,

        /// <summary>List_Long</summary>
        [LabelText("List_Long")]
        List_Long = 8,

        /// <summary>List_String</summary>
        [LabelText("List_String")]
        List_String = 9,

        /// <summary>Long</summary>
        [LabelText("Long")]
        Long = 10,

        /// <summary>Uint</summary>
        [LabelText("Uint")]
        Uint = 11,

        /// <summary>Ulong</summary>
        [LabelText("Ulong")]
        Ulong = 12,

        /// <summary>Double</summary>
        [LabelText("Double")]
        Double = 13,

        /// <summary>Vector2</summary>
        [LabelText("Vector2")]
        Vector2 = 14,

        /// <summary>Unity.GameObject</summary>
        [LabelText("Unity.GameObject")]
        UnityGameObject = 15,
    }
}
