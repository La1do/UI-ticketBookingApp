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
            this.BackColor = Color.FromArgb(15, 23, 42);

            // Header Panel - giảm height để content hiện hết
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };

            // Title với icon lightning
            titleLabel = new Label
            {
                Text = "⚡ Leader Election in Progress ⚡",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Subtitle
            subtitleLabel = new Label
            {
                Text = "Bully Algorithm - Highest Node ID Wins",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(20, 42),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Content Panel với gradient background
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 10, 20, 20)
            };

            // Scrollable panel cho danh sách events
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
                Location = new Point(480, 5),
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Vẽ gradient background cho form
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(139, 92, 246),  // Purple
                Color.FromArgb(236, 72, 153),   // Pink
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            // Vẽ header panel với gradient
            Rectangle headerRect = new Rectangle(0, 0, this.Width, headerPanel.Height);
            using (LinearGradientBrush headerBrush = new LinearGradientBrush(
                headerRect,
                Color.FromArgb(100, 139, 92, 246),
                Color.FromArgb(100, 236, 72, 153),
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(headerBrush, headerRect);
            }
        }

        private void LoadElectionData(List<ElectionDto>? initialHistory = null)
        {
            electionEvents = new List<ElectionEvent>();
            
            // Đảm bảo scrollPanel đã được khởi tạo
            if (scrollPanel == null) return;
            
            scrollPanel.Controls.Clear();
            
            // Nếu có initial history, load vào
            if (initialHistory != null && initialHistory.Count > 0)
            {
                foreach (var dto in initialHistory)
                {
                    var evt = ConvertToElectionEvent(dto);
                    electionEvents.Add(evt);
                }
                
                // Sắp xếp theo thời gian
                electionEvents = electionEvents.OrderBy(e => e.Timestamp).ToList();
                
                // Render ngay
                RenderElectionEvents();
                
                // Hiển thị thông báo nếu có socket
                if (socketService != null && socketService.IsConnected)
                {
                    Label statusLabel = new Label
                    {
                        Text = $"✅ Connected - Showing {electionEvents.Count} events (real-time updates active)",
                        Font = new Font("Segoe UI", 10, FontStyle.Regular),
                        ForeColor = Color.FromArgb(16, 185, 129),
                        AutoSize = true,
                        Location = new Point(30, 10),
                        BackColor = Color.Transparent
                    };
                    scrollPanel.Controls.Add(statusLabel);
                }
            }
            else
            {
                // Không có history, hiển thị message đang chờ
                Label waitingLabel = new Label
                {
                    Text = "⏳ Waiting for election events from socket...",
                    Font = new Font("Segoe UI", 14, FontStyle.Regular),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(30, 50),
                    BackColor = Color.Transparent
                };
                
                // Nếu có socket service và đã kết nối, hiển thị thông báo khác
                if (socketService != null && socketService.IsConnected)
                {
                    waitingLabel.Text = "✅ Connected to socket. Waiting for election events...";
                    waitingLabel.ForeColor = Color.FromArgb(16, 185, 129);
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
            
            // Parse eventType từ string (backend gửi: CANDIDATE, ELECTION, VICTORY)
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
                // Thêm events mới vào history (không thay thế toàn bộ)
                foreach (var dto in elections)
                {
                    Console.WriteLine($"[ElectionInfoForm] Processing: Node {dto.NodeId}, Type: {dto.EventType}, Message: {dto.Message}");
                    
                    var newEvent = ConvertToElectionEvent(dto);
                    Console.WriteLine($"[ElectionInfoForm] Converted to: Node {newEvent.NodeId}, Type: {newEvent.EventType}");
                    
                    // Tránh duplicate - check xem đã có event tương tự chưa
                    bool isDuplicate = electionEvents.Any(e => 
                        e.NodeId == newEvent.NodeId && 
                        e.EventType == newEvent.EventType && 
                        Math.Abs((e.Timestamp - newEvent.Timestamp).TotalSeconds) < 2); // Trong vòng 2 giây
                    
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
                
                // Giới hạn số lượng events trong history
                if (electionEvents.Count > MAX_HISTORY)
                {
                    electionEvents = electionEvents
                        .OrderByDescending(e => e.Timestamp)
                        .Take(MAX_HISTORY)
                        .OrderBy(e => e.Timestamp)
                        .ToList();
                }
                
                // Sắp xếp theo thời gian
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
                    ForeColor = Color.FromArgb(148, 163, 184),
                    AutoSize = true,
                    Location = new Point(20, 20),
                    BackColor = Color.Transparent
                };
                scrollPanel.Controls.Add(emptyLabel);
                return;
            }

            int yPos = 80;
            int cardWidth = scrollPanel.Width - 40; // Trừ padding và scrollbar

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
            RoundedPanel card = new RoundedPanel
            {
                Size = new Size(width, 70),
                BackColor = Color.FromArgb(60, 139, 92, 246), // Semi-transparent purple
                BorderRadius = 12,
                BorderColor = Color.FromArgb(80, 255, 255, 255),
                EnableHoverEffect = false
            };

            // Icon dựa trên loại event
            Label iconLabel = new Label
            {
                Text = GetEventIcon(evt.EventType),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = GetEventColor(evt.EventType),
                Location = new Point(20, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Node name - dịch sang phải để hiện rõ hơn
            Label nodeLabel = new Label
            {
                Text = $"Node {evt.NodeId}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(70, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Message - dịch sang phải để hiện rõ hơn
            Label messageLabel = new Label
            {
                Text = evt.Message,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(220, 220, 220),
                Location = new Point(70, 38),
                Size = new Size(width - 90, 25),
                BackColor = Color.Transparent
            };

            // Checkmark icon cho candidate
            if (evt.EventType == ElectionEventType.Candidate)
            {
                Label checkIcon = new Label
                {
                    Text = "✓",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(16, 185, 129), // Green
                    Location = new Point(width - 35, 10),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(checkIcon);
            }

            card.Controls.Add(iconLabel);
            card.Controls.Add(nodeLabel);
            card.Controls.Add(messageLabel);

            return card;
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

        private Color GetEventColor(ElectionEventType eventType)
        {
            return eventType switch
            {
                ElectionEventType.Candidate => Color.FromArgb(251, 191, 36), // Gold
                ElectionEventType.SentElection => Color.FromArgb(251, 191, 36), // Gold/Yellow
                ElectionEventType.Winner => Color.FromArgb(16, 185, 129), // Green
                ElectionEventType.Participating => Color.FromArgb(148, 163, 184), // Gray
                _ => Color.White
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (scrollPanel != null && electionEvents != null && electionEvents.Count > 0)
            {
                // Cập nhật lại width của các cards khi resize
                int cardWidth = scrollPanel.Width - 40;
                foreach (Control control in scrollPanel.Controls)
                {
                    if (control is RoundedPanel card)
                    {
                        card.Width = cardWidth;
                        // Cập nhật lại width của message label
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
            // Unsubscribe từ socket service khi form đóng
            if (socketService != null)
            {
                // Note: SocketService không có cách unsubscribe riêng, 
                // nhưng form sẽ được dispose nên không sao
            }
            base.OnFormClosing(e);
        }
    }

    // Class để lưu thông tin election event
    public class ElectionEvent
    {
        public int NodeId { get; set; }
        public ElectionEventType EventType { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum ElectionEventType
    {
        Candidate,      // Node là candidate cho leadership
        SentElection,   // Node gửi election message
        Winner,         // Node chiến thắng
        Participating    // Node tham gia (không làm gì)
    }
}
