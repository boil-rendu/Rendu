using RD.Core;
namespace RD.Client
{
    /// <summary>数值区间校验工具</summary>
    public static class NumChecker
    {
        /// <summary>
        /// 把 value 夹到 [min, max] 内并写回（ref）
        /// 返回 true 表示原本就在区间内、无需修改；false 表示被夹紧过
        /// </summary>
        public static bool CheckRange(ref float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
                return false;
            }

            if (value > max)
            {
                value = max;
                return false;
            }

            return true;
        }

        /// <summary>取夹到 [min, max] 后的值（不改动入参）</summary>
        public static float GetRange(float value, float min, float max)
        {
            CheckRange(ref value, min, max);
            return value;
        }
    }
}
