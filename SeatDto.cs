using Newtonsoft.Json;

public class SeatDto
{
    [JsonProperty("id")]
    public int id { get; set; }
    
    [JsonProperty("seat_number")]
    public string seatNumber { get; set; } = string.Empty;
    
    [JsonProperty("status")]
    public string status { get; set; } = string.Empty;  // Changed from int to string
    
    [JsonProperty("customer_name")]
    public string? customerName { get; set; }
    
    [JsonProperty("booked_by_node_id")]
    public int? bookedByNode { get; set; }

    // Helper để trả về bool
    public bool IsAvailable => status?.ToUpper() == "AVAILABLE";
    
    public bool IsOccupied => status?.ToUpper() == "BOOKED";
}