using DiscogsYoutube.Shared.Responses;
using System.Text.Json.Serialization;

namespace YoutubeDiscogs.Client;
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DiscogsSearchResponse))]
public partial class DiscogsJsonContext : JsonSerializerContext
{
}
