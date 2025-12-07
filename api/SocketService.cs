using System;
using System.Threading.Tasks;
using SocketIOClient;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace api
{
    public class SocketService : IDisposable
    {
        private SocketIOClient.SocketIO? client;
        private bool isConnected = false;
        private string serverUrl;

        // Events để notify AdminDashboard
        public event Action<List<SeatDto>>? OnSeatsUpdate;
        public event Action<List<NodeDto>>? OnNodesUpdate;
        public event Action<List<TransactionDto>>? OnTransactionsUpdate;
        public event Action<bool>? OnConnectionStatusChanged;

        public SocketService(string url = "http://localhost:3000")
        {
            serverUrl = url;
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (client != null && isConnected)
                {
                    return; // Đã kết nối rồi
                }

                client = new SocketIOClient.SocketIO(serverUrl, new SocketIOOptions
                {
                    Reconnection = true,
                    ReconnectionDelay = 1000,
                    ReconnectionDelayMax = 5000,
                    ReconnectionAttempts = 5
                });

                // Lắng nghe sự kiện kết nối
                client.OnConnected += (sender, e) =>
                {
                    isConnected = true;
                    OnConnectionStatusChanged?.Invoke(true);
                    Console.WriteLine("Socket connected");
                };

                // Lắng nghe sự kiện ngắt kết nối
                client.OnDisconnected += (sender, e) =>
                {
                    isConnected = false;
                    OnConnectionStatusChanged?.Invoke(false);
                    Console.WriteLine("Socket disconnected");
                };

                // Lắng nghe sự kiện lỗi
                client.OnError += (sender, e) =>
                {
                    Console.WriteLine($"Socket error: {e}");
                };

                // Lắng nghe cập nhật seats
                client.On("seatUpdate", response =>
                {
                    try
                    {
                        Console.WriteLine($"[Socket] Received seatUpdate event at {DateTime.Now:HH:mm:ss.fff}");
                        List<SeatDto>? seats = null;
                        
                        // Thử parse theo nhiều cách khác nhau
                        try
                        {
                            seats = response.GetValue<List<SeatDto>>();
                            Console.WriteLine($"[Socket] Parsed {seats?.Count ?? 0} seats directly");
                        }
                        catch
                        {
                            try
                            {
                                var json = response.GetValue<string>();
                                seats = JsonConvert.DeserializeObject<List<SeatDto>>(json);
                                Console.WriteLine($"[Socket] Parsed {seats?.Count ?? 0} seats from JSON string");
                            }
                            catch
                            {
                                var json = JsonConvert.SerializeObject(response.GetValue<object>());
                                seats = JsonConvert.DeserializeObject<List<SeatDto>>(json);
                                Console.WriteLine($"[Socket] Parsed {seats?.Count ?? 0} seats from object serialization");
                            }
                        }
                        
                        if (seats != null && seats.Count > 0)
                        {
                            Console.WriteLine($"[Socket] Invoking OnSeatsUpdate with {seats.Count} seats");
                            OnSeatsUpdate?.Invoke(seats);
                        }
                        else
                        {
                            Console.WriteLine("[Socket] Warning: seatUpdate received but no seats parsed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Socket] Error parsing seatUpdate: {ex.Message}");
                        Console.WriteLine($"[Socket] Stack trace: {ex.StackTrace}");
                    }
                });

                // Lắng nghe cập nhật nodes
                client.On("nodeUpdate", response =>
                {
                    try
                    {
                        Console.WriteLine($"[Socket] Received nodeUpdate event at {DateTime.Now:HH:mm:ss.fff}");
                        List<NodeDto>? nodes = null;
                        
                        // Thử parse theo nhiều cách khác nhau
                        try
                        {
                            nodes = response.GetValue<List<NodeDto>>();
                            Console.WriteLine($"[Socket] Parsed {nodes?.Count ?? 0} nodes directly");
                        }
                        catch
                        {
                            try
                            {
                                var json = response.GetValue<string>();
                                nodes = JsonConvert.DeserializeObject<List<NodeDto>>(json);
                                Console.WriteLine($"[Socket] Parsed {nodes?.Count ?? 0} nodes from JSON string");
                            }
                            catch
                            {
                                var json = JsonConvert.SerializeObject(response.GetValue<object>());
                                nodes = JsonConvert.DeserializeObject<List<NodeDto>>(json);
                                Console.WriteLine($"[Socket] Parsed {nodes?.Count ?? 0} nodes from object serialization");
                            }
                        }
                        
                        if (nodes != null && nodes.Count > 0)
                        {
                            Console.WriteLine($"[Socket] Invoking OnNodesUpdate with {nodes.Count} nodes");
                            OnNodesUpdate?.Invoke(nodes);
                        }
                        else
                        {
                            Console.WriteLine("[Socket] Warning: nodeUpdate received but no nodes parsed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Socket] Error parsing nodeUpdate: {ex.Message}");
                    }
                });

                // Lắng nghe cập nhật transactions
                client.On("transactionUpdate", response =>
                {
                    try
                    {
                        Console.WriteLine($"[Socket] Received transactionUpdate event at {DateTime.Now:HH:mm:ss.fff}");
                        List<TransactionDto>? transactions = null;
                        
                        // Thử parse theo nhiều cách khác nhau
                        try
                        {
                            transactions = response.GetValue<List<TransactionDto>>();
                            Console.WriteLine($"[Socket] Parsed {transactions?.Count ?? 0} transactions directly");
                        }
                        catch
                        {
                            try
                            {
                                var json = response.GetValue<string>();
                                transactions = JsonConvert.DeserializeObject<List<TransactionDto>>(json);
                                Console.WriteLine($"[Socket] Parsed {transactions?.Count ?? 0} transactions from JSON string");
                            }
                            catch
                            {
                                var json = JsonConvert.SerializeObject(response.GetValue<object>());
                                transactions = JsonConvert.DeserializeObject<List<TransactionDto>>(json);
                                Console.WriteLine($"[Socket] Parsed {transactions?.Count ?? 0} transactions from object serialization");
                            }
                        }
                        
                        if (transactions != null && transactions.Count > 0)
                        {
                            Console.WriteLine($"[Socket] Invoking OnTransactionsUpdate with {transactions.Count} transactions");
                            OnTransactionsUpdate?.Invoke(transactions);
                        }
                        else
                        {
                            Console.WriteLine("[Socket] Warning: transactionUpdate received but no transactions parsed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Socket] Error parsing transactionUpdate: {ex.Message}");
                    }
                });

                // Lắng nghe cập nhật tổng hợp (có thể server gửi tất cả cùng lúc)
                client.On("adminUpdate", response =>
                {
                    try
                    {
                        Console.WriteLine($"[Socket] Received adminUpdate event at {DateTime.Now:HH:mm:ss.fff}");
                        var json = JsonConvert.SerializeObject(response.GetValue<object>());
                        var data = JsonConvert.DeserializeObject<dynamic>(json);
                        
                        if (data != null)
                        {
                            if (data.seats != null)
                            {
                                var seatsJson = data.seats.ToString();
                                var seats = JsonConvert.DeserializeObject<List<SeatDto>>(seatsJson);
                                if (seats != null && seats.Count > 0)
                                {
                                    Console.WriteLine($"[Socket] Invoking OnSeatsUpdate from adminUpdate with {seats.Count} seats");
                                    OnSeatsUpdate?.Invoke(seats);
                                }
                            }
                            if (data.nodes != null)
                            {
                                var nodesJson = data.nodes.ToString();
                                var nodes = JsonConvert.DeserializeObject<List<NodeDto>>(nodesJson);
                                if (nodes != null && nodes.Count > 0)
                                {
                                    Console.WriteLine($"[Socket] Invoking OnNodesUpdate from adminUpdate with {nodes.Count} nodes");
                                    OnNodesUpdate?.Invoke(nodes);
                                }
                            }
                            if (data.transactions != null)
                            {
                                var transactionsJson = data.transactions.ToString();
                                var transactions = JsonConvert.DeserializeObject<List<TransactionDto>>(transactionsJson);
                                if (transactions != null && transactions.Count > 0)
                                {
                                    Console.WriteLine($"[Socket] Invoking OnTransactionsUpdate from adminUpdate with {transactions.Count} transactions");
                                    OnTransactionsUpdate?.Invoke(transactions);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Socket] Error parsing adminUpdate: {ex.Message}");
                    }
                });

                // Lắng nghe tất cả events để debug
                client.OnAny((eventName, response) =>
                {
                    Console.WriteLine($"[Socket] Received event: {eventName} at {DateTime.Now:HH:mm:ss.fff}");
                });

                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to socket: {ex.Message}");
                isConnected = false;
                OnConnectionStatusChanged?.Invoke(false);
            }
        }

        public async Task DisconnectAsync()
        {
            if (client != null)
            {
                await client.DisconnectAsync();
                isConnected = false;
                OnConnectionStatusChanged?.Invoke(false);
            }
        }

        public bool IsConnected => isConnected;

        public void Dispose()
        {
            DisconnectAsync().Wait();
            client?.Dispose();
        }
    }
}
