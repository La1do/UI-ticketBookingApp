using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaTicketBooking
{
    // Bảng màu giao diện hiện đại
    public static class AppColors
    {
        // Background colors
        public static Color BackgroundStart = Color.FromArgb(15, 23, 42);      // Slate-900
        public static Color BackgroundEnd = Color.FromArgb(30, 41, 59);        // Slate-800
        
        // Card colors
        public static Color CardBg = Color.FromArgb(30, 41, 59);               // Slate-800
        public static Color CardHover = Color.FromArgb(51, 65, 85);            // Slate-700
        
        // Text colors
        public static Color TextPrimary = Color.White;
        public static Color TextSecondary = Color.FromArgb(148, 163, 184);     // Slate-400
        
        // Accent colors
        public static Color AccentPurple = Color.FromArgb(139, 92, 246);       // Violet-500
        public static Color AccentPink = Color.FromArgb(236, 72, 153);         // Pink-500
        public static Color AccentGold = Color.FromArgb(251, 191, 36);         // Amber-400
        
        // Seat colors
        public static Color SeatAvailable = Color.FromArgb(226, 232, 240);
        public static Color SeatBooked = Color.FromArgb(203, 213, 225);
        public static Color SeatSelected = Color.FromArgb(251, 191, 36);
        public static Color SeatMyBooked = Color.FromArgb(16, 185, 129);
        
        // Button states
        public static Color ButtonNormal = Color.FromArgb(51, 65, 85);         // Slate-700
        public static Color ButtonHover = Color.FromArgb(71, 85, 105);         // Slate-600
        public static Color ButtonSelected = Color.FromArgb(139, 92, 246);     // Violet-500
    }

    // Panel bo tròn với shadow effect
    public class RoundedPanel : Panel
    {
        private bool isHovered = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 20;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(30, 255, 255, 255);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EnableHoverEffect { get; set; } = true;

        public RoundedPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (EnableHoverEffect)
            {
                isHovered = true;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (EnableHoverEffect)
            {
                isHovered = false;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            
            using (GraphicsPath path = GetRoundedPath(rect, BorderRadius))
            {
                // Shadow effect
                if (EnableHoverEffect)
                {
                    Rectangle shadowRect = rect;
                    shadowRect.Inflate(2, 2);
                    using (GraphicsPath shadowPath = GetRoundedPath(shadowRect, BorderRadius))
                    using (PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath))
                    {
                        shadowBrush.CenterColor = Color.FromArgb(isHovered ? 40 : 20, 0, 0, 0);
                        shadowBrush.SurroundColors = new[] { Color.Transparent };
                        e.Graphics.FillPath(shadowBrush, shadowPath);
                    }
                }

                // Background
                Color bgColor = isHovered && EnableHoverEffect 
                    ? AppColors.CardHover 
                    : this.BackColor == Color.Transparent 
                        ? AppColors.CardBg 
                        : this.BackColor;

                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Border
                using (Pen pen = new Pen(BorderColor, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
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

    // Nút bấm hiện đại với hover effect
    public class ModernButton : Button
    {
        private bool isHovered = false;
        private System.Windows.Forms.Timer animationTimer;
        private float hoverProgress = 0f;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 12;

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
            this.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            // Animation timer
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16; // ~60 FPS
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (isHovered && hoverProgress < 1f)
            {
                hoverProgress += 0.1f;
                if (hoverProgress >= 1f) 
                {
                    hoverProgress = 1f;
                    animationTimer.Stop();
                }
                this.Invalidate();
            }
            else if (!isHovered && hoverProgress > 0f)
            {
                hoverProgress -= 0.1f;
                if (hoverProgress <= 0f) 
                {
                    hoverProgress = 0f;
                    animationTimer.Stop();
                }
                this.Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            animationTimer.Start();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            animationTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = GetRoundedPath(rect, BorderRadius))
            {
                // Determine colors based on state
                Color fillColor, borderColor, textColor;

                if (IsSelected)
                {
                    fillColor = AppColors.ButtonSelected;
                    borderColor = AppColors.AccentPink;
                    textColor = Color.White;
                }
                else if (!this.Enabled)
                {
                    fillColor = Color.FromArgb(30, 41, 59);
                    borderColor = Color.FromArgb(51, 65, 85);
                    textColor = Color.FromArgb(100, 116, 139);
                }
                else
                {
                    // Interpolate between normal and hover colors
                    fillColor = InterpolateColor(
                        AppColors.ButtonNormal, 
                        AppColors.ButtonHover, 
                        hoverProgress);
                    borderColor = InterpolateColor(
                        Color.FromArgb(71, 85, 105), 
                        AppColors.AccentPurple, 
                        hoverProgress);
                    textColor = InterpolateColor(
                        Color.FromArgb(203, 213, 225), 
                        Color.White, 
                        hoverProgress);
                }

                // Fill
                using (SolidBrush brush = new SolidBrush(fillColor))
                {
                    pevent.Graphics.FillPath(brush, path);
                }

                // Border
                using (Pen pen = new Pen(borderColor, 2))
                {
                    pevent.Graphics.DrawPath(pen, path);
                }

                // Text
                TextRenderer.DrawText(
                    pevent.Graphics, 
                    this.Text, 
                    this.Font, 
                    ClientRectangle, 
                    textColor, 
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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

        private Color InterpolateColor(Color c1, Color c2, float progress)
        {
            int r = (int)(c1.R + (c2.R - c1.R) * progress);
            int g = (int)(c1.G + (c2.G - c1.G) * progress);
            int b = (int)(c1.B + (c2.B - c1.B) * progress);
            return Color.FromArgb(r, g, b);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}