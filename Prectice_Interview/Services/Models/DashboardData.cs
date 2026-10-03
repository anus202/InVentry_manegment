using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public class DashboardData
    {
        public decimal TodaySales, MonthSales, TodayProfit, MonthProfit, StockValue, Receivables;
        public int TodayInvoices, ProductCount, LowStockCount, OutOfStockCount, CustomerCount;
        public List<(DateTime day, decimal total)> Last7 = new();
        public List<(string name, int qty, decimal revenue)> TopProducts = new();
        public List<Product> LowStock = new();
        public List<Sale> Recent = new();
    }
}
