using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CinemaTicketBooking
{
    public class SeatSelectionForm : Form
    {
        private FlowLayoutPanel gridSeats = null!;
        private List<string> myBookedSeats = new List<string>();
        private Label lblScreen = null!;
        private Label lblMovieInfo = null!;
        private Label lblSelectedInfo = null!;
        private ModernButton btnConfirm = null!;
        private ModernButton btnBack = null!;

        private int rows = 8;
        private int cols = 10;
        private int movieId;
        private string showtime;
        private List<SeatDto> allSeats = new List<SeatDto>();
        private Label lblLoading = null!;
        private string customerName = string.Empty;

        public SeatSelectionForm(int movieId, string time)
        {
            this.movieId = movieId;
            this.showtime = time;
            SetupUI();
        }

        private void SetupUI()
        {
            // Form settings
            this.Text = "Select Your Seat";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppColors.BackgroundStart;
            this.DoubleBuffered = true;
            this.MinimumSize = new Size(1000, 700);

            // Header Panel
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 100;
            headerPanel.BackColor = Color.Transparent;
            this.Controls.Add(headerPanel);

            Label lblTitle = new Label();
            lblTitle.Text = "🎭 Select Your Seat";
            lblTitle.Font = new Font("Segoe UI", 26, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(40, 30);
            lblTitle.BackColor = Color.Transparent;
            headerPanel.Controls.Add(lblTitle);

            lblMovieInfo = new Label();
            lblMovieInfo.Text = $"Showtime: {showtime}";
            lblMovieInfo.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            lblMovieInfo.ForeColor = AppColors.TextSecondary;
            lblMovieInfo.AutoSize = true;
            lblMovieInfo.Location = new Point(42, 70);
            lblMovieInfo.BackColor = Color.Transparent;
            headerPanel.Controls.Add(lblMovieInfo);

            // Main content panel
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.Transparent;
            mainPanel.AutoScroll = true;
            this.Controls.Add(mainPanel);

            // Container for centered content
            Panel centerContainer = new Panel();
            centerContainer.Size = new Size(700, 700);
            centerContainer.BackColor = Color.Transparent;
            centerContainer.Location = new Point((mainPanel.Width - 700) / 2, 20);
            mainPanel.Controls.Add(centerContainer);

            mainPanel.Resize += (s, e) => {
                centerContainer.Location = new Point((mainPanel.Width - 700) / 2, 20);
            };

            // Screen label
            lblScreen = new Label();
            lblScreen.Text = "SCREEN";
            lblScreen.ForeColor = Color.FromArgb(165, 180, 252);
            lblScreen.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblScreen.AutoSize = true;
            lblScreen.BackColor = Color.Transparent;
            lblScreen.Location = new Point(315, 10);
            centerContainer.Controls.Add(lblScreen);

            // Screen panel
            Panel pnlScreen = new Panel();
            pnlScreen.Size = new Size(600, 60);
            pnlScreen.Location = new Point(50, 35);
            pnlScreen.BackColor = Color.Transparent;
            pnlScreen.Paint += PnlScreen_Paint;
            centerContainer.Controls.Add(pnlScreen);

            // Seats grid
            gridSeats = new FlowLayoutPanel();
            gridSeats.Size = new Size(cols * 55 + 20, rows * 55 + 20);
            gridSeats.Location = new Point((700 - gridSeats.Width) / 2, 130);
            gridSeats.BackColor = Color.Transparent;
            centerContainer.Controls.Add(gridSeats);

            // Loading label
            lblLoading = new Label();
            lblLoading.Text = "Loading seats...";
            lblLoading.Font = new Font("Segoe UI", 14, FontStyle.Regular);
            lblLoading.ForeColor = AppColors.TextSecondary;
            lblLoading.AutoSize = true;
            lblLoading.BackColor = Color.Transparent;
            lblLoading.Location = new Point((700 - 150) / 2, 200);
            centerContainer.Controls.Add(lblLoading);

            // Load seats from API
            _ = LoadSeatsAsync();

            // Legend
            int legendY = 130 + gridSeats.Height + 30;
            CreateLegend(centerContainer, (700 - 420) / 2, legendY);

            // Selected seat info
            lblSelectedInfo = new Label();
            lblSelectedInfo.Text = "No seat selected";
            lblSelectedInfo.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblSelectedInfo.ForeColor = AppColors.AccentGold;
            lblSelectedInfo.AutoSize = true;
            lblSelectedInfo.BackColor = Color.Transparent;
            lblSelectedInfo.Location = new Point((700 - 150) / 2, legendY + 70);
            centerContainer.Controls.Add(lblSelectedInfo);

            // Footer Panel
            Panel footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Height = 90;
            footerPanel.BackColor = AppColors.CardBg;
            this.Controls.Add(footerPanel);

            // Back button
            btnBack = new ModernButton();
            btnBack.Text = "← Back";
            btnBack.Size = new Size(140, 55);
            btnBack.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnBack.Location = new Point(40, 18);
            btnBack.BorderRadius = 12;
            btnBack.Click += (s, e) => {
                this.Close();
            };
            footerPanel.Controls.Add(btnBack);

            // Cart button (thay Confirm button)
            btnConfirm = new ModernButton();
            btnConfirm.Text = "🛒 Cart (0)";
            btnConfirm.Size = new Size(180, 55);
            btnConfirm.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnConfirm.BorderRadius = 12;
            btnConfirm.Click += BtnCart_Click;
            btnConfirm.Anchor = AnchorStyles.Right;
            btnConfirm.Location = new Point(footerPanel.Width - 220, 18);
            footerPanel.Controls.Add(btnConfirm);

            footerPanel.Resize += (s, e) => {
                btnConfirm.Location = new Point(footerPanel.Width - 220, 18);
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle, 
                AppColors.BackgroundStart, 
                AppColors.BackgroundEnd, 
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }

        private void PnlScreen_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                new Rectangle(0, 0, 600, 60), 
                Color.FromArgb(80, 99, 102, 241), 
                Color.Transparent, 
                LinearGradientMode.Vertical))
            {
                GraphicsPath path = new GraphicsPath();
                path.AddBezier(0, 0, 150, 20, 450, 20, 600, 0);
                path.AddLine(600, 0, 550, 50);
                path.AddLine(550, 50, 50, 50);
                path.AddLine(50, 50, 0, 0);
                g.FillPath(brush, path);
            }

            // Add glow effect
            using (Pen glowPen = new Pen(Color.FromArgb(50, 165, 180, 252), 3))
            {
                GraphicsPath glowPath = new GraphicsPath();
                glowPath.AddBezier(0, 0, 150, 20, 450, 20, 600, 0);
                g.DrawPath(glowPen, glowPath);
            }
        }

        private async Task LoadSeatsAsync()
        {
            try
            {
                // Fetch seats from API
                allSeats = await ApiServiceSeat.GetSeatsAsync();
                
                // Hide loading label
                if (lblLoading != null)
                {
                    lblLoading.Visible = false;
                }

                // Clear existing seats before generating new ones
                if (gridSeats != null)
                {
                    gridSeats.Controls.Clear();
                }

                // Generate seats from API data
                GenerateSeats();
            }
            catch (Exception ex)
            {
                // Show error message
                if (lblLoading != null)
                {
                    lblLoading.Text = "Error loading seats. Using default layout.";
                    lblLoading.ForeColor = Color.FromArgb(239, 68, 68);
                }
                
                MessageBox.Show(
                    $"Failed to load seats from API.\nError: {ex.Message}\n\nUsing default seat layout.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                // Clear existing seats before generating new ones
                if (gridSeats != null)
                {
                    gridSeats.Controls.Clear();
                }

                // Generate seats with empty data (all available)
                GenerateSeats();
            }
        }

        private void GenerateSeats()
        {
            char[] rowLabels = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 1; c <= cols; c++)
                {
                    string seatName = $"{rowLabels[r]}{c}";
                    
                    // Find seat in API data
                    SeatDto? seatDto = allSeats?.FirstOrDefault(s => 
                        s.seatNumber.Equals(seatName, StringComparison.OrdinalIgnoreCase));
                    
                    // Determine if seat is booked
                    bool isBooked = seatDto != null && seatDto.IsOccupied;

                    Button btnSeat = new Button();
                    btnSeat.Text = seatName;
                    btnSeat.Tag = new SeatInfo 
                    { 
                        Name = seatName, 
                        IsBooked = isBooked,
                        SeatId = seatDto?.id ?? 0,
                        SeatDto = seatDto
                    };
                    btnSeat.Size = new Size(50, 50);
                    btnSeat.Margin = new Padding(2);
                    btnSeat.FlatStyle = FlatStyle.Flat;
                    btnSeat.FlatAppearance.BorderSize = 0;
                    btnSeat.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                    btnSeat.Paint += BtnSeat_Paint;
                    btnSeat.Click += BtnSeat_Click;
                    btnSeat.Cursor = isBooked ? Cursors.No : Cursors.Hand;
                    
                    gridSeats.Controls.Add(btnSeat);
                }
            }
        }

        private class SeatInfo
        {
            public string Name { get; set; } = string.Empty;
            public bool IsBooked { get; set; } = false;
            public int SeatId { get; set; } = 0;
            public SeatDto? SeatDto { get; set; }
        }

        private void BtnSeat_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SeatInfo seatInfo)
            {
                Color seatColor;
                Color borderColor;
                Color textColor;

                if (seatInfo.IsBooked)
                {
                    // Booked by others
                    seatColor = AppColors.SeatBooked;
                    borderColor = Color.FromArgb(148, 163, 184);
                    textColor = Color.FromArgb(100, 116, 139);
                }
                else if (myBookedSeats.Contains(seatInfo.Name))
                {
                    // My booked seats
                    seatColor = AppColors.SeatMyBooked;
                    borderColor = AppColors.SeatMyBooked;
                    textColor = Color.White;
                }
                else
                {
                    // Available
                    seatColor = AppColors.CardBg;
                    borderColor = Color.FromArgb(71, 85, 105);
                    textColor = AppColors.TextSecondary;
                }

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw seat shape
                Rectangle seatRect = new Rectangle(4, 4, 42, 32);
                using (GraphicsPath seatPath = GetRoundedRect(seatRect, 8))
                using (SolidBrush brush = new SolidBrush(seatColor))
                {
                    e.Graphics.FillPath(brush, seatPath);
                }

                // Seat base
                Rectangle baseRect = new Rectangle(4, 30, 42, 12);
                using (SolidBrush brush = new SolidBrush(seatColor))
                {
                    e.Graphics.FillRectangle(brush, baseRect);
                }

                // Armrests
                using (SolidBrush brush = new SolidBrush(seatColor))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(1, 25, 5, 18));
                    e.Graphics.FillRectangle(brush, new Rectangle(44, 25, 5, 18));
                }

                // Border
                using (Pen pen = new Pen(borderColor, 2))
                {
                    e.Graphics.DrawPath(pen, GetRoundedRect(seatRect, 8));
                    e.Graphics.DrawRectangle(pen, baseRect);
                    e.Graphics.DrawRectangle(pen, new Rectangle(1, 25, 5, 18));
                    e.Graphics.DrawRectangle(pen, new Rectangle(44, 25, 5, 18));
                }

                // Draw seat label
                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(seatInfo.Name, btn.Font, textBrush, 
                        new Rectangle(0, 0, 50, 40), sf);
                }
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2.0F;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BtnSeat_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is SeatInfo seatInfo)
            {
                // Không cho click ghế đã được book bởi người khác
                if (seatInfo.IsBooked)
                {
                    return;
                }

                // Nếu ghế đã trong danh sách của mình, tự động xóa
                if (myBookedSeats.Contains(seatInfo.Name))
                {
                    myBookedSeats.Remove(seatInfo.Name);
                    UpdateUI();
                }
                else
                {
                    // Ghế available, tự động chọn
                    // Kiểm tra giới hạn trước
                    if (myBookedSeats.Count >= 8)
                    {
                        return;
                    }

                    myBookedSeats.Add(seatInfo.Name);
                    UpdateUI();
                }
            }
        }

        private void UpdateUI()
        {
            // Update all seat visuals
            foreach (Control c in gridSeats.Controls)
            {
                c.Invalidate();
            }

            // Update cart button
            btnConfirm.Text = $"🛒 Cart ({myBookedSeats.Count})";
            
            if (myBookedSeats.Count > 0)
            {
                lblSelectedInfo.Text = $"Selected: {myBookedSeats.Count} seat{(myBookedSeats.Count > 1 ? "s" : "")}";
                btnConfirm.IsSelected = true;
            }
            else
            {
                lblSelectedInfo.Text = "No seat selected";
                btnConfirm.IsSelected = false;
            }
            
            btnConfirm.Invalidate();
        }

        private void BtnCart_Click(object? sender, EventArgs e)
        {
            if (myBookedSeats.Count == 0)
            {
                MessageBox.Show(
                    "Your cart is empty!\nPlease select seats first.", 
                    "Empty Cart", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                return;
            }

            ShowCartDialog();
        }

        private void ShowCartDialog()
        {
            // Create cart dialog
            Form cartDialog = new Form();
            cartDialog.Text = "Shopping Cart";
            cartDialog.Size = new Size(550, 700);
            cartDialog.StartPosition = FormStartPosition.CenterParent;
            cartDialog.BackColor = AppColors.BackgroundStart;
            cartDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            cartDialog.MaximizeBox = false;
            cartDialog.MinimizeBox = false;

            // Header
            Label lblHeader = new Label();
            lblHeader.Text = "🛒 Your Selected Seats";
            lblHeader.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblHeader.ForeColor = Color.White;
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(35, 30);
            lblHeader.BackColor = Color.Transparent;
            cartDialog.Controls.Add(lblHeader);

            // Divider line
            Panel divider = new Panel();
            divider.Size = new Size(480, 2);
            divider.Location = new Point(35, 80);
            divider.BackColor = Color.FromArgb(71, 85, 105);
            cartDialog.Controls.Add(divider);

            // Scrollable panel for seat list
            Panel scrollPanel = new Panel();
            scrollPanel.Location = new Point(35, 100);
            scrollPanel.Size = new Size(480, 390);
            scrollPanel.AutoScroll = true;
            scrollPanel.BackColor = Color.Transparent;
            cartDialog.Controls.Add(scrollPanel);

            // Add seat items
            int yPos = 10;
            foreach (string seatName in myBookedSeats)
            {
                RoundedPanel seatItem = CreateSeatItem(seatName, cartDialog);
                seatItem.Location = new Point(10, yPos);
                scrollPanel.Controls.Add(seatItem);
                yPos += 75;
            }

            // Customer name input panel
            RoundedPanel namePanel = new RoundedPanel();
            namePanel.Size = new Size(480, 60);
            namePanel.Location = new Point(35, 490);
            namePanel.BackColor = AppColors.CardBg;
            namePanel.BorderRadius = 12;
            namePanel.EnableHoverEffect = false;
            cartDialog.Controls.Add(namePanel);

            Label lblCustomerName = new Label();
            lblCustomerName.Text = "Customer Name:";
            lblCustomerName.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblCustomerName.ForeColor = Color.White;
            lblCustomerName.AutoSize = true;
            lblCustomerName.Location = new Point(25, 0);
            lblCustomerName.BackColor = Color.Transparent;
            namePanel.Controls.Add(lblCustomerName);

            TextBox txtCustomerName = new TextBox();
            txtCustomerName.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            txtCustomerName.ForeColor = Color.White;
            txtCustomerName.BackColor = Color.FromArgb(51, 65, 85);
            txtCustomerName.BorderStyle = BorderStyle.FixedSingle;
            txtCustomerName.Size = new Size(430, 30);
            txtCustomerName.Location = new Point(25, 22);
            txtCustomerName.PlaceholderText = "Enter your name...";
            namePanel.Controls.Add(txtCustomerName);

            // Summary panel
            RoundedPanel summaryPanel = new RoundedPanel();
            summaryPanel.Size = new Size(480, 60);
            summaryPanel.Location = new Point(35, 570);
            summaryPanel.BackColor = AppColors.CardBg;
            summaryPanel.BorderRadius = 12;
            summaryPanel.EnableHoverEffect = false;

            Label lblTotal = new Label();
            lblTotal.Text = $"Total Seats: {myBookedSeats.Count}";
            lblTotal.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lblTotal.ForeColor = Color.White;
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(25, 13);
            lblTotal.BackColor = Color.Transparent;
            summaryPanel.Controls.Add(lblTotal);

            Label lblPrice = new Label();
            lblPrice.Text = $"${myBookedSeats.Count * 12}.00";
            lblPrice.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblPrice.ForeColor = AppColors.AccentGold;
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(370, 10);
            lblPrice.BackColor = Color.Transparent;
            summaryPanel.Controls.Add(lblPrice);

            cartDialog.Controls.Add(summaryPanel);

            // Confirm booking button
            ModernButton btnConfirmBooking = new ModernButton();
            btnConfirmBooking.Text = "✓ Confirm Booking";
            btnConfirmBooking.Size = new Size(220, 55);
            btnConfirmBooking.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            btnConfirmBooking.Location = new Point(165, 650);
            btnConfirmBooking.BorderRadius = 12;
            btnConfirmBooking.IsSelected = true;
            btnConfirmBooking.Click += (s, e) => {
                // Validate customer name
                if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
                {
                    MessageBox.Show(
                        "Please enter your name before confirming booking.",
                        "Name Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtCustomerName.Focus();
                    return;
                }

                customerName = txtCustomerName.Text.Trim();
                cartDialog.DialogResult = DialogResult.OK;
                cartDialog.Close();
            };
            cartDialog.Controls.Add(btnConfirmBooking);

            // Adjust dialog height to accommodate new input
            cartDialog.Size = new Size(550, 760);

            // Show dialog
            if (cartDialog.ShowDialog() == DialogResult.OK)
            {
                // Confirm booking
                BtnConfirmBooking_Click();
            }
        }

        private RoundedPanel CreateSeatItem(string seatName, Form parentForm)
        {
            RoundedPanel item = new RoundedPanel();
            item.Size = new Size(460, 65);
            item.BackColor = AppColors.CardBg;
            item.BorderRadius = 10;
            item.BorderColor = Color.FromArgb(40, 255, 255, 255);
            item.EnableHoverEffect = false;

            // Seat icon
            Panel seatIcon = new Panel();
            seatIcon.Size = new Size(45, 45);
            seatIcon.Location = new Point(15, 10);
            seatIcon.BackColor = AppColors.SeatMyBooked;
            seatIcon.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(AppColors.SeatMyBooked, 2))
                {
                    e.Graphics.DrawRectangle(pen, 3, 3, 39, 39);
                }
                // Draw seat symbol
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(brush, 10, 10, 25, 15);
                    e.Graphics.FillRectangle(brush, 10, 22, 25, 10);
                    e.Graphics.FillRectangle(brush, 7, 17, 5, 15);
                    e.Graphics.FillRectangle(brush, 33, 17, 5, 15);
                }
            };
            item.Controls.Add(seatIcon);

            // Seat name
            Label lblSeatName = new Label();
            lblSeatName.Text = $"Seat {seatName}";
            lblSeatName.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lblSeatName.ForeColor = Color.White;
            lblSeatName.AutoSize = true;
            lblSeatName.Location = new Point(75, 12);
            lblSeatName.BackColor = Color.Transparent;
            item.Controls.Add(lblSeatName);

            // Price
            Label lblPrice = new Label();
            lblPrice.Text = "$12.00";
            lblPrice.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            lblPrice.ForeColor = AppColors.TextSecondary;
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(75, 37);
            lblPrice.BackColor = Color.Transparent;
            item.Controls.Add(lblPrice);

            // Remove button (X)
            ModernButton btnRemove = new ModernButton();
            btnRemove.Text = "✕";
            btnRemove.Size = new Size(45, 45);
            btnRemove.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            btnRemove.Location = new Point(400, 10);
            btnRemove.BorderRadius = 8;
            btnRemove.ForeColor = Color.FromArgb(239, 68, 68);
            btnRemove.Click += (s, e) => {
                myBookedSeats.Remove(seatName);
                UpdateUI();
                
                // Close and reopen cart if empty
                if (myBookedSeats.Count == 0)
                {
                    parentForm.Close();
                }
                else
                {
                    // Refresh cart dialog
                    parentForm.Close();
                    ShowCartDialog();
                }
            };
            item.Controls.Add(btnRemove);

            return item;
        }

        private async void BtnConfirmBooking_Click()
        {
            if (myBookedSeats.Count > 0)
            {
                // Book all seats via API - gọi API cho từng ghế một
                // Mỗi lần gọi sẽ gửi: {"seatId": "A1", "customerName": "John Doe"}
                List<string> failedSeats = new List<string>();
                List<string> successSeats = new List<string>();

                foreach (string seatId in myBookedSeats)
                {
                    // Gọi API POST /seat/book với body {"seatId": seatId, "customerName": customerName}
                    bool success = await ApiServiceSeat.BookSeatAsync(seatId, customerName);
                    if (success)
                    {
                        successSeats.Add(seatId);
                    }
                    else
                    {
                        failedSeats.Add(seatId);
                    }
                }

                string seatList = string.Join(", ", myBookedSeats);
                decimal totalPrice = myBookedSeats.Count * 12.00m;
                
                // Show result message
                string message = $"✅ Booking Confirmed!\n\n" +
                    $"👤 Customer: {customerName}\n" +
                    $"🎬 Movie ID: {movieId}\n" +
                    $"⏰ Showtime: {showtime}\n" +
                    $"💺 Seats: {seatList}\n" +
                    $"🎫 Quantity: {myBookedSeats.Count} ticket{(myBookedSeats.Count > 1 ? "s" : "")}\n" +
                    $"💰 Total: ${totalPrice:F2}\n\n";

                if (failedSeats.Count > 0)
                {
                    message += $"⚠️ Warning: {failedSeats.Count} seat(s) failed to book: {string.Join(", ", failedSeats)}\n\n";
                }

                message += $"Thank you for your booking!";

                MessageBox.Show(
                    message,
                    failedSeats.Count > 0 ? "Booking Partially Successful" : "Booking Successful",
                    MessageBoxButtons.OK,
                    failedSeats.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information
                );

                // Clear selected seats after booking
                myBookedSeats.Clear();
                customerName = string.Empty;
                UpdateUI();
                
                // Reload seats to refresh booking status
                _ = LoadSeatsAsync();
            }
        }

        private void CreateLegend(Panel parent, int x, int y)
        {
            RoundedPanel legendPanel = new RoundedPanel();
            legendPanel.Location = new Point(x, y);
            legendPanel.Size = new Size(420, 50);
            legendPanel.BackColor = AppColors.CardBg;
            legendPanel.BorderRadius = 12;
            legendPanel.BorderColor = Color.FromArgb(40, 255, 255, 255);
            legendPanel.EnableHoverEffect = false;

            AddLegendItem(legendPanel, "Available", AppColors.CardBg, Color.FromArgb(71, 85, 105), 30);
            AddLegendItem(legendPanel, "My Seats", AppColors.SeatMyBooked, AppColors.SeatMyBooked, 170);
            AddLegendItem(legendPanel, "Booked", AppColors.SeatBooked, Color.FromArgb(148, 163, 184), 310);

            parent.Controls.Add(legendPanel);
        }

        private void AddLegendItem(Panel parent, string text, Color fillColor, Color borderColor, int x)
        {
            // Seat icon
            Panel seatIcon = new Panel();
            seatIcon.Size = new Size(24, 24);
            seatIcon.Location = new Point(x, 13);
            seatIcon.BackColor = fillColor;
            seatIcon.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(borderColor, 2))
                {
                    e.Graphics.DrawRectangle(pen, 2, 2, 20, 20);
                }
            };

            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x + 30, 15);
            label.AutoSize = true;
            label.ForeColor = AppColors.TextSecondary;
            label.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            label.BackColor = Color.Transparent;

            parent.Controls.Add(seatIcon);
            parent.Controls.Add(label);
        }
    }
}