using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public record CartLine(int ProductId, string Name, string Sku, int Qty, decimal UnitPrice, decimal UnitCost);
}
