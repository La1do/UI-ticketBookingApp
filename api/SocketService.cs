using System;
using System.Threading.Tasks;
using SocketIOClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json;

namespace api
{
    public class SocketService : IDisposable
    {
        private SocketIOClient.SocketIO? client;
        private bool isConnected = false;
        private bool isDisposing = false;
        private string serverUrl;
        private int reconnectAttemptCount = 0;
        private const int maxReconnectAttempts = 5;

        // Events để notify AdminDashboard
        public event Action<List<SeatDto>>? OnSeatsUpdate;
        public event Action<List<NodeDto>>? OnNodesUpdate;
        public event Action<List<TransactionDto>>? OnTransactionsUpdate;
        public event Action<List<ElectionDto>>? OnElectionUpdate;
        public event Action<bool>? OnConnectionStatusChanged;

        public SocketService(string url = "http://10.15.240.149:4000")
        {
            serverUrl = url.Replace("ws://", "http://").Replace("wss://", "https://").TrimEnd('/');
            Console.WriteLine($"[Socket] SocketService initialized with URL: {serverUrl}");
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (client != null && isConnected)
                {
                    Console.WriteLine($"[Socket] Already connected to {serverUrl}");
                    return;
                }

                Console.WriteLine($"[Socket] 🔌 Attempting to connect to {serverUrl}...");
                Console.WriteLine($"[Socket] Connection URL: {serverUrl}");

                client = new SocketIOClient.SocketIO(serverUrl, new SocketIOOptions
                {
                    Reconnection = true,
                    ReconnectionDelay = 1000,
                    ReconnectionDelayMax = 5000,
                    ReconnectionAttempts = 1
                });
                
                Console.WriteLine($"[Socket] Reconnection settings:");
                Console.WriteLine($"[Socket]   - Enabled: true");
                Console.WriteLine($"[Socket]   - Max attempts: 1");
                Console.WriteLine($"[Socket]   - Delay: 1000ms - 5000ms");

                // Setup event handlers với dispose check
                SetupEventHandlers();

                Console.WriteLine($"[Socket] Starting connection process...");
                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                // Log tất cả lỗi kết nối, bao gồm cả "Cannot connect"
                Console.WriteLine($"[Socket] ❌ FAILED to connect to {serverUrl}");
                Console.WriteLine($"[Socket] Error Type: {ex.GetType().Name}");
                Console.WriteLine($"[Socket] Error Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Socket] Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }
                Console.WriteLine($"[Socket] Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"[Socket] ⚠️ Will attempt to reconnect automatically...");
                isConnected = false;
                OnConnectionStatusChanged?.Invoke(false);
            }
        }

        private void SetupEventHandlers()
        {
            if (client == null) return;

            client.OnConnected += (sender, e) =>
            {
                if (isDisposing) return;
                isConnected = true;
                OnConnectionStatusChanged?.Invoke(true);
                reconnectAttemptCount = 0; // Reset counter khi connect thành công
                Console.WriteLine($"[Socket] ✅ Successfully connected to {serverUrl}");
                Console.WriteLine($"[Socket] Connected at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            };

            client.OnDisconnected += (sender, e) =>
            {
                if (isDisposing) return;
                isConnected = false;
                OnConnectionStatusChanged?.Invoke(false);
                reconnectAttemptCount++;
                Console.WriteLine($"[Socket] ⚠️ DISCONNECTED from {serverUrl}");
                Console.WriteLine($"[Socket] Disconnect Reason: {e}");
                Console.WriteLine($"[Socket] 🔄 Reconnection will be attempted automatically...");
                Console.WriteLine($"[Socket] Reconnection attempt count: {reconnectAttemptCount}/{maxReconnectAttempts}");
                Console.WriteLine($"[Socket] Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                
                if (reconnectAttemptCount >= maxReconnectAttempts)
                {
                    Console.WriteLine($"[Socket] ⚠️ WARNING: Maximum reconnection attempts ({maxReconnectAttempts}) reached!");
                    Console.WriteLine($"[Socket] ⚠️ Will stop trying to reconnect.");
                }
            };

            client.OnError += (sender, e) =>
            {
                if (isDisposing) return;
                Console.WriteLine($"[Socket] ❌ ERROR occurred: {e}");
                Console.WriteLine($"[Socket] Error details: {e?.ToString() ?? "Unknown error"}");
            };

            // Event khi reconnect thành công
            client.OnReconnected += (sender, e) =>
            {
                if (isDisposing) return;
                isConnected = true;
                OnConnectionStatusChanged?.Invoke(true);
                Console.WriteLine($"[Socket] ✅ RECONNECTED successfully to {serverUrl}");
                Console.WriteLine($"[Socket] Total reconnection attempts before success: {reconnectAttemptCount}");
                Console.WriteLine($"[Socket] Reconnected at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                reconnectAttemptCount = 0; // Reset counter sau khi reconnect thành công
            };

            // Seats update
            client.On("seatUpdate", response =>
            {
                if (isDisposing) return;
                try
                {
                    var seats = ParseResponse<List<SeatDto>>(response);
                    if (seats != null && seats.Count > 0)
                    {
                        OnSeatsUpdate?.Invoke(seats);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Socket] Error parsing seatUpdate: {ex.Message}");
                }
            });

            // Nodes update
            client.On("nodeUpdate", response =>
            {
                if (isDisposing) return;
                try
                {
                    var nodes = ParseResponse<List<NodeDto>>(response);
                    if (nodes != null && nodes.Count > 0)
                    {
                        OnNodesUpdate?.Invoke(nodes);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Socket] Error parsing nodeUpdate: {ex.Message}");
                }
            });

            // Transactions update
            client.On("transactionUpdate", response =>
            {
                if (isDisposing) return;
                try
                {
                    var transactions = ParseResponse<List<TransactionDto>>(response);
                    if (transactions != null && transactions.Count > 0)
                    {
                        OnTransactionsUpdate?.Invoke(transactions);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Socket] Error parsing transactionUpdate: {ex.Message}");
                }
            });

            // Admin update (tổng hợp)
            client.On("adminUpdate", response =>
            {
                if (isDisposing) return;
                try
                {
                    var json = JsonConvert.SerializeObject(response.GetValue<object>());
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    
                    if (data != null)
                    {
                        if (data.seats != null)
                        {
                            var seatsJson = data.seats.ToString();
                            if (!string.IsNullOrEmpty(seatsJson))
                            {
                                var seats = JsonConvert.DeserializeObject<List<SeatDto>>(seatsJson);
                                if (seats != null && seats.Count > 0)
                                {
                                    OnSeatsUpdate?.Invoke(seats);
                                }
                            }
                        }
                        if (data.nodes != null)
                        {
                            var nodesJson = data.nodes.ToString();
                            if (!string.IsNullOrEmpty(nodesJson))
                            {
                                var nodes = JsonConvert.DeserializeObject<List<NodeDto>>(nodesJson);
                                if (nodes != null && nodes.Count > 0)
                                {
                                    OnNodesUpdate?.Invoke(nodes);
                                }
                            }
                        }
                        if (data.transactions != null)
                        {
                            var transactionsJson = data.transactions.ToString();
                            if (!string.IsNullOrEmpty(transactionsJson))
                            {
                                var transactions = JsonConvert.DeserializeObject<List<TransactionDto>>(transactionsJson);
                                if (transactions != null && transactions.Count > 0)
                                {
                                    OnTransactionsUpdate?.Invoke(transactions);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Socket] Error parsing adminUpdate: {ex.Message}");
                }
            });

            // Election events
            client.On("election", response =>
            {
                if (isDisposing) return;
                try
                {
                    var election = ParseElectionEvent(response);
                    if (election != null && election.NodeId > 0)
                    {
                        OnElectionUpdate?.Invoke(new List<ElectionDto> { election });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Socket] Error handling election: {ex.Message}");
                }
            });

            // Election update (list)
            client.On("electionUpdate", response =>
            {
                if (isDisposing) return;
                try
                {
                    var elections = ParseResponse<List<ElectionDto>>(response);
                    if (elections != null && elections.Count > 0)
                    {
                        OnElectionUpdate?.Invoke(elections);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Socket] Error parsing electionUpdate: {ex.Message}");
                }
            });
        }

        private T? ParseResponse<T>(SocketIOResponse response) where T : class
        {
            try
            {
                return response.GetValue<T>();
            }
            catch
            {
                try
                {
                    var json = response.GetValue<string>();
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch
                {
                    var json = JsonConvert.SerializeObject(response.GetValue<object>());
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
        }

        private ElectionDto? ParseElectionEvent(SocketIOResponse response)
        {
            // Try direct parse
            try
            {
                var election = response.GetValue<ElectionDto>();
                if (election != null && election.NodeId > 0)
                    return election;
            }
            catch { }

            // Try from JSON string
            try
            {
                var json = response.GetValue<string>();
                if (!string.IsNullOrEmpty(json))
                {
                    var election = JsonConvert.DeserializeObject<ElectionDto>(json);
                    if (election != null && election.NodeId > 0)
                        return election;
                }
            }
            catch { }

            // Try from JsonElement
            try
            {
                var jsonElement = response.GetValue<JsonElement>();
                return ParseElectionFromJsonElement(jsonElement);
            }
            catch { }

            // Try from dynamic object
            try
            {
                var json = JsonConvert.SerializeObject(response.GetValue<object>());
                var data = JsonConvert.DeserializeObject<dynamic>(json);
                
                if (data?.nodeId != null)
                {
                    return new ElectionDto
                    {
                        NodeId = Convert.ToInt32(data.nodeId.ToString()),
                        EventType = data.type?.ToString() ?? "",
                        Message = data.message?.ToString() ?? "",
                        Timestamp = DateTime.Now
                    };
                }
            }
            catch { }

            return null;
        }

        private ElectionDto? ParseElectionFromJsonElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("nodeId", out var nodeIdProp))
                {
                    return new ElectionDto
                    {
                        NodeId = nodeIdProp.GetInt32(),
                        EventType = element.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "",
                        Message = element.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "",
                        Timestamp = DateTime.Now
                    };
                }
            }
            else if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0)
            {
                return ParseElectionFromJsonElement(element[0]);
            }
            return null;
        }

        public async Task DisconnectAsync()
        {
            if (client != null && !isDisposing)
            {
                try
                {
                    await client.DisconnectAsync();
                    isConnected = false;
                }
                catch
                {
                    // Ignore errors during disconnect
                }
            }
        }

        public bool IsConnected => isConnected && !isDisposing;

        public void Dispose()
        {
            if (isDisposing) return;
            
            isDisposing = true;
            isConnected = false;

            // Unsubscribe all events IMMEDIATELY
            OnSeatsUpdate = null;
            OnNodesUpdate = null;
            OnTransactionsUpdate = null;
            OnElectionUpdate = null;
            OnConnectionStatusChanged = null;

            // Disconnect asynchronously without blocking
            if (client != null)
            {
                try
                {
                    // Fire and forget - không chờ disconnect hoàn thành
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await client.DisconnectAsync();
                        }
                        catch { }
                        finally
                        {
                            client?.Dispose();
                        }
                    });
                }
                catch
                {
                    // Nếu Task.Run fail, dispose trực tiếp
                    try
                    {
                        client?.Dispose();
                    }
                    catch { }
                }
            }
        }
    }
}