using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class SuppliersPage : MasterPage<Supplier>
    {
        public override string PageTitle => "Suppliers";
        public override string PageSubtitle => "Vendors you purchase stock from";
        protected override string EntityName => "Supplier";
        public SuppliersPage() => BuildUi();

        protected override List<ColumnDef<Supplier>> Columns() => new()
        {
            new("Name", s => s.Name, 2), new("Contact", s => s.ContactPerson, 1.5f), new("Phone", s => s.Phone, 1.2f),
            new("Email", s => s.Email, 2), new("Address", s => s.Address, 3)
        };
        protected override async Task<List<Supplier>> LoadAsync()
        {
            await using var db = App.NewDb();
            return await db.Suppliers.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        }
        protected override List<FieldDef> Fields(Supplier s) => new()
        {
            new() { Label = "Company name", Required = true, Get = () => s.Name, Set = v => s.Name = (string)v! },
            new() { Label = "Contact person", Get = () => s.ContactPerson, Set = v => s.ContactPerson = Nz(v) },
            new() { Label = "Phone", Get = () => s.Phone, Set = v => s.Phone = Nz(v) },
            new() { Label = "Email", Get = () => s.Email, Set = v => s.Email = Nz(v) },
            new() { Label = "Address", Kind = FieldKind.Multiline, Get = () => s.Address, Set = v => s.Address = Nz(v) }
        };
        internal static string? Nz(object? v) => string.IsNullOrWhiteSpace(v as string) ? null : ((string)v).Trim();
        protected override bool CanDelete(Supplier s, out string? reason)
        {
            using var db = App.NewDb();
            var n = db.Products.Count(p => p.SupplierId == s.Id);
            reason = n > 0 ? $"This supplier is linked to {n} product(s). Re-assign them first." : null;
            return n == 0;
        }
    }
}
