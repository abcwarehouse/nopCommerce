namespace Nop.Plugin.Misc.AbcEventSurveys.Services
{
    /// <summary>
    /// Times are stored in UTC (the 24-hour duplicate-entry check and event open/closed check
    /// compare against DateTime.UtcNow), but shown to staff in Detroit time - Eastern, with
    /// daylight saving handled automatically.
    /// </summary>
    public static class SurveyTimeZone
    {
        public const string Label = "ET";

        private static readonly TimeZoneInfo _detroit = FindDetroitTimeZone();

        public static DateTime FromUtc(DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _detroit);
        }

        public static DateTime? FromUtc(DateTime? utc)
        {
            return utc.HasValue ? FromUtc(utc.Value) : null;
        }

        /// <summary>
        /// Converts a Detroit wall-clock time entered by staff (e.g. the event End Date) to UTC for storage.
        /// </summary>
        public static DateTime? ToUtc(DateTime? detroitTime)
        {
            if (!detroitTime.HasValue)
                return null;

            var local = DateTime.SpecifyKind(detroitTime.Value, DateTimeKind.Unspecified);

            // 2:00-2:59 AM on the spring-forward day doesn't exist; ConvertTimeToUtc would throw.
            if (_detroit.IsInvalidTime(local))
                local = local.AddHours(1);

            return TimeZoneInfo.ConvertTimeToUtc(local, _detroit);
        }

        /// <summary>
        /// Same as <see cref="FromUtc(DateTime)"/> but keeps the -05:00 / -04:00 offset, for machine-readable exports.
        /// </summary>
        public static DateTimeOffset FromUtcWithOffset(DateTime utc)
        {
            return TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)), _detroit);
        }

        private static TimeZoneInfo FindDetroitTimeZone()
        {
            // IANA id works on Linux and on Windows with ICU; fall back to the Windows id.
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Detroit");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }
        }
    }
}
