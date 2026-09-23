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
    /// UI界面组件
    /// </summary>
    [ComponentOf(typeof(UIChild))]
    public partial class UIViewComponent : Entity, IAwake, IUIInitialize, IDestroy
    {
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
                    else
                    {
                        _uiBase = default;
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
                    var uiEntity = UIBase?.OwnerUIEntity;
                    if (uiEntity is { IsDisposed: false })
                    {
                        _ownerUIEntity = uiEntity;
                    }
                    else
                    {
                        _ownerUIEntity = default;
                    }
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
                    var window = UIBase?.GetComponent<UIWindowComponent>();
                    if (window is { IsDisposed: false })
                    {
                        _uiWindow = window;
                    }
                    else
                    {
                        _uiWindow = default;
                    }
                }

                return _UIWindow;
            }
        }

        public EViewWindowType ViewWindowType = EViewWindowType.View;

        public EViewStackOption StackOption = EViewStackOption.Visible;
    }
}