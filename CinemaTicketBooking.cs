using System;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaTicketBooking
{
    public partial class Form1 : Form
    {
        private const int ROWS = 7;
        private const int COLS = 10;
        private Button[,] seats;
        private Label lblMovieInfo;
        private Label lblTotalPrice;
        private Button btnConfirm;
        private const int SEAT_PRICE = 50000;

        // Vibrant color palette
        private Color colorBackground = Color.FromArgb(20, 20, 35);
        private Color colorAvailable = Color.FromArgb(100, 149, 237); // Cornflower Blue
        private Color colorAvailableHover = Color.FromArgb(135, 206, 250); // Light Sky Blue
        private Color colorSelected = Color.FromArgb(255, 69, 150); // Hot Pink
        private Color colorSelectedHover = Color.FromArgb(255, 105, 180); // Bright Pink
        private Color colorSold = Color.FromArgb(253, 253, 0); // Yellow
        private Color colorAccent = Color.FromArgb(255, 215, 0); // Gold
        private Color colorSuccess = Color.FromArgb(50, 205, 50); // Lime Green

        public Form1()
        {
            InitializeComponent();
            InitializeSeats();
        }

        private void InitializeComponent()
        {
            this.Text = "Đặt Vé Xem Phim";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = colorBackground;

            // Thông tin phim
            lblMovieInfo = new Label
            {
                Text = "🎬 AVENGERS: ENDGAME\n📅 Ngày: 06/12/2025  |  ⏰ Giờ: 19:30  |  🏛️ Rạp: CGV Vincom",
                Location = new Point(20, 20),
                Size = new Size(840, 60),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = colorAccent,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(30, 30, 50),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(lblMovieInfo);

            // Màn hình
            Label lblScreen = new Label
            {
                Text = "━━━━━ MÀN HÌNH ━━━━━",
                Location = new Point(250, 90),
                Size = new Size(380, 35),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = colorAccent,
                BackColor = Color.FromArgb(40, 40, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblScreen);

            // Chú thích
            CreateLegend();

            // Tổng tiền
            lblTotalPrice = new Label
            {
                Text = "Tổng tiền: 0 VNĐ",
                Location = new Point(20, 600),
                Size = new Size(400, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = colorSuccess,
                BackColor = Color.FromArgb(30, 30, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(lblTotalPrice);

            // Nút xác nhận
            btnConfirm = new Button
            {
                Text = "XÁC NHẬN ĐẶT VÉ ✓",
                Location = new Point(680, 600),
                Size = new Size(180, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = colorSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;
            btnConfirm.MouseEnter += (s, e) => btnConfirm.BackColor = Color.FromArgb(60, 220, 60);
            btnConfirm.MouseLeave += (s, e) => btnConfirm.BackColor = colorSuccess;
            this.Controls.Add(btnConfirm);
        }

        private void CreateLegend()
        {
            int startX = 200;
            int startY = 550;

            // Ghế trống
            Button btnAvailable = new Button
            {
                Location = new Point(startX, startY),
                Size = new Size(40, 40),
                BackColor = colorAvailable,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnAvailable.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnAvailable);

            Label lblAvailable = new Label
            {
                Text = "Trống",
                Location = new Point(startX + 50, startY + 10),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblAvailable);

            // Ghế đã chọn
            Button btnSelected = new Button
            {
                Location = new Point(startX + 180, startY),
                Size = new Size(40, 40),
                BackColor = colorSelected,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnSelected.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnSelected);

            Label lblSelected = new Label
            {
                Text = "Đang chọn",
                Location = new Point(startX + 230, startY + 10),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblSelected);

            // Ghế đã bán
            Button btnSold = new Button
            {
                Location = new Point(startX + 380, startY),
                Size = new Size(40, 40),
                BackColor = colorSold,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnSold.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnSold);

            Label lblSold = new Label
            {
                Text = "Đã bán",
                Location = new Point(startX + 430, startY + 10),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblSold);
        }

        private void InitializeSeats()
        {
            seats = new Button[ROWS, COLS];
            int startX = 150;
            int startY = 160;
            int seatSize = 45;
            int spacing = 10;

            // Tạo một số ghế đã bán ngẫu nhiên
            Random rand = new Random();

            for (int i = 0; i < ROWS; i++)
            {
                // Label hàng ghế
                Label lblRow = new Label
                {
                    Text = ((char)('A' + i)).ToString(),
                    Location = new Point(startX - 40, startY + i * (seatSize + spacing) + 10),
                    Size = new Size(30, 30),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = colorAccent,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(lblRow);

                for (int j = 0; j < COLS; j++)
                {
                    // Label số ghế (chỉ hiện ở hàng đầu)
                    if (i == 0)
                    {
                        Label lblCol = new Label
                        {
                            Text = (j + 1).ToString(),
                            Location = new Point(startX + j * (seatSize + spacing) + 10, startY - 30),
                            Size = new Size(30, 20),
                            Font = new Font("Segoe UI", 10, FontStyle.Bold),
                            ForeColor = colorAccent,
                            TextAlign = ContentAlignment.MiddleCenter
                        };
                        this.Controls.Add(lblCol);
                    }

                    Button seat = new Button
                    {
                        Location = new Point(startX + j * (seatSize + spacing), startY + i * (seatSize + spacing)),
                        Size = new Size(seatSize, seatSize),
                        Tag = $"{(char)('A' + i)}{j + 1}",
                        Font = new Font("Segoe UI", 8, FontStyle.Bold),
                        BackColor = colorAvailable,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };

                    seat.FlatAppearance.BorderSize = 0;
                    seat.FlatAppearance.MouseOverBackColor = colorAvailableHover;

                    // Đặt một số ghế đã bán ngẫu nhiên
                    if (rand.Next(0, 10) < 2)
                    {
                        seat.BackColor = colorSold;
                        seat.Enabled = false;
                        seat.Cursor = Cursors.No;
                    }
                    else
                    {
                        seat.Click += Seat_Click;
                        seat.MouseEnter += Seat_MouseEnter;
                        seat.MouseLeave += Seat_MouseLeave;
                    }

                    seats[i, j] = seat;
                    this.Controls.Add(seat);
                }
            }
        }

        private void Seat_MouseEnter(object sender, EventArgs e)
        {
            Button seat = (Button)sender;
            
            if (seat.BackColor == colorAvailable)
            {
                seat.BackColor = colorAvailableHover;
            }
            else if (seat.BackColor == colorSelected)
            {
                seat.BackColor = colorSelectedHover;
            }
        }

        private void Seat_MouseLeave(object sender, EventArgs e)
        {
            Button seat = (Button)sender;
            
            if (seat.BackColor == colorAvailableHover)
            {
                seat.BackColor = colorAvailable;
            }
            else if (seat.BackColor == colorSelectedHover)
            {
                seat.BackColor = colorSelected;
            }
        }

        private void Seat_Click(object sender, EventArgs e)
        {
            Button seat = (Button)sender;

            if (seat.BackColor == colorAvailable || seat.BackColor == colorAvailableHover)
            {
                // Chọn ghế
                seat.BackColor = colorSelected;
            }
            else if (seat.BackColor == colorSelected || seat.BackColor == colorSelectedHover)
            {
                // Bỏ chọn ghế
                seat.BackColor = colorAvailable;
            }

            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            int count = 0;
            foreach (Button seat in seats)
            {
                if (seat.BackColor == colorSelected || seat.BackColor == colorSelectedHover)
                {
                    count++;
                }
            }

            int total = count * SEAT_PRICE;
            lblTotalPrice.Text = $"Tổng tiền: {total:N0} VNĐ ({count} ghế)";
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            string selectedSeats = "";
            int count = 0;

            foreach (Button seat in seats)
            {
                if (seat.BackColor == colorSelected || seat.BackColor == colorSelectedHover)
                {
                    selectedSeats += seat.Tag.ToString() + ", ";
                    count++;
                }
            }

            if (count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            selectedSeats = selectedSeats.TrimEnd(',', ' ');
            int total = count * SEAT_PRICE;

            DialogResult result = MessageBox.Show(
                $"Xác nhận đặt vé?\n\n" +
                $"Ghế: {selectedSeats}\n" +
                $"Số lượng: {count} ghế\n" +
                $"Tổng tiền: {total:N0} VNĐ",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show(
                    "Đặt vé thành công!\n\n" +
                    "Vui lòng thanh toán tại quầy trước giờ chiếu 15 phút.",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Reset form
                ResetSeats();
            }
        }

        private void ResetSeats()
        {
            foreach (Button seat in seats)
            {
                if (seat.BackColor == colorSelected || seat.BackColor == colorSelectedHover)
                {
                    seat.BackColor = colorAvailable;
                }
            }
            UpdateTotalPrice();
        }
    }
}