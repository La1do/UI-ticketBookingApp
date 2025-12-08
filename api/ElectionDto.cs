using Newtonsoft.Json;

namespace api;

public class ElectionDto
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("nodeId")]
    public int NodeId { get; set; }

    [JsonProperty("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("senderId")]
    public int? SenderId { get; set; }

    [JsonProperty("leaderId")]
    public int? LeaderId { get; set; }
}
