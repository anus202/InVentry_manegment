using System.Drawing.Drawing2D;

namespace Prectice_Interview.UI
{
    public class InputBox : Control
    {
        private readonly TextBox _tb = new();
        private bool _focus;

        public InputBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Height = 38;
            Font = Theme.Base;
            _tb.BorderStyle = BorderStyle.None;
            _tb.Font = Theme.Base;
            _tb.BackColor = Color.White;
            _tb.ForeColor = Theme.Text;
            _tb.GotFocus += (_, _) => { _focus = true; Invalidate(); };
            _tb.LostFocus += (_, _) => { _focus = false; Invalidate(); };
            _tb.TextChanged += (_, e) => OnTextChanged(e);
            _tb.KeyDown += (_, e) => OnKeyDown(e);
            _tb.KeyPress += (_, e) => OnKeyPress(e);
            Controls.Add(_tb);
            LayoutInner();
        }

        public override string Text { get => _tb.Text; set => _tb.Text = value; }
        public string PlaceholderText { get => _tb.PlaceholderText; set => _tb.PlaceholderText = value; }
        public bool Password { get => _tb.UseSystemPasswordChar; set => _tb.UseSystemPasswordChar = value; }
        public bool Multiline
        {
            get => _tb.Multiline;
            set { _tb.Multiline = value; _tb.AcceptsReturn = value; LayoutInner(); }
        }
        public int MaxLength { get => _tb.MaxLength; set => _tb.MaxLength = value; }
        public bool ReadOnly { get => _tb.ReadOnly; set => _tb.ReadOnly = value; }
        public new void Focus() => _tb.Focus();
        public void SelectAllText() => _tb.SelectAll();

        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); LayoutInner(); }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); LayoutInner(); }
        protected override void OnGotFocus(EventArgs e) { _tb.Focus(); base.OnGotFocus(e); }
        protected override void OnEnabledChanged(EventArgs e) { _tb.Enabled = Enabled; _tb.BackColor = Enabled ? Color.White : Theme.Background; Invalidate(); base.OnEnabledChanged(e); }

        private void LayoutInner()
        {
            if (_tb == null) return;
            if (_tb.Multiline) _tb.SetBounds(10, 8, Width - 20, Height - 16);
            else _tb.SetBounds(12, (Height - _tb.PreferredHeight) / 2, Math.Max(10, Width - 24), _tb.PreferredHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = Theme.RoundRect(new RectangleF(0.5f, 0.5f, Width - 2, Height - 2), 8);
            using var fill = new SolidBrush(Enabled ? Color.White : Theme.Background);
            g.FillPath(fill, path);
            using var pen = new Pen(_focus ? Theme.Primary : Theme.Border, _focus ? 1.6f : 1f);
            g.DrawPath(pen, path);
        }
    }
}
