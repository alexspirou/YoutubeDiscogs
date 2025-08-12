
// SystemTextJsonSerializer.cs
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared.Serialization;
public static class SystemTextJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, _options);

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);

    }
    public static T Deserialize<T>(ReadOnlySpan<byte> bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes)
            ?? throw new InvalidOperationException("Deserialization failed");
    }

}

