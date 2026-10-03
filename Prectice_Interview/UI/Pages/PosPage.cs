using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class PosPage : PageBase
    {
        public override string PageTitle => "New Sale";
        public override string PageSubtitle => "Scan or search products, build the cart and print the invoice  (F9 = complete sale)";

        private readonly InventoryService _inv = App.Get<InventoryService>();
        private readonly InputBox _search = UIx.Input("Scan barcode / type SKU or product name — press Enter", 300);
        private readonly DataGridView _products = new() { Dock = DockStyle.Fill };
        private readonly DataGridView _cartGrid = new() { Dock = DockStyle.Fill };
        private readonly ComboBox _customer = UIx.Combo(280);
        private readonly ComboBox _discType = UIx.Combo(90);
        private readonly NumericUpDown _discount = UIx.Number(0, 99_999_999, 2, 100);
        private readonly ComboBox _method = UIx.Combo(160);
        private readonly NumericUpDown _received = UIx.Number(0, 999_999_999, 2, 160);
        private readonly Label _lblSub = UIx.Label("0.00", Theme.Base), _lblTax = UIx.Label("0.00", Theme.Base), _lblTaxCap = UIx.Label("Tax", Theme.Base, Theme.Muted),
            _lblTotal = UIx.Label("0.00", Theme.Big, Theme.Primary), _lblChange = UIx.Label("", Theme.Bold), _lblItems = UIx.Label("0 items", Theme.Small, Theme.Muted);
        private readonly FlatButton _btnComplete = UIx.Button("Complete Sale  (F9)", BtnKind.Success, width: 200);

        private List<Product> _all = new();
        private List<Customer> _customers = new();
        private readonly List<CartLine> _cart = new();
        private bool _receivedTouched, _updating;

        public PosPage()
        {
            Padding = new Padding(24, 8, 24, 20);
            BuildLayout();
        }

        private void BuildLayout()
        {
            var left = new Card { Dock = DockStyle.Fill, Padding = new Padding(16) };
            _search.Dock = DockStyle.Top;
            _search.Margin = new Padding(0);
            UIx.StyleGrid(_products);
            _products.Col("Product", 3); _products.Col("SKU", 1.2f); _products.Col("Price", 1.2f, true); _products.Col("Stock", 0.9f, true);
            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 0), BackColor = Color.White };
            gridHost.Controls.Add(_products);
            left.Controls.Add(gridHost);
            left.Controls.Add(_search);

            var right = new Card { Dock = DockStyle.Right, Width = 520, Padding = new Padding(16) };
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = Color.White };
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

            var custPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            custPanel.Controls.Add(new Label { Text = "Customer", Font = Theme.Small, ForeColor = Theme.Muted, Location = new Point(0, 0), AutoSize = true });
            _customer.Location = new Point(0, 22); _customer.Width = 330;
            var btnNewCust = UIx.Button("＋ New", BtnKind.Secondary, (_, _) => _ = Guard(QuickAddCustomerAsync), 90);
            btnNewCust.Height = 30; btnNewCust.Location = new Point(340, 21);
            custPanel.Controls.AddRange(new Control[] { _customer, btnNewCust });
            table.Controls.Add(custPanel, 0, 0);

            UIx.StyleGrid(_cartGrid);
            _cartGrid.RowTemplate.Height = 40;
            _cartGrid.ReadOnly = false;
            _cartGrid.Col("Item", 2.6f).ReadOnly = true;
            _cartGrid.Col("Qty", 0.9f, true);
            _cartGrid.Col("Price", 1.5f, true).ReadOnly = true;
            _cartGrid.Col("Total", 1.6f, true).ReadOnly = true;
            _cartGrid.CellEndEdit += OnCartQtyEdited;
            table.Controls.Add(_cartGrid, 0, 1);

            var cartBtns = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 6, 0, 0) };
            var plus = UIx.Button("＋", BtnKind.Secondary, (_, _) => ChangeQty(+1), 48);
            var minus = UIx.Button("－", BtnKind.Secondary, (_, _) => ChangeQty(-1), 48);
            var remove = UIx.Button("Remove", BtnKind.Secondary, (_, _) => RemoveSelected(), 90);
            var clear = UIx.Button("Clear cart", BtnKind.Ghost, (_, _) => ClearCart(true), 100);
            foreach (var b in new[] { plus, minus, remove, clear }) { b.Height = 34; b.Margin = new Padding(0, 0, 6, 0); }
            _lblItems.Margin = new Padding(8, 9, 0, 0);
            cartBtns.Controls.AddRange(new Control[] { plus, minus, remove, clear, _lblItems });
            table.Controls.Add(cartBtns, 0, 2);

            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7, BackColor = Color.White };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            for (var i = 0; i < 7; i++) t.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 3 ? 52 : 34));
            void Line(int row, Control cap, Control val)
            {
                cap.Dock = DockStyle.Fill; val.Dock = DockStyle.Fill;
                if (cap is Label cl) cl.TextAlign = ContentAlignment.MiddleLeft;
                if (val is Label vl) vl.TextAlign = ContentAlignment.MiddleRight;
                t.Controls.Add(cap, 0, row); t.Controls.Add(val, 1, row);
            }
            Line(0, UIx.Label("Subtotal", Theme.Base, Theme.Muted), _lblSub);
            var discPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White, Margin = new Padding(0), Padding = new Padding(0, 2, 0, 0) };
            _discType.Items.AddRange(new object[] { "Amount", "Percent %" }); _discType.SelectedIndex = 0; _discType.Width = 100;
            discPanel.Controls.Add(_discType); discPanel.Controls.Add(_discount);
            _discount.Margin = new Padding(6, 0, 0, 0);
            Line(1, UIx.Label("Discount", Theme.Base, Theme.Muted), discPanel);
            Line(2, _lblTaxCap, _lblTax);
            Line(3, UIx.Label("TOTAL", Theme.H2), _lblTotal);
            _method.Items.AddRange(new object[] { PaymentMethod.Cash, PaymentMethod.Card, PaymentMethod.BankTransfer }); _method.SelectedIndex = 0;
            var mp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White, Padding = new Padding(0, 2, 0, 0) };
            mp.Controls.Add(_method);
            Line(4, UIx.Label("Payment method", Theme.Base, Theme.Muted), mp);
            var rp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White, Padding = new Padding(0, 2, 0, 0) };
            rp.Controls.Add(_received);
            Line(5, UIx.Label("Amount received", Theme.Base, Theme.Muted), rp);
            t.Controls.Add(_lblChange, 0, 6); t.SetColumnSpan(_lblChange, 2);
            _lblChange.Dock = DockStyle.Fill; _lblChange.TextAlign = ContentAlignment.MiddleRight;
            table.Controls.Add(t, 0, 3);

            _btnComplete.Dock = DockStyle.Fill; _btnComplete.Height = 46; _btnComplete.Font = new Font("Segoe UI Semibold", 12f);
            _btnComplete.Margin = new Padding(0, 8, 0, 0);
            _btnComplete.Click += async (_, _) => await Guard(CompleteAsync);
            table.Controls.Add(_btnComplete, 0, 4);
            right.Controls.Add(table);

            var spacer = new Panel { Dock = DockStyle.Right, Width = 16, BackColor = Theme.Background };
            Controls.Add(left);
            Controls.Add(spacer);
            Controls.Add(right);

            _search.TextChanged += (_, _) => RenderProducts();
            _search.KeyDown += OnSearchKey;
            _products.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) AddFromGrid(); };
            _products.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; AddFromGrid(); } };
            _discount.ValueChanged += (_, _) => Recalc();
            _discType.SelectedIndexChanged += (_, _) => Recalc();
            _received.ValueChanged += (_, _) => { if (!_updating) { _receivedTouched = true; UpdateChange(); } };
            _method.SelectedIndexChanged += (_, _) => UpdateChange();
        }

        public override async Task RefreshAsync()
        {
            await Guard(async () =>
            {
                await using var db = App.NewDb();
                _all = await db.Products.AsNoTracking().Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
                _customers = await db.Customers.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
                var sel = _customer.SelectedValue as int?;
                var list = new List<KeyValuePair<int?, string>> { new(null, "Walk-in customer") };
                list.AddRange(_customers.Select(c => new KeyValuePair<int?, string>(c.Id, c.Name + (string.IsNullOrEmpty(c.Phone) ? "" : "  •  " + c.Phone))));
                _customer.DataSource = null;
                _customer.DisplayMember = "Value"; _customer.ValueMember = "Key";
                _customer.DataSource = list;
                var idx = list.FindIndex(x => x.Key == sel);
                _customer.SelectedIndex = idx < 0 ? 0 : idx;
                _lblTaxCap.Text = $"Tax ({App.Get<SettingsService>().Current.TaxRate:0.##}%)";
                RenderProducts();
                Recalc();
            });
            _search.Focus();
        }

        private IEnumerable<Product> FilteredProducts()
        {
            var t = _search.Text.Trim();
            return string.IsNullOrEmpty(t) ? _all
                : _all.Where(p => p.Name.Contains(t, StringComparison.OrdinalIgnoreCase) || p.Sku.Contains(t, StringComparison.OrdinalIgnoreCase)
                                  || (p.Barcode ?? "").Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        private void RenderProducts()
        {
            _products.Rows.Clear();
            foreach (var p in FilteredProducts().Take(300))
            {
                var i = _products.Rows.Add(p.Name, p.Sku, Fmt.Money(p.SalePrice), $"{p.StockQty} {p.Unit}");
                var row = _products.Rows[i];
                row.Tag = p;
                if (p.StockQty <= 0) row.DefaultCellStyle.ForeColor = Theme.Muted;
                else if (p.IsLowStock) row.Cells[3].Style.ForeColor = Theme.Warning;
            }
        }

        private void OnSearchKey(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && _products.Rows.Count > 0) { _products.Focus(); e.Handled = true; return; }
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            var t = _search.Text.Trim();
            if (t.Length == 0) return;
            var exact = _all.FirstOrDefault(p => string.Equals(p.Barcode, t, StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sku, t, StringComparison.OrdinalIgnoreCase));
            var hits = FilteredProducts().Take(2).ToList();
            var pick = exact ?? (hits.Count == 1 ? hits[0] : null);
            if (pick != null) { AddToCart(pick); _search.Text = ""; }
            else if (hits.Count == 0) UIx.Toast(this, "No product found", true);
            else _products.Focus();
        }

        private void AddFromGrid()
        {
            if (_products.CurrentRow?.Tag is Product p) AddToCart(p);
        }

        private void AddToCart(Product p)
        {
            var i = _cart.FindIndex(c => c.ProductId == p.Id);
            var current = i >= 0 ? _cart[i].Qty : 0;
            if (current + 1 > p.StockQty)
            {
                UIx.Toast(this, p.StockQty <= 0 ? $"{p.Name} is out of stock" : $"Only {p.StockQty} {p.Unit} of {p.Name} in stock", true);
                return;
            }
            if (i >= 0) _cart[i] = _cart[i] with { Qty = current + 1 };
            else _cart.Add(new CartLine(p.Id, p.Name, p.Sku, 1, p.SalePrice, p.CostPrice));
            RenderCart(p.Id);
        }

        private void RenderCart(int? selectProduct = null)
        {
            _cartGrid.Rows.Clear();
            foreach (var l in _cart)
            {
                var i = _cartGrid.Rows.Add(l.Name, l.Qty, l.UnitPrice.ToString("N2"), (l.Qty * l.UnitPrice).ToString("N2"));
                _cartGrid.Rows[i].Tag = l.ProductId;
                if (l.ProductId == selectProduct) _cartGrid.CurrentCell = _cartGrid.Rows[i].Cells[0];
            }
            _lblItems.Text = $"{_cart.Sum(c => c.Qty)} item(s)";
            _receivedTouched = false;
            Recalc();
        }

        private int SelectedCartIndex()
        {
            if (_cartGrid.CurrentRow?.Tag is not int pid) return -1;
            return _cart.FindIndex(c => c.ProductId == pid);
        }

        private void ChangeQty(int delta)
        {
            var i = SelectedCartIndex();
            if (i < 0) return;
            var line = _cart[i];
            var stock = _all.FirstOrDefault(p => p.Id == line.ProductId)?.StockQty ?? 0;
            var q = line.Qty + delta;
            if (q > stock) { UIx.Toast(this, $"Only {stock} in stock", true); return; }
            if (q <= 0) _cart.RemoveAt(i); else _cart[i] = line with { Qty = q };
            RenderCart(q > 0 ? line.ProductId : null);
        }

        private void RemoveSelected()
        {
            var i = SelectedCartIndex();
            if (i < 0) return;
            _cart.RemoveAt(i);
            RenderCart();
        }

        private void ClearCart(bool confirm)
        {
            if (_cart.Count == 0) return;
            if (confirm && !UIx.Confirm("Clear all items from the cart?")) return;
            _cart.Clear();
            _discount.Value = 0;
            RenderCart();
        }

        private void OnCartQtyEdited(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
            var i = SelectedCartIndex();
            if (i < 0) return;
            var line = _cart[i];
            var raw = _cartGrid.Rows[e.RowIndex].Cells[1].Value?.ToString();
            var stock = _all.FirstOrDefault(p => p.Id == line.ProductId)?.StockQty ?? 0;
            if (!int.TryParse(raw, out var q) || q < 0) q = line.Qty;
            if (q > stock) { UIx.Toast(this, $"Only {stock} in stock", true); q = Math.Min(line.Qty, stock); }
            if (q == 0) _cart.RemoveAt(i); else _cart[i] = line with { Qty = q };
            BeginInvoke(() => RenderCart(q > 0 ? line.ProductId : null));
        }

        private decimal DiscountAmount()
        {
            var sub = _cart.Sum(c => c.Qty * c.UnitPrice);
            return _discType.SelectedIndex == 1 ? Math.Round(sub * Math.Min(_discount.Value, 100) / 100m, 2) : _discount.Value;
        }

        private (decimal sub, decimal discount, decimal tax, decimal total) Current() =>
            InventoryService.Totals(_cart, DiscountAmount(), App.Get<SettingsService>().Current.TaxRate);

        private void Recalc()
        {
            var (sub, disc, tax, total) = Current();
            _lblSub.Text = Fmt.Money(sub);
            _lblTax.Text = Fmt.Money(tax);
            _lblTotal.Text = Fmt.Money(total);
            if (!_receivedTouched)
            {
                _updating = true;
                _received.Value = Math.Min(total, _received.Maximum);
                _updating = false;
            }
            UpdateChange();
            _btnComplete.Enabled = _cart.Count > 0;
        }

        private void UpdateChange()
        {
            var total = Current().total;
            var diff = _received.Value - total;
            if (_cart.Count == 0) { _lblChange.Text = ""; return; }
            if (diff >= 0) { _lblChange.Text = $"Change to return:  {Fmt.Money(diff)}"; _lblChange.ForeColor = Theme.Success; }
            else { _lblChange.Text = $"Balance due:  {Fmt.Money(-diff)}"; _lblChange.ForeColor = Theme.Danger; }
        }

        private async Task QuickAddCustomerAsync()
        {
            var c = new Customer();
            using var dlg = new EntityDialog("New customer", new()
            {
                new() { Label = "Full name", Required = true, Get = () => c.Name, Set = v => c.Name = (string)v! },
                new() { Label = "Phone", Get = () => c.Phone, Set = v => c.Phone = SuppliersPage.Nz(v) }
            });
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await App.Get<GenaricService>().AddAsync(c);
            await RefreshAsync();
            var list = (List<KeyValuePair<int?, string>>)_customer.DataSource!;
            _customer.SelectedIndex = Math.Max(0, list.FindIndex(x => x.Key == c.Id));
        }

        private async Task CompleteAsync()
        {
            if (_cart.Count == 0) return;
            var method = _method.SelectedItem is PaymentMethod m ? m : PaymentMethod.Cash;
            var custId = _customer.SelectedValue as int?;
            var (_, _, _, total) = Current();
            if (_received.Value < total && custId == null)
            {
                UIx.Error("Partial or unpaid sales need a customer. Pick a customer, or collect the full amount.");
                return;
            }
            _btnComplete.Enabled = false;
            try
            {
                var sale = await _inv.CreateSaleAsync(new SaleRequest
                {
                    CustomerId = custId,
                    Lines = _cart.ToList(),
                    DiscountAmount = DiscountAmount(),
                    TaxRate = App.Get<SettingsService>().Current.TaxRate,
                    Tendered = _received.Value,
                    PaymentMethod = method
                });
                var full = await App.Get<ReportService>().GetSaleWithItemsAsync(sale.Id);
                ClearCart(false);
                await RefreshAsync();
                UIx.Toast(this, $"Invoice {sale.InvoiceNo} saved");
                if (full != null && UIx.Confirm($"Invoice {sale.InvoiceNo} created.\r\nTotal: {Fmt.Money(sale.Total)}\r\n\r\nOpen print preview now?", "Sale completed"))
                    InvoicePrinter.Preview(full);
            }
            finally { _btnComplete.Enabled = _cart.Count > 0; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F9 && _btnComplete.Enabled) { _btnComplete.PerformClick(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
