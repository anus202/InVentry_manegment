using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public abstract class RangePage : PageBase
    {
        protected readonly DataGridView Grid = new() { Dock = DockStyle.Fill };
        protected readonly DateTimePicker From = UIx.DatePicker(DateTime.Today.AddDays(-30));
        protected readonly DateTimePicker To = UIx.DatePicker(DateTime.Today);
        protected readonly InputBox Search = UIx.Input("Search...", 170);
        protected readonly FlowLayoutPanel Toolbar = new() { Dock = DockStyle.Top, Height = 56, WrapContents = false, Padding = new Padding(0, 0, 0, 10), BackColor = Theme.Background };
        protected readonly Label Summary = UIx.Label("", Theme.Bold, Theme.Text);
        private readonly System.Windows.Forms.Timer _debounce = new() { Interval = 300 };

        protected void BuildBase(IEnumerable<Control> extras)
        {
            Padding = new Padding(24, 8, 24, 20);
            UIx.StyleGrid(Grid);
            var l1 = UIx.Label("From", Theme.Small, Theme.Muted); l1.Margin = new Padding(0, 11, 6, 0);
            var l2 = UIx.Label("To", Theme.Small, Theme.Muted); l2.Margin = new Padding(8, 11, 6, 0);
            From.Margin = new Padding(0, 6, 0, 0); To.Margin = new Padding(0, 6, 0, 0);
            Search.Margin = new Padding(14, 0, 8, 0);
            Toolbar.Controls.AddRange(new Control[] { l1, From, l2, To, Search });
            foreach (var c in extras) { c.Margin = new Padding(0, 0, 8, 0); Toolbar.Controls.Add(c); }
            var export = UIx.Button("Export CSV", BtnKind.Secondary, (_, _) => CsvExporter.Export(Grid, PageTitle.Replace(' ', '_')), 120);
            Toolbar.Controls.Add(export);

            var card = new Card { Dock = DockStyle.Fill, Padding = new Padding(1) };
            card.Controls.Add(Grid);
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Theme.Background, Padding = new Padding(4, 8, 0, 0) };
            Summary.Dock = DockStyle.Fill;
            footer.Controls.Add(Summary);
            Controls.Add(card);
            Controls.Add(footer);
            Controls.Add(Toolbar);

            _debounce.Tick += async (_, _) => { _debounce.Stop(); await RefreshAsync(); };
            Search.TextChanged += (_, _) => { _debounce.Stop(); _debounce.Start(); };
            From.ValueChanged += async (_, _) => await RefreshAsync();
            To.ValueChanged += async (_, _) => await RefreshAsync();
        }
    }
}
