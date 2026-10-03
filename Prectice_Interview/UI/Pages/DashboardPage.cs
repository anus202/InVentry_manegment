using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class DashboardPage : PageBase
    {
        public override string PageTitle => "Dashboard";
        public override string PageSubtitle => $"Welcome back, {Session.User?.FullName}. Here's how the business is doing.";

        private readonly StatCard _today = new() { Title = "Today's Sales", Accent = Theme.Primary, Glyph = "💰" };
        private readonly StatCard _month = new() { Title = "This Month", Accent = Theme.Info, Glyph = "📈" };
        private readonly StatCard _profit = new() { Title = "Profit (Month)", Accent = Theme.Success, Glyph = "🏦" };
        private readonly StatCard _due = new() { Title = "Receivables", Accent = Theme.Danger, Glyph = "🧾" };
        private readonly StatCard _products = new() { Title = "Products", Accent = Theme.Primary, Glyph = "📦" };
        private readonly StatCard _stockVal = new() { Title = "Stock Value (cost)", Accent = Theme.Info, Glyph = "🏷" };
        private readonly StatCard _low = new() { Title = "Low Stock Items", Accent = Theme.Warning, Glyph = "⚠" };
        private readonly StatCard _customers = new() { Title = "Customers", Accent = Theme.Success, Glyph = "👥" };
        private readonly BarChart _chart = new() { Dock = DockStyle.Fill };
        private readonly DataGridView _recent = new() { Dock = DockStyle.Fill };
        private readonly DataGridView _lowGrid = new() { Dock = DockStyle.Fill };
        private readonly DataGridView _top = new() { Dock = DockStyle.Fill };

        public DashboardPage()
        {
            Padding = new Padding(24, 8, 24, 16);
            AutoScroll = true;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Theme.Background, MinimumSize = new Size(900, 640) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 240));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var kpi = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, BackColor = Theme.Background };
            for (var i = 0; i < 4; i++) kpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpi.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            kpi.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            var cards = new[] { _today, _month, _profit, _due, _products, _stockVal, _low, _customers };
            for (var i = 0; i < cards.Length; i++)
            {
                cards[i].Dock = DockStyle.Fill;
                cards[i].Margin = new Padding(0, 0, i % 4 == 3 ? 0 : 14, 14);
                kpi.Controls.Add(cards[i], i % 4, i / 4);
            }
            root.Controls.Add(kpi, 0, 0);

            var row2 = TwoCols(
                Section("Sales — last 7 days", _chart),
                Section("Recent invoices", _recent));
            root.Controls.Add(row2, 0, 1);

            var row3 = TwoCols(
                Section("Low stock alerts", _lowGrid),
                Section("Top products this month", _top));
            root.Controls.Add(row3, 0, 2);
            Controls.Add(root);

            UIx.StyleGrid(_recent); _recent.RowTemplate.Height = 34;
            _recent.Col("Invoice", 1.3f); _recent.Col("Customer", 1.6f); _recent.Col("Total", 1.2f, true); _recent.Col("Status", 1f);
            UIx.StyleGrid(_lowGrid); _lowGrid.RowTemplate.Height = 34;
            _lowGrid.Col("Product", 2.5f); _lowGrid.Col("SKU", 1.2f); _lowGrid.Col("In stock", 1, true); _lowGrid.Col("Reorder at", 1, true);
            UIx.StyleGrid(_top); _top.RowTemplate.Height = 34;
            _top.Col("Product", 2.5f); _top.Col("Qty sold", 1, true); _top.Col("Revenue", 1.4f, true);
            _chart.ValueFormat = v => v >= 1000 ? (v / 1000m).ToString("0.#") + "k" : v.ToString("0");
        }

        private static TableLayoutPanel TwoCols(Control a, Control b)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.Background };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            a.Dock = DockStyle.Fill; b.Dock = DockStyle.Fill;
            a.Margin = new Padding(0, 0, 14, 14); b.Margin = new Padding(0, 0, 0, 14);
            t.Controls.Add(a, 0, 0); t.Controls.Add(b, 1, 0);
            return t;
        }

        private static Card Section(string title, Control content)
        {
            var c = new Card { Padding = new Padding(16, 12, 16, 12) };
            var hdr = new Label { Text = title, Font = Theme.H2, ForeColor = Theme.Text, Dock = DockStyle.Top, Height = 32, BackColor = Color.White };
            content.BackColor = Color.White;
            c.Controls.Add(content);
            c.Controls.Add(hdr);
            return c;
        }

        public override async Task RefreshAsync()
        {
            await Guard(async () =>
            {
                var d = await App.Get<ReportService>().GetDashboardAsync();
                _today.Set(Fmt.Money(d.TodaySales), $"{d.TodayInvoices} invoice(s) today");
                _month.Set(Fmt.Money(d.MonthSales), DateTime.Today.ToString("MMMM yyyy"));
                _profit.Set(Fmt.Money(d.MonthProfit), $"Today: {Fmt.Money(d.TodayProfit)}");
                _due.Set(Fmt.Money(d.Receivables), "Unpaid customer balances");
                _products.Set(d.ProductCount.ToString("N0"), $"{d.OutOfStockCount} out of stock");
                _stockVal.Set(Fmt.Money(d.StockValue), "At cost price");
                _low.Set(d.LowStockCount.ToString("N0"), d.LowStockCount == 0 ? "All stock healthy" : "Needs reordering");
                _customers.Set(d.CustomerCount.ToString("N0"), "Registered customers");

                _chart.Data = d.Last7.Select(x => (x.day.ToString("ddd dd"), x.total)).ToList();
                _chart.Invalidate();

                _recent.Rows.Clear();
                foreach (var s in d.Recent)
                {
                    var i = _recent.Rows.Add(s.InvoiceNo, s.Customer?.Name ?? "Walk-in", Fmt.Money(s.Total), s.Status == SaleStatus.Voided ? "Voided" : s.Balance > 0 ? "Due" : "Paid");
                    _recent.Rows[i].Cells[3].Style.ForeColor = s.Status == SaleStatus.Voided ? Theme.Muted : s.Balance > 0 ? Theme.Danger : Theme.Success;
                }
                _lowGrid.Rows.Clear();
                foreach (var p in d.LowStock)
                {
                    var i = _lowGrid.Rows.Add(p.Name, p.Sku, p.StockQty, p.ReorderLevel);
                    _lowGrid.Rows[i].Cells[2].Style.ForeColor = p.StockQty <= 0 ? Theme.Danger : Theme.Warning;
                }
                _top.Rows.Clear();
                foreach (var t in d.TopProducts) _top.Rows.Add(t.name, t.qty, Fmt.Money(t.revenue));
            });
        }
    }
}
