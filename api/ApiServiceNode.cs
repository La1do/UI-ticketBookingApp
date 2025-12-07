using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace api;

public static class ApiServiceNode
{
  private static readonly HttpClient client = new HttpClient()
  {
    BaseAddress = new Uri("http://localhost:3000"),  // API Node.js
    Timeout = TimeSpan.FromSeconds(5)                // timeout 5 giây
  };

  public static async Task<List<NodeDto>> GetNodesAsync()
  {
    try
    {
      var response = await client.GetStringAsync("/node");
      return JsonConvert.DeserializeObject<List<NodeDto>>(response);
    }
    catch (HttpRequestException ex)
    {
      // Lỗi kết nối mạng, DNS, server unreachable
      Console.WriteLine("Network error: " + ex.Message);
      return new List<NodeDto>(); // trả về rỗng
    }
    catch (TaskCanceledException)
    {
      // Timeout
      Console.WriteLine("Request timed out.");
      return new List<NodeDto>();
    }
    catch (Exception ex)
    {
      // Lỗi khác
      Console.WriteLine("Unexpected error: " + ex.Message);
      return new List<NodeDto>();
    }
  }


}