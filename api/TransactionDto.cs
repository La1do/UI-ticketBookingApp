using Newtonsoft.Json;

public class TransactionDto
{
    [JsonProperty("id")]
    public int id { get; set; }

    [JsonProperty("node_id")]
    public int nodeId { get; set; }

    [JsonProperty("action_type")]
    public string actionType { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string description { get; set; } = string.Empty;
    [JsonProperty("created_at")]
    public DateTime timestamp { get; set; }
}