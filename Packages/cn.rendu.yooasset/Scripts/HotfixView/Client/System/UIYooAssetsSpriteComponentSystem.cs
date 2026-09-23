using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using RD.Client;
using YooAsset;
using RD.Core;

namespace RD.Client
{
    [FriendOf(typeof(UIYooAssetsSpriteComponent))]
    [EntitySystemOf(typeof(UIYooAssetsSpriteComponent))]
    public static partial class UIYooAssetsSpriteComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIYooAssetsSpriteComponent self)
        {
            self.m_UILoadRef = self.GetParent<UILoadComponent>();
        }

#if RD_UI_ATLAS_MODULE
        public static async Promise LoadAtlasAsync(this UIYooAssetsSpriteComponent self)
        {
            EntityRef<UIYooAssetsSpriteComponent> selfRef = self;
            var handle = YooAssets.LoadAssetAsync<UIAtlasData>(UIConstAsset.AtlasDataName);

            await handle;

            var atlasData = handle.AssetObject as UIAtlasData;
            if (atlasData == null)
            {
                return;
            }

            self = selfRef;
            self.m_SpritePathMap.Clear();
            foreach (var info in atlasData.Infos)
            {
                foreach (var spriteName in info.SpriteNames)
                {
                    self.m_SpritePathMap.Add(spriteName, info.AtlasName);
                }
            }

            handle.Release();
        }

#endif
        public static string GetSpriteAtlasPath(this UIYooAssetsSpriteComponent self, string spriteName)
        {
            return self.m_SpritePathMap.GetValueOrDefault(spriteName);
        }

        #if !UIMACRO_SYNCLOAD_CLOSE
        public static Sprite GetSprite(this UIYooAssetsSpriteComponent self, string spriteName)
        {
            Sprite sprite = null;

            if (!self.m_SpritePathMap.TryGetValue(spriteName, out string atlasPath))
            {
                #if UNITY_EDITOR
                if (!self.UILoad.VerifyAssetValidity(spriteName))
                {
                    Log.Error($"验证资产有效性 没有这个资源 图片无法加载 请检查 {spriteName}");
                    return null;
                }
                #endif

                sprite = self.UILoad.LoadAsset<Sprite>(spriteName);

                if (sprite == null)
                {
                    Log.Error($"加载失败 没有这个资源 图片无法加载 请检查 {spriteName}");
                }
            }
            else
            {
                var spriteAtlas = self.UILoad.LoadAsset<SpriteAtlas>(atlasPath);
                sprite = spriteAtlas?.GetSprite(spriteName);
                if (sprite == null)
                {
                    Log.Error($"找到图集{atlasPath},但是没有找不到Sprite：{spriteName}");
                    return null;
                }

                if (!self.m_LoadedSprites.TryAdd(sprite, spriteAtlas))
                {
                    Log.Error($"重复添加Sprite：{spriteName}");
                }
            }

            return sprite;
        }
        #endif

        public static async Promise<Sprite> GetSpriteAsync(this UIYooAssetsSpriteComponent self, string spriteName)
        {
            Sprite sprite = null;

            if (!self.m_SpritePathMap.TryGetValue(spriteName, out string atlasPath))
            {
                #if UNITY_EDITOR
                if (!self.UILoad.VerifyAssetValidity(spriteName))
                {
                    Log.Error($"验证资产有效性 没有这个资源 图片无法加载 请检查 {spriteName}");
                    return null;
                }
                #endif

                sprite = await self.UILoad.LoadAssetAsync<Sprite>(spriteName);

                if (sprite == null)
                {
                    Log.Error($"加载失败 没有这个资源 图片无法加载 请检查 {spriteName}");
                }
            }
            else
            {
                EntityRef<UIYooAssetsSpriteComponent> selfRef = self;
                var spriteAtlas = await self.UILoad.LoadAssetAsync<SpriteAtlas>(atlasPath);
                sprite = spriteAtlas?.GetSprite(spriteName);
                if (sprite == null)
                {
                    Log.Error($"找到图集{atlasPath},但是没有找不到Sprite：{spriteName}");
                    return null;
                }

                self = selfRef;
                if (!self.m_LoadedSprites.TryAdd(sprite, spriteAtlas))
                {
                    Log.Error($"重复添加Sprite：{spriteName}");
                }
            }

            return sprite;
        }

        public static void ReleaseSprite(this UIYooAssetsSpriteComponent self, Sprite sprite)
        {
            if (self.m_LoadedSprites.ContainsKey(sprite))
            {
                var spriteAtlas = self.m_LoadedSprites[sprite];
                self.m_LoadedSprites.Remove(sprite);
                self.UILoad.Release(spriteAtlas);
            }
            else
            {
                self.UILoad.Release(sprite);
            }
        }
    }
}