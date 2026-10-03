using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class ReportsPage : RangePage
    {
        private readonly ComboBox _report = UIx.Combo(240);
        public override string PageTitle => "Reports";
        public override string PageSubtitle => "Sales, profit, stock valuation and customer balances — export any report to CSV";

        public ReportsPage()
        {
            _report.Items.AddRange(new object[] { "Daily sales summary", "Product performance & profit", "Stock valuation", "Low stock / reorder list", "Customer balances (receivables)" });
            _report.SelectedIndex = 0;
            _report.SelectedIndexChanged += async (_, _) => await RefreshAsync();
            BuildBase(new Control[] { _report });
            Search.Visible = false;
        }

        private void Reset(params (string header, float weight, bool right)[] cols)
        {
            Grid.Rows.Clear();
            Grid.Columns.Clear();
            foreach (var c in cols) Grid.Col(c.header, c.weight, c.right);
        }

        public override async Task RefreshAsync()
        {
            await Guard(async () =>
            {
                var rs = App.Get<ReportService>();
                var useRange = _report.SelectedIndex <= 1;
                From.Enabled = To.Enabled = useRange;
                switch (_report.SelectedIndex)
                {
                    case 0:
                    {
                        var sales = (await rs.GetSalesAsync(From.Value, To.Value, null, SaleStatus.Completed));
                        Reset(("Date", 1.5f, false), ("Invoices", 1, true), ("Gross sales", 1.4f, true), ("Discounts", 1.2f, true), ("Tax", 1.2f, true), ("Net total", 1.4f, true), ("Collected", 1.4f, true));
                        foreach (var g in sales.GroupBy(s => s.SaleDate.Date).OrderByDescending(g => g.Key))
                            Grid.Rows.Add(Fmt.Date(g.Key), g.Count(), Fmt.Money(g.Sum(s => s.SubTotal)), Fmt.Money(g.Sum(s => s.DiscountAmount)),
                                Fmt.Money(g.Sum(s => s.TaxAmount)), Fmt.Money(g.Sum(s => s.Total)), Fmt.Money(g.Sum(s => s.PaidAmount)));
                        Summary.Text = $"{sales.Count} invoices   •   Net sales {Fmt.Money(sales.Sum(s => s.Total))}   •   Discounts {Fmt.Money(sales.Sum(s => s.DiscountAmount))}   •   Tax {Fmt.Money(sales.Sum(s => s.TaxAmount))}";
                        break;
                    }
                    case 1:
                    {
                        var rows = await rs.GetProductPerformanceAsync(From.Value, To.Value);
                        Reset(("Product", 3, false), ("Qty sold", 1, true), ("Revenue", 1.4f, true), ("Profit", 1.4f, true), ("Margin", 1, true));
                        foreach (var r in rows)
                            Grid.Rows.Add(r.name, r.qty, Fmt.Money(r.revenue), Fmt.Money(r.profit), r.revenue == 0 ? "-" : $"{r.profit / r.revenue:P1}");
                        Summary.Text = $"{rows.Count} product(s)   •   Revenue {Fmt.Money(rows.Sum(r => r.revenue))}   •   Gross profit {Fmt.Money(rows.Sum(r => r.profit))}  (before discounts & tax)";
                        break;
                    }
                    case 2:
                    {
                        await using var db = App.NewDb();
                        var items = await db.Products.AsNoTracking().Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
                        Reset(("SKU", 1, false), ("Product", 2.5f, false), ("Category", 1.4f, false), ("Qty", 0.8f, true), ("Cost", 1.2f, true), ("Cost value", 1.4f, true), ("Retail value", 1.4f, true));
                        foreach (var p in items)
                            Grid.Rows.Add(p.Sku, p.Name, p.Category?.Name, p.StockQty, Fmt.Money(p.CostPrice), Fmt.Money(p.StockQty * p.CostPrice), Fmt.Money(p.StockQty * p.SalePrice));
                        Summary.Text = $"{items.Count} product(s)   •   Stock at cost {Fmt.Money(items.Sum(p => p.StockQty * p.CostPrice))}   •   At retail {Fmt.Money(items.Sum(p => p.StockQty * p.SalePrice))}";
                        break;
                    }
                    case 3:
                    {
                        await using var db = App.NewDb();
                        var items = await db.Products.AsNoTracking().Include(p => p.Supplier).Where(p => p.StockQty <= p.ReorderLevel).OrderBy(p => p.StockQty).ToListAsync();
                        Reset(("SKU", 1, false), ("Product", 2.5f, false), ("Supplier", 1.6f, false), ("In stock", 0.9f, true), ("Reorder at", 0.9f, true), ("Suggested order", 1.2f, true));
                        foreach (var p in items)
                            Grid.Rows.Add(p.Sku, p.Name, p.Supplier?.Name, p.StockQty, p.ReorderLevel, Math.Max(p.ReorderLevel * 2 - p.StockQty, 1));
                        Summary.Text = $"{items.Count} item(s) at or below reorder level";
                        break;
                    }
                    default:
                    {
                        var custs = (await rs.GetCustomersWithDueAsync()).Where(c => c.Due > 0).OrderByDescending(c => c.Due).ToList();
                        Reset(("Customer", 2.5f, false), ("Phone", 1.4f, false), ("Balance due", 1.4f, true));
                        foreach (var c in custs) Grid.Rows.Add(c.Name, c.Phone, Fmt.Money(c.Due));
                        Summary.Text = $"{custs.Count} customer(s) owe {Fmt.Money(custs.Sum(c => c.Due))}";
                        break;
                    }
                }
            });
        }
    }
}
