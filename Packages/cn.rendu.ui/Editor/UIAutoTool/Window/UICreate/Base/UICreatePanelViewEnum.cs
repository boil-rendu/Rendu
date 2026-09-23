#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 界面的枚举
    /// </summary>
    public static class UICreatePanelViewEnum
    {
        public static string Get(UIBindCDETable cdeTable)
        {
            var sb = SbPool.Get();
            cdeTable.GetViewEnum(sb);
            return SbPool.PutAndToStr(sb);
        }

        private static void GetViewEnum(this UIBindCDETable self, StringBuilder sb)
        {
            var splitData = self.PanelSplitData;
            if (splitData == null) return;
            if (!splitData.CreatePanelViewEnum) return;
            if (splitData.AllCommonView.Count + splitData.AllCreateView.Count + splitData.AllPopupView.Count <= 0) return;
            var index = 1;

            sb.Append("    /// <summary>\n    /// 由UI工具自动创建 请勿修改\n    /// 当前Panel所有可用view枚举\n    /// </summary>\n");
            sb.AppendFormat("    public enum E{0}ViewEnum\r\n    {{\r\n", self.name);
            AddViewEnum(splitData.AllCommonView, sb, ref index);
            AddViewEnum(splitData.AllCreateView, sb, ref index);
            AddViewEnum(splitData.AllPopupView, sb, ref index);
            sb.Append("    }");
        }

        private static void AddViewEnum(List<RectTransform> viewList, StringBuilder sb, ref int index)
        {
            foreach (var viewParent in viewList)
            {
                var viewName = viewParent.name.Replace(UIConstHelper.Const.UIParentName, "");
                sb.AppendFormat("        {0} = {1},\r\n", $"{viewName}", index);
                index++;
            }
        }
    }
}
#endif
