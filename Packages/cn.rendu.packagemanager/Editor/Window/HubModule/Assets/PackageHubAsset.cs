
using System;
using System.Collections.Generic;
using RD.Draw;
using RD.Serialization;
using RD.Serialization.Editor;

namespace RD.PackageManager.Editor
{
    [HideLabel]
    public class PackageHubAsset : SerializedScriptableObject
    {
        [Serialize]
        [ReadOnly]
        [LabelText("RD包信息")]
        public Dictionary<string, PackageHubData> AllPackageData = new();

        [NonSerialized]
        [Serialize]
        [LabelText("上一次更新时间")]
        public long LastUpdateTime; //上一次更新时间 防止重复更新 如果想强制更新可以手动设置改为0即可

        [NonSerialized]
        [Serialize]
        [LabelText("更新间隔")]
        public long UpdateInterval = 3600; //更新间隔 真不需要很高的频率 哪里有插件一直更新的
    }
}
