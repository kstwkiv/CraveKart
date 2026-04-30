namespace FoodFleet.Shared.Events;

/// <summary>
/// Provides the current date and time in Indian Standard Time (IST, UTC+5:30).
/// Use <see cref="Now"/> instead of <see cref="DateTime.UtcNow"/> throughout the application
/// so all timestamps are stored and displayed in IST.
/// </summary>
public static class IstClock
{
    private static readonly TimeZoneInfo Ist =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "India Standard Time"
                : "Asia/Kolkata");

    /// <summary>Gets the current date and time in IST (UTC+5:30).</summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);
}
