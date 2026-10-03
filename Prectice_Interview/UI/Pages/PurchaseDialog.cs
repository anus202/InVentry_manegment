using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class PurchaseDialog : DialogBase
    {
        private readonly ComboBox _supplier = UIx.Combo(260);
        private readonly DateTimePicker _date = UIx.DatePicker(DateTime.Today, 150);
        private readonly InputBox _ref = UIx.Input("Supplier invoice no.", 200);
        private readonly ComboBox _product = UIx.Combo(300);
        private readonly NumericUpDown _qty = UIx.Number(1, 1_000_000, 0, 80);
        private readonly NumericUpDown _cost = UIx.Number(0, 99_999_999, 2, 120);
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
        private readonly Label _total = UIx.Label("Total: 0.00", Theme.H2, Theme.Primary);
        private readonly List<(int id, string name, int qty, decimal cost)> _lines = new();
        private List<Product> _products = new();

        public PurchaseDialog() : base("New purchase", "Add the items received from your supplier", new Size(820, 640))
        {
            BtnSave.Text = "Save Purchase";
            BtnSave.Width = 150;
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 66, BackColor = Theme.Background, WrapContents = false };
            top.Controls.Add(Wrap("Supplier", _supplier, 14));
            top.Controls.Add(Wrap("Date", _date, 14));
            top.Controls.Add(Wrap("Reference", _ref, 0));

            var add = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 70, BackColor = Theme.Background, WrapContents = false };
            add.Controls.Add(Wrap("Product", _product, 10));
            add.Controls.Add(Wrap("Qty", _qty, 10));
            add.Controls.Add(Wrap("Unit cost", _cost, 10));
            var btn = UIx.Button("Add item", BtnKind.Secondary, (_, _) => AddLine(), 100);
            btn.Margin = new Padding(0, 22, 0, 0);
            add.Controls.Add(btn);

            UIx.StyleGrid(_grid);
            _grid.Col("Product", 3); _grid.Col("Qty", 0.8f, true); _grid.Col("Unit cost", 1.2f, true); _grid.Col("Line total", 1.2f, true);
            var removeCol = new DataGridViewButtonColumn { HeaderText = "", Text = "✕", UseColumnTextForButtonValue = true, FillWeight = 40, FlatStyle = FlatStyle.Flat, MinimumWidth = 40 };
            _grid.Columns.Add(removeCol);
            _grid.CellClick += (_, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == 4) { _lines.RemoveAt(e.RowIndex); RenderLines(); } };
            var gridCard = new Card { Dock = DockStyle.Fill, Padding = new Padding(1) };
            gridCard.Controls.Add(_grid);

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Theme.Background };
            _total.Dock = DockStyle.Right; _total.AutoSize = false; _total.Width = 300; _total.TextAlign = ContentAlignment.MiddleRight;
            bottom.Controls.Add(_total);

            Body.Controls.Add(gridCard);
            Body.Controls.Add(bottom);
            Body.Controls.Add(add);
            Body.Controls.Add(top);
            _product.SelectedIndexChanged += (_, _) =>
            {
                if (_product.SelectedItem is Product p) _cost.Value = Math.Min(p.CostPrice, _cost.Maximum);
            };
            BtnSave.Click += async (_, _) => await SaveAsync();
        }

        private static Panel Wrap(string label, Control c, int rightMargin)
        {
            var p = new Panel { Width = c.Width + rightMargin, Height = 62, BackColor = Theme.Background };
            p.Controls.Add(new Label { Text = label, Font = Theme.Small, ForeColor = Theme.Muted, AutoSize = true, Top = 0 });
            c.Top = 22;
            p.Controls.Add(c);
            return p;
        }

        public async Task LoadAsync()
        {
            await using var db = App.NewDb();
            var sups = new List<KeyValuePair<int?, string>> { new(null, "— No supplier —") };
            sups.AddRange((await db.Suppliers.OrderBy(s => s.Name).ToListAsync()).Select(s => new KeyValuePair<int?, string>(s.Id, s.Name)));
            _supplier.DisplayMember = "Value"; _supplier.ValueMember = "Key"; _supplier.DataSource = sups;
            _products = await db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            _product.DisplayMember = nameof(Product.Name);
            _product.DataSource = _products;
        }

        private void AddLine()
        {
            if (_product.SelectedItem is not Product p) { UIx.Error("Choose a product."); return; }
            var qty = (int)_qty.Value;
            var i = _lines.FindIndex(l => l.id == p.Id);
            if (i >= 0) _lines[i] = (p.Id, p.Name, _lines[i].qty + qty, _cost.Value);
            else _lines.Add((p.Id, p.Name, qty, _cost.Value));
            _qty.Value = 1;
            RenderLines();
        }

        private void RenderLines()
        {
            _grid.Rows.Clear();
            foreach (var l in _lines) _grid.Rows.Add(l.name, l.qty, l.cost.ToString("N2"), (l.qty * l.cost).ToString("N2"));
            _total.Text = "Total: " + Fmt.Money(_lines.Sum(l => l.qty * l.cost));
        }

        private async Task SaveAsync()
        {
            try
            {
                await App.Get<InventoryService>().CreatePurchaseAsync(_supplier.SelectedValue as int?, _date.Value.Date,
                    string.IsNullOrWhiteSpace(_ref.Text) ? null : _ref.Text.Trim(), null,
                    _lines.Select(l => (l.id, l.name, l.qty, l.cost)).ToList());
                DialogResult = DialogResult.OK;
            }
            catch (BusinessException ex) { UIx.Error(ex.Message); }
            catch (Exception ex) { Log.Error(ex, "PurchaseDialog"); UIx.Error("Could not save: " + (ex.InnerException?.Message ?? ex.Message)); }
        }
    }
}
