using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Responses;

public class ErrorApiResponse
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("errors")]
    public required Dictionary<string, List<string>> ErrorsByRequestFieldName { get; init; }

    [JsonPropertyName("info")]
    public required string Info { get; init; }
}