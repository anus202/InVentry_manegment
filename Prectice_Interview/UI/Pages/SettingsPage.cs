using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class SettingsPage : PageBase
    {
        public override string PageTitle => "Settings";
        public override string PageSubtitle => "Company profile, tax, invoice branding and database backup";

        private readonly InputBox _name = UIx.Input("", 420), _address = UIx.Input("", 420), _phone = UIx.Input("", 200), _email = UIx.Input("", 200),
            _taxNo = UIx.Input("", 200), _currency = UIx.Input("", 100), _prefix = UIx.Input("", 120), _footer = UIx.Input("", 420);
        private readonly NumericUpDown _tax = UIx.Number(0, 100, 2, 100);

        public SettingsPage()
        {
            Padding = new Padding(24, 8, 24, 20);
            AutoScroll = true;
            var card = new Card { Width = 520, Height = 760, Padding = new Padding(24), Anchor = AnchorStyles.Top | AnchorStyles.Left, Location = new Point(24, 8) };
            var y = 18;
            void Add(string label, Control c)
            {
                card.Controls.Add(new Label { Text = label, Font = Theme.Small, ForeColor = Theme.Muted, AutoSize = true, Location = new Point(24, y) });
                c.Location = new Point(24, y + 20);
                card.Controls.Add(c);
                y += 66;
            }
            Add("Company name", _name);
            Add("Address", _address);
            Add("Phone", _phone);
            Add("Email", _email);
            Add("Tax / registration number", _taxNo);
            Add("Currency symbol (e.g. Rs., $, AED)", _currency);
            Add("Default tax rate % (applied to every sale)", _tax);
            Add("Invoice number prefix", _prefix);
            Add("Invoice footer message", _footer);
            var save = UIx.Button("Save Settings", BtnKind.Primary, async (_, _) => await Guard(SaveAsync), 160);
            save.Location = new Point(24, y + 6);
            var backup = UIx.Button("Backup Database", BtnKind.Secondary, async (_, _) => await Guard(BackupAsync), 170);
            backup.Location = new Point(196, y + 6);
            card.Controls.Add(save); card.Controls.Add(backup);
            card.Height = y + 70;
            Controls.Add(card);
        }

        public override Task RefreshAsync()
        {
            var s = App.Get<SettingsService>().Current;
            _name.Text = s.CompanyName; _address.Text = s.Address ?? ""; _phone.Text = s.Phone ?? ""; _email.Text = s.Email ?? "";
            _taxNo.Text = s.TaxNumber ?? ""; _currency.Text = s.Currency; _tax.Value = s.TaxRate; _prefix.Text = s.InvoicePrefix; _footer.Text = s.InvoiceFooter;
            return Task.CompletedTask;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(_name.Text)) { UIx.Error("Company name is required."); return; }
            if (string.IsNullOrWhiteSpace(_currency.Text)) { UIx.Error("Currency symbol is required."); return; }
            var cur = App.Get<SettingsService>().Current;
            var s = new CompanySetting
            {
                Id = cur.Id, CreateBy = cur.CreateBy, CreateDate = cur.CreateDate, IsActive = true,
                CompanyName = _name.Text.Trim(), Address = Nz(_address.Text), Phone = Nz(_phone.Text), Email = Nz(_email.Text),
                TaxNumber = Nz(_taxNo.Text), Currency = _currency.Text.Trim(), TaxRate = _tax.Value,
                InvoicePrefix = string.IsNullOrWhiteSpace(_prefix.Text) ? "INV-" : _prefix.Text.Trim(), InvoiceFooter = _footer.Text.Trim()
            };
            await App.Get<SettingsService>().SaveAsync(s);
            UIx.Toast(this, "Settings saved");
        }

        private static string? Nz(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private async Task BackupAsync()
        {
            using var dlg = new SaveFileDialog { Filter = "SQL Server backup (*.bak)|*.bak", FileName = $"InventoryBackup_{DateTime.Now:yyyyMMdd_HHmm}.bak" };
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            try
            {
                await App.Get<SettingsService>().BackupAsync(dlg.FileName);
                UIx.Toast(this, "Backup completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Backup");
                UIx.Error("Backup failed. The folder must be writable by the SQL Server service account (try C:\\Backups or SQL Server's default Backup folder).\r\n\r\n" + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
