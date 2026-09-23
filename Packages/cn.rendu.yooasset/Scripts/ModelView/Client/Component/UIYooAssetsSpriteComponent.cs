using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 有图集时用图集加载,否则正常加载
    /// 如果你要访问图集干其他事情 请自行实现 因为不利于统一管理
    /// 这里只负责加载图片
    /// </summary>
    [ComponentOf(typeof(UILoadComponent))]
    public class UIYooAssetsSpriteComponent : Entity, IAwake
    {
        public EntityRef<UILoadComponent> m_UILoadRef;
        public UILoadComponent UILoad => m_UILoadRef;

        public readonly Dictionary<string, string> m_SpritePathMap = new();
        public readonly Dictionary<Sprite, SpriteAtlas> m_LoadedSprites = new();
    }
}