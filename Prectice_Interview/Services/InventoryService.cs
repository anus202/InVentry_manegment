using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public class InventoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        private readonly SettingsService _settings;

        public InventoryService(IDbContextFactory<ApplicationDbContext> factory, SettingsService settings)
        {
            _factory = factory;
            _settings = settings;
        }

        public static (decimal sub, decimal discount, decimal tax, decimal total) Totals(IEnumerable<CartLine> lines, decimal discount, decimal taxRate)
        {
            var sub = Math.Round(lines.Sum(l => l.Qty * l.UnitPrice), 2);
            discount = Math.Round(Math.Min(Math.Max(discount, 0), sub), 2);
            var tax = Math.Round((sub - discount) * taxRate / 100m, 2);
            return (sub, discount, tax, sub - discount + tax);
        }

        public async Task<Sale> CreateSaleAsync(SaleRequest req)
        {
            if (req.Lines.Count == 0) throw new BusinessException("The cart is empty.");
            var (sub, discount, tax, total) = Totals(req.Lines, req.DiscountAmount, req.TaxRate);
            var paid = Math.Min(Math.Max(req.Tendered, 0), total);
            if (paid < total && req.CustomerId == null)
                throw new BusinessException("Select a customer to sell on credit (partial / unpaid invoices need a customer).");

            await using var db = await _factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();

            var sale = new Sale
            {
                InvoiceNo = "TEMP",
                SaleDate = DateTime.Now,
                CustomerId = req.CustomerId,
                SubTotal = sub,
                DiscountAmount = discount,
                TaxRate = req.TaxRate,
                TaxAmount = tax,
                Total = total,
                PaidAmount = paid,
                PaymentMethod = paid < total && paid == 0 ? PaymentMethod.Credit : req.PaymentMethod,
                Notes = req.Notes,
                CashierName = Session.UserName
            };

            foreach (var l in req.Lines)
            {
                if (l.Qty <= 0) throw new BusinessException($"Invalid quantity for {l.Name}.");
                var rows = await db.Products.Where(p => p.Id == l.ProductId && p.StockQty >= l.Qty)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQty, p => p.StockQty - l.Qty));
                if (rows == 0)
                {
                    var available = await db.Products.Where(p => p.Id == l.ProductId).Select(p => (int?)p.StockQty).FirstOrDefaultAsync();
                    throw new BusinessException($"Insufficient stock for \"{l.Name}\". Available: {available ?? 0}, requested: {l.Qty}.");
                }
                sale.Items.Add(new SaleItem
                {
                    ProductId = l.ProductId,
                    ProductName = l.Name,
                    Sku = l.Sku,
                    Quantity = l.Qty,
                    UnitPrice = l.UnitPrice,
                    UnitCost = l.UnitCost,
                    LineTotal = Math.Round(l.Qty * l.UnitPrice, 2)
                });
            }

            db.Sales.Add(sale);
            await db.SaveChangesAsync();
            sale.InvoiceNo = $"{_settings.Current.InvoicePrefix}{sale.Id:D6}";

            foreach (var l in req.Lines)
            {
                var bal = await db.Products.Where(p => p.Id == l.ProductId).Select(p => p.StockQty).FirstAsync();
                db.StockMovements.Add(new StockMovement
                {
                    ProductId = l.ProductId, ProductName = l.Name, Type = MovementType.Sale, Quantity = -l.Qty,
                    BalanceAfter = bal, Reference = sale.InvoiceNo, UserName = Session.UserName
                });
            }
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return sale;
        }

        public async Task VoidSaleAsync(int saleId, string reason)
        {
            await using var db = await _factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();
            var sale = await db.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == saleId)
                       ?? throw new BusinessException("Invoice not found.");
            if (sale.Status == SaleStatus.Voided) throw new BusinessException("This invoice is already voided.");

            foreach (var i in sale.Items)
            {
                await db.Products.IgnoreQueryFilters().Where(p => p.Id == i.ProductId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQty, p => p.StockQty + i.Quantity));
                var bal = await db.Products.IgnoreQueryFilters().Where(p => p.Id == i.ProductId).Select(p => p.StockQty).FirstAsync();
                db.StockMovements.Add(new StockMovement
                {
                    ProductId = i.ProductId, ProductName = i.ProductName, Type = MovementType.SaleVoid, Quantity = i.Quantity,
                    BalanceAfter = bal, Reference = sale.InvoiceNo, Note = reason, UserName = Session.UserName
                });
            }
            sale.Status = SaleStatus.Voided;
            sale.Notes = string.IsNullOrWhiteSpace(reason) ? sale.Notes : $"{sale.Notes} [VOID: {reason}]".Trim();
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task ReceivePaymentAsync(int saleId, decimal amount)
        {
            if (amount <= 0) throw new BusinessException("Enter a payment amount greater than zero.");
            await using var db = await _factory.CreateDbContextAsync();
            var sale = await db.Sales.FirstOrDefaultAsync(s => s.Id == saleId) ?? throw new BusinessException("Invoice not found.");
            if (sale.Status == SaleStatus.Voided) throw new BusinessException("Cannot receive payment on a voided invoice.");
            if (amount > sale.Balance) throw new BusinessException($"Amount exceeds the balance due ({Fmt.Money(sale.Balance)}).");
            sale.PaidAmount += amount;
            await db.SaveChangesAsync();
        }

        public async Task<Purchase> CreatePurchaseAsync(int? supplierId, DateTime date, string? reference, string? notes,
            List<(int productId, string name, int qty, decimal cost)> lines)
        {
            if (lines.Count == 0) throw new BusinessException("Add at least one item.");
            await using var db = await _factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();

            var purchase = new Purchase
            {
                PurchaseNo = "TEMP", PurchaseDate = date, SupplierId = supplierId, Reference = reference, Notes = notes,
                Total = Math.Round(lines.Sum(l => l.qty * l.cost), 2)
            };
            foreach (var l in lines)
                purchase.Items.Add(new PurchaseItem
                {
                    ProductId = l.productId, ProductName = l.name, Quantity = l.qty, UnitCost = l.cost,
                    LineTotal = Math.Round(l.qty * l.cost, 2)
                });
            db.Purchases.Add(purchase);
            await db.SaveChangesAsync();
            purchase.PurchaseNo = $"PUR-{purchase.Id:D6}";

            foreach (var l in lines)
            {
                await db.Products.Where(p => p.Id == l.productId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQty, p => p.StockQty + l.qty)
                                              .SetProperty(p => p.CostPrice, l.cost));
                var bal = await db.Products.Where(p => p.Id == l.productId).Select(p => p.StockQty).FirstAsync();
                db.StockMovements.Add(new StockMovement
                {
                    ProductId = l.productId, ProductName = l.name, Type = MovementType.Purchase, Quantity = l.qty,
                    BalanceAfter = bal, Reference = purchase.PurchaseNo, UserName = Session.UserName
                });
            }
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return purchase;
        }

        public async Task AdjustStockAsync(int productId, int newQty, string reason)
        {
            if (newQty < 0) throw new BusinessException("Stock cannot be negative.");
            await using var db = await _factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();
            var p = await db.Products.FirstOrDefaultAsync(x => x.Id == productId) ?? throw new BusinessException("Product not found.");
            var diff = newQty - p.StockQty;
            if (diff == 0) return;
            p.StockQty = newQty;
            db.StockMovements.Add(new StockMovement
            {
                ProductId = p.Id, ProductName = p.Name, Type = MovementType.Adjustment, Quantity = diff,
                BalanceAfter = newQty, Note = reason, UserName = Session.UserName
            });
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task RecordOpeningStockAsync(Product p)
        {
            if (p.StockQty <= 0) return;
            await using var db = await _factory.CreateDbContextAsync();
            db.StockMovements.Add(new StockMovement
            {
                ProductId = p.Id, ProductName = p.Name, Type = MovementType.Opening, Quantity = p.StockQty,
                BalanceAfter = p.StockQty, Note = "Opening stock", UserName = Session.UserName
            });
            await db.SaveChangesAsync();
        }
    }
}
