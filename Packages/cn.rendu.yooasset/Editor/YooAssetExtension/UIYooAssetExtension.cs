#if YOOASSET_2_FILTER_RULE_API
// TODO(RD): 本扩展基于 YooAsset 2.x 的 IFilterRule/FilterRuleData 接口, 当前 YooAsset 3.0.5 已移除该接口, 需按 3.x 的收集规则 API 重写后再启用
using System;
using System.IO;
using System.Linq;

namespace YooAsset.Editor
{
    /// <summary>
    /// 收集预制体资源+所有图片
    /// 没有图集
    /// </summary>
    [DisplayName("UI_预制体+所有图片")]
    public class UIFilterRule : IFilterRule
    {
        #if YOO_ASSET_2_3_OR_NEWER
        public string FindAssetType
        {
            get { return nameof(EAssetSearchType.All); }
        }
        #endif

        public bool IsCollectAsset(FilterRuleData data)
        {
            if (Path.GetExtension(data.AssetPath) == ".prefab")
            {
                return data.AssetPath.Contains("/Prefabs/");
            }

            if (data.AssetPath.IndexOf("/Sprites/", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 指定文件下的所有资源,不包含子文件夹内的资源
    /// </summary>
    [DisplayName("UI_根文件")]
    public class UIFilterRule_Root : IFilterRule
    {
        #if YOO_ASSET_2_3_OR_NEWER
        public string FindAssetType
        {
            get { return nameof(EAssetSearchType.All); }
        }
        #endif

        public bool IsCollectAsset(FilterRuleData data)
        {
            var path = data.AssetPath;
            var collectPath = data.CollectPath;

            var relativePath = path.Substring(collectPath.Length).TrimStart('/', '\\');

            if (!relativePath.Contains('/') && !relativePath.Contains('\\'))
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 只收集预制体资源
    /// </summary>
    [DisplayName("UI_预制体")]
    public class UIFilterRule_Prefab : IFilterRule
    {
        #if YOO_ASSET_2_3_OR_NEWER
        public string FindAssetType
        {
            get { return nameof(EAssetSearchType.Prefab); }
        }
        #endif

        public bool IsCollectAsset(FilterRuleData data)
        {
            if (Path.GetExtension(data.AssetPath) == ".prefab")
            {
                return data.AssetPath.Contains("/Prefabs/");
            }

            return false;
        }
    }

    /// <summary>
    /// 只收集图片资源
    /// </summary>
    [DisplayName("UI_图片")]
    public class UIFilterRule_Sprite : IFilterRule
    {
        #if YOO_ASSET_2_3_OR_NEWER
        public string FindAssetType
        {
            get { return nameof(EAssetSearchType.Sprite); }
        }
        #endif

        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.IndexOf("/Sprites/", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 只收集图集资源
    /// </summary>
    [DisplayName("UI_图集")]
    public class UIFilterRule_Atlas : IFilterRule
    {
        #if YOO_ASSET_2_3_OR_NEWER
        public string FindAssetType
        {
            get { return nameof(EAssetSearchType.All); }
        }
        #endif

        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.IndexOf("/Atlas/", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 只收集没有图集的图片
    /// </summary>
    [DisplayName("UI_没有图集的图片")]
    public class UIFilterRule_NoAtlas_Sprite : IFilterRule
    {
        #if YOO_ASSET_2_3_OR_NEWER
        public string FindAssetType
        {
            get { return nameof(EAssetSearchType.All); }
        }
        #endif

        public bool IsCollectAsset(FilterRuleData data)
        {
            var path = data.AssetPath;
            var spritesIndex = path.IndexOf("/Sprites/", StringComparison.Ordinal);
            if (spritesIndex < 0)
            {
                return false;
            }

            if (path.IndexOf("/Sprites/AtlasIgnore/", StringComparison.Ordinal) >= 0)
            {
                return true; //忽略图集的图片
            }

            var relativePath = path.Substring(spritesIndex + "/Sprites/".Length);
            int slashIndex = relativePath.IndexOf('/');
            if (slashIndex < 0)
            {
                return true; // Sprites根目录下的图片，默认收集
            }

            var spritesDir = path.Substring(0, spritesIndex + "/Sprites/".Length - 1);
            var atlasDir = spritesDir.Replace("/Sprites", "/Atlas");

            if (!Directory.Exists(atlasDir))
            {
                return true;
            }

            var folderName = relativePath.Substring(0, slashIndex);

            var atlasFileNames = Directory.GetFiles(atlasDir).Where(file => !file.EndsWith(".meta")).ToArray();

            foreach (var atlasPath in atlasFileNames)
            {
                var atlasFileName = Path.GetFileNameWithoutExtension(atlasPath);
                if (atlasFileName.EndsWith("_" + folderName, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif
