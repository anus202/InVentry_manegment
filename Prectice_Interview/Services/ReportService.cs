using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public class ReportService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        public ReportService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        public async Task<DashboardData> GetDashboardAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            var d = new DashboardData();
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var completed = db.Sales.AsNoTracking().Where(s => s.Status == SaleStatus.Completed);

            d.TodaySales = await completed.Where(s => s.SaleDate >= today).SumAsync(s => (decimal?)s.Total) ?? 0;
            d.TodayInvoices = await completed.CountAsync(s => s.SaleDate >= today);
            d.MonthSales = await completed.Where(s => s.SaleDate >= monthStart).SumAsync(s => (decimal?)s.Total) ?? 0;
            d.Receivables = await completed.SumAsync(s => (decimal?)(s.Total - s.PaidAmount)) ?? 0;

            async Task<decimal> Profit(DateTime from)
            {
                var net = await completed.Where(s => s.SaleDate >= from).SumAsync(s => (decimal?)(s.SubTotal - s.DiscountAmount)) ?? 0;
                var cogs = await db.SaleItems.Where(i => completed.Any(s => s.Id == i.SaleId && s.SaleDate >= from))
                    .SumAsync(i => (decimal?)(i.UnitCost * i.Quantity)) ?? 0;
                return net - cogs;
            }
            d.TodayProfit = await Profit(today);
            d.MonthProfit = await Profit(monthStart);

            d.ProductCount = await db.Products.CountAsync();
            d.LowStockCount = await db.Products.CountAsync(p => p.StockQty <= p.ReorderLevel);
            d.OutOfStockCount = await db.Products.CountAsync(p => p.StockQty <= 0);
            d.StockValue = await db.Products.SumAsync(p => (decimal?)(p.StockQty * p.CostPrice)) ?? 0;
            d.CustomerCount = await db.Customers.CountAsync();

            var from7 = today.AddDays(-6);
            var raw = await completed.Where(s => s.SaleDate >= from7).Select(s => new { s.SaleDate, s.Total }).ToListAsync();
            for (var i = 0; i < 7; i++)
            {
                var day = from7.AddDays(i);
                d.Last7.Add((day, raw.Where(r => r.SaleDate.Date == day).Sum(r => r.Total)));
            }

            d.TopProducts = (await db.SaleItems.Where(i => completed.Any(s => s.Id == i.SaleId && s.SaleDate >= monthStart))
                    .GroupBy(i => i.ProductName)
                    .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.Quantity), Rev = g.Sum(x => x.LineTotal) })
                    .OrderByDescending(x => x.Rev).Take(5).ToListAsync())
                .Select(x => (x.Name, x.Qty, x.Rev)).ToList();

            d.LowStock = await db.Products.AsNoTracking().Where(p => p.StockQty <= p.ReorderLevel)
                .OrderBy(p => p.StockQty).Take(8).ToListAsync();
            d.Recent = await db.Sales.AsNoTracking().Include(s => s.Customer)
                .OrderByDescending(s => s.Id).Take(8).ToListAsync();
            return d;
        }

        public async Task<List<Sale>> GetSalesAsync(DateTime from, DateTime to, string? search = null, SaleStatus? status = null)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var end = to.Date.AddDays(1);
            var q = db.Sales.AsNoTracking().Include(s => s.Customer).Where(s => s.SaleDate >= from.Date && s.SaleDate < end);
            if (status != null) q = q.Where(s => s.Status == status);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var t = search.Trim();
                q = q.Where(s => s.InvoiceNo.Contains(t) || (s.Customer != null && s.Customer.Name.Contains(t)) || (s.CashierName != null && s.CashierName.Contains(t)));
            }
            return await q.OrderByDescending(s => s.Id).ToListAsync();
        }

        public async Task<Sale?> GetSaleWithItemsAsync(int id)
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Sales.AsNoTracking().Include(s => s.Items).Include(s => s.Customer).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Purchase>> GetPurchasesAsync(DateTime from, DateTime to)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var end = to.Date.AddDays(1);
            return await db.Purchases.AsNoTracking().Include(p => p.Supplier).Include(p => p.Items)
                .Where(p => p.PurchaseDate >= from.Date && p.PurchaseDate < end).OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<List<StockMovement>> GetMovementsAsync(DateTime from, DateTime to, string? search, MovementType? type)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var end = to.Date.AddDays(1);
            var q = db.StockMovements.AsNoTracking().Where(m => m.Date >= from.Date && m.Date < end);
            if (type != null) q = q.Where(m => m.Type == type);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var t = search.Trim();
                q = q.Where(m => m.ProductName.Contains(t) || (m.Reference != null && m.Reference.Contains(t)));
            }
            return await q.OrderByDescending(m => m.Id).Take(2000).ToListAsync();
        }

        public async Task<List<(string name, int qty, decimal revenue, decimal profit)>> GetProductPerformanceAsync(DateTime from, DateTime to)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var end = to.Date.AddDays(1);
            var rows = await db.SaleItems.AsNoTracking()
                .Where(i => db.Sales.Any(s => s.Id == i.SaleId && s.Status == SaleStatus.Completed && s.SaleDate >= from.Date && s.SaleDate < end))
                .GroupBy(i => i.ProductName)
                .Select(g => new { g.Key, Qty = g.Sum(x => x.Quantity), Rev = g.Sum(x => x.LineTotal), Cost = g.Sum(x => x.UnitCost * x.Quantity) })
                .OrderByDescending(x => x.Rev).ToListAsync();
            return rows.Select(r => (r.Key, r.Qty, r.Rev, r.Rev - r.Cost)).ToList();
        }

        public async Task<List<Customer>> GetCustomersWithDueAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            var dues = await db.Sales.AsNoTracking().Where(s => s.Status == SaleStatus.Completed && s.CustomerId != null)
                .GroupBy(s => s.CustomerId!.Value)
                .Select(g => new { Id = g.Key, Due = g.Sum(x => x.Total - x.PaidAmount) }).ToListAsync();
            var customers = await db.Customers.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            foreach (var c in customers) c.Due = dues.FirstOrDefault(x => x.Id == c.Id)?.Due ?? 0;
            return customers;
        }
    }
}
