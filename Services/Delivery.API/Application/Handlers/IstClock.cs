namespace Delivery.API.Application.Handlers;

/// <summary>Provides the current date/time in Indian Standard Time (IST, UTC+5:30).</summary>
internal static class IstClock
{
    private static readonly TimeZoneInfo Ist =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);
}
