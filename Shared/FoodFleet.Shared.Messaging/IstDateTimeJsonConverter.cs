using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoodFleet.Shared.Messaging;

/// <summary>
/// System.Text.Json converter that serialises <see cref="DateTime"/> values with the
/// IST offset (+05:30) so clients receive an unambiguous, timezone-aware string
/// (e.g. "2026-04-30T15:30:00+05:30") instead of the bare local form that has no
/// timezone indicator.
/// </summary>
public sealed class IstDateTimeJsonConverter : JsonConverter<DateTime>
{
    private static readonly TimeZoneInfo Ist =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");

    private static readonly TimeSpan IstOffset = TimeSpan.FromHours(5.5);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Convert to IST if the value is UTC or unspecified, then write with +05:30 offset
        var ist = value.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(value, Ist)
            : value;

        var dto = new DateTimeOffset(ist, IstOffset);
        writer.WriteStringValue(dto.ToString("yyyy-MM-ddTHH:mm:sszzz"));
    }
}

/// <summary>Same as <see cref="IstDateTimeJsonConverter"/> but for nullable DateTime.</summary>
public sealed class IstNullableDateTimeJsonConverter : JsonConverter<DateTime?>
{
    private static readonly IstDateTimeJsonConverter Inner = new();

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else Inner.Write(writer, value.Value, options);
    }
}
