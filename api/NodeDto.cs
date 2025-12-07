using Newtonsoft.Json;
public class NodeDto
{
  [JsonProperty("id")]
  public int id { get; set; }

  [JsonProperty("isAlive")]
  public bool isAlive { get; set; }
  
  [JsonProperty("isLeader")]
  public bool isLeader { get; set; }

  [JsonProperty("lastHeartbeat")]
  public string lastHeartbeat { get; set; } = string.Empty;

}