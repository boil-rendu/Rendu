using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeGetUIRootHandler : AInvokeEntityHandler<UIInvokeEntity_GetUIRoot, GameObject>
    {
        public override GameObject Handle(Entity entity, UIInvokeEntity_GetUIRoot args)
        {
            return entity.UIMgr().UIRoot;
        }
    }

    [Invoke(EInvokeType.Sync)]
    public class UIInvokeGetUICanvasRootHandler : AInvokeEntityHandler<UIInvokeEntity_GetUICanvasRoot, GameObject>
    {
        public override GameObject Handle(Entity entity, UIInvokeEntity_GetUICanvasRoot args)
        {
            return entity.UIMgr().UICanvasRoot;
        }
    }

    [Invoke(EInvokeType.Sync)]
    public class UIInvokeGetUILayerRootHandler : AInvokeEntityHandler<UIInvokeEntity_GetUILayerRoot, RectTransform>
    {
        public override RectTransform Handle(Entity entity, UIInvokeEntity_GetUILayerRoot args)
        {
            return entity.UIMgr().UILayerRoot;
        }
    }

    [Invoke(EInvokeType.Sync)]
    public class UIInvokeGetUICameraHandler : AInvokeEntityHandler<UIInvokeEntity_GetUICamera, Camera>
    {
        public override Camera Handle(Entity entity, UIInvokeEntity_GetUICamera args)
        {
            return entity.UIMgr().UICamera;
        }
    }

    [Invoke(EInvokeType.Sync)]
    public class UIInvokeGetUICanvasHandler : AInvokeEntityHandler<UIInvokeEntity_GetUICanvas, Canvas>
    {
        public override Canvas Handle(Entity entity, UIInvokeEntity_GetUICanvas args)
        {
            return entity.UIMgr().UICanvas;
        }
    }
}