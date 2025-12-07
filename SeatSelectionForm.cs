using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CinemaTicketBooking
{
    public class SeatSelectionForm : Form
    {
        private FlowLayoutPanel gridSeats = null!;
        private List<string> myBookedSeats = new List<string>();
        private string? currentSelectedSeat = null;
        private Label lblScreen = null!;

        private int rows = 4;
        private int cols = 8;

        public SeatSelectionForm(int movieId, string time)
        {
            SetupUI(movieId, time);
        }

        private void SetupUI(int movieId, string time)
        {
            this.Text = "Select Your Seat";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            Label lblTitle = new Label();
            lblTitle.Text = "Select Your Seat";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.Black;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point((this.Width - 250) / 2, 20);
            this.Controls.Add(lblTitle);

            Panel pnlScreen = new Panel();
            pnlScreen.Size = new Size(600, 60);
            pnlScreen.Location = new Point((this.Width - 600) / 2, 100);
            pnlScreen.Paint += PnlScreen_Paint;
            this.Controls.Add(pnlScreen);

            lblScreen = new Label();
            lblScreen.Text = "SCREEN";
            lblScreen.ForeColor = Color.FromArgb(165, 180, 252);
            lblScreen.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblScreen.AutoSize = true;
            lblScreen.Location = new Point((this.Width - 50) / 2, 80);
            this.Controls.Add(lblScreen);

            gridSeats = new FlowLayoutPanel();
            gridSeats.Size = new Size(cols * 60 + 20, rows * 60 + 20);
            gridSeats.Location = new Point((this.Width - gridSeats.Width) / 2, 200);
            this.Controls.Add(gridSeats);

            GenerateSeats();
            CreateLegend((this.Width - 400) / 2, 500);

            ModernButton btnBack = new ModernButton();
            btnBack.Text = "Back";
            btnBack.ForeColor = Color.Black; 
            btnBack.IsSelected = false;
            btnBack.Location = new Point(50, 700);
            btnBack.Click += (s, e) => this.Close();
            this.Controls.Add(btnBack);
        }

        private void PnlScreen_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                new Rectangle(0, 0, 600, 60), 
                Color.FromArgb(50, 99, 102, 241), 
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
        }

        private void GenerateSeats()
        {
            char[] rowLabels = { 'A', 'B', 'C', 'D' };
            for (int r = 0; r < rows; r++)
            {
                for (int c = 1; c <= cols; c++)
                {
                    string seatName = $"{rowLabels[r]}{c}";
                    Button btnSeat = new Button();
                    btnSeat.Text = ""; 
                    btnSeat.Tag = seatName;
                    btnSeat.Size = new Size(50, 50);
                    btnSeat.Margin = new Padding(5);
                    btnSeat.FlatStyle = FlatStyle.Flat;
                    btnSeat.FlatAppearance.BorderSize = 0;
                    btnSeat.Paint += BtnSeat_Paint; 
                    btnSeat.Click += BtnSeat_Click;
                    gridSeats.Controls.Add(btnSeat);
                }
            }
        }

        private void BtnSeat_Paint(object? sender, PaintEventArgs e)
{
    if (sender is Button btn)
    {
        string seatName = btn.Tag?.ToString() ?? "";
        
        Color seatColor = AppColors.SeatAvailable;
        Color borderColor = Color.FromArgb(203, 213, 225); // Màu viền xám

        if (myBookedSeats.Contains(seatName)) 
        {
            seatColor = AppColors.SeatMyBooked;
            borderColor = AppColors.SeatMyBooked; // Ghế mình mua thì viền cùng màu
        }
        else if (seatName == currentSelectedSeat) 
        {
            seatColor = AppColors.SeatSelected;
            borderColor = AppColors.SeatSelected;
        }
        
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // 1. Vẽ nền ghế (Fill)
        using (SolidBrush brush = new SolidBrush(seatColor))
        {
            e.Graphics.FillPath(brush, GetRoundedRect(new Rectangle(5, 5, 40, 30), 8)); // Lưng ghế
            e.Graphics.FillRectangle(brush, new Rectangle(5, 30, 40, 10)); // Đệm ngồi
            e.Graphics.FillRectangle(brush, new Rectangle(2, 25, 6, 20));  // Tay vịn trái
            e.Graphics.FillRectangle(brush, new Rectangle(42, 25, 6, 20)); // Tay vịn phải
        }

        // 2. THÊM MỚI: Vẽ viền ghế (Draw - giúp ghế nổi bật trên nền trắng)
        if (seatColor == AppColors.SeatAvailable) // Chỉ vẽ viền nếu là ghế trống
        {
            using (Pen pen = new Pen(borderColor, 2))
            {
                e.Graphics.DrawPath(pen, GetRoundedRect(new Rectangle(5, 5, 40, 30), 8));
                e.Graphics.DrawRectangle(pen, new Rectangle(5, 30, 40, 10));
                e.Graphics.DrawRectangle(pen, new Rectangle(2, 25, 6, 20));
                e.Graphics.DrawRectangle(pen, new Rectangle(42, 25, 6, 20));
            }
        }

        // 3. Hiệu ứng chọn (giữ nguyên code cũ)
        if (seatName == currentSelectedSeat)
        {
            using (Pen p = new Pen(Color.FromArgb(253, 230, 138), 2))
            {
                e.Graphics.DrawRectangle(p, 1, 1, 48, 48);
            }
        }
    }
}

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);
            path.CloseFigure();
            return path;
        }

        private void BtnSeat_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                string seatName = btn.Tag?.ToString() ?? "";
                if (myBookedSeats.Contains(seatName)) return;

                currentSelectedSeat = seatName;
                foreach (Control c in gridSeats.Controls) c.Invalidate();

                DialogResult result = MessageBox.Show($"Are you sure you want to book seat #{seatName}?", 
                    "Confirm Selection", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (result == DialogResult.OK)
                {
                    myBookedSeats.Add(seatName);
                    currentSelectedSeat = null;
                }
                else
                {
                    currentSelectedSeat = null;
                }
                foreach (Control c in gridSeats.Controls) c.Invalidate();
            }
        }

        private void CreateLegend(int x, int y)
        {
            Panel pnlLegend = new Panel();
            pnlLegend.Location = new Point(x, y);
            pnlLegend.Size = new Size(400, 50);
            
            AddLegendItem(pnlLegend, "Available", AppColors.SeatAvailable, 0);
            AddLegendItem(pnlLegend, "My Seats", AppColors.SeatMyBooked, 120);
            AddLegendItem(pnlLegend, "Booked", AppColors.SeatBooked, 240);

            this.Controls.Add(pnlLegend);
        }

        private void AddLegendItem(Panel parent, string text, Color color, int x)
        {
            Panel p = new Panel();
            p.Size = new Size(20, 20);
            p.Location = new Point(x, 15);
            p.BackColor = color;
            p.BorderStyle = BorderStyle.FixedSingle;
            
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x + 25, 17);
            l.AutoSize = true;
            l.ForeColor = Color.Gray;

            parent.Controls.Add(p);
            parent.Controls.Add(l);
        }
    }
}