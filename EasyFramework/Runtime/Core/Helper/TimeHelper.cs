
using System;

namespace EasyFramework
{
    public static class TimeHelper
    {
        /// <summary>
        /// 将时间转换成 "00:00:00" 格式
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public static string FormatTimeString(this int second)
        {
            if (second <= 0) return "00:00:00";
            return TimeSpan.FromSeconds(second).ToString("hh\\:mm\\:ss");
        }
        
        public static DateTime ConvertMillSecondsToLocalDateTime(long millSeconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(millSeconds).LocalDateTime;
        }
        
        // 解析目标时间（HH:mm:ss）
        public static bool TryCalculateRemainingSeconds(string timeStr, out double remainingSeconds)
        {
            remainingSeconds = 0;
            // 1. 解析目标时间（HH:mm:ss）
            if (!TimeSpan.TryParse(timeStr, out TimeSpan targetSpan)) return false;

            // 2. 获取当前时间的时分秒
            TimeSpan nowSpan = DateTime.Now.TimeOfDay;
            // 3. 计算剩余秒数：如果当前时间已过目标时间 → 取明天的目标时间（跨天处理）
            if (nowSpan <= targetSpan)
            {
                // 今天还没到目标时间 → 直接计算差值
                remainingSeconds = (targetSpan - nowSpan).TotalSeconds;
            }
            else
            {
                // 今天已过目标时间 → 计算到明天目标时间的差值（24小时 - 今天已过时间 + 目标时间）
                remainingSeconds = 24 * 3600 - nowSpan.TotalSeconds + targetSpan.TotalSeconds;
            }
            return true;
        }

    }
}
