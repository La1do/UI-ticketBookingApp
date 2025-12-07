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
            SetupUI();
            LoadMovies();
        }

        private void SetupUI()
        {
            this.Text = "Choose Your Movie";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            Label lblHeader = new Label();
            lblHeader.Text = "Choose Your Movie";
            lblHeader.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblHeader.ForeColor = Color.White;
            lblHeader.AutoSize = true;
            lblHeader.BackColor = Color.Transparent;
            lblHeader.Location = new Point((this.Width - 280) / 2, 30);
            this.Controls.Add(lblHeader);

            flowMovies = new FlowLayoutPanel();
            flowMovies.Location = new Point(50, 100);
            flowMovies.Size = new Size(1100, 550);
            flowMovies.AutoScroll = true;
            flowMovies.BackColor = Color.Transparent;
            this.Controls.Add(flowMovies);

            btnConfirm = new ModernButton();
            btnConfirm.Text = "Confirm Selection";
            btnConfirm.Size = new Size(200, 50);
            btnConfirm.Location = new Point((this.Width - 200) / 2, 700);
            btnConfirm.Enabled = false;
            btnConfirm.Click += BtnConfirm_Click;
            this.Controls.Add(btnConfirm);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, 
                AppColors.BackgroundStart, AppColors.BackgroundEnd, 45F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }

        private void LoadMovies()
        {
            foreach (var movie in movies)
            {
                RoundedPanel card = new RoundedPanel();
                card.Size = new Size(250, 450);
                card.Margin = new Padding(10);

                PictureBox pbPoster = new PictureBox();
                pbPoster.Size = new Size(250, 300);
                pbPoster.Location = new Point(0, 0);
                pbPoster.SizeMode = PictureBoxSizeMode.Zoom;
                try { pbPoster.LoadAsync(movie.ImageUrl); } catch { pbPoster.BackColor = Color.Gray; }
                
                Label lblRating = new Label();
                lblRating.Text = "★ " + movie.Rating;
                lblRating.BackColor = Color.FromArgb(180, 0, 0, 0);
                lblRating.ForeColor = Color.Yellow;
                lblRating.AutoSize = true;
                lblRating.Location = new Point(190, 10);
                pbPoster.Controls.Add(lblRating);

                Label lblTitle = new Label();
                lblTitle.Text = movie.Title;
                lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                lblTitle.ForeColor = AppColors.TextPrimary;
                lblTitle.Location = new Point(10, 310);
                lblTitle.Size = new Size(230, 25);

                Label lblInfo = new Label();
                lblInfo.Text = $"{movie.Genre} • {movie.Duration}";
                lblInfo.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                lblInfo.ForeColor = AppColors.TextSecondary;
                lblInfo.Location = new Point(10, 335);
                lblInfo.Size = new Size(230, 20);

                FlowLayoutPanel flowTimes = new FlowLayoutPanel();
                flowTimes.Location = new Point(10, 360);
                flowTimes.Size = new Size(230, 80);

                foreach (var time in movie.Showtimes)
                {
                    ModernButton btnTime = new ModernButton();
                    btnTime.Text = time;
                    btnTime.Size = new Size(60, 30);
                    btnTime.Font = new Font("Segoe UI", 8);
                    btnTime.BorderRadius = 8;
                    btnTime.Click += (s, e) => HandleTimeSelect(movie.Id, time);
                    btnTime.Tag = new { MovieId = movie.Id, Time = time };
                    flowTimes.Controls.Add(btnTime);
                }

                card.Controls.Add(pbPoster);
                card.Controls.Add(lblTitle);
                card.Controls.Add(lblInfo);
                card.Controls.Add(flowTimes);

                flowMovies.Controls.Add(card);
            }
        }

        private void HandleTimeSelect(int movieId, string time)
        {
            selectedMovieId = movieId;
            selectedTime = time;

            foreach (Control card in flowMovies.Controls)
            {
                foreach (Control c in card.Controls)
                {
                    if (c is FlowLayoutPanel flowTimes)
                    {
                        foreach (ModernButton btn in flowTimes.Controls)
                        {
                            dynamic? tag = btn.Tag;
                            if (tag != null && tag.MovieId == movieId && tag.Time == time)
                                btn.IsSelected = true;
                            else
                                btn.IsSelected = false;
                            btn.Invalidate();
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
                SeatSelectionForm seatForm = new SeatSelectionForm(selectedMovieId, selectedTime);
                this.Hide();
                seatForm.ShowDialog();
                this.Show();
            }
        }
    }
}