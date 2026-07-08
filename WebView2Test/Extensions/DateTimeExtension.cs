namespace WebView2Test.Extensions
{
    public static class DateTimeExtension
    {
        public static long ToTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime - DateTime.UnixEpoch).TotalMilliseconds;
        }
        public static DateTime ToUtcDateTime(this long timestamp)
        {
            return DateTime.UnixEpoch.AddMilliseconds(timestamp);
        }
    }
}
