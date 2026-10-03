using System.Drawing.Drawing2D;

namespace Prectice_Interview.UI
{
    public static class UIx
    {
        public static Label Label(string text, Font? font = null, Color? color = null, bool autoSize = true) => new()
        {
            Text = text, Font = font ?? Theme.Base, ForeColor = color ?? Theme.Text, AutoSize = autoSize, BackColor = Color.Transparent, UseMnemonic = false
        };

        public static FlatButton Button(string text, BtnKind kind = BtnKind.Primary, EventHandler? onClick = null, int width = 120)
        {
            var b = new FlatButton { Text = text, Kind = kind, Width = width };
            if (onClick != null) b.Click += onClick;
            return b;
        }

        public static InputBox Input(string placeholder = "", int width = 240) => new() { PlaceholderText = placeholder, Width = width };

        public static ComboBox Combo(int width = 200)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = Theme.Base,
                Width = width, BackColor = Color.White, ForeColor = Theme.Text, Height = 34
            };
        }

        public static NumericUpDown Number(decimal min, decimal max, int decimals, int width = 140) => new()
        {
            Minimum = min, Maximum = max, DecimalPlaces = decimals, Width = width, Font = Theme.Base,
            ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right, BorderStyle = BorderStyle.FixedSingle
        };

        public static DateTimePicker DatePicker(DateTime value, int width = 130) => new()
        {
            Format = DateTimePickerFormat.Custom, CustomFormat = "dd MMM yyyy", Value = value, Width = width, Font = Theme.Base
        };

        public static void StyleGrid(DataGridView g)
        {
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Theme.Border;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersDefaultCellStyle.BackColor = Theme.Background;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Background;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.Muted;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 40;
            g.DefaultCellStyle.Font = Theme.Base;
            g.DefaultCellStyle.ForeColor = Theme.Text;
            g.DefaultCellStyle.BackColor = Color.White;
            g.DefaultCellStyle.SelectionBackColor = Theme.PrimarySoft;
            g.DefaultCellStyle.SelectionForeColor = Theme.Text;
            g.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            g.RowTemplate.Height = 38;
            g.RowHeadersVisible = false;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.AllowUserToOrderColumns = false;
            g.MultiSelect = false;
            g.ReadOnly = true;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.ScrollBars = ScrollBars.Both;
            typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(g, true);
        }

        public static DataGridViewColumn Col(this DataGridView g, string header, float weight = 1, bool right = false, string? name = null)
        {
            var c = new DataGridViewTextBoxColumn
            {
                HeaderText = header, Name = name ?? header, FillWeight = weight * 100, SortMode = DataGridViewColumnSortMode.NotSortable,
                MinimumWidth = 50
            };
            if (right) c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            g.Columns.Add(c);
            return c;
        }

        public static void Toast(Control owner, string message, bool error = false)
        {
            var form = owner.FindForm();
            if (form == null) return;
            var lbl = new Label
            {
                Text = "  " + message + "  ", AutoSize = true, Font = Theme.Bold, ForeColor = Color.White,
                BackColor = error ? Theme.Danger : Theme.Success, Padding = new Padding(12, 10, 12, 10)
            };
            form.Controls.Add(lbl);
            lbl.BringToFront();
            lbl.Location = new Point(form.ClientSize.Width - lbl.PreferredWidth - 30, form.ClientSize.Height - lbl.PreferredHeight - 30);
            var t = new System.Windows.Forms.Timer { Interval = 2600 };
            t.Tick += (_, _) => { t.Stop(); t.Dispose(); form.Controls.Remove(lbl); lbl.Dispose(); };
            t.Start();
        }

        public static void Error(string message) => MessageBox.Show(message, "Inventory Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        public static bool Confirm(string message, string title = "Please confirm") =>
            MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
