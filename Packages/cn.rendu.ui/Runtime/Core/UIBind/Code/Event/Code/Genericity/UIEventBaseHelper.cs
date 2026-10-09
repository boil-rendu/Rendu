using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIEventBase 的辅助/扩展：
    /// 按参数个数动态创建同步事件、以及统一的 Invoke / AddEvent / RemoveEvent / 参数比较等扩展方法
    /// </summary>
    public static class UIEventBaseHelper
    {
        #region 创建

        /// <summary>按参数类型列表动态创建对应泛型实参的 UIEventBase（同步事件）</summary>
        public static UIEventBase CreatorUIEventBase(string name, List<EUIEventParamType> paramList)
        {
            UIEventBase uiEventBase = null;
            switch (paramList.Count)
            {
                case 0:
                    uiEventBase = CreatorUIEventP0();
                    break;
                case 1:
                    uiEventBase = CreatorUIEventP1(paramList);
                    break;
                case 2:
                    uiEventBase = CreatorUIEventP2(paramList);
                    break;
                case 3:
                    uiEventBase = CreatorUIEventP3(paramList);
                    break;
                case 4:
                    uiEventBase = CreatorUIEventP4(paramList);
                    break;
                case 5:
                    uiEventBase = CreatorUIEventP5(paramList);
                    break;
                default:
                    Logger.LogError($"最高支持5个参数 如果还想支持就自己扩展 {paramList.Count}");
                    break;
            }

            if (uiEventBase == null)
            {
                Logger.LogError($"{name} 动态创建泛型事件 失败 请检查原因");
                return null;
            }

            uiEventBase.SetName(name);
            uiEventBase.RefreshAllEventParamType(paramList);
            return uiEventBase;
        }

        private static UIEventBase CreatorUIEventP0()
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP0)));
        }

        private static UIEventBase CreatorUIEventP1(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP1<>).MakeGenericType(paramList[0].GetParamType())));
        }

        private static UIEventBase CreatorUIEventP2(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP2<,>).MakeGenericType(paramList[0].GetParamType(), paramList[1].GetParamType())));
        }

        private static UIEventBase CreatorUIEventP3(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP3<,,>).MakeGenericType(paramList[0].GetParamType(), paramList[1].GetParamType(), paramList[2].GetParamType())));
        }

        private static UIEventBase CreatorUIEventP4(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP4<,,,>).MakeGenericType(paramList[0].GetParamType(), paramList[1].GetParamType(), paramList[2].GetParamType(), paramList[3].GetParamType())));
        }

        private static UIEventBase CreatorUIEventP5(List<EUIEventParamType> paramList)
        {
            return (UIEventBase)(Activator.CreateInstance(typeof(UIEventP5<,,,,>).MakeGenericType(paramList[0].GetParamType(), paramList[1].GetParamType(), paramList[2].GetParamType(), paramList[3].GetParamType(), paramList[4].GetParamType())));
        }

        #endregion

        #region 调用

        /// <summary>调用事件（0 参）；参数个数不匹配时报错</summary>
        public static void Invoke(this UIEventBase self)
        {
            if (self is IUIEventInvoke uiEventInvoke)
            {
                uiEventInvoke.Invoke();
            }
            else
            {
                Logger.LogError($"调用 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        /// <summary>调用事件（1 参）；参数个数不匹配时报错</summary>
        public static void Invoke<P1>(this UIEventBase self, P1 p1)
        {
            if (self is IUIEventInvoke<P1> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1);
            }
            else
            {
                Logger.LogError($"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        /// <summary>调用事件（2 参）；参数个数不匹配时报错</summary>
        public static void Invoke<P1, P2>(this UIEventBase self, P1 p1, P2 p2)
        {
            if (self is IUIEventInvoke<P1, P2> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1, p2);
            }
            else
            {
                Logger.LogError($"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        /// <summary>调用事件（3 参）；参数个数不匹配时报错</summary>
        public static void Invoke<P1, P2, P3>(this UIEventBase self, P1 p1, P2 p2, P3 p3)
        {
            if (self is IUIEventInvoke<P1, P2, P3> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1, p2, p3);
            }
            else
            {
                Logger.LogError($"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        /// <summary>调用事件（4 参）；参数个数不匹配时报错</summary>
        public static void Invoke<P1, P2, P3, P4>(this UIEventBase self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (self is IUIEventInvoke<P1, P2, P3, P4> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1, p2, p3, p4);
            }
            else
            {
                Logger.LogError($"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        /// <summary>调用事件（5 参）；参数个数不匹配时报错</summary>
        public static void Invoke<P1, P2, P3, P4, P5>(this UIEventBase self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (self is IUIEventInvoke<P1, P2, P3, P4, P5> uiEventInvoke)
            {
                uiEventInvoke.Invoke(p1, p2, p3, p4, p5);
            }
            else
            {
                Logger.LogError($"调用 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} {typeof(P5).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }
        }

        #endregion

        #region 添加事件

        /// <summary>添加回调（0 参）</summary>
        public static UIEventHandleP0 AddEvent(this UIEventBase self, UIEventDelegate callback)
        {
            if (self is UIEventP0 uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError($"添加 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        /// <summary>添加回调（1 参）</summary>
        public static UIEventHandleP1<P1> AddEvent<P1>(this UIEventBase self, UIEventDelegate<P1> callback)
        {
            if (self is UIEventP1<P1> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError($"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        /// <summary>添加回调（2 参）</summary>
        public static UIEventHandleP2<P1, P2> AddEvent<P1, P2>(this UIEventBase self, UIEventDelegate<P1, P2> callback)
        {
            if (self is UIEventP2<P1, P2> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError($"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        /// <summary>添加回调（3 参）</summary>
        public static UIEventHandleP3<P1, P2, P3> AddEvent<P1, P2, P3>(this UIEventBase self, UIEventDelegate<P1, P2, P3> callback)
        {
            if (self is UIEventP3<P1, P2, P3> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError($"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        /// <summary>添加回调（4 参）</summary>
        public static UIEventHandleP4<P1, P2, P3, P4> AddEvent<P1, P2, P3, P4>(this UIEventBase self, UIEventDelegate<P1, P2, P3, P4> callback)
        {
            if (self is UIEventP4<P1, P2, P3, P4> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError($"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        /// <summary>添加回调（5 参）</summary>
        public static UIEventHandleP5<P1, P2, P3, P4, P5> AddEvent<P1, P2, P3, P4, P5>(this UIEventBase self, UIEventDelegate<P1, P2, P3, P4, P5> callback)
        {
            if (self is UIEventP5<P1, P2, P3, P4, P5> uiEventParam)
            {
                return uiEventParam.Add(callback);
            }
            else
            {
                Logger.LogError($"添加 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} {typeof(P5).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return null;
        }

        #endregion

        #region 移除事件

        /// <summary>移除回调（0 参）</summary>
        public static bool RemoveEvent(this UIEventBase self, UIEventHandleP0 handle)
        {
            if (self is UIEventP0 uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError($"移除 当前事件 {self.EventName} 参数不一致 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        /// <summary>移除回调（1 参）</summary>
        public static bool RemoveEvent<P1>(this UIEventBase self, UIEventHandleP1<P1> handle)
        {
            if (self is UIEventP1<P1> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError($"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        /// <summary>移除回调（2 参）</summary>
        public static bool RemoveEvent<P1, P2>(this UIEventBase self, UIEventHandleP2<P1, P2> handle)
        {
            if (self is UIEventP2<P1, P2> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError($"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        /// <summary>移除回调（3 参）</summary>
        public static bool RemoveEvent<P1, P2, P3>(this UIEventBase self, UIEventHandleP3<P1, P2, P3> handle)
        {
            if (self is UIEventP3<P1, P2, P3> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError($"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        /// <summary>移除回调（4 参）</summary>
        public static bool RemoveEvent<P1, P2, P3, P4>(this UIEventBase self, UIEventHandleP4<P1, P2, P3, P4> handle)
        {
            if (self is UIEventP4<P1, P2, P3, P4> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError($"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        /// <summary>移除回调（5 参）</summary>
        public static bool RemoveEvent<P1, P2, P3, P4, P5>(this UIEventBase self, UIEventHandleP5<P1, P2, P3, P4, P5> handle)
        {
            if (self is UIEventP5<P1, P2, P3, P4, P5> uiEventParam)
            {
                return uiEventParam.Remove(handle);
            }
            else
            {
                Logger.LogError($"移除 当前事件 {self.EventName} 参数不一致 -- 当前使用参数: {typeof(P1).Name} {typeof(P2).Name} {typeof(P3).Name} {typeof(P4).Name} {typeof(P5).Name} 请使用: {self.AllEventParamType.GetAllParamTypeTips()}");
            }

            return false;
        }

        #endregion

        #region 判断

        /// <summary>比较事件自身参数列表与目标参数列表是否一致</summary>
        public static bool ParamEquals(this UIEventBase self, List<EUIEventParamType> targetParamList)
        {
            return self.AllEventParamType.ParamEquals(targetParamList);
        }

        /// <summary>逐项比较两个参数类型列表是否一致（长度 + 每项相等）</summary>
        public static bool ParamEquals(this List<EUIEventParamType> self, List<EUIEventParamType> target)
        {
            if (self == null || target == null)
            {
                return false;
            }

            if (self.Count == 0 && target.Count == 0)
            {
                return true;
            }

            if (self.Count != target.Count)
            {
                return false; //长度都不一致还检查什么
            }

            for (var i = 0; i < self.Count; i++)
            {
                var selfParam   = self[i];
                var targetParam = target[i];
                if (selfParam != targetParam)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region 其他

        /// <summary>参数列表转字符串 List</summary>
        public static List<string> GetAllParamTypeToListString(this List<EUIEventParamType> self)
        {
            var list = new List<string>();
            foreach (var paramType in self)
            {
                list.Add(paramType.ToString());
            }

            return list;
        }

        /// <summary>参数列表拼接为字符串（空格分隔）</summary>
        public static string GetAllParamTypeToString(this List<EUIEventParamType> self)
        {
            var value = "";

            foreach (var paramType in self)
            {
                value += $" {paramType}";
            }

            return value;
        }

        /// <summary>参数提示串（含个数），用于参数不匹配时打印「应该用什么参数」</summary>
        public static string GetAllParamTypeTips(this List<EUIEventParamType> self)
        {
            if (self == null)
            {
                return "";
            }

            var value = $"参数个数:{self.Count}";

            foreach (var paramType in self)
            {
                value += $" , {paramType}";
            }

            return value;
        }

        #endregion
    }
}
