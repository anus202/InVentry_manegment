using System.Drawing.Drawing2D;

namespace Prectice_Interview.UI
{
    public static class Theme
    {
        public static readonly Color Primary = ColorTranslator.FromHtml("#4F46E5");
        public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#4338CA");
        public static readonly Color PrimarySoft = ColorTranslator.FromHtml("#EEF2FF");
        public static readonly Color Sidebar = ColorTranslator.FromHtml("#0F172A");
        public static readonly Color SidebarHover = ColorTranslator.FromHtml("#1E293B");
        public static readonly Color Background = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color Surface = Color.White;
        public static readonly Color Border = ColorTranslator.FromHtml("#E2E8F0");
        public static readonly Color Text = ColorTranslator.FromHtml("#0F172A");
        public static readonly Color Muted = ColorTranslator.FromHtml("#64748B");
        public static readonly Color Success = ColorTranslator.FromHtml("#10B981");
        public static readonly Color Danger = ColorTranslator.FromHtml("#EF4444");
        public static readonly Color Warning = ColorTranslator.FromHtml("#F59E0B");
        public static readonly Color Info = ColorTranslator.FromHtml("#0EA5E9");

        public static readonly Font Base = new("Segoe UI", 10f);
        public static readonly Font Bold = new("Segoe UI Semibold", 10f);
        public static readonly Font Small = new("Segoe UI", 9f);
        public static readonly Font H1 = new("Segoe UI Semibold", 20f);
        public static readonly Font H2 = new("Segoe UI Semibold", 13f);
        public static readonly Font Big = new("Segoe UI Semibold", 22f);

        public static GraphicsPath RoundRect(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            var d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static Color Blend(Color a, Color b, float t) => Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
    }
}
