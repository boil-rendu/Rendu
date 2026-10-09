using System;
using System.Text;
using RD.Core;

namespace RD
{
    /// <summary>时长 / 时间戳的显示格式化工具</summary>
    public static partial class TimeDisplayHelper
    {
        [StaticField]
        private static readonly TimeSpan DayThreshold = TimeSpan.FromDays(1);

        [StaticField]
        private static readonly TimeSpan HourThreshold = TimeSpan.FromHours(1);

        [StaticField]
        private static readonly TimeSpan MinuteThreshold = TimeSpan.FromMinutes(1);

        #if UILocalize
        [StaticField]
        private static readonly string DayLocalize = "D";

        [StaticField]
        private static readonly string HourLocalize = "H";

        [StaticField]
        private static readonly string MinuteLocalize = "M";

        [StaticField]
        private static readonly string SecondLocalize = "S";
        #else
        [StaticField]
        private static readonly string DayLocalize = "D";

        [StaticField]
        private static readonly string HourLocalize = "H";

        [StaticField]
        private static readonly string MinuteLocalize = "M";

        [StaticField]
        private static readonly string SecondLocalize = "S";
        #endif

        // 时间格式化（支持多语言和灵活单位）
        /// <summary>
        /// 把秒数格式化为可读时长（负数一律返回「0 秒」）
        /// Compact：最多显示两级单位（如 1D2H、3M4S）；Full：显示全部非零单位并以空格分隔
        /// </summary>
        public static string FormatDuration(long seconds, EDurationFormat format = EDurationFormat.Compact)
        {
            if (seconds < 0)
            {
                return $"0{SecondLocalize}";
            }

            var timeSpan = TimeSpan.FromSeconds(seconds);
            var sb = new StringBuilder();

            if (format == EDurationFormat.Compact)
            {
                if (timeSpan.Days > 0)
                {
                    sb.AppendFormat($"{timeSpan.Days}{DayLocalize}");
                    if (timeSpan.Hours > 0)
                    {
                        sb.AppendFormat($"{timeSpan.Hours}{HourLocalize}");
                    }
                }
                else if (timeSpan.Hours > 0)
                {
                    sb.AppendFormat($"{timeSpan.Hours}{HourLocalize}");
                    if (timeSpan.Minutes > 0)
                    {
                        sb.AppendFormat($"{timeSpan.Minutes}{MinuteLocalize}");
                    }
                }
                else
                {
                    if (timeSpan.Minutes > 0)
                    {
                        sb.AppendFormat($"{timeSpan.Minutes}{MinuteLocalize}");
                    }

                    if (timeSpan.Seconds > 0)
                    {
                        sb.AppendFormat($"{timeSpan.Seconds}{SecondLocalize}");
                    }
                }
            }
            else
            {
                if (timeSpan.Days > 0)
                {
                    sb.AppendFormat($"{timeSpan.Days}{DayLocalize}").Append(' ');
                }

                if (timeSpan.Hours > 0)
                {
                    sb.AppendFormat($"{timeSpan.Hours}{HourLocalize}").Append(' ');
                }

                if (timeSpan.Minutes > 0)
                {
                    sb.AppendFormat($"{timeSpan.Minutes}{MinuteLocalize}").Append(' ');
                }

                if (timeSpan.Seconds > 0 || sb.Length == 0)
                {
                    sb.AppendFormat($"{timeSpan.Seconds}{SecondLocalize}");
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>把毫秒格式化为 mm:ss</summary>
        public static string FormatMinutesSeconds(long milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }

        /// <summary>把 Unix 秒级时间戳按指定格式串格式化（toLocal 控制是否转本地时间）</summary>
        public static string FormatTimestamp(long timestamp, string format = "yyyy-MM-dd HH:mm:ss", bool toLocal = false)
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            return (toLocal ? dateTime.LocalDateTime : dateTime.DateTime).ToString(format);
        }
    }

    /// <summary>时长格式模式</summary>
    public enum EDurationFormat
    {
        Compact, // 紧凑模式（最多显示两个单位）
        Full // 完整模式（显示所有非零单位）
    }
}