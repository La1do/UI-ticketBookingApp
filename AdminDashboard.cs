using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private bool isLoadingFromApi = false;

        public AdminDashboard()
        {
            InitializeComponents();
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            SetupLayout();

            // Load data từ API
            LoadDataFromApi();

            // Setup timer để refresh định kỳ (mỗi 5 giây)
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 5000;
            refreshTimer.Tick += async (s, e) => await RefreshData();
            refreshTimer.Start();
        }

        private void InitializeComponents()
        {
            this.Text = "Bully Algorithm - Admin Dashboard";
            this.Size = new Size(1200, 800);
            this.BackColor = ColorTranslator.FromHtml("#EC4899");
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
                ForeColor = Color.White,
                Location = new Point(20, 10),
                AutoSize = true
            };

            subtitleLabel = new Label
            {
                Text = "Bully Algorithm • Real-time Node Coordination",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 45),
                AutoSize = true
            };

            systemActiveButton = new Button
            {
                Text = "⚡ System Active",
                Size = new Size(140, 40),
                Location = new Point(1030, 20),
                BackColor = ColorTranslator.FromHtml("#DB2777"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            systemActiveButton.FlatAppearance.BorderSize = 0;

            refreshButton = new Button
            {
                Text = "🔄 Refresh",
                Size = new Size(100, 40),
                Location = new Point(920, 20),
                BackColor = ColorTranslator.FromHtml("#DB2777"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += async (s, e) => await RefreshData();

            // Node Status Panel
            nodePanel = new Panel
            {
                Location = new Point(20, 90),
                Size = new Size(1160, 160),
                BackColor = ColorTranslator.FromHtml("#F9A8D4")
            };

            nodeStatusLabel = new Label
            {
                Text = "Node Cluster Status",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#831843"),
                Location = new Point(15, 10),
                AutoSize = true

            };

            // Seat Map Panel
            seatMapPanel = new Panel
            {
                Location = new Point(20, 260),
                Size = new Size(1160, 280),
                BackColor = ColorTranslator.FromHtml("#F9A8D4"),
                AutoScroll = true
            };

            seatMapLabel = new Label
            {
                Text = "Cinema Seat Map (Distributed Database)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#831843"),
                Location = new Point(15, 10),
                AutoSize = true
            };

            // Transaction Panel
            transactionPanel = new Panel
            {
                Location = new Point(20, 550),
                Size = new Size(1160, 200),
                BackColor = ColorTranslator.FromHtml("#F9A8D4")
            };

            transactionLabel = new Label
            {
                Text = "Live Transaction Logs",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#831843"),
                Location = new Point(15, 10),
                AutoSize = true,

            };

            transactionGrid = new DataGridView
            {
                Location = new Point(15, 40),
                Size = new Size(1130, 145),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            transactionGrid.Columns.Add("Time", "Time");
            transactionGrid.Columns.Add("Source", "Source");
            transactionGrid.Columns.Add("Action", "Action");
            transactionGrid.Columns.Add("Message", "Message");

            transactionGrid.Columns[0].Width = 100;
            transactionGrid.Columns[1].Width = 150;
            transactionGrid.Columns[2].Width = 150;
            transactionGrid.Columns[3].Width = 700;
        }

        private void SetupLayout()
        {
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
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
            // Tạo node controls với dữ liệu mẫu ban đầu, sẽ được cập nhật từ API
            nodeControls = new NodeControl[6];

            for (int i = 0; i < 6; i++)
            {
                nodeControls[i] = new NodeControl(i + 1, false)
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

                // Load cả seats và nodes từ API
                var seatsTask = ApiServiceSeat.GetSeatsAsync();
                var nodesTask = ApiServiceNode.GetNodesAsync();

                await Task.WhenAll(seatsTask, nodesTask);

                var seats = await seatsTask;
                var nodes = await nodesTask;

                // Load node data
                if (nodes != null && nodes.Count > 0)
                {
                    Console.WriteLine($"Loaded {nodes.Count} nodes from API");
                    UpdateNodesFromApi(nodes);
                }
                else
                {
                    Console.WriteLine("No nodes data from API, keeping default state");
                    // Nếu không có dữ liệu từ API, giữ trạng thái mặc định (alive)
                }

                if (seats != null && seats.Count > 0)
                {
                    // Load thành công từ API
                    CreateSeatMapFromApi(seats);
                    subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Live Data)";
                    isLoadingFromApi = false;
                }
                else
                {
                    // Không load được, hiển thị bảng trống
                    CreateEmptySeatMap();
                    subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Server Offline)";
                    systemActiveButton.Text = "⚠️ Server Offline";
                    systemActiveButton.BackColor = ColorTranslator.FromHtml("#EF4444");
                    isLoadingFromApi = false;
                }
            }
            catch (Exception ex)
            {
                // Lỗi, hiển thị bảng trống
                Console.WriteLine($"Error loading from API: {ex.Message}");
                CreateEmptySeatMap();
                subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Connection Error)";
                systemActiveButton.Text = "⚠️ Connection Error";
                systemActiveButton.BackColor = ColorTranslator.FromHtml("#EF4444");
                isLoadingFromApi = false;
            }
        }

        private async Task RefreshData()
        {
            if (isLoadingFromApi) return;

            try
            {
                // Refresh cả seats và nodes
                var seatsTask = ApiServiceSeat.GetSeatsAsync();
                var nodesTask = ApiServiceNode.GetNodesAsync();

                await Task.WhenAll(seatsTask, nodesTask);

                var seats = await seatsTask;
                var nodes = await nodesTask;

                // Update node data
                if (nodes != null && nodes.Count > 0)
                {
                    UpdateNodesFromApi(nodes);
                }
                else
                {
                    Console.WriteLine("No nodes data from API during refresh");
                }

                if (seats != null && seats.Count > 0)
                {
                    UpdateSeatMapFromApi(seats);
                    systemActiveButton.Text = "⚡ System Active";
                    systemActiveButton.BackColor = ColorTranslator.FromHtml("#DB2777");
                    subtitleLabel.Text = "Bully Algorithm • Real-time Node Coordination (Live Data)";
                }
                else
                {
                    systemActiveButton.Text = "⚠️ Server Offline";
                    systemActiveButton.BackColor = ColorTranslator.FromHtml("#EF4444");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing data: {ex.Message}");
                systemActiveButton.Text = "⚠️ Connection Error";
                systemActiveButton.BackColor = ColorTranslator.FromHtml("#EF4444");
            }
        }

        private void CreateSeatMapFromApi(List<SeatDto> seats)
        {
            // Clear existing seats
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

                    // Tìm seat trong API data
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

            Console.WriteLine($"Updating {nodes.Count} nodes...");
            
            foreach (var nodeData in nodes)
            {
                // Tìm node control tương ứng với node id
                // Node id có thể bắt đầu từ 1 hoặc 0, cần xử lý cả hai trường hợp
                int index = -1;
                
                // Thử với id bắt đầu từ 1
                if (nodeData.id >= 1 && nodeData.id <= nodeControls.Length)
                {
                    index = nodeData.id - 1;
                }
                // Thử với id bắt đầu từ 0
                else if (nodeData.id >= 0 && nodeData.id < nodeControls.Length)
                {
                    index = nodeData.id;
                }
                
                if (index >= 0 && index < nodeControls.Length && nodeControls[index] != null)
                {
                    // API trả về boolean, không cần convert
                    bool isAlive = nodeData.isAlive;
                    bool isLeader = nodeData.isLeader;
                    
            
                    
                    nodeControls[index].UpdateNode(isAlive, isLeader);
                }
                else
                {
                    Console.WriteLine($"Warning: Node id {nodeData.id} is out of range (0-{nodeControls.Length - 1})");
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
            // Clear existing seats
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

        private void AddTransactionRow(string time, string source, string action, string message)
        {
            int rowIndex = transactionGrid.Rows.Add(time, source, action, message);
            DataGridViewRow row = transactionGrid.Rows[rowIndex];

            // Style source cell
            DataGridViewCell sourceCell = row.Cells[1];
            sourceCell.Style.BackColor = ColorTranslator.FromHtml("#8B5CF6");
            sourceCell.Style.ForeColor = Color.White;
            sourceCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            sourceCell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // Style action cell
            DataGridViewCell actionCell = row.Cells[2];
            switch (action)
            {
                case "HEARTBEAT":
                    actionCell.Style.BackColor = ColorTranslator.FromHtml("#EC4899");
                    break;
                case "BUY":
                    actionCell.Style.BackColor = ColorTranslator.FromHtml("#10B981");
                    break;
                case "LOG":
                    actionCell.Style.BackColor = ColorTranslator.FromHtml("#F59E0B");
                    break;
            }
            actionCell.Style.ForeColor = Color.White;
            actionCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            actionCell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }

    public class NodeControl : Panel
    {
        private Label nodeLabel = null!;
        private Label statusLabel = null!;
        private Label stateLabel = null!;
        private Label roleLabel = null!;
        private Button killButton = null!;
        private Button reviveButton = null!;
        private Label leaderIcon = null!;

        private int nodeNumber;
        private bool isLeader;

        public NodeControl(int number, bool leader)
        {
            nodeNumber = number;
            isLeader = leader;

            this.Size = new Size(180, 110);
            this.BackColor = ColorTranslator.FromHtml("#F0ABFC");

            InitializeNodeComponents();
        }

        private void InitializeNodeComponents()
        {
            nodeLabel = new Label
            {
                Text = $"● Node {nodeNumber}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#701A75"),
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
                ForeColor = ColorTranslator.FromHtml("#FCD34D")
            };

            statusLabel = new Label
            {
                Text = "Status",
                Font = new Font("Segoe UI", 8),
                ForeColor = ColorTranslator.FromHtml("#701A75"),
                Location = new Point(10, 30),
                AutoSize = true
            };

            stateLabel = new Label
            {
                Text = "Alive",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#701A75"),
                Location = new Point(10, 45),
                AutoSize = true
            };

            roleLabel = new Label
            {
                Text = isLeader ? "Leader" : "Follower",
                Font = new Font("Segoe UI", 8),
                ForeColor = ColorTranslator.FromHtml("#701A75"),
                Location = new Point(10, 60),
                AutoSize = true
            };

            killButton = new Button
            {
                Text = "Kill Node",
                Size = new Size(160, 30),
                Location = new Point(10, 75),
                BackColor = ColorTranslator.FromHtml("#DB2777"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            killButton.FlatAppearance.BorderSize = 0;
            killButton.Click += KillButton_Click;

            reviveButton = new Button
            {
                Text = "Revive Node",
                Size = new Size(160, 30),
                Location = new Point(10, 75),
                BackColor = ColorTranslator.FromHtml("#10B981"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Visible = false
            };
            reviveButton.FlatAppearance.BorderSize = 0;
            reviveButton.Click += ReviveButton_Click;

            this.Controls.Add(nodeLabel);
            this.Controls.Add(leaderIcon);
            this.Controls.Add(statusLabel);
            this.Controls.Add(stateLabel);
            this.Controls.Add(roleLabel);
            this.Controls.Add(killButton);
            this.Controls.Add(reviveButton);
        }

        private void KillButton_Click(object? sender, EventArgs e)
        {
            stateLabel.Text = "Dead";
            stateLabel.ForeColor = Color.Red;
            killButton.Visible = false;
            reviveButton.Visible = true;
            this.BackColor = ColorTranslator.FromHtml("#FCA5A5");
        }

        private void ReviveButton_Click(object? sender, EventArgs e)
        {
            stateLabel.Text = "Alive";
            stateLabel.ForeColor = ColorTranslator.FromHtml("#701A75");
            reviveButton.Visible = false;
            killButton.Visible = true;
            this.BackColor = ColorTranslator.FromHtml("#F0ABFC");
        }

        public void UpdateNode(bool isAlive, bool isLeader)
        {
            // Cập nhật trạng thái alive/dead
            if (isAlive)
            {
                stateLabel.Text = "Alive";
                stateLabel.ForeColor = ColorTranslator.FromHtml("#701A75");
                killButton.Visible = true;
                reviveButton.Visible = false;
                this.BackColor = ColorTranslator.FromHtml("#F0ABFC");
            }
            else
            {
                stateLabel.Text = "Dead";
                stateLabel.ForeColor = Color.Red;
                killButton.Visible = false;
                reviveButton.Visible = true;
                this.BackColor = ColorTranslator.FromHtml("#FCA5A5");
            }

            // Cập nhật leader status
            this.isLeader = isLeader;
            leaderIcon.Visible = isLeader && isAlive; // Chỉ hiển thị vương miện nếu node còn alive
            roleLabel.Text = isLeader ? "Leader" : "Follower";
            
            // Làm nổi bật vương miện khi là leader
            if (isLeader && isAlive)
            {
                leaderIcon.ForeColor = ColorTranslator.FromHtml("#FCD34D"); // Vàng
                leaderIcon.Font = new Font("Segoe UI", 16, FontStyle.Bold);
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
            this.BackColor = isBooked ? ColorTranslator.FromHtml("#7C3AED") : Color.White;

            InitializeSeatComponents();

        }

        private void InitializeSeatComponents()
        {

            seatLabel = new Label
            {
                Text = seatName,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = isBooked ? Color.White : Color.Black,
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
                ForeColor = isBooked ? Color.White : Color.Gray,
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
                    ForeColor = Color.White,
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
                return; // Không có thay đổi
            }

            isBooked = Booked;
            realCustomerName = customerName;
            realNodeNumber = nodeNumber;

            // Update UI
            this.BackColor = isBooked ? ColorTranslator.FromHtml("#7C3AED") : Color.White;
            seatLabel.ForeColor = isBooked ? Color.White : Color.Black;

            string occupantText = isBooked
                ? (realCustomerName ?? "Unknown")
                : "Available";

            occupantLabel.Text = occupantText;
            occupantLabel.ForeColor = isBooked ? Color.White : Color.Gray;

            // Update node label
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
                    ForeColor = Color.White,
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