using System;

namespace ImageScannerPicker.DynamicWebTwain
{
    internal static class DateTimeOffsetExtension
    {
        internal static long ToUnixTimeMilliseconds(this DateTimeOffset dateTimeOffset)
        {
            // 1970년 1월 1일부터 현재까지의 TimeSpan을 얻습니다.
            TimeSpan timeSpan = dateTimeOffset - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

            // TimeSpan을 밀리초로 변환하여 반환합니다.
            return (long)timeSpan.TotalMilliseconds;
        }
    }
}
