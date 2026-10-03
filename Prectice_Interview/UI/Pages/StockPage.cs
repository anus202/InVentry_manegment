using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class StockPage : RangePage
    {
        private readonly ComboBox _type = UIx.Combo(160);
        public override string PageTitle => "Stock Ledger";
        public override string PageSubtitle => "Every stock movement — purchases, sales, voids and manual adjustments";

        public StockPage()
        {
            _type.Items.Add("All movements");
            foreach (var t in Enum.GetValues<MovementType>()) _type.Items.Add(t);
            _type.SelectedIndex = 0;
            _type.SelectedIndexChanged += async (_, _) => await RefreshAsync();
            BuildBase(new Control[] { _type });
            Grid.Col("Date", 1.6f); Grid.Col("Product", 2.5f); Grid.Col("Type", 1.1f); Grid.Col("Change", 0.8f, true);
            Grid.Col("Balance", 0.8f, true); Grid.Col("Reference", 1.2f); Grid.Col("Note", 2f); Grid.Col("By", 1.1f);
        }

        public override async Task RefreshAsync()
        {
            await Guard(async () =>
            {
                MovementType? t = _type.SelectedItem is MovementType m ? m : null;
                var list = await App.Get<ReportService>().GetMovementsAsync(From.Value, To.Value, Search.Text, t);
                Grid.Rows.Clear();
                foreach (var m2 in list)
                {
                    var i = Grid.Rows.Add(Fmt.DateTimeText(m2.Date), m2.ProductName, m2.Type, (m2.Quantity > 0 ? "+" : "") + m2.Quantity, m2.BalanceAfter, m2.Reference, m2.Note, m2.UserName);
                    Grid.Rows[i].Cells[3].Style.ForeColor = m2.Quantity > 0 ? Theme.Success : Theme.Danger;
                    Grid.Rows[i].Cells[3].Style.Font = Theme.Bold;
                }
                Summary.Text = $"{list.Count} movement(s) shown (max 2,000)";
            });
        }
    }
}
