using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text;
using Prectice_Interview.Data;
using Prectice_Interview.UI;

namespace Prectice_Interview.Services
{
    public class InvoicePrinter
    {
        private readonly Sale _sale;
        private readonly CompanySetting _co;
        private int _itemIndex;

        public InvoicePrinter(Sale sale, CompanySetting company)
        {
            _sale = sale;
            _co = company;
        }

        public static void Preview(Sale sale)
        {
            var printer = new InvoicePrinter(sale, App.Get<SettingsService>().Current);
            var doc = printer.CreateDocument();
            using var dlg = new PrintPreviewDialog
            {
                Document = doc,
                Width = 1000,
                Height = 800,
                StartPosition = FormStartPosition.CenterScreen,
                Text = $"Invoice {sale.InvoiceNo}",
                UseAntiAlias = true
            };
            dlg.ShowDialog();
        }

        public PrintDocument CreateDocument()
        {
            var doc = new PrintDocument { DocumentName = $"Invoice {_sale.InvoiceNo}" };
            doc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            doc.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);
            doc.BeginPrint += (_, _) => _itemIndex = 0;
            doc.PrintPage += OnPrintPage;
            return doc;
        }

        private void OnPrintPage(object? sender, PrintPageEventArgs e)
        {
            var g = e.Graphics!;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            var area = e.MarginBounds;
            float left = area.Left, right = area.Right, y = area.Top;
            float width = area.Width;

            using var fTitle = new Font("Segoe UI", 26, FontStyle.Bold);
            using var fCo = new Font("Segoe UI", 15, FontStyle.Bold);
            using var fBold = new Font("Segoe UI", 10, FontStyle.Bold);
            using var fText = new Font("Segoe UI", 10);
            using var fSmall = new Font("Segoe UI", 9);
            using var primary = new SolidBrush(Theme.Primary);
            using var dark = new SolidBrush(Theme.Text);
            using var muted = new SolidBrush(Theme.Muted);
            using var white = new SolidBrush(Color.White);
            using var line = new Pen(Theme.Border, 1f);
            var right_ = new StringFormat { Alignment = StringAlignment.Far };

            g.FillRectangle(primary, left, y, width, 6);
            y += 18;
            g.DrawString(_co.CompanyName, fCo, dark, left, y);
            g.DrawString("INVOICE", fTitle, primary, new RectangleF(left, y - 12, width, 50), right_);
            y += 30;
            var contact = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(_co.Address)) contact.AppendLine(_co.Address);
            if (!string.IsNullOrWhiteSpace(_co.Phone)) contact.AppendLine("Tel: " + _co.Phone);
            if (!string.IsNullOrWhiteSpace(_co.Email)) contact.AppendLine(_co.Email);
            if (!string.IsNullOrWhiteSpace(_co.TaxNumber)) contact.AppendLine("Tax No: " + _co.TaxNumber);
            g.DrawString(contact.ToString().TrimEnd(), fSmall, muted, left, y);

            var meta = new RectangleF(left + width * 0.55f, y, width * 0.45f, 80);
            DrawMeta(g, "Invoice #", _sale.InvoiceNo, meta.Left, meta.Right, y, fBold, fText, dark);
            DrawMeta(g, "Date", Fmt.DateTimeText(_sale.SaleDate), meta.Left, meta.Right, y + 20, fBold, fText, dark);
            DrawMeta(g, "Payment", _sale.PaymentMethod.ToString(), meta.Left, meta.Right, y + 40, fBold, fText, dark);
            DrawMeta(g, "Cashier", _sale.CashierName ?? "-", meta.Left, meta.Right, y + 60, fBold, fText, dark);
            y += 100;

            g.DrawLine(line, left, y, right, y);
            y += 10;
            g.DrawString("BILL TO", fSmall, muted, left, y);
            y += 18;
            g.DrawString(_sale.Customer?.Name ?? "Walk-in Customer", fBold, dark, left, y);
            y += 20;
            var cust = new[] { _sale.Customer?.Phone, _sale.Customer?.Address }.Where(s => !string.IsNullOrWhiteSpace(s));
            foreach (var c in cust) { g.DrawString(c!, fSmall, muted, left, y); y += 16; }
            if (_sale.Status == SaleStatus.Voided)
            {
                using var fv = new Font("Segoe UI", 28, FontStyle.Bold);
                using var vb = new SolidBrush(Color.FromArgb(180, Theme.Danger));
                g.DrawString("VOIDED", fv, vb, right - 220, area.Top + 150);
            }
            y += 14;

            float cNo = left + 8, cName = left + 40, cQty = left + width * 0.62f, cPrice = left + width * 0.80f, cTot = right - 8;
            g.FillRectangle(primary, left, y, width, 28);
            g.DrawString("#", fBold, white, cNo, y + 5);
            g.DrawString("ITEM", fBold, white, cName, y + 5);
            g.DrawString("QTY", fBold, white, new RectangleF(cQty - 50, y + 5, 50, 20), right_);
            g.DrawString("PRICE", fBold, white, new RectangleF(cPrice - 80, y + 5, 80, 20), right_);
            g.DrawString("AMOUNT", fBold, white, new RectangleF(cTot - 100, y + 5, 100, 20), right_);
            y += 34;

            var reservedBottom = 230f;
            var alt = false;
            while (_itemIndex < _sale.Items.Count)
            {
                if (y > area.Bottom - reservedBottom) { e.HasMorePages = true; return; }
                var it = _sale.Items[_itemIndex];
                if (alt) { using var altB = new SolidBrush(Theme.Background); g.FillRectangle(altB, left, y - 3, width, 26); }
                g.DrawString((_itemIndex + 1).ToString(), fText, muted, cNo, y);
                g.DrawString(it.ProductName, fText, dark, new RectangleF(cName, y, cQty - 60 - cName, 20), new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap });
                g.DrawString(it.Quantity.ToString(), fText, dark, new RectangleF(cQty - 50, y, 50, 20), right_);
                g.DrawString(it.UnitPrice.ToString("N2"), fText, dark, new RectangleF(cPrice - 80, y, 80, 20), right_);
                g.DrawString(it.LineTotal.ToString("N2"), fText, dark, new RectangleF(cTot - 100, y, 100, 20), right_);
                y += 26;
                alt = !alt;
                _itemIndex++;
            }
            g.DrawLine(line, left, y, right, y);
            y += 12;

            float tl = left + width * 0.55f;
            void Row(string label, string value, Font f, Brush b)
            {
                g.DrawString(label, f, b, tl, y);
                g.DrawString(value, f, b, new RectangleF(tl, y, right - tl - 8, 22), right_);
                y += 22;
            }
            Row("Subtotal", Fmt.Money(_sale.SubTotal), fText, dark);
            if (_sale.DiscountAmount > 0) Row("Discount", "- " + Fmt.Money(_sale.DiscountAmount), fText, dark);
            if (_sale.TaxAmount > 0) Row($"Tax ({_sale.TaxRate:0.##}%)", Fmt.Money(_sale.TaxAmount), fText, dark);
            y += 4;
            g.FillRectangle(primary, tl - 8, y - 2, right - tl + 8, 32);
            g.DrawString("TOTAL", fBold, white, tl, y + 5);
            g.DrawString(Fmt.Money(_sale.Total), fBold, white, new RectangleF(tl, y + 5, right - tl - 8, 22), right_);
            y += 40;
            Row("Paid", Fmt.Money(_sale.PaidAmount), fText, dark);
            if (_sale.Balance > 0)
            {
                using var danger = new SolidBrush(Theme.Danger);
                Row("Balance Due", Fmt.Money(_sale.Balance), fBold, danger);
            }

            var fy = area.Bottom - 40f;
            g.DrawLine(line, left, fy - 8, right, fy - 8);
            g.DrawString(_co.InvoiceFooter ?? "", fSmall, muted, new RectangleF(left, fy, width, 24), new StringFormat { Alignment = StringAlignment.Center });
            e.HasMorePages = false;
        }

        private static void DrawMeta(Graphics g, string label, string value, float l, float r, float y, Font fb, Font ft, Brush b)
        {
            g.DrawString(label, fb, b, l, y);
            g.DrawString(value, ft, b, new RectangleF(l, y, r - l, 20), new StringFormat { Alignment = StringAlignment.Far });
        }
    }
}
