using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class ApiService
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
            return JsonConvert.DeserializeObject<List<SeatDto>>(response);
        }
        catch (HttpRequestException ex)
        {
            // Lỗi kết nối mạng, DNS, server unreachable
            Console.WriteLine("Network error: " + ex.Message);
            return new List<SeatDto>(); // trả về rỗng
        }
        catch (TaskCanceledException)
        {
            // Timeout
            Console.WriteLine("Request timed out.");
            return new List<SeatDto>();
        }
        catch (Exception ex)
        {
            // Lỗi khác
            Console.WriteLine("Unexpected error: " + ex.Message);
            return new List<SeatDto>();
        }
    }

    public static async Task<bool> BookSeatAsync(string seatNumber, string customerName)
    {
        try
        {
            var data = new
            {
                seatNumber = seatNumber,
                customerName = customerName
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/seats/book", content);

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
