using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaTicketBooking
{
    // Bảng màu giao diện
    public static class AppColors
    {
        public static Color BackgroundStart = Color.FromArgb(20, 20, 30);
        public static Color BackgroundEnd = Color.FromArgb(40, 20, 40);
        public static Color CardBg = Color.FromArgb(100, 30, 30, 40);
        public static Color TextPrimary = Color.White;
        public static Color TextSecondary = Color.FromArgb(156, 163, 175);
        public static Color AccentPurple = Color.FromArgb(147, 51, 234);
        public static Color AccentPink = Color.FromArgb(219, 39, 119);
        public static Color SeatAvailable = Color.FromArgb(226, 232, 240);
        public static Color SeatBooked = Color.FromArgb(203, 213, 225);
        public static Color SeatSelected = Color.FromArgb(251, 191, 36);
        public static Color SeatMyBooked = Color.FromArgb(16, 185, 129);
    }

    // Panel bo tròn (Dùng làm Card phim)
    public class RoundedPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 20;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(50, 255, 255, 255);

        public RoundedPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Xử lý vẽ hình bo tròn
            Rectangle rect = ClientRectangle;
            rect.Width--; rect.Height--;
            using (GraphicsPath path = GetRoundedPath(rect, BorderRadius))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(180, 30, 30, 40)))
            using (Pen pen = new Pen(BorderColor, 1))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2.0F;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // Nút bấm hiện đại
    public class ModernButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 15;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected { get; set; } = false;

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(100, 40);
            this.BackColor = Color.Transparent;
            this.ForeColor = Color.White;
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            GraphicsPath path = new GraphicsPath();
            int d = BorderRadius;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            Color fillColor = IsSelected ? AppColors.AccentPurple : Color.FromArgb(50, 255, 255, 255);
            Color borderColor = IsSelected ? AppColors.AccentPink : Color.FromArgb(100, 255, 255, 255);
            Color textColor = IsSelected ? Color.White : Color.Gainsboro;

            using (SolidBrush brush = new SolidBrush(fillColor))
                pevent.Graphics.FillPath(brush, path);

            using (Pen pen = new Pen(borderColor, 1))
                pevent.Graphics.DrawPath(pen, path);

            TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, ClientRectangle, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}