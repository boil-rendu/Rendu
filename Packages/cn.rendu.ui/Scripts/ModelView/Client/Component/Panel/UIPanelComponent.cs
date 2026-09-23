//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI面板组件
    /// </summary>
    [ComponentOf(typeof(UIChild))]
    public partial class UIPanelComponent : Entity, IAwake, IUIInitialize, IDestroy
    {
        public UIBindVo UIBindVo;

        private EntityRef<UIChild> _uiBase;
        private UIChild            _UIBase => _uiBase;

        public UIChild UIBase
        {
            get
            {
                if (_UIBase == null)
                {
                    var uiChild = this.GetParent<UIChild>();
                    if (uiChild is { IsDisposed: false })
                    {
                        _uiBase = uiChild;
                    }
                }

                return _UIBase;
            }
        }

        private EntityRef<Entity> _ownerUIEntity;
        private Entity            _OwnerUIEntity => _ownerUIEntity;

        public Entity OwnerUIEntity
        {
            get
            {
                if (_OwnerUIEntity == null)
                {
                    _ownerUIEntity = UIBase?.OwnerUIEntity;
                }

                return _OwnerUIEntity;
            }
        }

        private EntityRef<UIWindowComponent> _uiWindow;
        private UIWindowComponent            _UIWindow => _uiWindow;

        public UIWindowComponent UIWindow
        {
            get
            {
                if (_UIWindow == null)
                {
                    _uiWindow = UIBase?.GetComponent<UIWindowComponent>();
                }

                return _UIWindow;
            }
        }

        /// <summary>
        /// 所在层级
        /// </summary>
        public EPanelLayer Layer = EPanelLayer.Panel;

        /// <summary>
        /// 界面选项
        /// </summary>
        public EPanelOption PanelOption = EPanelOption.None;

        /// <summary>
        /// 堆栈操作
        /// </summary>
        public EPanelStackOption StackOption = EPanelStackOption.Visible;

        /// <summary>
        /// 优先级，用于同层级排序,
        /// 大的在前 小的在后
        /// 相同时 后添加的在前
        /// </summary>
        public int Priority = 0;
    }
}
