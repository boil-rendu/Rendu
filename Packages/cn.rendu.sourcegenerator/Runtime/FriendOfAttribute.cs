using System;

namespace RD
{
    /// <summary>
    /// 数据修改友好标记, 用于允许修改指定Component或Child数据的类上
    /// 例如:MoveComponentSystem需要修改MoveComponent的数据, 需要在MoveComponentSystem加上[FriendOf(typeof(MoveComponent))]
    /// </summary>
    
    /// <summary>
    /// 数据修改友好标记, 用于允许修改指定Component或Child数据的类上
    /// 例如:UnitHelper需要修改MoveComponent的数据, 需要在UnitHelper加上[FriendOf(typeof(MoveComponent))]
    /// 说明: 类名为 XxxComponentSystem / XxxSystem(去掉末尾 System 等于实体类型名) 或声明了 [EntitySystemOf(typeof(T))]
    /// 时已隐式视为 T 的友元, 无需再写 [FriendOf(typeof(T))]; 仅跨实体访问(如 UnitHelper 访问 MoveComponent)才需显式标注
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class FriendOfAttribute : Attribute
    {
        public Type Type;
        
        public FriendOfAttribute(Type type)
        {
            this.Type = type;
        }
    }
}