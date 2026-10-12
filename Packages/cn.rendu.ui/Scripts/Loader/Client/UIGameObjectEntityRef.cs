using System;
using UnityEngine;
using RD.Core;

namespace RD
{
    /// <summary>
    /// 挂在 UI GameObject 上，用于反向关联到所属 Entity
    /// 编辑器（ENABLE_VIEW）下额外提供只读的 ViewGO 展示，便于在 Inspector 里定位对应的查看对象
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public partial class UIGameObjectEntityRef : MonoBehaviour
    {
        /// <summary>所属实体的安全引用（实体销毁后自动失效）</summary>
        public EntityRef<Entity> EntityRef;
        /// <summary>所属实体（内部即 EntityRef 的隐式转换）</summary>
        public Entity Entity => EntityRef;

        #if ENABLE_VIEW && UNITY_EDITOR

        [NonSerialized]
        private GameObject m_ViewGO;

        /// <summary>编辑器视图对象（懒取实体上的 ViewGO，只读展示用）</summary>
        [PropertyOrder(int.MinValue)]
        [ReadOnly]
        [ShowInInspector]
        [HideInEditorMode]
        [LabelText("ViewGO")]
        public GameObject ViewGO
        {
            get
            {
                if (m_ViewGO == null)
                {
                    m_ViewGO = Entity?.ViewGO;
                }

                return m_ViewGO;
            }
        }
        #endif
    }
}