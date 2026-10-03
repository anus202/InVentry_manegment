using System.Drawing.Drawing2D;

namespace Prectice_Interview.UI
{
    public class NavButton : Control
    {
        private bool _hover;
        public bool Active { get; set; }
        public string Glyph { get; set; } = "•";

        public NavButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Height = 38;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Theme.Sidebar);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(10, 2, Width - 20, Height - 4);
            if (Active || _hover)
            {
                using var path = Theme.RoundRect(r, 10);
                using var b = new SolidBrush(Active ? Theme.Primary : Theme.SidebarHover);
                g.FillPath(b, path);
            }
            var fg = Active ? Color.White : Color.FromArgb(203, 213, 225);
            TextRenderer.DrawText(g, Glyph, new Font("Segoe UI Emoji", 12f), new Rectangle(22, 0, 32, Height), fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, Text, new Font("Segoe UI Semibold", 10f), new Rectangle(58, 0, Width - 60, Height), fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
