using System.Drawing.Drawing2D;

namespace Prectice_Interview.UI
{
    public class BarChart : Control
    {
        public List<(string label, decimal value)> Data { get; set; } = new();
        public Func<decimal, string> ValueFormat { get; set; } = v => v.ToString("N0");

        public BarChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            if (Data.Count == 0) return;

            const int bottom = 26, top = 22;
            var plotH = Height - bottom - top;
            var max = Data.Max(d => d.value);
            if (max <= 0) max = 1;
            var slot = (float)Width / Data.Count;
            var barW = Math.Min(54f, slot * 0.55f);

            using var grid = new Pen(Theme.Border) { DashStyle = DashStyle.Dash };
            for (var i = 0; i <= 3; i++)
            {
                var y = top + plotH - plotH * i / 3f;
                g.DrawLine(grid, 0, y, Width, y);
            }
            for (var i = 0; i < Data.Count; i++)
            {
                var h = (float)(Data[i].value / max) * plotH;
                var x = slot * i + (slot - barW) / 2;
                var rect = new RectangleF(x, top + plotH - Math.Max(h, 3), barW, Math.Max(h, 3));
                using var path = Theme.RoundRect(rect, Math.Min(6, rect.Height / 2));
                using var br = new LinearGradientBrush(rect, Theme.Primary, Theme.Blend(Theme.Primary, Color.White, 0.45f), 90f);
                g.FillPath(br, path);
                TextRenderer.DrawText(g, Data[i].label, Theme.Small, new Rectangle((int)(slot * i), Height - bottom + 4, (int)slot, 20), Theme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                if (Data[i].value > 0)
                    TextRenderer.DrawText(g, ValueFormat(Data[i].value), Theme.Small, new Rectangle((int)(slot * i), (int)rect.Y - 20, (int)slot, 18), Theme.Text,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            }
        }
    }
}
