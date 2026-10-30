using System;
using UnityEngine;

namespace RD.Serialization
{
    /// <summary>
    /// Unity 原生"值包"类型的判定：曲线 / 渐变。
    ///
    /// 这类类型的数据在原生对象里（曲线的 m_Ptr）, 只能按整体读写（取公开读写属性）:
    /// 展开字段拿不到东西, 也不能绕过构造函数去创建实例。
    ///
    /// 判定住在本包（序列化器, 位于绘制引擎之下）是刻意的: 它是**数据事实**, 序列化侧要用;
    /// 绘制引擎(cn.rendu.draw)位于本包之上, 由它向下引用本判定 —— 方向合法, 且全仓只有一份实现,
    /// 绘制与序列化不会各判各的。
    /// </summary>
    public static class ValueBagTypes
    {
        public static bool IsUnityValueBagType(Type type)
        {
            return type == typeof(AnimationCurve) || type == typeof(Gradient);
        }
    }
}
