using System.Drawing.Drawing2D;

namespace Prectice_Interview.UI
{
    public class StatCard : Control
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "0";
        public string Sub { get; set; } = "";
        public Color Accent { get; set; } = Theme.Primary;
        public string Glyph { get; set; } = "";

        public StatCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Height = 110;
        }

        public void Set(string value, string sub) { Value = value; Sub = sub; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using var path = Theme.RoundRect(new RectangleF(0.5f, 0.5f, Width - 2, Height - 2), 12);
            using (var b = new SolidBrush(Color.White)) g.FillPath(b, path);
            using (var p = new Pen(Theme.Border)) g.DrawPath(p, path);

            var bubble = new RectangleF(Width - 58, 16, 40, 40);
            using (var b = new SolidBrush(Color.FromArgb(28, Accent))) g.FillEllipse(b, bubble);
            TextRenderer.DrawText(g, Glyph, new Font("Segoe UI Emoji", 14f), Rectangle.Round(bubble), Accent,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, Title, Theme.Small, new Point(18, 16), Theme.Muted);
            var valueFont = new Font("Segoe UI Semibold", 18f);
            var maxWidth = Width - 70;
            while (valueFont.Size > 11f && TextRenderer.MeasureText(Value, valueFont).Width > maxWidth)
                valueFont = new Font(valueFont.FontFamily, valueFont.Size - 1f);
            TextRenderer.DrawText(g, Value, valueFont, new Rectangle(14, 34, maxWidth, 34), Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, Sub, Theme.Small, new Rectangle(18, Height - 27, Width - 30, 20), Accent,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }
    }
}
