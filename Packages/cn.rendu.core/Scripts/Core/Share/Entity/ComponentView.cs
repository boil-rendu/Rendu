#if ENABLE_VIEW && UNITY_EDITOR
using UnityEngine;

namespace RD.Core
{
    /// <summary>
    /// Editor 视图组件：仅在 <c>ENABLE_VIEW</c> + Editor 下编译，给 <see cref="Entity"/> 在 Scene 中提供可视化容器，
    /// 通过 <see cref="Component"/> 字段反向引用回 Entity 自身。
    /// </summary>
    public class ComponentView: MonoBehaviour
    {
        /// <summary>关联的 <see cref="Entity"/> 引用（由 <see cref="Entity.IsRegister"/> 在注册时建立）。</summary>
        public Entity Component
        {
            get;
            set;
        }
    }
}
#endif
