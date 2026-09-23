using System;
using System.Collections.Generic;
using YooAsset;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI资源管理器 yooasset扩展
    /// </summary>
    [ComponentOf(typeof(UILoadComponent))]
    public class UIYooAssetsLoadComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<int, AssetHandle> m_AllHandle = new();
        public ResourcePackage m_Package;
    }
}