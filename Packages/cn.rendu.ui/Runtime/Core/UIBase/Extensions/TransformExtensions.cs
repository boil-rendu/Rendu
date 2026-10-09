using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>Transform 的位置设置与按名字查找扩展</summary>
    public static class TransformExtensions
    {
        /// <summary>一次性设置世界坐标</summary>
        public static void SetPosition(this Transform transform, float x, float y, float z)
        {
            transform.position = new Vector3(x, y, z);
        }

        /// <summary>一次性设置局部坐标</summary>
        public static void SetLocalPosition(this Transform transform, float x, float y, float z)
        {
            transform.localPosition = new Vector3(x, y, z);
        }

        /// <summary>一次性设置局部缩放</summary>
        public static void SetLocalScale(this Transform transform, float x, float y, float z)
        {
            transform.localScale = new Vector3(x, y, z);
        }

        /// <summary>
        /// 按 "A/B/C" 路径逐层精确查找（不走 Transform.Find，避免其字典缓存的额外开销）
        /// 空路径返回自身；任一层找不到即整体返回 null
        /// </summary>
        public static Transform FindHard(this Transform trans, string path)
        {
            if (path == string.Empty)
            {
                return trans;
            }

            var target = trans;

            var names = path.Split('/');
            foreach (var name in names)
            {
                bool find = false;
                foreach (Transform child in target)
                {
                    if (child.name == name)
                    {
                        target = child;
                        find   = true;
                        break;
                    }
                }

                if (!find)
                {
                    target = null;
                    break;
                }
            }

            return target;
        }

        /// <summary>深度优先查找名字等于 name1 或 name2 的子节点（含自身）</summary>
        public static Transform FindByName(this Transform trans, string name1, string name2)
        {
            if (trans.name.Equals(name1) || trans.name.Equals(name2))
            {
                return trans;
            }

            for (int i = 0; i < trans.childCount; ++i)
            {
                var child  = trans.GetChild(i);
                var result = FindByName(child, name1, name2);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>深度优先查找名字等于 name1 / name2 / name3 的子节点（含自身）</summary>
        public static Transform FindByName(this Transform trans,
                                           string         name1,
                                           string         name2,
                                           string         name3)
        {
            if (trans.name.Equals(name1) ||
                trans.name.Equals(name2) ||
                trans.name.Equals(name3))
            {
                return trans;
            }

            for (int i = 0; i < trans.childCount; ++i)
            {
                var child  = trans.GetChild(i);
                var result = FindByName(child, name1, name2, name3);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>深度优先查找指定名字的子节点（含自身）</summary>
        public static Transform FindByName(this Transform trans, string name)
        {
            if (trans.name == name)
            {
                return trans;
            }

            for (int i = 0; i < trans.childCount; ++i)
            {
                var child  = trans.GetChild(i);
                var result = FindByName(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>先按直接子节点找，找不到再深度优先递归（仅匹配直接子节点名，不含自身）</summary>
        public static Transform FindChildByName(this Transform trans, string childName)
        {
            var childT = trans.Find(childName);
            if (childT != null) return childT;

            for (int i = 0; i < trans.childCount; i++)
            {
                childT = FindChildByName(trans.GetChild(i), childName);
                if (childT != null) return childT;
            }

            return null;
        }
    }
}
