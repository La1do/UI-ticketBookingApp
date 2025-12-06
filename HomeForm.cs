using System;
using System.Windows.Forms;

namespace MovieSeatSelection
{
    public class HomeForm : Form
    {
        private Button btnAdminDashboard;
        private Button btnCinemaBooking;

        public HomeForm()
        {
            this.Text = "Home Page";
            this.Size = new System.Drawing.Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += HomeForm_FormClosed;

            btnAdminDashboard = new Button
            {
                Text = "Go to Admin Dashboard",
                Size = new System.Drawing.Size(250, 40),
                Location = new System.Drawing.Point(70, 40)
            };
            btnAdminDashboard.Click += BtnAdminDashboard_Click;
            Controls.Add(btnAdminDashboard);

            btnCinemaBooking = new Button
            {
                Text = "Go to Cinema Ticket Booking",
                Size = new System.Drawing.Size(250, 40),
                Location = new System.Drawing.Point(70, 110)
            };
            btnCinemaBooking.Click += BtnCinemaBooking_Click;
            Controls.Add(btnCinemaBooking);
        }
        private void HomeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        private void BtnAdminDashboard_Click(object sender, EventArgs e)
        {
            BullyAlgorithmDemo.AdminDashboard form = new BullyAlgorithmDemo.AdminDashboard();
            form.Show();
            form.FormClosed += (s, args) => this.Show();
            this.Hide();
        }

        private void BtnCinemaBooking_Click(object sender, EventArgs e)
        {
            CinemaTicketBooking.Form1 form = new CinemaTicketBooking.Form1();
            form.Show();
            form.FormClosed += (s, args) => this.Show();
            this.Hide();
        }
    }
}
