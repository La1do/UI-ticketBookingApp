using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CinemaTicketBooking;
using api;

namespace BullyAlgorithmDemo
{
    public class ElectionInfoForm : Form
    {
        private Panel headerPanel = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private Panel contentPanel = null!;
        private Panel scrollPanel = null!;
        private Button btnClose = null!;

        // Dữ liệu election - lưu history để hiển thị
        private List<ElectionEvent> electionEvents = new List<ElectionEvent>();
        private SocketService? socketService;
        private const int MAX_HISTORY = 100; // Giới hạn số lượng events trong history

        public ElectionInfoForm(SocketService? socketService = null, List<ElectionDto>? initialHistory = null)
        {
            this.socketService = socketService;
            InitializeComponents();
            SetupLayout();
            LoadElectionData(initialHistory);
            SetupSocketService();
        }

        private void InitializeComponents()
        {
            this.Text = "Leader Election Information";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252); // Light gray background

            // Header Panel - màu đơn giản
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(59, 130, 246) // Clean blue
            };

            // Title
            titleLabel = new Label
            {
                Text = "⚡ Leader Election in Progress",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Subtitle
            subtitleLabel = new Label
            {
                Text = "Bully Algorithm - Highest Node ID Wins",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(240, 245, 255),
                Location = new Point(20, 45),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(20, 10, 20, 20)
            };

            // Scrollable panel
            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            // Close button
            btnClose = new Button
            {
                Text = "✕ Close",
                Size = new Size(100, 30),
                Location = new Point(480, 25),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
        }

        private void SetupLayout()
        {
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Controls.Add(btnClose);

            contentPanel.Controls.Add(scrollPanel);

            this.Controls.Add(headerPanel);
            this.Controls.Add(contentPanel);
        }

        private void LoadElectionData(List<ElectionDto>? initialHistory = null)
        {
            electionEvents = new List<ElectionEvent>();

            if (scrollPanel == null) return;

            scrollPanel.Controls.Clear();

            if (initialHistory != null && initialHistory.Count > 0)
            {
                foreach (var dto in initialHistory)
                {
                    var evt = ConvertToElectionEvent(dto);
                    electionEvents.Add(evt);
                }

                electionEvents = electionEvents.OrderBy(e => e.Timestamp).ToList();
                RenderElectionEvents();

                if (socketService != null && socketService.IsConnected)
                {
                    Label statusLabel = new Label
                    {
                        Text = $"✅ Connected - Showing {electionEvents.Count} events (real-time updates active)",
                        Font = new Font("Segoe UI", 10, FontStyle.Regular),
                        ForeColor = Color.FromArgb(22, 163, 74),
                        AutoSize = true,
                        Location = new Point(30, 10),
                        BackColor = Color.Transparent
                    };
                    scrollPanel.Controls.Add(statusLabel);
                }
            }
            else
            {
                Label waitingLabel = new Label
                {
                    Text = "⏳ Waiting for election events from socket...",
                    Font = new Font("Segoe UI", 12, FontStyle.Regular),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    AutoSize = true,
                    Location = new Point(30, 50),
                    BackColor = Color.Transparent
                };

                if (socketService != null && socketService.IsConnected)
                {
                    waitingLabel.Text = "✅ Connected to socket. Waiting for election events...";
                    waitingLabel.ForeColor = Color.FromArgb(22, 163, 74);
                }
                else if (socketService != null && !socketService.IsConnected)
                {
                    waitingLabel.Text = "⚠️ Socket not connected. Waiting for connection...";
                    waitingLabel.ForeColor = Color.FromArgb(239, 68, 68);
                }
                else if (socketService == null)
                {
                    waitingLabel.Text = "⚠️ Socket service not available. Cannot receive election events.";
                    waitingLabel.ForeColor = Color.FromArgb(239, 68, 68);
                }

                scrollPanel.Controls.Add(waitingLabel);
            }
        }

        private ElectionEvent ConvertToElectionEvent(ElectionDto dto)
        {
            ElectionEventType eventType = ElectionEventType.Participating;

            if (dto.EventType != null)
            {
                string eventTypeStr = dto.EventType.ToUpper();
                if (eventTypeStr == "CANDIDATE" || eventTypeStr.Contains("CANDIDATE"))
                    eventType = ElectionEventType.Candidate;
                else if (eventTypeStr == "ELECTION" || eventTypeStr.Contains("ELECTION") || eventTypeStr.Contains("SENT"))
                    eventType = ElectionEventType.SentElection;
                else if (eventTypeStr == "VICTORY" || eventTypeStr.Contains("WINNER") || eventTypeStr.Contains("VICTORY"))
                    eventType = ElectionEventType.Winner;
                else if (eventTypeStr.Contains("PARTICIPAT"))
                    eventType = ElectionEventType.Participating;
            }

            return new ElectionEvent
            {
                NodeId = dto.NodeId,
                EventType = eventType,
                Message = string.IsNullOrEmpty(dto.Message) ? GenerateMessage(dto.NodeId, eventType) : dto.Message,
                Timestamp = dto.Timestamp == default(DateTime) ? DateTime.Now : dto.Timestamp
            };
        }

        private string GenerateMessage(int nodeId, ElectionEventType eventType)
        {
            return eventType switch
            {
                ElectionEventType.Candidate => $"Node {nodeId} is a candidate for leadership",
                ElectionEventType.SentElection => $"Node {nodeId} sends election message (ID: {nodeId})",
                ElectionEventType.Winner => $"Node {nodeId} wins the election and becomes leader",
                ElectionEventType.Participating => $"Node {nodeId} participates in election",
                _ => $"Node {nodeId} - {eventType}"
            };
        }

        private void SetupSocketService()
        {
            if (socketService != null)
            {
                socketService.OnElectionUpdate += (elections) =>
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => HandleElectionUpdate(elections)));
                    }
                    else
                    {
                        HandleElectionUpdate(elections);
                    }
                };
            }
        }

        private void HandleElectionUpdate(List<ElectionDto> elections)
        {
            Console.WriteLine($"[ElectionInfoForm] HandleElectionUpdate called with {elections?.Count ?? 0} events");

            if (elections != null && elections.Count > 0)
            {
                foreach (var dto in elections)
                {
                    Console.WriteLine($"[ElectionInfoForm] Processing: Node {dto.NodeId}, Type: {dto.EventType}, Message: {dto.Message}");

                    var newEvent = ConvertToElectionEvent(dto);
                    Console.WriteLine($"[ElectionInfoForm] Converted to: Node {newEvent.NodeId}, Type: {newEvent.EventType}");

                    bool isDuplicate = electionEvents.Any(e =>
                        e.NodeId == newEvent.NodeId &&
                        e.EventType == newEvent.EventType &&
                        Math.Abs((e.Timestamp - newEvent.Timestamp).TotalSeconds) < 2);

                    if (!isDuplicate)
                    {
                        electionEvents.Add(newEvent);
                        Console.WriteLine($"[ElectionInfoForm] Added new event, total: {electionEvents.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"[ElectionInfoForm] Duplicate event skipped");
                    }
                }

                if (electionEvents.Count > MAX_HISTORY)
                {
                    electionEvents = electionEvents
                        .OrderByDescending(e => e.Timestamp)
                        .Take(MAX_HISTORY)
                        .OrderBy(e => e.Timestamp)
                        .ToList();
                }

                electionEvents = electionEvents.OrderBy(e => e.Timestamp).ToList();

                Console.WriteLine($"[ElectionInfoForm] Rendering {electionEvents.Count} events");
                RenderElectionEvents();
            }
            else
            {
                Console.WriteLine($"[ElectionInfoForm] No events to process");
            }
        }

        public void UpdateElectionData(List<ElectionEvent> events)
        {
            electionEvents = events ?? new List<ElectionEvent>();
            RenderElectionEvents();
        }

        private void RenderElectionEvents()
        {
            if (scrollPanel == null) return;

            scrollPanel.Controls.Clear();
            Console.WriteLine($"[ElectionInfoForm] RenderElectionEvents: {electionEvents?.Count ?? 0} events");

            if (electionEvents == null || electionEvents.Count == 0)
            {
                Label emptyLabel = new Label
                {
                    Text = "No election events available",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    AutoSize = true,
                    Location = new Point(20, 20),
                    BackColor = Color.Transparent
                };
                scrollPanel.Controls.Add(emptyLabel);
                return;
            }

            int yPos = 80;
            int cardWidth = scrollPanel.Width - 40;

            foreach (var evt in electionEvents)
            {
                Console.WriteLine($"[ElectionInfoForm] Creating card for: Node {evt.NodeId}, Type: {evt.EventType}");
                RoundedPanel eventCard = CreateEventCard(evt, cardWidth);
                eventCard.Location = new Point(10, yPos);
                scrollPanel.Controls.Add(eventCard);
                yPos += eventCard.Height + 10;
            }

            Console.WriteLine($"[ElectionInfoForm] Rendered {electionEvents.Count} cards");
        }

        private RoundedPanel CreateEventCard(ElectionEvent evt, int width)
        {
            // Màu nền của card dựa trên loại event - nổi bật hơn
            Color cardBackColor = GetCardBackgroundColor(evt.EventType);
            Color borderColor = GetCardBorderColor(evt.EventType);

            RoundedPanel card = new RoundedPanel
            {
                Size = new Size(width, 70),
                BackColor = cardBackColor,
                BorderRadius = 8,
                BorderColor = borderColor,
                EnableHoverEffect = false
            };

            // Icon dựa trên loại event
            Label iconLabel = new Label
            {
                Text = GetEventIcon(evt.EventType),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = GetEventIconColor(evt.EventType),
                Location = new Point(20, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Node name
            Label nodeLabel = new Label
            {
                Text = $"Node {evt.NodeId}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = GetTextColor(evt.EventType),
                Location = new Point(70, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Message
            Label messageLabel = new Label
            {
                Text = evt.Message,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = GetSecondaryTextColor(evt.EventType),
                Location = new Point(70, 38),
                Size = new Size(width - 90, 25),
                BackColor = Color.Transparent
            };

            card.Controls.Add(iconLabel);
            card.Controls.Add(nodeLabel);
            card.Controls.Add(messageLabel);

            return card;
        }

        // Màu nền card dựa trên event type - nổi bật hơn
        private Color GetCardBackgroundColor(ElectionEventType eventType)
        {
            return eventType switch
            {
                ElectionEventType.Candidate => Color.FromArgb(254, 249, 195),    // Light yellow
                ElectionEventType.SentElection => Color.FromArgb(219, 234, 254), // Light blue
                ElectionEventType.Winner => Color.FromArgb(220, 252, 231),       // Light green
                ElectionEventType.Participating => Color.FromArgb(241, 245, 249), // Light gray
                _ => Color.White
            };
        }

        // Màu border card
        private Color GetCardBorderColor(ElectionEventType eventType)
        {
            return eventType switch
            {
                ElectionEventType.Candidate => Color.FromArgb(234, 179, 8),     // Dark yellow
                ElectionEventType.SentElection => Color.FromArgb(59, 130, 246), // Blue
                ElectionEventType.Winner => Color.FromArgb(22, 163, 74),        // Green
                ElectionEventType.Participating => Color.FromArgb(203, 213, 225), // Gray
                _ => Color.LightGray
            };
        }

        // Màu text chính
        private Color GetTextColor(ElectionEventType eventType)
        {
            return eventType switch
            {
                ElectionEventType.Candidate => Color.FromArgb(133, 77, 14),     // Dark yellow-brown
                ElectionEventType.SentElection => Color.FromArgb(30, 64, 175),  // Dark blue
                ElectionEventType.Winner => Color.FromArgb(21, 128, 61),        // Dark green
                ElectionEventType.Participating => Color.FromArgb(51, 65, 85),  // Dark gray
                _ => Color.Black
            };
        }

        // Màu text phụ (message)
        private Color GetSecondaryTextColor(ElectionEventType eventType)
        {
            return eventType switch
            {
                ElectionEventType.Candidate => Color.FromArgb(161, 98, 7),      // Medium yellow-brown
                ElectionEventType.SentElection => Color.FromArgb(37, 99, 235),  // Medium blue
                ElectionEventType.Winner => Color.FromArgb(22, 163, 74),        // Medium green
                ElectionEventType.Participating => Color.FromArgb(71, 85, 105), // Medium gray
                _ => Color.Gray
            };
        }

        // Màu icon
        private Color GetEventIconColor(ElectionEventType eventType)
        {
            return eventType switch
            {
                ElectionEventType.Candidate => Color.FromArgb(234, 179, 8),     // Gold
                ElectionEventType.SentElection => Color.FromArgb(59, 130, 246), // Blue
                ElectionEventType.Winner => Color.FromArgb(22, 163, 74),        // Green
                ElectionEventType.Participating => Color.FromArgb(100, 116, 139), // Gray
                _ => Color.Black
            };
        }

        private string GetEventIcon(ElectionEventType eventType)
        {
            return eventType switch
            {
                ElectionEventType.Candidate => "👑",
                ElectionEventType.SentElection => "⚡",
                ElectionEventType.Winner => "🏆",
                ElectionEventType.Participating => "👤",
                _ => "•"
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (scrollPanel != null && electionEvents != null && electionEvents.Count > 0)
            {
                int cardWidth = scrollPanel.Width - 40;
                foreach (Control control in scrollPanel.Controls)
                {
                    if (control is RoundedPanel card)
                    {
                        card.Width = cardWidth;
                        foreach (Control child in card.Controls)
                        {
                            if (child is Label label &&
                                (label.Text.Contains("sends") || label.Text.Contains("participates") ||
                                 label.Text.Contains("wins") || label.Text.Contains("candidate")))
                            {
                                label.Width = cardWidth - 90;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }

    public class ElectionEvent
    {
        public int NodeId { get; set; }
        public ElectionEventType EventType { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum ElectionEventType
    {
        Candidate,
        SentElection,
        Winner,
        Participating
    }
}