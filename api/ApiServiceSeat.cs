using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class ApiServiceSeat
{
    private static readonly HttpClient client = new HttpClient()
    {
        BaseAddress = new Uri("http://localhost:3000"),  // API Node.js
        Timeout = TimeSpan.FromSeconds(5)                // timeout 5 giây
    };

    public static async Task<List<SeatDto>> GetSeatsAsync()
    {
        try
        {
            var response = await client.GetStringAsync("/seat");
            var seats = JsonConvert.DeserializeObject<List<SeatDto>>(response);
            return seats ?? new List<SeatDto>();
        }
        catch (HttpRequestException ex)
        {
            // Lỗi kết nối mạng, DNS, server unreachable - throw để caller biết
            Console.WriteLine("Network error: " + ex.Message);
            throw; // Throw lại exception để caller có thể xử lý
        }
        catch (TaskCanceledException ex)
        {
            // Timeout - throw để caller biết
            Console.WriteLine("Request timed out.");
            throw new HttpRequestException("Request timed out. Server may be offline.", ex);
        }
        catch (Exception ex)
        {
            // Lỗi khác - throw để caller biết
            Console.WriteLine("Unexpected error: " + ex.Message);
            throw;
        }
    }

    public static async Task<bool> BookSeatAsync(string seatId, string customerName)
    {
        try
        {
            var data = new
            {
                seatId = seatId,
                customerName = customerName
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/seat/book", content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Server returned error: " + response.StatusCode);
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Network error: " + ex.Message);
            return false;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Request timed out.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error: " + ex.Message);
            return false;
        }
    }
}
