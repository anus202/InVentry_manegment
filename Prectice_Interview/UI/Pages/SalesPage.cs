using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class SalesPage : RangePage
    {
        private readonly ComboBox _status = UIx.Combo(140);
        public override string PageTitle => "Sales & Invoices";
        public override string PageSubtitle => "Browse invoices, reprint, collect balances or void a sale";

        public SalesPage()
        {
            _status.Items.AddRange(new object[] { "All statuses", "Completed", "Voided" });
            _status.SelectedIndex = 0;
            _status.SelectedIndexChanged += async (_, _) => await RefreshAsync();
            var print = UIx.Button("View / Print", BtnKind.Primary, async (_, _) => await Guard(PrintAsync), 120);
            var pay = UIx.Button("Receive Payment", BtnKind.Success, async (_, _) => await Guard(ReceiveAsync), 150);
            var voidBtn = UIx.Button("Void", BtnKind.Danger, async (_, _) => await Guard(VoidAsync), 80);
            voidBtn.Visible = Session.IsManagerOrAdmin;
            BuildBase(new Control[] { _status, print, pay, voidBtn });
            Grid.Col("Invoice", 1.2f); Grid.Col("Date", 2.1f); Grid.Col("Customer", 1.8f); Grid.Col("Method", 1f);
            Grid.Col("Total", 1.2f, true); Grid.Col("Paid", 1.2f, true); Grid.Col("Balance", 1.2f, true); Grid.Col("Status", 1f); Grid.Col("Cashier", 1.2f);
            Grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await Guard(PrintAsync); };
        }

        public override async Task RefreshAsync()
        {
            await Guard(async () =>
            {
                SaleStatus? st = _status.SelectedIndex == 1 ? SaleStatus.Completed : _status.SelectedIndex == 2 ? SaleStatus.Voided : null;
                var list = await App.Get<ReportService>().GetSalesAsync(From.Value, To.Value, Search.Text, st);
                Grid.Rows.Clear();
                foreach (var s in list)
                {
                    var i = Grid.Rows.Add(s.InvoiceNo, Fmt.DateTimeText(s.SaleDate), s.Customer?.Name ?? "Walk-in", s.PaymentMethod,
                        Fmt.Money(s.Total), Fmt.Money(s.PaidAmount), Fmt.Money(s.Balance), s.Status == SaleStatus.Voided ? "Voided" : s.Balance > 0 ? "Due" : "Paid", s.CashierName);
                    var r = Grid.Rows[i];
                    r.Tag = s.Id;
                    r.Cells[7].Style.ForeColor = s.Status == SaleStatus.Voided ? Theme.Muted : s.Balance > 0 ? Theme.Danger : Theme.Success;
                    r.Cells[7].Style.Font = Theme.Bold;
                    if (s.Status == SaleStatus.Voided) r.DefaultCellStyle.ForeColor = Theme.Muted;
                }
                var ok = list.Where(s => s.Status == SaleStatus.Completed).ToList();
                Summary.Text = $"{ok.Count} invoice(s)   •   Sales {Fmt.Money(ok.Sum(s => s.Total))}   •   Collected {Fmt.Money(ok.Sum(s => s.PaidAmount))}   •   Outstanding {Fmt.Money(ok.Sum(s => s.Balance))}";
            });
        }

        private int? SelectedId => Grid.CurrentRow?.Tag as int?;

        private async Task PrintAsync()
        {
            if (SelectedId is not int id) { UIx.Error("Select an invoice first."); return; }
            var sale = await App.Get<ReportService>().GetSaleWithItemsAsync(id);
            if (sale != null) InvoicePrinter.Preview(sale);
        }

        private async Task ReceiveAsync()
        {
            if (SelectedId is not int id) { UIx.Error("Select an invoice first."); return; }
            var sale = await App.Get<ReportService>().GetSaleWithItemsAsync(id);
            if (sale == null) return;
            if (sale.Balance <= 0) { UIx.Error("This invoice has no balance due."); return; }
            var amount = sale.Balance;
            using var dlg = new EntityDialog($"Receive payment — {sale.InvoiceNo}", new()
            {
                new() { Label = $"Amount (balance due {Fmt.Money(sale.Balance)})", Kind = FieldKind.Decimal, Get = () => amount, Set = v => amount = (decimal)v! }
            });
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await App.Get<InventoryService>().ReceivePaymentAsync(id, amount);
            await RefreshAsync();
            UIx.Toast(this, "Payment recorded");
        }

        private async Task VoidAsync()
        {
            if (SelectedId is not int id) { UIx.Error("Select an invoice first."); return; }
            var reason = "";
            using var dlg = new EntityDialog("Void invoice", new()
            {
                new() { Label = "Reason for voiding (stock will be returned)", Required = true, Get = () => reason, Set = v => reason = (string)v! }
            });
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await App.Get<InventoryService>().VoidSaleAsync(id, reason);
            await RefreshAsync();
            UIx.Toast(this, "Invoice voided and stock restored");
        }
    }
}
