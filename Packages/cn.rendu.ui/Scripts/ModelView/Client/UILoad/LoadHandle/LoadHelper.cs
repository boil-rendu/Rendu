using System.Collections.Generic;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>LoadHandle 的全局登记表（包名 → 资源名 → 句柄），实现同一资源只加载一份</summary>
    [EnableClass]
    public static partial class LoadHelper
    {
        /// <summary>包名为空时使用的占位包名</summary>
        private const string c_NullPkgName = "Default";

        /// <summary>包名 → 资源名 → 加载句柄</summary>
        [StaticField]
        private static readonly Dictionary<string, Dictionary<string, LoadHandle>> m_AllLoadDic = new();

        /// <summary>
        /// 取（不存在则创建）指定包名+资源名的加载句柄
        /// 包名为空时归入 Default 包
        /// </summary>
        public static LoadHandle GetLoad(string pkgName, string resName)
        {
            if (string.IsNullOrEmpty(pkgName))
            {
                pkgName = c_NullPkgName;
            }

            if (!m_AllLoadDic.ContainsKey(pkgName))
            {
                m_AllLoadDic.Add(pkgName, new Dictionary<string, LoadHandle>());
            }

            var pkgDic = m_AllLoadDic[pkgName];

            if (!pkgDic.ContainsKey(resName))
            {
                var group = RefPool.Get<LoadHandle>();
                group.SetGroupHandle(pkgName, resName);
                pkgDic.Add(resName, group);
            }

            return pkgDic[resName];
        }

        /// <summary>移除登记并从对象反查表中摘除，句柄归还池；包名或资源名不存在返回 false</summary>
        public static bool PutLoad(string pkgName, string resName)
        {
            if (string.IsNullOrEmpty(pkgName))
            {
                pkgName = c_NullPkgName;
            }

            if (!m_AllLoadDic.TryGetValue(pkgName, out var pkgDic))
            {
                return false;
            }

            if (!pkgDic.Remove(resName, out LoadHandle load))
            {
                return false;
            }

            RemoveLoadHandle(load);
            RefPool.Put(load);
            return true;
        }

        /// <summary>清空全部登记与对象反查表（句柄统一归还池）</summary>
        public static void PutAll()
        {
            foreach (var pkgDic in m_AllLoadDic.Values)
            {
                foreach (var load in pkgDic.Values)
                {
                    RefPool.Put(load);
                }
            }

            m_AllLoadDic.Clear();
            m_ObjLoadHandle.Clear();
        }
    }
}