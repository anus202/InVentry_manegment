using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text;
using Prectice_Interview.Data;
using Prectice_Interview.UI;

namespace Prectice_Interview.Services
{
    public static class CsvExporter
    {
        public static void Export(DataGridView grid, string defaultName)
        {
            using var dlg = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = $"{defaultName}_{DateTime.Now:yyyyMMdd_HHmm}.csv" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            var sb = new StringBuilder();
            var cols = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();
            sb.AppendLine(string.Join(",", cols.Select(c => Esc(c.HeaderText))));
            foreach (DataGridViewRow r in grid.Rows)
                sb.AppendLine(string.Join(",", cols.Select(c => Esc(r.Cells[c.Index].FormattedValue?.ToString()))));
            File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show("Exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string Esc(string? s)
        {
            s ??= "";
            return s.Contains(',') || s.Contains('"') || s.Contains('\n') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
        }
    }
}
