using System.Drawing.Drawing2D;

namespace Prectice_Interview.UI
{
    public class Card : Panel
    {
        public int Radius { get; set; } = 12;
        public Card()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Padding = new Padding(16);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = Theme.RoundRect(new RectangleF(0.5f, 0.5f, Width - 2, Height - 2), Radius);
            using var b = new SolidBrush(BackColor);
            g.FillPath(b, path);
            using var p = new Pen(Theme.Border);
            g.DrawPath(p, path);
        }
    }
}
