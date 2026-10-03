using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class MainForm : Form
    {
        private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = Theme.Background };
        private readonly Label _title = UIx.Label("", Theme.H1), _subtitle = UIx.Label("", Theme.Base, Theme.Muted), _clock = UIx.Label("", Theme.Bold, Theme.Muted);
        private readonly List<(NavButton btn, Type page)> _nav = new();
        private PageBase? _current;
        private bool _loggedOut;

        public bool LoggedOut => _loggedOut;

        public MainForm()
        {
            Text = $"{App.Get<SettingsService>().Current.CompanyName} — Inventory Management";
            Size = new Size(1360, 820);
            MinimumSize = new Size(1180, 700);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = Theme.Background;
            Font = Theme.Base;

            var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Theme.Background, Padding = new Padding(28, 14, 28, 0) };
            _title.Location = new Point(24, 12);
            _subtitle.Location = new Point(27, 56);
            _clock.AutoSize = false; _clock.Size = new Size(320, 28); _clock.TextAlign = ContentAlignment.MiddleRight;
            _clock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            header.Controls.AddRange(new Control[] { _title, _subtitle, _clock });
            header.Resize += (_, _) => _clock.Location = new Point(header.Width - _clock.Width - 28, 20);

            var side = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Theme.Sidebar };
            var brand = new Label
            {
                Text = "📦  Inventory Pro", Dock = DockStyle.Top, Height = 64, ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 15f), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(22, 0, 0, 0)
            };
            var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Theme.Sidebar, Padding = new Padding(0, 6, 0, 0) };

            void Item(string text, string glyph, Type page, bool allowed = true)
            {
                if (!allowed) return;
                var b = new NavButton { Text = text, Glyph = glyph, Width = 250, Margin = new Padding(0) };
                b.Click += async (_, _) => await ShowAsync(page);
                nav.Controls.Add(b);
                _nav.Add((b, page));
            }
            void Section(string text) => nav.Controls.Add(new Label
            {
                Text = text, ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI Semibold", 8f), Width = 250, Height = 28,
                TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(24, 0, 0, 4), Margin = new Padding(0, 6, 0, 0)
            });

            Section("OVERVIEW");
            Item("Dashboard", "🏠", typeof(DashboardPage));
            Section("SALES");
            Item("New Sale (POS)", "🛒", typeof(PosPage));
            Item("Invoices", "🧾", typeof(SalesPage));
            Item("Customers", "👥", typeof(CustomersPage));
            Section("INVENTORY");
            Item("Products", "📦", typeof(ProductsPage));
            Item("Categories", "🗂", typeof(CategoriesPage));
            Item("Stock Ledger", "📋", typeof(StockPage));
            Item("Purchases", "🚚", typeof(PurchasesPage), Session.IsManagerOrAdmin);
            Item("Suppliers", "🏭", typeof(SuppliersPage), Session.IsManagerOrAdmin);
            Section("INSIGHTS");
            Item("Reports", "📊", typeof(ReportsPage), Session.IsManagerOrAdmin);
            Section("ADMIN");
            Item("Users", "🔐", typeof(UsersPage), Session.IsAdmin);
            Item("Settings", "⚙", typeof(SettingsPage), Session.IsAdmin);

            var userCard = new Panel { Dock = DockStyle.Bottom, Height = 96, BackColor = Theme.Sidebar, Padding = new Padding(20, 10, 20, 10) };
            var name = new Label { Text = Session.User?.FullName, ForeColor = Color.White, Font = Theme.Bold, Dock = DockStyle.Top, Height = 24 };
            var role = new Label { Text = Session.User?.Role.ToString(), ForeColor = Color.FromArgb(148, 163, 184), Font = Theme.Small, Dock = DockStyle.Top, Height = 22 };
            var logout = UIx.Button("Sign out", BtnKind.Secondary, (_, _) => { _loggedOut = true; Close(); }, 120);
            logout.Dock = DockStyle.Bottom; logout.Height = 32;
            userCard.Controls.Add(logout); userCard.Controls.Add(role); userCard.Controls.Add(name);

            side.Controls.Add(nav);
            side.Controls.Add(userCard);
            side.Controls.Add(brand);

            Controls.Add(_content);
            Controls.Add(header);
            Controls.Add(side);

            var timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (_, _) => _clock.Text = DateTime.Now.ToString("dddd, dd MMM yyyy   hh:mm:ss tt");
            timer.Start();
            _clock.Text = DateTime.Now.ToString("dddd, dd MMM yyyy   hh:mm:ss tt");

            Shown += async (_, _) => await ShowAsync(typeof(DashboardPage));
            FormClosing += (_, e) =>
            {
                if (!_loggedOut && e.CloseReason == CloseReason.UserClosing && !UIx.Confirm("Exit the application?", "Exit"))
                    e.Cancel = true;
            };
        }

        private async Task ShowAsync(Type pageType)
        {
            if (_current?.GetType() == pageType) { await _current.RefreshAsync(); return; }
            var page = (PageBase)App.Services.GetRequiredService(pageType);
            SuspendLayout();
            _content.Controls.Clear();
            _current?.Dispose();
            _current = page;
            _content.Controls.Add(page);
            _title.Text = page.PageTitle;
            _subtitle.Text = page.PageSubtitle;
            foreach (var (btn, t) in _nav) { btn.Active = t == pageType; btn.Invalidate(); }
            ResumeLayout(true);
            await page.RefreshAsync();
        }
    }
}
