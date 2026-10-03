using System.Drawing.Drawing2D;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class LoginForm : Form
    {
        private readonly AuthService _auth = App.Get<AuthService>();
        private readonly InputBox _full = UIx.Input("Full name", 340), _user = UIx.Input("Username", 340), _pass = UIx.Input("Password", 340), _pass2 = UIx.Input("Confirm password", 340);
        private readonly FlatButton _go = UIx.Button("Sign In", BtnKind.Primary, width: 340);
        private readonly Label _title = UIx.Label("Welcome back", new Font("Segoe UI Semibold", 22f)), _sub = UIx.Label("Sign in to continue", Theme.Base, Theme.Muted), _error = UIx.Label("", Theme.Small, Theme.Danger);
        private bool _setup;

        public LoginForm()
        {
            Text = "Inventory Management — Sign in";
            Size = new Size(960, 600);
            MinimumSize = Size; MaximumSize = Size;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = Theme.Base;
            DoubleBuffered = true;

            var left = new Panel { Dock = DockStyle.Left, Width = 440 };
            left.Paint += PaintBrand;

            var right = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _pass.Password = true; _pass2.Password = true;
            _title.Location = new Point(60, 80); _sub.Location = new Point(62, 128);
            _full.Location = new Point(60, 180); _user.Location = new Point(60, 230);
            _pass.Location = new Point(60, 280); _pass2.Location = new Point(60, 330);
            _error.Location = new Point(60, 372); _error.MaximumSize = new Size(340, 0);
            _go.Location = new Point(60, 410); _go.Height = 46;
            right.Controls.AddRange(new Control[] { _title, _sub, _full, _user, _pass, _pass2, _error, _go });
            Controls.Add(right);
            Controls.Add(left);

            _go.Click += async (_, _) => await SubmitAsync();
            foreach (var t in new[] { _full, _user, _pass, _pass2 })
                t.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _go.PerformClick(); } };
            AcceptButton = null;
            Shown += async (_, _) => await InitAsync();
        }

        private async Task InitAsync()
        {
            try
            {
                _setup = !await _auth.AnyUserAsync();
                _full.Visible = _pass2.Visible = _setup;
                if (_setup)
                {
                    _title.Text = "Create administrator";
                    _sub.Text = "First run — set up the owner account";
                    _go.Text = "Create Account & Continue";
                    _user.Location = new Point(60, 230);
                    _full.Focus();
                }
                else
                {
                    _title.Text = "Welcome back";
                    _sub.Text = "Sign in to continue";
                    _user.Location = new Point(60, 180);
                    _pass.Location = new Point(60, 230);
                    _error.Location = new Point(60, 280);
                    _go.Location = new Point(60, 312);
                    _user.Focus();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Login init");
                MessageBox.Show("Cannot connect to the database.\r\n\r\n" + (ex.InnerException?.Message ?? ex.Message) + "\r\n\r\nCheck the connection string in appsettings.json.",
                    "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private async Task SubmitAsync()
        {
            _error.Text = "";
            try
            {
                _go.Enabled = false;
                if (_setup)
                {
                    if (string.IsNullOrWhiteSpace(_full.Text) || string.IsNullOrWhiteSpace(_user.Text)) { _error.Text = "Full name and username are required."; return; }
                    if (_pass.Text.Length < 6) { _error.Text = "Password must be at least 6 characters."; return; }
                    if (_pass.Text != _pass2.Text) { _error.Text = "Passwords do not match."; return; }
                    await _auth.CreateUserAsync(_full.Text, _user.Text, _pass.Text, UserRole.Admin);
                }
                var user = await _auth.LoginAsync(_user.Text, _pass.Text);
                if (user == null) { _error.Text = "Invalid username or password."; _pass.SelectAllText(); _pass.Focus(); return; }
                Session.SignIn(user);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Login");
                _error.Text = "Error: " + (ex.InnerException?.Message ?? ex.Message);
            }
            finally { _go.Enabled = true; }
        }

        private void PaintBrand(object? sender, PaintEventArgs e)
        {
            var p = (Panel)sender!;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (var br = new LinearGradientBrush(p.ClientRectangle, Color.FromArgb(79, 70, 229), Color.FromArgb(15, 23, 42), 60f))
                g.FillRectangle(br, p.ClientRectangle);
            using (var b = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
            {
                g.FillEllipse(b, -90, 380, 330, 330);
                g.FillEllipse(b, 250, -90, 280, 280);
            }
            using var white = new SolidBrush(Color.White);
            using var soft = new SolidBrush(Color.FromArgb(200, 224, 231, 255));
            using var logoBg = new SolidBrush(Color.FromArgb(40, 255, 255, 255));
            g.FillEllipse(logoBg, 50, 60, 72, 72);
            TextRenderer.DrawText(g, "📦", new Font("Segoe UI Emoji", 28f), new Rectangle(50, 60, 72, 72), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            g.DrawString("Inventory\nManagement", new Font("Segoe UI Semibold", 30f), white, 48, 160);
            g.DrawString("Stock, sales, billing and reports —\neverything your shop needs in one place.", new Font("Segoe UI", 12f), soft, 50, 285);
            var y = 380;
            foreach (var t in new[] { "✔  Fast point-of-sale billing", "✔  Real-time stock tracking", "✔  Professional printable invoices", "✔  Profit & stock reports" })
            {
                g.DrawString(t, new Font("Segoe UI", 11f), white, 52, y);
                y += 30;
            }
        }
    }
}
