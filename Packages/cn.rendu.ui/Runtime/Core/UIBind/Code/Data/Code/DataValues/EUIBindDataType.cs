using RD.EditorKit;
using RD.Core;

namespace RD.Client
{
    [LabelText("类型")]

    //禁止修改ID
    //因为涉及到Mask 所以最高只有32个 请注意
    //@lsy
    /// <summary>
    /// UI 绑定数据类型枚举
    /// 描述一个 <see cref="UIDataValue"/> 承载的运行时类型，
    /// 用于按类型分发到具体的 UIDataValue 子类、以及面板展示
    /// 注意：ID 禁止修改 —— 底层按位（Mask）存储，故最多 32 种
    /// </summary>
    public enum EUIBindDataType
    {
        /// <summary>布尔（UIDataValueBool）</summary>
        [LabelText("Bool")]
        Bool = 0,

        /// <summary>字符串（UIDataValueString）</summary>
        [LabelText("String")]
        String = 1,

        /// <summary>32 位整型（UIDataValueInt）</summary>
        [LabelText("Int")]
        Int = 2,

        /// <summary>单精度浮点（UIDataValueFloat）</summary>
        [LabelText("Float")]
        Float = 3,

        /// <summary>三维向量（UIDataValueVector3）</summary>
        [LabelText("Vector3")]
        Vector3 = 4,

        /// <summary>整型列表（UIDataValueListInt）</summary>
        [LabelText("List_Int")]
        List_Int = 5,

        /// <summary>长整型列表（UIDataValueListLong）</summary>
        [LabelText("List_Long")]
        List_Long = 6,

        /// <summary>字符串列表</summary>
        [LabelText("List_String")]
        List_String = 7,

        /// <summary>64 位长整型（UIDataValueLong）</summary>
        [LabelText("Long")]
        Long = 8,

        /// <summary>无符号 32 位整型（UIDataValueUInt）</summary>
        [LabelText("Uint")]
        Uint = 9,

        /// <summary>无符号 64 位整型</summary>
        [LabelText("Ulong")]
        Ulong = 10,

        /// <summary>双精度浮点</summary>
        [LabelText("Double")]
        Double = 11,

        /// <summary>二维向量（UIDataValueVector2）</summary>
        [LabelText("Vector2")]
        Vector2 = 12,

        /// <summary>颜色（UIDataValueColor）</summary>
        [LabelText("Color")]
        Color = 13,
    }
}
