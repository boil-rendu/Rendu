#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 变量的生成
    /// </summary>
    public static class CreateVariables
    {
        public static string Get(UIBindCDETable cdeTable)
        {
            var sb = SbPool.Get();
            cdeTable.GetOverrideConfig(sb);
            cdeTable.GetComponentTable(sb);
            cdeTable.GetDataTable(sb);
            cdeTable.GetCDETable(sb);
            cdeTable.GetEventTable(sb);
            return SbPool.PutAndToStr(sb);
        }

        private static void GetComponentTable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.ComponentTable;
            if (tab == null) return;

            foreach (var value in tab.AllBindDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var bindCom = value.Value;
                if (bindCom == null) continue;
                sb.AppendFormat("        public {0} {1};\r\n", bindCom.GetType(), name);
            }
        }

        private static void GetDataTable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.DataTable;
            if (tab == null) return;

            foreach (var value in tab.DataDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var uiData    = value.Value;
                var dataValue = uiData?.DataValue;
                if (dataValue == null) continue;
                sb.AppendFormat("        public {0} {1};\r\n", dataValue.GetType(), name);
            }
        }

        private static void GetEventTable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.EventTable;
            if (tab == null) return;

            foreach (var value in tab.EventDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var uiEventBase = value.Value;
                if (uiEventBase == null) continue;
                sb.AppendFormat("        public {0} {1};\r\n", uiEventBase.GetEventType(), name);
                sb.AppendFormat("        public {0} {1};\r\n", uiEventBase.GetEventHandleType(), $"{name}Handle");
                var onEvent = $"OnEvent{name.Replace($"{NameUtility.FirstName}{NameUtility.EventName}", "")}Invoke";
                sb.AppendFormat("        public const string {0} = \"{1}.{0}\";\r\n", onEvent, $"{self.ResName}Component");
            }
        }

        private static void GetCDETable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.AllChildCdeTable;
            if (tab == null) return;
            var existName = new HashSet<string>();

            foreach (var value in tab)
            {
                var name = value.name;
                if (string.IsNullOrEmpty(name)) continue;
                var pkgName = value.PkgName;
                var resName = value.ResName;
                if (string.IsNullOrEmpty(pkgName) || string.IsNullOrEmpty(resName)) continue;
                var newName = GetCDEUIName(name);
                if (existName.Contains(newName))
                {
                    Debug.LogError($"{self.name} 内部公共组件存在同名 请修改 {name} 当前会被忽略 {newName}");
                    continue;
                }

                existName.Add(newName);
                sb.AppendFormat("        public {0} {1};\r\n",
                    $"EntityRef<{UIConstHelper.Const.Namespace}.{resName}Component>", newName);
                sb.AppendFormat("        public {0} {1} => {2};\r\n",
                    $"{UIConstHelper.Const.Namespace}.{resName}Component", newName.Replace(NameUtility.FirstName, ""), newName);
            }
        }

        internal static string GetCDEUIName(string oldName)
        {
            var newName = oldName;

            if (!oldName.CheckFirstName(NameUtility.UIName))
            {
                newName = $"{NameUtility.FirstName}{NameUtility.UIName}{oldName}";
            }

            newName = Regex.Replace(newName, NameUtility.NameRegex, "");

            return newName.ChangeToBigName(NameUtility.UIName);
        }

        private static void GetOverrideConfig(this UIBindCDETable self, StringBuilder sb)
        {
            switch (self.UICodeType)
            {
                case EUICodeType.Common:
                    sb.AppendFormat("        public EntityRef<UIChild> u_UIBase;\r\n");
                    sb.AppendFormat("        public UIChild UIBase => u_UIBase;\r\n");
                    return;
                case EUICodeType.Panel:
                    sb.AppendFormat("        public EntityRef<UIChild> u_UIBase;\r\n");
                    sb.AppendFormat("        public UIChild UIBase => u_UIBase;\r\n");

                    sb.AppendFormat("        public EntityRef<UIWindowComponent> u_UIWindow;\r\n");
                    sb.AppendFormat("        public UIWindowComponent UIWindow => u_UIWindow;\r\n");

                    sb.AppendFormat("        public EntityRef<UIPanelComponent> u_UIPanel;\r\n");
                    sb.AppendFormat("        public UIPanelComponent UIPanel => u_UIPanel;\r\n");
                    break;
                case EUICodeType.View:
                    sb.AppendFormat("        public EntityRef<UIChild> u_UIBase;\r\n");
                    sb.AppendFormat("        public UIChild UIBase => u_UIBase;\r\n");

                    sb.AppendFormat("        public EntityRef<UIWindowComponent> u_UIWindow;\r\n");
                    sb.AppendFormat("        public UIWindowComponent UIWindow => u_UIWindow;\r\n");

                    sb.AppendFormat("        public EntityRef<UIViewComponent> u_UIView;\r\n");
                    sb.AppendFormat("        public UIViewComponent UIView => u_UIView;\r\n");
                    break;
                default:
                    Debug.LogError($"新增类型未实现 {self.UICodeType}");
                    break;
            }
        }
    }
}
#endif