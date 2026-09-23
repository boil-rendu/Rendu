using RD.Core;
//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace RD.Client
{
    /// <summary>
    /// UI通用组件
    /// </summary>
    [ComponentOf(typeof(UIChild))]
    public partial class UICommonComponent : Entity, IAwake, IUIInitialize, IDestroy
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
                }

                return _UIBase;
            }
        }
    }
}
