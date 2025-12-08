using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace api;

public static class ApiServiceElection
{
    private static readonly HttpClient client = new HttpClient()
    {
        BaseAddress = new Uri("http://localhost:3000"),
        Timeout = TimeSpan.FromSeconds(5)
    };

    /// <summary>
    /// Lấy danh sách tất cả election events
    /// </summary>
    public static async Task<List<ElectionDto>> GetElectionEventsAsync()
    {
        try
        {
            var response = await client.GetStringAsync("/election/events");
            return JsonConvert.DeserializeObject<List<ElectionDto>>(response) ?? new List<ElectionDto>();
        }
        catch (HttpRequestException ex)
        {
            return new List<ElectionDto>();
        }
        catch (TaskCanceledException)
        {
            return new List<ElectionDto>();
        }
        catch (Exception ex)
        {
            return new List<ElectionDto>();
        }
    }

    /// <summary>
    /// Ping endpoint để kiểm tra node còn sống
    /// </summary>
    public static async Task<bool> PingAsync()
    {
        try
        {
            var response = await client.GetAsync("/election/ping");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gửi election request
    /// </summary>
    public static async Task<bool> SendElectionAsync(int senderId)
    {
        try
        {
            var data = new { senderId = senderId };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/election/election", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Gửi victory notification
    /// </summary>
    public static async Task<bool> SendVictoryAsync(int leaderId)
    {
        try
        {
            var response = await client.PostAsync($"/election/victory/{leaderId}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
