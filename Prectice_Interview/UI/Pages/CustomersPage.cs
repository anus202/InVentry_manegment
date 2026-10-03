using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class CustomersPage : MasterPage<Customer>
    {
        public override string PageTitle => "Customers";
        public override string PageSubtitle => "Your customer directory and outstanding balances";
        protected override string EntityName => "Customer";
        public CustomersPage() => BuildUi();

        protected override List<ColumnDef<Customer>> Columns() => new()
        {
            new("Name", c => c.Name, 2), new("Phone", c => c.Phone, 1.2f), new("Email", c => c.Email, 2),
            new("Address", c => c.Address, 3), new("Balance Due", c => Fmt.Money(c.Due), 1.3f, true)
        };
        protected override Task<List<Customer>> LoadAsync() => App.Get<ReportService>().GetCustomersWithDueAsync();
        protected override List<FieldDef> Fields(Customer c) => new()
        {
            new() { Label = "Full name", Required = true, Get = () => c.Name, Set = v => c.Name = (string)v! },
            new() { Label = "Phone", Get = () => c.Phone, Set = v => c.Phone = SuppliersPage.Nz(v) },
            new() { Label = "Email", Get = () => c.Email, Set = v => c.Email = SuppliersPage.Nz(v) },
            new() { Label = "Address", Kind = FieldKind.Multiline, Get = () => c.Address, Set = v => c.Address = SuppliersPage.Nz(v) }
        };
        protected override void StyleRow(DataGridViewRow row, Customer c)
        {
            if (c.Due > 0) row.Cells[4].Style.ForeColor = Theme.Danger;
        }
        protected override bool CanDelete(Customer c, out string? reason)
        {
            reason = c.Due > 0 ? $"This customer owes {Fmt.Money(c.Due)}. Collect payment before deleting." : null;
            return c.Due <= 0;
        }
        protected override Task SaveAsync(Customer item, bool isNew) => isNew ? Svc.AddAsync(item) : Svc.UpdateAsync(item);
    }
}
