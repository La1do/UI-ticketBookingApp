using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace BullyAlgorithmDemo
{
    public class AdminDashboard : Form
    {
        private Panel headerPanel = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private Button systemActiveButton = null!;

        private Panel nodePanel = null!;
        private Label nodeStatusLabel = null!;
        private NodeControl[] nodeControls = null!;

        private Panel seatMapPanel = null!;
        private Label seatMapLabel = null!;
        private SeatControl[,] seatControls = null!;

        private Panel transactionPanel = null!;
        private Label transactionLabel = null!;
        private DataGridView transactionGrid = null!;

        public AdminDashboard()
        {
            InitializeComponents();
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            SetupLayout();
            PopulateSampleData();
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
            headerPanel.Controls.Add(systemActiveButton);

            nodePanel.Controls.Add(nodeStatusLabel);
            CreateNodeControls();

            seatMapPanel.Controls.Add(seatMapLabel);
            CreateSeatMap();

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
                nodeControls[i] = new NodeControl(i + 1, i == 2)
                {
                    Location = new Point(15 + (i * 190), 40)
                };
                nodePanel.Controls.Add(nodeControls[i]);
            }
        }

        private void CreateSeatMap()
        {
            string[] rows = { "A", "B", "C", "D", "E", "F", "G" };
            int cols = 10;

            seatControls = new SeatControl[rows.Length, cols];

            int seatWidth = (seatMapPanel.Width - 70) / cols; // trừ padding
            int seatHeight = 50;

            for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    string seatName = $"{rows[i]}{j + 1}";
                    bool isOccupied = (i == 0 && j == 1) || (i == 1 && j == 1) ||
                                      (i == 1 && j == 3) || (i == 2 && j == 2) ||
                                      (i == 3 && j == 0) || (i == 3 && j == 3);

                    var seat = new SeatControl(seatName, isOccupied)
                    {
                        Size = new Size(seatWidth, seatHeight),
                        Location = new Point(20 + j * (seatWidth + 3), 40 + i * (seatHeight + 10)),
                    };

                    seatControls[i, j] = seat;
                    seatMapPanel.Controls.Add(seat);
                }
            }
        }


        private void PopulateSampleData()
        {
            AddTransactionRow("22:03:36", "Node 3", "HEARTBEAT", "Leader Node 3 sent heartbeat");
            AddTransactionRow("22:03:33", "Node 5", "BUY", "Customer Lisa Ray bought Seat B4");
            AddTransactionRow("14:32:16", "Node 3", "BUY", "Customer Jane Doe bought Seat A2");
            AddTransactionRow("14:32:18", "Node 7", "LOG", "Node 7 locked Seat A4");
            AddTransactionRow("14:32:28", "Node 7", "BUY", "Customer John Smith bought Seat A4");
            AddTransactionRow("14:32:26", "Node 5", "BUY", "Customer Alice Johnson bought Seat B2");
            AddTransactionRow("14:32:38", "Node 3", "HEARTBEAT", "Leader Node 3 sent heartbeat");
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
                Font = new Font("Segoe UI", 12),
                Location = new Point(150, 5),
                AutoSize = true,
                Visible = isLeader
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
    }

    public class SeatControl : Panel
    {
        private Label seatLabel = null!;
        private Label occupantLabel = null!;
        private Label? nodeLabel;

        private string seatName;
        private bool isOccupied;

        public SeatControl(string name, bool occupied)
        {
            seatName = name;
            isOccupied = occupied;

            this.Size = new Size(200, 45);
            this.BackColor = isOccupied ? ColorTranslator.FromHtml("#7C3AED") : Color.White;

            InitializeSeatComponents();
        }

        private void InitializeSeatComponents()
        {
            seatLabel = new Label
            {
                Text = seatName,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = isOccupied ? Color.White : Color.Black,
                Location = new Point(10, 5),
                AutoSize = true
            };

            occupantLabel = new Label
            {
                Text = isOccupied ? GetOccupantName() : "Available",
                Font = new Font("Segoe UI", 9),
                ForeColor = isOccupied ? Color.White : Color.Gray,
                Location = new Point(10, 23),
                AutoSize = true
            };

            this.Controls.Add(seatLabel);
            this.Controls.Add(occupantLabel);

            if (isOccupied)
            {
                nodeLabel = new Label
                {
                    Text = $"via Node {GetNodeNumber()}",
                    Font = new Font("Segoe UI", 7),
                    ForeColor = Color.White,
                    Location = new Point(130, 15),
                    Size = new Size(65, 20),
                    BackColor = ColorTranslator.FromHtml("#5B21B6"),
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