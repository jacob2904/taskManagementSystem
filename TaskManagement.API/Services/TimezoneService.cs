using System;

namespace TaskManagement.API.Services;

/// <summary>
/// Service for handling timezone conversions using computer's local timezone
/// </summary>
public static class TimezoneService
{
    private static readonly TimeZoneInfo LocalTimeZone = TimeZoneInfo.Local;

    /// <summary>
    /// Get current time in local timezone
    /// </summary>
    public static DateTime Now => DateTime.Now;

    /// <summary>
    /// Convert UTC to local timezone
    /// </summary>
    public static DateTime FromUtc(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, LocalTimeZone);
    }

    /// <summary>
    /// Convert local time to UTC
    /// </summary>
    public static DateTime ToUtc(DateTime localDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, LocalTimeZone);
    }

    /// <summary>
    /// Parse datetime-local input and convert to UTC for storage
    /// </summary>
    public static DateTime ParseLocalToUtc(DateTime localDateTime)
    {
        // Treat the input as local time and convert to UTC
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, LocalTimeZone);
    }
}
