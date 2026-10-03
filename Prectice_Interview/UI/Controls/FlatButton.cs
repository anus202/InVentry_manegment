using System.Drawing.Drawing2D;

namespace Prectice_Interview.UI
{
    public class FlatButton : Button
    {
        private bool _hover, _down;
        public BtnKind Kind { get; set; } = BtnKind.Primary;
        public int Radius { get; set; } = 8;

        public FlatButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = Theme.Bold;
            Cursor = Cursors.Hand;
            Height = 38;
            MinimumSize = new Size(60, 30);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent) { _down = true; Invalidate(); base.OnMouseDown(mevent); }
        protected override void OnMouseUp(MouseEventArgs mevent) { _down = false; Invalidate(); base.OnMouseUp(mevent); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var (bg, fg, border) = Kind switch
            {
                BtnKind.Primary => (Theme.Primary, Color.White, Theme.Primary),
                BtnKind.Success => (Theme.Success, Color.White, Theme.Success),
                BtnKind.Danger => (Theme.Danger, Color.White, Theme.Danger),
                BtnKind.Ghost => (Color.Transparent, Theme.Muted, Color.Transparent),
                _ => (Color.White, Theme.Text, Theme.Border)
            };
            if (_hover && Enabled)
            {
                bg = Kind switch
                {
                    BtnKind.Secondary => Theme.Background,
                    BtnKind.Ghost => Theme.Border,
                    _ => Theme.Blend(bg, Color.Black, 0.12f)
                };
            }
            if (_down && Enabled && bg != Color.Transparent) bg = Theme.Blend(bg, Color.Black, 0.2f);
            if (!Enabled) { bg = Kind == BtnKind.Secondary || Kind == BtnKind.Ghost ? Theme.Background : Theme.Blend(bg, Color.White, 0.6f); fg = Theme.Muted; }

            var rect = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
            using var path = Theme.RoundRect(rect, Radius);
            if (bg != Color.Transparent) { using var b = new SolidBrush(bg); g.FillPath(b, path); }
            if (border != Color.Transparent) { using var p = new Pen(border); g.DrawPath(p, path); }
            TextRenderer.DrawText(g, Text, Font, Rectangle.Round(rect), fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
