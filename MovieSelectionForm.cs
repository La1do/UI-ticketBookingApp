using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaTicketBooking
{
    public class MovieData
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string[] Showtimes { get; set; } = Array.Empty<string>();
    }

    public class MovieSelectionForm : Form
    {
        private FlowLayoutPanel flowMovies = null!;
        private ModernButton btnConfirm = null!;
        private Panel scrollContainer = null!;
        private int selectedMovieId = -1;
        private string? selectedTime = null;

        private List<MovieData> movies = new List<MovieData>
        {
            new MovieData { Id = 4, Title = "Avengers: Endgame", ImageUrl = "https://image.tmdb.org/t/p/w500/or06FN3Dka5tukK1e9sl16pB3iy.jpg", Rating = 8.4, Genre = "Action", Duration = "3h 1m", Showtimes = new[] { "09:15", "10:30", "13:00", "17:30", "21:00" } },
            new MovieData { Id = 5, Title = "Avatar: Way of Water", ImageUrl = "https://image.tmdb.org/t/p/w500/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg", Rating = 7.6, Genre = "Sci-Fi", Duration = "3h 12m", Showtimes = new[] { "08:00", "15:00", "22:00" } },
            new MovieData { Id = 10, Title = "Top Gun: Maverick", ImageUrl = "https://image.tmdb.org/t/p/w500/62HCnUTziyWcpDaBO2i1DX17ljH.jpg", Rating = 8.2, Genre = "Action", Duration = "2h 10m", Showtimes = new[] { "10:30", "18:00", "20:30" } },
            new MovieData { Id = 18, Title = "Coco", ImageUrl = "https://image.tmdb.org/t/p/w500/gGEsBPAijhVUFoiNpgZXqRVWJt2.jpg", Rating = 8.2, Genre = "Animation", Duration = "1h 45m", Showtimes = new[] { "09:00", "13:00", "17:00" } },
        };

        public MovieSelectionForm()
        {
            InitializeComponent();
            SetupUI();
            LoadMovies();
            this.Resize += MovieSelectionForm_Resize;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // Form settings
            this.Text = "Cinema Ticket Booking";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.BackColor = AppColors.BackgroundStart;
            this.MinimumSize = new Size(1000, 600);

            // Header Panel
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 100;
            headerPanel.BackColor = Color.Transparent;
            this.Controls.Add(headerPanel);

            Label lblHeader = new Label();
            lblHeader.Text = "🎬 Choose Your Movie";
            lblHeader.Font = new Font("Segoe UI", 26, FontStyle.Bold);
            lblHeader.ForeColor = Color.White;
            lblHeader.AutoSize = true;
            lblHeader.BackColor = Color.Transparent;
            lblHeader.Location = new Point(40, 30);
            headerPanel.Controls.Add(lblHeader);

            // Footer Panel (đặt trước để đúng thứ tự z-index)
            Panel footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Height = 90;
            footerPanel.BackColor = AppColors.CardBg;
            this.Controls.Add(footerPanel);

            btnConfirm = new ModernButton();
            btnConfirm.Text = "Confirm Selection →";
            btnConfirm.Size = new Size(220, 55);
            btnConfirm.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnConfirm.Enabled = false;
            btnConfirm.BorderRadius = 12;
            btnConfirm.Click += BtnConfirm_Click;
            btnConfirm.Anchor = AnchorStyles.None;
            btnConfirm.Location = new Point((footerPanel.Width - 220) / 2, 18);
            footerPanel.Controls.Add(btnConfirm);

            footerPanel.Resize += (s, e) => {
                btnConfirm.Location = new Point((footerPanel.Width - 220) / 2, 18);
            };

            // Movies scroll container
            scrollContainer = new Panel();
            scrollContainer.Dock = DockStyle.Fill;
            scrollContainer.BackColor = Color.Transparent;
            scrollContainer.AutoScroll = true;
            this.Controls.Add(scrollContainer);

            flowMovies = new FlowLayoutPanel();
            flowMovies.Dock = DockStyle.Fill;
            flowMovies.Padding = new Padding(30, 25, 30, 15);
            flowMovies.BackColor = Color.Transparent;
            flowMovies.WrapContents = true;
            flowMovies.AutoScroll = false;
            scrollContainer.Controls.Add(flowMovies);
        }

        private void MovieSelectionForm_Resize(object? sender, EventArgs e)
        {
            // Force refresh layout khi resize
            if (flowMovies != null && flowMovies.Controls.Count > 0)
            {
                flowMovies.SuspendLayout();
                flowMovies.ResumeLayout(true);
            }
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

        private class TimeSlotTag
        {
            public int MovieId { get; set; }
            public string Time { get; set; } = string.Empty;
        }

        private void LoadMovies()
        {
            flowMovies.SuspendLayout();

            foreach (var movie in movies)
            {
                // Movie Card - COMPACT SIZE
                RoundedPanel card = new RoundedPanel();
                card.Size = new Size(300, 650);
                card.Margin = new Padding(8);
                card.BackColor = AppColors.CardBg;
                card.BorderRadius = 14;
                card.BorderColor = Color.FromArgb(40, 255, 255, 255);
                card.EnableHoverEffect = true;

                // === POSTER IMAGE ===
                Panel posterContainer = new Panel();
                posterContainer.Size = new Size(300, 450);
                posterContainer.Location = new Point(0, 0);
                posterContainer.BackColor = Color.FromArgb(51, 65, 85);

                PictureBox pbPoster = new PictureBox();
                pbPoster.Size = new Size(300, 450);
                pbPoster.Location = new Point(0, 0);
                pbPoster.SizeMode = PictureBoxSizeMode.StretchImage;
                pbPoster.BackColor = Color.Transparent;
                
                try 
                { 
                    pbPoster.LoadAsync(movie.ImageUrl); 
                } 
                catch 
                { 
                    pbPoster.BackColor = Color.FromArgb(51, 65, 85);
                }
                posterContainer.Controls.Add(pbPoster);

                // Rating Badge
                RoundedPanel ratingBadge = new RoundedPanel();
                ratingBadge.Size = new Size(70, 32);
                ratingBadge.Location = new Point(180, 12);
                ratingBadge.BackColor = Color.FromArgb(220, 0, 0, 0);
                ratingBadge.BorderRadius = 8;
                ratingBadge.BorderColor = Color.FromArgb(100, 251, 191, 36);
                ratingBadge.EnableHoverEffect = false;
                
                Label lblRating = new Label();
                lblRating.Text = "⭐ " + movie.Rating.ToString("0.0");
                lblRating.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                lblRating.ForeColor = AppColors.AccentGold;
                lblRating.AutoSize = true;
                lblRating.BackColor = Color.Transparent;
                lblRating.Location = new Point(8, 7);
                ratingBadge.Controls.Add(lblRating);
                posterContainer.Controls.Add(ratingBadge);

                card.Controls.Add(posterContainer);

                // === MOVIE INFO SECTION - SAT POSTER ===
                int yPos = 458; // Sát poster hơn

                // Movie Title
                Label lblTitle = new Label();
                lblTitle.Text = movie.Title;
                lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                lblTitle.ForeColor = Color.White;
                lblTitle.Location = new Point(10, yPos);
                lblTitle.Size = new Size(240, 42);
                lblTitle.BackColor = Color.Transparent;
                lblTitle.TextAlign = ContentAlignment.TopLeft;
                card.Controls.Add(lblTitle);
                yPos += 44;

                // Genre & Duration
                Label lblInfo = new Label();
                lblInfo.Text = $"🎭 {movie.Genre}  •  ⏱ {movie.Duration}";
                lblInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
                lblInfo.ForeColor = AppColors.TextSecondary;
                lblInfo.Location = new Point(10, yPos);
                lblInfo.Size = new Size(240, 18);
                lblInfo.BackColor = Color.Transparent;
                card.Controls.Add(lblInfo);
                yPos += 22;

                // Showtimes Label
                Label lblShowtimes = new Label();
                lblShowtimes.Text = "🕐 Showtimes:";
                lblShowtimes.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                lblShowtimes.ForeColor = AppColors.TextSecondary;
                lblShowtimes.Location = new Point(10, yPos);
                lblShowtimes.AutoSize = true;
                lblShowtimes.BackColor = Color.Transparent;
                card.Controls.Add(lblShowtimes);
                yPos += 20;

                // === SHOWTIMES BUTTONS ===
                FlowLayoutPanel flowTimes = new FlowLayoutPanel();
                flowTimes.Location = new Point(10, yPos);
                flowTimes.Size = new Size(240, 60);
                flowTimes.WrapContents = true;
                flowTimes.BackColor = Color.Transparent;

                foreach (var time in movie.Showtimes)
                {
                    ModernButton btnTime = new ModernButton();
                    btnTime.Text = time;
                    btnTime.Size = new Size(70, 32);
                    btnTime.Margin = new Padding(2);
                    btnTime.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    btnTime.BorderRadius = 8;
                    btnTime.Click += (s, e) => HandleTimeSelect(movie.Id, time);
                    btnTime.Tag = new TimeSlotTag { MovieId = movie.Id, Time = time };
                    flowTimes.Controls.Add(btnTime);
                }

                card.Controls.Add(flowTimes);
                flowMovies.Controls.Add(card);
            }

            flowMovies.ResumeLayout();
        }

        private void HandleTimeSelect(int movieId, string time)
        {
            selectedMovieId = movieId;
            selectedTime = time;

            // Update all time buttons
            foreach (Control card in flowMovies.Controls)
            {
                if (card is RoundedPanel)
                {
                    foreach (Control c in card.Controls)
                    {
                        if (c is FlowLayoutPanel flowTimes)
                        {
                            foreach (Control btn in flowTimes.Controls)
                            {
                                if (btn is ModernButton modernBtn && modernBtn.Tag is TimeSlotTag tag)
                                {
                                    modernBtn.IsSelected = (tag.MovieId == movieId && tag.Time == time);
                                    modernBtn.Invalidate();
                                }
                            }
                        }
                    }
                }
            }

            btnConfirm.Enabled = true;
            btnConfirm.IsSelected = true;
            btnConfirm.Invalidate();
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (selectedMovieId != -1 && selectedTime != null)
            {
                var selectedMovie = movies.Find(m => m.Id == selectedMovieId);
                
                try
                {
                    // Chuyển sang form chọn ghế
                    SeatSelectionForm seatForm = new SeatSelectionForm(selectedMovieId, selectedTime);
                    this.Hide();
                    seatForm.ShowDialog();
                    this.Show(); // Show lại form thay vì Close
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Cannot open seat selection form.\nError: {ex.Message}", 
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    this.Show();
                }
            }
            else
            {
                MessageBox.Show(
                    "Please select a movie and showtime first!", 
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
    }
}