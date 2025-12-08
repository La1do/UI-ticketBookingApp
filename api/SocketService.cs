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

        // Events để notify AdminDashboard
        public event Action<List<SeatDto>>? OnSeatsUpdate;
        public event Action<List<NodeDto>>? OnNodesUpdate;
        public event Action<List<TransactionDto>>? OnTransactionsUpdate;
        public event Action<List<ElectionDto>>? OnElectionUpdate;
        public event Action<bool>? OnConnectionStatusChanged;

        public SocketService(string url = "http://localhost:4000")
        {
            serverUrl = url.Replace("ws://", "http://").Replace("wss://", "https://").TrimEnd('/');
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (client != null && isConnected)
                {
                    return;
                }

                client = new SocketIOClient.SocketIO(serverUrl, new SocketIOOptions
                {
                    Reconnection = true,
                    ReconnectionDelay = 1000,
                    ReconnectionDelayMax = 5000,
                    ReconnectionAttempts = 5
                });

                // Setup event handlers với dispose check
                SetupEventHandlers();

                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Cannot connect"))
                {
                    Console.WriteLine($"[Socket] Connection error: {ex.Message}");
                }
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
                Console.WriteLine("[Socket] Connected");
            };

            client.OnDisconnected += (sender, e) =>
            {
                if (isDisposing) return;
                isConnected = false;
                OnConnectionStatusChanged?.Invoke(false);
                Console.WriteLine("[Socket] Disconnected");
            };

            client.OnError += (sender, e) =>
            {
                if (isDisposing) return;
                Console.WriteLine($"[Socket] Error: {e}");
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
                            var seats = JsonConvert.DeserializeObject<List<SeatDto>>(data.seats.ToString());
                            if (seats != null && seats.Count > 0)
                            {
                                OnSeatsUpdate?.Invoke(seats);
                            }
                        }
                        if (data.nodes != null)
                        {
                            var nodes = JsonConvert.DeserializeObject<List<NodeDto>>(data.nodes.ToString());
                            if (nodes != null && nodes.Count > 0)
                            {
                                OnNodesUpdate?.Invoke(nodes);
                            }
                        }
                        if (data.transactions != null)
                        {
                            var transactions = JsonConvert.DeserializeObject<List<TransactionDto>>(data.transactions.ToString());
                            if (transactions != null && transactions.Count > 0)
                            {
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