using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class ProductsPage : MasterPage<Product>
    {
        private List<KeyValuePair<int?, string>> _cats = new(), _sups = new();
        private readonly CheckBox _lowOnly = new() { Text = "Low stock only", AutoSize = true, Font = Theme.Base, Padding = new Padding(0, 8, 0, 0), ForeColor = Theme.Text };
        private readonly FlatButton _btnAdjust = UIx.Button("Adjust Stock", BtnKind.Secondary, width: 120);

        public override string PageTitle => "Products";
        public override string PageSubtitle => "Manage your catalogue, prices and stock levels";
        protected override string EntityName => "Product";
        public ProductsPage()
        {
            BuildUi();
            _lowOnly.CheckedChanged += async (_, _) => await RefreshAsync();
            _btnAdjust.Click += async (_, _) => await Guard(AdjustAsync);
        }
        protected override IEnumerable<Control> ExtraToolbar() => new Control[] { _btnAdjust, _lowOnly };
        protected override bool Filter(Product p) => !_lowOnly.Checked || p.IsLowStock;

        protected override List<ColumnDef<Product>> Columns() => new()
        {
            new("SKU", p => p.Sku, 1), new("Product", p => p.Name, 2.5f), new("Category", p => p.Category?.Name, 1.4f),
            new("Cost", p => Fmt.Money(p.CostPrice), 1.2f, true), new("Price", p => Fmt.Money(p.SalePrice), 1.2f, true),
            new("Stock", p => $"{p.StockQty} {p.Unit}", 1, true), new("Reorder At", p => p.ReorderLevel, 0.9f, true),
            new("Status", p => p.StockQty <= 0 ? "Out of stock" : p.IsLowStock ? "Low stock" : "In stock", 1.1f)
        };
        protected override async Task<List<Product>> LoadAsync()
        {
            await using var db = App.NewDb();
            _cats = new() { new(null, "— None —") };
            _cats.AddRange((await db.Categories.OrderBy(c => c.Name).ToListAsync()).Select(c => new KeyValuePair<int?, string>(c.Id, c.Name)));
            _sups = new() { new(null, "— None —") };
            _sups.AddRange((await db.Suppliers.OrderBy(c => c.Name).ToListAsync()).Select(c => new KeyValuePair<int?, string>(c.Id, c.Name)));
            return await db.Products.AsNoTracking().Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
        }
        protected override List<FieldDef> Fields(Product p)
        {
            var list = new List<FieldDef>
            {
                new() { Label = "SKU / Item code", Required = true, Get = () => p.Sku, Set = v => p.Sku = (string)v!, Hint = "Must be unique" },
                new() { Label = "Barcode", Get = () => p.Barcode, Set = v => p.Barcode = SuppliersPage.Nz(v), Hint = "Scan or type — used for fast lookup at the till" },
                new() { Label = "Product name", Required = true, Get = () => p.Name, Set = v => p.Name = (string)v! },
                new() { Label = "Category", Kind = FieldKind.Combo, Options = _cats, Get = () => p.CategoryId, Set = v => p.CategoryId = (int?)v },
                new() { Label = "Supplier", Kind = FieldKind.Combo, Options = _sups, Get = () => p.SupplierId, Set = v => p.SupplierId = (int?)v },
                new() { Label = "Unit (pcs, kg, box...)", Required = true, Get = () => p.Unit, Set = v => p.Unit = (string)v! },
                new() { Label = "Cost price", Kind = FieldKind.Decimal, Get = () => p.CostPrice, Set = v => p.CostPrice = (decimal)v! },
                new() { Label = "Sale price", Kind = FieldKind.Decimal, Get = () => p.SalePrice, Set = v => p.SalePrice = (decimal)v! },
            };
            if (p.Id == 0)
                list.Add(new() { Label = "Opening stock", Kind = FieldKind.Int, Get = () => p.StockQty, Set = v => p.StockQty = (int)v! });
            list.Add(new() { Label = "Reorder level (low stock alert)", Kind = FieldKind.Int, Get = () => p.ReorderLevel, Set = v => p.ReorderLevel = (int)v! });
            list.Add(new() { Label = "Description", Kind = FieldKind.Multiline, Get = () => p.Description, Set = v => p.Description = SuppliersPage.Nz(v) });
            return list;
        }
        protected override string? Validate(Product p, bool isNew)
        {
            using var db = App.NewDb();
            if (db.Products.Any(x => x.Sku == p.Sku && x.Id != p.Id)) return "This SKU is already used by another product.";
            if (!string.IsNullOrEmpty(p.Barcode) && db.Products.Any(x => x.Barcode == p.Barcode && x.Id != p.Id)) return "This barcode is already used by another product.";
            if (p.SalePrice < p.CostPrice && !UIx.Confirm("Sale price is lower than cost price. Save anyway?")) return "Please correct the prices.";
            return null;
        }
        protected override Task SaveAsync(Product item, bool isNew)
        {
            item.Category = null; item.Supplier = null;
            return isNew ? Svc.AddAsync(item) : Svc.UpdateAsync(item);
        }
        protected override Task AfterSaveAsync(Product item, bool isNew) =>
            isNew ? App.Get<InventoryService>().RecordOpeningStockAsync(item) : Task.CompletedTask;

        protected override void StyleRow(DataGridViewRow row, Product p)
        {
            var c = row.Cells[7];
            c.Style.ForeColor = p.StockQty <= 0 ? Theme.Danger : p.IsLowStock ? Theme.Warning : Theme.Success;
            c.Style.Font = Theme.Bold;
        }

        private async Task AdjustAsync()
        {
            var p = Selected;
            if (p == null) { UIx.Error("Select a product first."); return; }
            var qty = p.StockQty; var reason = "";
            using var dlg = new EntityDialog($"Adjust stock — {p.Name}", new()
            {
                new() { Label = $"Counted quantity (system shows {p.StockQty})", Kind = FieldKind.Int, Get = () => qty, Set = v => qty = (int)v! },
                new() { Label = "Reason (damage, stock-take, correction...)", Required = true, Get = () => reason, Set = v => reason = (string)v! }
            });
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await App.Get<InventoryService>().AdjustStockAsync(p.Id, qty, reason);
            await RefreshAsync();
            UIx.Toast(this, "Stock adjusted");
        }
    }
}
