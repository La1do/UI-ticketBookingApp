using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using api;

namespace BullyAlgorithmDemo
{
    public class AdminDashboard : Form
    {
        private Panel headerPanel = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private Button systemActiveButton = null!;
        private Button refreshButton = null!;
        private Button electionInfoButton = null!;
        private bool isDisposing = false;
        private Panel nodePanel = null!;
        private Label nodeStatusLabel = null!;
        private NodeControl[] nodeControls = null!;

        private Panel seatMapPanel = null!;
        private Label seatMapLabel = null!;
        private SeatControl[,] seatControls = null!;

        private Panel transactionPanel = null!;
        private Label transactionLabel = null!;
        private DataGridView transactionGrid = null!;

        private System.Windows.Forms.Timer refreshTimer = null!;
        private System.Windows.Forms.Timer pingTimer = null!;
        private bool isLoadingFromApi = false;
        private SocketService? socketService;
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

        // Lưu election events để truyền vào form khi mở
        private List<ElectionDto> electionEventsHistory = new List<ElectionDto>();

        // Danh sách node URLs để ping
        private readonly Dictionary<int, string> nodeUrls = new Dictionary<int, string>
        {
            { 1, "http://10.15.240.214:3000" }, // Hùng
            { 2, "http://10.15.240.99:3000" },  // Hậu
            { 3, "http://10.15.240.171:3000" }, // Khánh
            { 4, "http://10.15.240.248:3000" },      // Trương (test)
            { 5, "http://10.15.240.47:3000" },   // Giang
            { 6, "http://10.15.240.149:3000" }  // Tuấn
        };

        // Danh sách tên node theo ID
        private readonly Dictionary<int, string> nodeNames = new Dictionary<int, string>
        {
            { 1, "Hùng" },
            { 2, "Hậu" },
            { 3, "Khánh" },
            { 4, "Trương" },
            { 5, "Giang" },
            { 6, "Tuấn" }
        };

        public AdminDashboard()
        {
            InitializeComponents();
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            SetupLayout();

            // Load data từ API
            LoadDataFromApi();

            // Setup socket service cho real-time updates
            InitializeSocketService();

            // Setup timer để refresh định kỳ (fallback nếu socket không hoạt động, mỗi 2 giây)
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 2000; 
            refreshTimer.Tick += async (s, e) =>
            {
                if (socketService == null || !socketService.IsConnected)
                {
                    await RefreshData();
                }
            };
            refreshTimer.Start();

            // Setup timer để ping nodes định kỳ (mỗi 3 giây)
            pingTimer = new System.Windows.Forms.Timer();
            pingTimer.Interval = 1000; 
            pingTimer.Tick += async (s, e) =>
            {
                await PingAllNodes();
            };
            pingTimer.Start();

            _ = Task.Run(async () => await PingAllNodes());
        }

        private void InitializeComponents()
        {
            this.Text = "Bully Algorithm - Admin Dashboard";
            this.Size = new Size(1200, 800);
            // Modern dark background
            this.BackColor = ColorTranslator.FromHtml("#0A0E27"); 
            this.StartPosition = FormStartPosition.CenterScreen;

            // Header
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };

            titleLabel = new Label
            {
                Text = "Admin Dashboard",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#E0E7FF"),
                Location = new Point(20, 10),
                AutoSize = true
            };

            subtitleLabel = new Label
            {
                Text = "Bully Algorithm • Real-time Node Coordination",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorTranslator.FromHtml("#94A3B8"),
                Location = new Point(20, 45),
                AutoSize = true
            };

            systemActiveButton = new Button
            {
                Text = "⚡ System Active",
                Size = new Size(140, 40),
                Location = new Point(1030, 20),
                BackColor = ColorTranslator.FromHtml("#10B981"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            systemActiveButton.FlatAppearance.BorderSize = 0;

            electionInfoButton = new Button
            {
                Text = "⚡ Election Info",
                Size = new Size(120, 40),
                Location = new Point(790, 20),
                BackColor = ColorTranslator.FromHtml("#6366F1"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            electionInfoButton.FlatAppearance.BorderSize = 0;
            electionInfoButton.Click += ElectionInfoButton_Click;

            refreshButton = new Button
            {
                Text = "🔄 Refresh",
                Size = new Size(100, 40),
                Location = new Point(920, 20),
                BackColor = ColorTranslator.FromHtml("#475569"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += RefreshButton_Click;

            // Node Status Panel
            nodePanel = new Panel
            {
                Location = new Point(20, 90),
                Size = new Size(1160, 160),
                BackColor = Color.Transparent
            };

            nodeStatusLabel = new Label
            {
                Text = "Node Cluster Status",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#E0E7FF"),
                Location = new Point(15, 10),
                AutoSize = true

            };

            // Seat Map Panel
            seatMapPanel = new Panel
            {
                Location = new Point(20, 260),
                Size = new Size(1160, 280),
                BackColor = ColorTranslator.FromHtml("#16213E"),
                AutoScroll = true
            };

            seatMapLabel = new Label
            {
                Text = "Cinema Seat Map (Distributed Database)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#E0E7FF"),
                Location = new Point(15, 10),
                AutoSize = true
            };

            // Transaction Panel
            transactionPanel = new Panel
            {
                Location = new Point(20, 550),
                Size = new Size(1160, 200),
                BackColor = ColorTranslator.FromHtml("#16213E")
            };

            transactionLabel = new Label
            {
                Text = "Live Transaction Logs",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#E0E7FF"),
                Location = new Point(15, 10),
                AutoSize = true,

            };

            transactionGrid = new DataGridView
            {
                Location = new Point(15, 40),
                Size = new Size(1130, 145),
                BackgroundColor = ColorTranslator.FromHtml("#0F172A"),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = ColorTranslator.FromHtml("#334155")
            };

            // Style cho Header của Grid
            transactionGrid.EnableHeadersVisualStyles = false;
            transactionGrid.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#1E293B");
            transactionGrid.ColumnHeadersDefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#E0E7FF");
            transactionGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            transactionGrid.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#0F172A");
            transactionGrid.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#CBD5E1");

            transactionGrid.Columns.Add("Time", "Time");
            transactionGrid.Columns.Add("Source", "Source");
            transactionGrid.Columns.Add("Action", "Action");
            transactionGrid.Columns.Add("Message", "Message");

            transactionGrid.Columns[0].Width = 170;
            transactionGrid.Columns[1].Width = 150;
            transactionGrid.Columns[2].Width = 150;
            transactionGrid.Columns[3].Width = 640;
        }

        private void SetupLayout()
        {
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Controls.Add(electionInfoButton);
            headerPanel.Controls.Add(refreshButton);
            headerPanel.Controls.Add(systemActiveButton);

            nodePanel.Controls.Add(nodeStatusLabel);
            CreateNodeControls();

            seatMapPanel.Controls.Add(seatMapLabel);

            transactionPanel.Controls.Add(transactionLabel);
            transactionPanel.Controls.Add(transactionGrid);

            this.Controls.Add(headerPanel);
            this.Controls.Add(nodePanel);
            this.Controls.Add(seatMapPanel);
            this.Controls.Add(transactionPanel);
        }

        private void CreateNodeControls()
        {
            nodeControls = new NodeControl[6];

            for (int i = 0; i < 6; i++)
            {
                int nodeId = i + 1;
                string nodeName = nodeNames.ContainsKey(nodeId) ? nodeNames[nodeId] : "";
                nodeControls[i] = new NodeControl(nodeId, false, nodeName)
                {
                    Location = new Point(15 + (i * 190), 40)
                };
                nodePanel.Controls.Add(nodeControls[i]);
            }
        }

        private async void LoadDataFromApi()
        {
            try
            {
                isLoadingFromApi = true;
                subtitleLabel.Text = "Loading data from server...";

                var seatsTask = ApiServiceSeat.GetSeatsAsync();
                var nodesTask = ApiServiceNode.GetNodesAsync();
                var transactionsTask = ApiServiceLog.GetTransactionsAsync();

                await Task.WhenAll(seatsTask, nodesTask, transactionsTask);

                var seats = await seatsTask;
                var nodes = await nodesTask;
                var transactions = await transactionsTask;

                if (nodes != null && nodes.Count > 0)
                {
                    UpdateNodesFromApi(nodes);
                }

                if (seats != null && seats.Count > 0)
                {
                    CreateSeatMapFromApi(seats);
                    subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Live Data)";
                    isLoadingFromApi = false;
                }
                else
                {
                    CreateEmptySeatMap();
                    subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Server Offline)";
                    systemActiveButton.Text = "⚠️ Server Offline";
                    systemActiveButton.BackColor = ColorTranslator.FromHtml("#EF4444");
                    isLoadingFromApi = false;
                }

                if (transactions != null && transactions.Count > 0)
                {
                    UpdateTransactionGridFromApi(transactions);
                }
            }
            catch (Exception ex)
            {
                CreateEmptySeatMap();
                subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Connection Error)";
                systemActiveButton.Text = "⚠️ Connection Error";
                systemActiveButton.BackColor = ColorTranslator.FromHtml("#EF4444");
                isLoadingFromApi = false;
            }
        }

        private async Task RefreshData()
        {
            Console.WriteLine("[AdminDashboard] RefreshData called");

            try
            {
                var seatsTask = ApiServiceSeat.GetSeatsAsync();
                var nodesTask = ApiServiceNode.GetNodesAsync();
                var transactionsTask = ApiServiceLog.GetTransactionsAsync();

                await Task.WhenAll(seatsTask, nodesTask, transactionsTask);

                var seats = await seatsTask;
                var nodes = await nodesTask;
                var transactions = await transactionsTask;

                if (nodes != null && nodes.Count > 0)
                {
                    UpdateNodesFromApi(nodes);
                }

                if (seats != null && seats.Count > 0)
                {
                    UpdateSeatMapFromApi(seats);
                    systemActiveButton.Text = "⚡ System Active";
                    systemActiveButton.BackColor = ColorTranslator.FromHtml("#10B981");
                    subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Live Data)";
                }
                else
                {
                    systemActiveButton.Text = "⚠️ Server Offline";
                    systemActiveButton.BackColor = ColorTranslator.FromHtml("#EF4444");
                }

                if (transactions != null && transactions.Count > 0)
                {
                    UpdateTransactionGridFromApi(transactions);
                }
            }
            catch (Exception ex)
            {
                systemActiveButton.Text = "⚠️ Connection Error";
                systemActiveButton.BackColor = ColorTranslator.FromHtml("#EF4444");
            }
        }

        private void CreateSeatMapFromApi(List<SeatDto> seats)
        {
            seatMapPanel.Controls.Clear();
            seatMapPanel.Controls.Add(seatMapLabel);

            string[] rows = { "A", "B", "C", "D", "E", "F", "G" };
            int cols = 10;

            seatControls = new SeatControl[rows.Length, cols];

            int seatWidth = (seatMapPanel.Width - 70) / cols;
            int seatHeight = 50;

            for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    string seatName = $"{rows[i]}{j + 1}";
                    var seatData = seats.FirstOrDefault(s => s.seatNumber == seatName);

                    bool isBooked = seatData != null && seatData.IsOccupied;
                    string customerName = isBooked ? seatData?.customerName ?? "Unknown" : "";
                    int? nodeNumber = isBooked ? seatData?.bookedByNode : null;

                    var seat = new SeatControl(seatName, isBooked, customerName, nodeNumber)
                    {
                        Size = new Size(seatWidth, seatHeight),
                        Location = new Point(20 + j * (seatWidth + 3), 40 + i * (seatHeight + 10)),
                    };

                    seatControls[i, j] = seat;
                    seatMapPanel.Controls.Add(seat);
                }
            }
        }

        private void UpdateNodesFromApi(List<NodeDto> nodes)
        {
            if (nodeControls == null) return;

            foreach (var nodeData in nodes)
            {
                int index = -1;

                if (nodeData.id >= 1 && nodeData.id <= nodeControls.Length)
                {
                    index = nodeData.id - 1;
                }
                else if (nodeData.id >= 0 && nodeData.id < nodeControls.Length)
                {
                    index = nodeData.id;
                }

                if (index >= 0 && index < nodeControls.Length && nodeControls[index] != null)
                {
                    bool isLeader = nodeData.isLeader;
                    nodeControls[index].UpdateNode(isLeader);
                }
            }
        }

        private async Task PingAllNodes()
        {
            if (nodeControls == null || isDisposing) return;

            var pingTasks = new List<Task>();

            for (int i = 0; i < nodeControls.Length; i++)
            {
                int nodeId = i + 1;
                int index = i;

                if (nodeUrls.ContainsKey(nodeId))
                {
                    pingTasks.Add(Task.Run(async () =>
                    {
                        bool isAlive = await PingNode(nodeUrls[nodeId]);
                        
                        if (!isDisposing && !this.IsDisposed)
                        {
                            this.Invoke(new Action(() =>
                            {
                                if (index >= 0 && index < nodeControls.Length && nodeControls[index] != null)
                                {
                                    nodeControls[index].UpdateAliveState(isAlive);
                                }
                            }));
                        }
                    }));
                }
            }

            await Task.WhenAll(pingTasks);
        }

        private async Task<bool> PingNode(string url)
        {
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
                {
                    var response = await httpClient.GetAsync($"{url}/node", cts.Token);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                try
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
                    {
                        var response = await httpClient.GetAsync(url, cts.Token);
                        return response.IsSuccessStatusCode;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private void UpdateSeatMapFromApi(List<SeatDto> seats)
        {
            if (seatControls == null) return;

            string[] rows = { "A", "B", "C", "D", "E", "F", "G" };
            int cols = 10;

            for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    string seatName = $"{rows[i]}{j + 1}";
                    var seatData = seats.FirstOrDefault(s => s.seatNumber == seatName);

                    if (seatData != null && seatControls[i, j] != null)
                    {
                        bool isBooked = seatData.IsOccupied;
                        string customerName = isBooked ? seatData.customerName ?? "Unknown" : "";
                        int? nodeNumber = isBooked ? seatData.bookedByNode : null;

                        seatControls[i, j].UpdateSeat(isBooked, customerName, nodeNumber);
                    }
                }
            }
        }

        private void CreateEmptySeatMap()
        {
            seatMapPanel.Controls.Clear();
            seatMapPanel.Controls.Add(seatMapLabel);

            string[] rows = { "A", "B", "C", "D", "E", "F", "G" };
            int cols = 10;

            seatControls = new SeatControl[rows.Length, cols];

            int seatWidth = (seatMapPanel.Width - 70) / cols;
            int seatHeight = 50;

            for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    string seatName = $"{rows[i]}{j + 1}";

                    var seat = new SeatControl(seatName, false)
                    {
                        Size = new Size(seatWidth, seatHeight),
                        Location = new Point(20 + j * (seatWidth + 3), 40 + i * (seatHeight + 10)),
                    };

                    seatControls[i, j] = seat;
                    seatMapPanel.Controls.Add(seat);
                }
            }
        }

        private void UpdateTransactionGridFromApi(List<TransactionDto> transactions)
        {
            transactionGrid.Rows.Clear();
            var sortedTransactions = transactions.OrderByDescending(t => t.timestamp).ToList();

            foreach (var transaction in sortedTransactions)
            {
                string time = transaction.timestamp.ToString("dd/MM/yyyy HH:mm:ss");

                string source = $"Node {transaction.nodeId}";
                string action = transaction.actionType.ToUpper();
                string message = transaction.description;

                AddTransactionRow(time, source, action, message);
            }
        }

        private void AddTransactionRow(string time, string source, string action, string message)
        {
            int rowIndex = transactionGrid.Rows.Add(time, source, action, message);
            DataGridViewRow row = transactionGrid.Rows[rowIndex];

            row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#0F172A");
            row.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#CBD5E1");

            DataGridViewCell sourceCell = row.Cells[1];
            sourceCell.Style.BackColor = ColorTranslator.FromHtml("#475569");
            sourceCell.Style.ForeColor = Color.White;
            sourceCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            sourceCell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            DataGridViewCell actionCell = row.Cells[2];
            switch (action.ToUpper())
            {
                case "HEARTBEAT":
                    actionCell.Style.BackColor = ColorTranslator.FromHtml("#64748B");
                    actionCell.Style.ForeColor = Color.White;
                    break;
                case "BUY":
                case "BOOK":
                    actionCell.Style.BackColor = ColorTranslator.FromHtml("#10B981");
                    actionCell.Style.ForeColor = Color.White;
                    break;
                case "LOG":
                    actionCell.Style.BackColor = ColorTranslator.FromHtml("#8B5CF6");
                    actionCell.Style.ForeColor = Color.White;
                    break;
                default:
                    actionCell.Style.BackColor = ColorTranslator.FromHtml("#475569");
                    actionCell.Style.ForeColor = Color.White;
                    break;
            }
            actionCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            actionCell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        }

        private async void InitializeSocketService()
        {
            try
            {
                socketService = new SocketService();

                socketService.OnSeatsUpdate += (seats) =>
                {
                    if (isDisposing || this.IsDisposed) return;
                    try
                    {
                        if (this.InvokeRequired)
                            this.BeginInvoke(new Action(() => HandleSeatsUpdate(seats)));
                        else
                            HandleSeatsUpdate(seats);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception ex) { Console.WriteLine($"Error in OnSeatsUpdate: {ex.Message}"); }
                };

                socketService.OnNodesUpdate += (nodes) =>
                {
                    if (isDisposing || this.IsDisposed) return;
                    try
                    {
                        if (this.InvokeRequired)
                            this.BeginInvoke(new Action(() => HandleNodesUpdate(nodes)));
                        else
                            HandleNodesUpdate(nodes);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception ex) { Console.WriteLine($"Error in OnNodesUpdate: {ex.Message}"); }
                };

                socketService.OnTransactionsUpdate += (transactions) =>
                {
                    if (isDisposing || this.IsDisposed) return;
                    try
                    {
                        if (this.InvokeRequired)
                            this.BeginInvoke(new Action(() => HandleTransactionsUpdate(transactions)));
                        else
                            HandleTransactionsUpdate(transactions);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception ex) { Console.WriteLine($"Error in OnTransactionsUpdate: {ex.Message}"); }
                };

                socketService.OnElectionUpdate += (elections) =>
                {
                    if (isDisposing || this.IsDisposed) return;

                    if (elections != null && elections.Count > 0)
                    {
                        foreach (var election in elections)
                        {
                            bool isDuplicate = electionEventsHistory.Any(e =>
                                e.NodeId == election.NodeId &&
                                e.EventType == election.EventType &&
                                Math.Abs((e.Timestamp - election.Timestamp).TotalSeconds) < 2);

                            if (!isDuplicate)
                            {
                                electionEventsHistory.Add(election);
                            }
                        }

                        if (electionEventsHistory.Count > 100)
                        {
                            electionEventsHistory = electionEventsHistory
                                .OrderByDescending(e => e.Timestamp)
                                .Take(100)
                                .ToList();
                        }
                        else
                        {
                            // Đảm bảo sắp xếp mới nhất lên đầu
                            electionEventsHistory = electionEventsHistory
                                .OrderByDescending(e => e.Timestamp)
                                .ToList();
                        }
                    }
                };

                socketService.OnConnectionStatusChanged += (isConnected) =>
                {
                    if (isDisposing || this.IsDisposed) return;
                    try
                    {
                        if (this.InvokeRequired)
                            this.BeginInvoke(new Action(() => HandleConnectionStatusChanged(isConnected)));
                        else
                            HandleConnectionStatusChanged(isConnected);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception ex) { Console.WriteLine($"Error in OnConnectionStatusChanged: {ex.Message}"); }
                };

                await socketService.ConnectAsync();
            }
            catch (Exception ex)
            {
                subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Socket Offline - Using Polling)";
                Console.WriteLine($"Error initializing socket: {ex.Message}");
            }
        }

        private void HandleSeatsUpdate(List<SeatDto> seats)
        {
            if (isDisposing || this.IsDisposed) return;

            if (seats != null && seats.Count > 0)
            {
                if (seatControls == null)
                    CreateSeatMapFromApi(seats);
                else
                    UpdateSeatMapFromApi(seats);
                
                systemActiveButton.Text = "⚡ System Active";
                systemActiveButton.BackColor = ColorTranslator.FromHtml("#10B981");
                subtitleLabel.Text = $"Bully Algorithm • Real-time Node Coordination (Last update: {DateTime.Now:HH:mm:ss})";
            }
        }

        private void HandleNodesUpdate(List<NodeDto> nodes)
        {
            if (isDisposing || this.IsDisposed) return;

            if (nodes != null && nodes.Count > 0)
            {
                UpdateNodesFromApi(nodes);
            }
        }

        private void HandleTransactionsUpdate(List<TransactionDto> transactions)
        {
            if (isDisposing || this.IsDisposed) return;

            if (transactions != null && transactions.Count > 0)
            {
                UpdateTransactionGridFromApi(transactions);
            }
        }

        private void HandleConnectionStatusChanged(bool isConnected)
        {
            if (isDisposing || this.IsDisposed) return;

            if (isConnected)
            {
                subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Socket Connected - Real-time Active)";
                systemActiveButton.Text = "⚡ System Active";
                systemActiveButton.BackColor = ColorTranslator.FromHtml("#10B981");
            }
            else
            {
                subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Socket Disconnected - Using Polling)";
                systemActiveButton.Text = "⚠️ Socket Offline";
                systemActiveButton.BackColor = ColorTranslator.FromHtml("#F59E0B");
            }
        }

        private void ElectionInfoButton_Click(object? sender, EventArgs e)
        {
            ElectionInfoForm electionForm = new ElectionInfoForm(socketService, electionEventsHistory);
            electionForm.ShowDialog(this);
        }

        private async void RefreshButton_Click(object? sender, EventArgs e)
        {
            if (refreshButton == null) return;

            refreshButton.Enabled = false;
            string originalText = refreshButton.Text;
            refreshButton.Text = "⏳ Refreshing...";

            try
            {
                Console.WriteLine("[AdminDashboard] Refresh button clicked");
                await RefreshData();
                Console.WriteLine("[AdminDashboard] Refresh completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminDashboard] Error in refresh: {ex.Message}");
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Refresh Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                refreshButton.Enabled = true;
                refreshButton.Text = originalText;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isDisposing = true;

            if (refreshTimer != null)
            {
                refreshTimer.Stop();
                refreshTimer.Dispose();
            }

            if (pingTimer != null)
            {
                pingTimer.Stop();
                pingTimer.Dispose();
            }

            socketService?.Dispose();

            base.OnFormClosing(e);
        }
    }

    public class NodeControl : Panel
    {
        private Label nodeLabel = null!;
        private Label statusLabel = null!;
        private Label stateLabel = null!;
        private Label roleLabel = null!;
        private Label leaderIcon = null!;

        private int nodeNumber;
        private bool isLeader;
        private string nodeName;

        public NodeControl(int number, bool leader, string name = "")
        {
            nodeNumber = number;
            isLeader = leader;
            nodeName = name;

            this.Size = new Size(180, 110);
            this.BackColor = ColorTranslator.FromHtml("#1E293B");

            InitializeNodeComponents();
        }

        private void InitializeNodeComponents()
        {
            string nodeText = string.IsNullOrEmpty(nodeName) 
                ? $"● Node {nodeNumber}" 
                : $"● Node {nodeNumber} - {nodeName}";
            
            nodeLabel = new Label
            {
                Text = nodeText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#E0E7FF"),
                Location = new Point(10, 8),
                AutoSize = true
            };

            leaderIcon = new Label
            {
                Text = "👑",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(145, 3),
                AutoSize = true,
                Visible = isLeader,
                ForeColor = ColorTranslator.FromHtml("#FBBF24")
            };

            statusLabel = new Label
            {
                Text = "Status",
                Font = new Font("Segoe UI", 8),
                ForeColor = ColorTranslator.FromHtml("#94A3B8"),
                Location = new Point(10, 30),
                AutoSize = true
            };

            stateLabel = new Label
            {
                Text = "Alive",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#10B981"),
                Location = new Point(10, 45),
                AutoSize = true
            };

            roleLabel = new Label
            {
                Text = isLeader ? "Leader" : "Follower",
                Font = new Font("Segoe UI", 8),
                ForeColor = ColorTranslator.FromHtml("#CBD5E1"),
                Location = new Point(10, 60),
                AutoSize = true
            };

            this.Controls.Add(nodeLabel);
            this.Controls.Add(leaderIcon);
            this.Controls.Add(statusLabel);
            this.Controls.Add(stateLabel);
            this.Controls.Add(roleLabel);
        }

        public void UpdateNode(bool isLeader)
        {
            this.isLeader = isLeader;
            leaderIcon.Visible = isLeader;
            roleLabel.Text = isLeader ? "Leader" : "Follower";

            if (isLeader)
            {
                leaderIcon.ForeColor = ColorTranslator.FromHtml("#FBBF24");
                leaderIcon.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            }
        }

        public void UpdateAliveState(bool isAlive)
        {
            if (stateLabel == null) return;

            if (isAlive)
            {
                stateLabel.Text = "Alive";
                stateLabel.ForeColor = ColorTranslator.FromHtml("#10B981");
            }
            else
            {
                stateLabel.Text = "Dead";
                stateLabel.ForeColor = ColorTranslator.FromHtml("#64748B");
            }
        }
    }

    public class SeatControl : Panel
    {
        private Label seatLabel = null!;
        private Label occupantLabel = null!;
        private Label? nodeLabel;

        private string seatName;
        private bool isBooked;
        private string? realCustomerName;
        private int? realNodeNumber;

        public SeatControl(string name, bool Booked, string? customerName = null, int? nodeNumber = null)
        {
            seatName = name;
            isBooked = Booked;
            realCustomerName = customerName;
            realNodeNumber = nodeNumber;

            this.Size = new Size(200, 45);
            this.BackColor = isBooked ? ColorTranslator.FromHtml("#DC2626") : ColorTranslator.FromHtml("#0F172A");
            if (!isBooked) this.BorderStyle = BorderStyle.FixedSingle;
            else this.BorderStyle = BorderStyle.None;

            InitializeSeatComponents();
        }

        private void InitializeSeatComponents()
        {
            seatLabel = new Label
            {
                Text = seatName,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 5),
                AutoSize = true
            };

            string occupantText = isBooked
                ? (realCustomerName ?? GetOccupantName())
                : "Available";

            occupantLabel = new Label
            {
                Text = occupantText,
                Font = new Font("Segoe UI", 9),
                ForeColor = isBooked ? ColorTranslator.FromHtml("#FEE2E2") : ColorTranslator.FromHtml("#64748B"),
                Location = new Point(10, 23),
                AutoSize = true
            };

            this.Controls.Add(seatLabel);
            this.Controls.Add(occupantLabel);

            if (isBooked)
            {
                int displayNodeNumber = realNodeNumber ?? GetNodeNumber();

                nodeLabel = new Label
                {
                    Text = $"via Node {displayNodeNumber}",
                    Font = new Font("Segoe UI", 7),
                    ForeColor = ColorTranslator.FromHtml("#FCA5A5"),
                    Location = new Point(50, 0),
                    Size = new Size(65, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(nodeLabel);
            }
        }

        public void UpdateSeat(bool Booked, string? customerName, int? nodeNumber)
        {
            if (isBooked == Booked &&
                realCustomerName == customerName &&
                realNodeNumber == nodeNumber)
            {
                return;
            }

            isBooked = Booked;
            realCustomerName = customerName;
            realNodeNumber = nodeNumber;

            // Update UI
            this.BackColor = isBooked ? ColorTranslator.FromHtml("#DC2626") : ColorTranslator.FromHtml("#0F172A");
            if (!isBooked) this.BorderStyle = BorderStyle.FixedSingle;
            else this.BorderStyle = BorderStyle.None;

            seatLabel.ForeColor = Color.White;

            string occupantText = isBooked
                ? (realCustomerName ?? "Unknown")
                : "Available";

            occupantLabel.Text = occupantText;
            occupantLabel.ForeColor = isBooked ? ColorTranslator.FromHtml("#FEE2E2") : ColorTranslator.FromHtml("#64748B");

            if (nodeLabel != null)
            {
                this.Controls.Remove(nodeLabel);
                nodeLabel = null;
            }

            if (isBooked && nodeNumber.HasValue)
            {
                nodeLabel = new Label
                {
                    Text = $"via Node {nodeNumber.Value}",
                    Font = new Font("Segoe UI", 7),
                    ForeColor = ColorTranslator.FromHtml("#FCA5A5"),
                    Location = new Point(50, 0),
                    Size = new Size(65, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(nodeLabel);
            }
        }

        private string GetOccupantName()
        {
            string[] names = { "Jane Doe", "John Smith", "Alice Johnson",
                             "Bob Wilson", "Carl White", "David Brown",
                             "Emma Davis", "Lisa Ray" };
            Random rnd = new Random(seatName.GetHashCode());
            return names[rnd.Next(names.Length)];
        }

        private int GetNodeNumber()
        {
            Random rnd = new Random(seatName.GetHashCode());
            return rnd.Next(1, 8);
        }
    }
}