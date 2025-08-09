using System;
using System.Globalization;

namespace Kart.Project_Files.Scripts.Extensions
{
    public static class LapTimeExtensions
    {
        public static string ToRaceFormat(this string rawLapTime)
        {
            if (string.IsNullOrEmpty(rawLapTime))
                return "N/A";

            var normalized = rawLapTime
                .TrimEnd('s')
                .Replace(',', '.');
            if (!float.TryParse(normalized, NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var seconds))
                return rawLapTime;

            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}:{ts.Milliseconds:00}";
        }
    }
}