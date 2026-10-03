using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class PurchasesPage : RangePage
    {
        public override string PageTitle => "Purchases";
        public override string PageSubtitle => "Receive stock from suppliers — quantities and cost update automatically";

        public PurchasesPage()
        {
            var add = UIx.Button("＋  New Purchase", BtnKind.Primary, async (_, _) => await Guard(NewAsync), 160);
            BuildBase(new Control[] { add });
            Grid.Col("Purchase #", 1.1f); Grid.Col("Date", 1.1f); Grid.Col("Supplier", 1.8f); Grid.Col("Reference", 1.2f); Grid.Col("Items", 3f); Grid.Col("Total", 1.2f, true);
        }

        public override async Task RefreshAsync()
        {
            await Guard(async () =>
            {
                var list = await App.Get<ReportService>().GetPurchasesAsync(From.Value, To.Value);
                var t = Search.Text.Trim();
                if (t.Length > 0)
                    list = list.Where(p => p.PurchaseNo.Contains(t, StringComparison.OrdinalIgnoreCase) || (p.Supplier?.Name ?? "").Contains(t, StringComparison.OrdinalIgnoreCase)
                        || (p.Reference ?? "").Contains(t, StringComparison.OrdinalIgnoreCase) || p.Items.Any(i => i.ProductName.Contains(t, StringComparison.OrdinalIgnoreCase))).ToList();
                Grid.Rows.Clear();
                foreach (var p in list)
                    Grid.Rows.Add(p.PurchaseNo, Fmt.Date(p.PurchaseDate), p.Supplier?.Name ?? "—", p.Reference,
                        string.Join(", ", p.Items.Select(i => $"{i.ProductName} ×{i.Quantity}")), Fmt.Money(p.Total));
                Summary.Text = $"{list.Count} purchase(s)   •   Total {Fmt.Money(list.Sum(p => p.Total))}";
            });
        }

        private async Task NewAsync()
        {
            using var dlg = new PurchaseDialog();
            await dlg.LoadAsync();
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await RefreshAsync();
            UIx.Toast(this, "Purchase saved — stock updated");
        }
    }
}
